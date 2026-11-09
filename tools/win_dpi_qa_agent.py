"""
Windows UI Automation and QA Agent for High-DPI Scaling Environments
====================================================================
Designed for Windows 11 environments with display scaling > 100% (e.g. 125%, 150%, 200%).
Addresses DPI virtualization, coordinate normalization, window overflow detection,
programmatic/keyboard fallback layout strategies, and visual bounding-box verification.
"""

import sys
import os
import time
import math
import ctypes
from ctypes import wintypes
from typing import Dict, Tuple, List, Optional, Any
from dataclasses import dataclass

# ==============================================================================
# 1. Win32 API Definitions & Enums
# ==============================================================================

user32 = ctypes.windll.user32
shcore = ctypes.windll.shcore
kernel32 = ctypes.windll.kernel32
gdi32 = ctypes.windll.gdi32

# Process DPI Awareness Enums
class PROCESS_DPI_AWARENESS:
    PROCESS_DPI_UNAWARE = 0
    PROCESS_SYSTEM_DPI_AWARE = 1
    PROCESS_PER_MONITOR_DPI_AWARE = 2

# DPI Awareness Contexts
DPI_AWARENESS_CONTEXT_UNAWARE              = ctypes.c_void_p(-1)
DPI_AWARENESS_CONTEXT_SYSTEM_AWARE         = ctypes.c_void_p(-2)
DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE    = ctypes.c_void_p(-3)
DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = ctypes.c_void_p(-4)
DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED    = ctypes.c_void_p(-5)

# Win32 Constants
SM_CXSCREEN = 0
SM_CYSCREEN = 1
SM_CXVIRTUALSCREEN = 78
SM_CYVIRTUALSCREEN = 79

SWP_NOSIZE     = 0x0001
SWP_NOMOVE     = 0x0002
SWP_NOZORDER   = 0x0004
SWP_NOACTIVATE = 0x0010
SWP_FRAMECHANGED = 0x0020
SWP_SHOWWINDOW = 0x0040

WM_MOUSEMOVE   = 0x0200
WM_LBUTTONDOWN = 0x0201
WM_LBUTTONUP   = 0x0202
WM_KEYDOWN     = 0x0100
WM_KEYUP       = 0x0101
WM_SYSCOMMAND  = 0x0112
SC_MOVE        = 0xF010

VK_TAB    = 0x09
VK_RETURN = 0x0D
VK_SPACE  = 0x20
VK_LEFT   = 0x25
VK_UP     = 0x26
VK_RIGHT  = 0x27
VK_DOWN   = 0x28

PW_RENDERFULLCONTENT = 0x00000002

# Structures
class RECT(ctypes.Structure):
    _fields_ = [
        ("left", wintypes.LONG),
        ("top", wintypes.LONG),
        ("right", wintypes.LONG),
        ("bottom", wintypes.LONG),
    ]

    @property
    def width(self) -> int:
        return self.right - self.left

    @property
    def height(self) -> int:
        return self.bottom - self.top

    def as_tuple(self) -> Tuple[int, int, int, int]:
        return (self.left, self.top, self.right, self.bottom)

class POINT(ctypes.Structure):
    _fields_ = [("x", wintypes.LONG), ("y", wintypes.LONG)]

# Function Signatures
user32.SetProcessDpiAwarenessContext.argtypes = [ctypes.c_void_p]
user32.SetProcessDpiAwarenessContext.restype = wintypes.BOOL

user32.GetDpiForSystem.argtypes = []
user32.GetDpiForSystem.restype = wintypes.UINT

user32.GetDpiForWindow.argtypes = [wintypes.HWND]
user32.GetDpiForWindow.restype = wintypes.UINT

user32.GetWindowDpiAwarenessContext.argtypes = [wintypes.HWND]
user32.GetWindowDpiAwarenessContext.restype = ctypes.c_void_p

user32.GetAwarenessFromDpiAwarenessContext.argtypes = [ctypes.c_void_p]
user32.GetAwarenessFromDpiAwarenessContext.restype = ctypes.c_int

user32.GetWindowRect.argtypes = [wintypes.HWND, ctypes.POINTER(RECT)]
user32.GetWindowRect.restype = wintypes.BOOL

user32.GetClientRect.argtypes = [wintypes.HWND, ctypes.POINTER(RECT)]
user32.GetClientRect.restype = wintypes.BOOL

user32.SetWindowPos.argtypes = [
    wintypes.HWND, wintypes.HWND,
    ctypes.c_int, ctypes.c_int, ctypes.c_int, ctypes.c_int,
    wintypes.UINT
]
user32.SetWindowPos.restype = wintypes.BOOL

user32.PostMessageW.argtypes = [wintypes.HWND, wintypes.UINT, wintypes.WPARAM, wintypes.LPARAM]
user32.PostMessageW.restype = wintypes.BOOL

user32.SendMessageW.argtypes = [wintypes.HWND, wintypes.UINT, wintypes.WPARAM, wintypes.LPARAM]
user32.SendMessageW.restype = ctypes.c_ssize_t

user32.PrintWindow.argtypes = [wintypes.HWND, wintypes.HDC, wintypes.UINT]
user32.PrintWindow.restype = wintypes.BOOL

shcore.GetProcessDpiAwareness.argtypes = [wintypes.HANDLE, ctypes.POINTER(ctypes.c_int)]
shcore.GetProcessDpiAwareness.restype = wintypes.HRESULT

shcore.SetProcessDpiAwareness.argtypes = [ctypes.c_int]
shcore.SetProcessDpiAwareness.restype = wintypes.HRESULT

# ==============================================================================
# 2. Data Classes & Results
# ==============================================================================

@dataclass
class DisplayMetrics:
    dpi: int
    scale_factor: float
    screen_width: int
    screen_height: int
    virtual_width: int
    virtual_height: int

@dataclass
class WindowProcessDpiMeta:
    hwnd: int
    title: str
    pid: int
    dpi: int
    scale_factor: float
    awareness_id: int
    awareness_desc: str
    bounds_raw: Tuple[int, int, int, int]
    bounds_adjusted: Tuple[float, float, float, float]

@dataclass
class OverflowCheckResult:
    is_overflowing: bool
    clip_left: int
    clip_top: int
    clip_right: int
    clip_bottom: int
    delta_x: int
    delta_y: int

# ==============================================================================
# 3. Windows UI Automation & QA Agent Class
# ==============================================================================

class WindowsDpiQAAgent:
    """
    Expert UI Automation & QA Agent handling High-DPI Windows 11 Game Environments.
    """

    def __init__(self, force_agent_per_monitor_v2: bool = True):
        # 1. Initialize agent process awareness to Per-Monitor V2 so our own inspection
        # queries report true unvirtualized physical coordinates from the OS.
        if force_agent_per_monitor_v2:
            self._set_current_process_dpi_awareness_v2()

    def _set_current_process_dpi_awareness_v2(self) -> bool:
        try:
            return bool(user32.SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
        except Exception:
            try:
                hr = shcore.SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE)
                return hr == 0
            except Exception:
                return False

    # --------------------------------------------------------------------------
    # Step 1: Pre-Check DPI & Resolution
    # --------------------------------------------------------------------------
    def query_display_settings(self) -> DisplayMetrics:
        """
        Query the active display settings, baseline DPI, and calculate scaling factor S.
        Standard baseline is 96 DPI (100% = 1.0, 125% = 1.25, 150% = 1.5, etc.).
        """
        system_dpi = user32.GetDpiForSystem()
        if system_dpi == 0:
            system_dpi = 96

        scale_factor = round(system_dpi / 96.0, 4)
        cx_screen = user32.GetSystemMetrics(SM_CXSCREEN)
        cy_screen = user32.GetSystemMetrics(SM_CYSCREEN)
        cx_virtual = user32.GetSystemMetrics(SM_CXVIRTUALSCREEN)
        cy_virtual = user32.GetSystemMetrics(SM_CYVIRTUALSCREEN)

        return DisplayMetrics(
            dpi=system_dpi,
            scale_factor=scale_factor,
            screen_width=cx_screen,
            screen_height=cy_screen,
            virtual_width=cx_virtual,
            virtual_height=cy_virtual,
        )

    def inspect_window_dpi_metadata(self, hwnd: int) -> WindowProcessDpiMeta:
        """
        Inspect the target window's process metadata for DPI awareness level,
        effective window DPI, and physical/adjusted bounding boxes.
        """
        # Window title
        length = user32.GetWindowTextLengthW(hwnd)
        buf = ctypes.create_unicode_buffer(length + 1)
        user32.GetWindowTextW(hwnd, buf, length + 1)
        title = buf.value

        # Window PID
        pid = wintypes.DWORD()
        user32.GetWindowThreadProcessId(hwnd, ctypes.byref(pid))

        # Effective DPI for window
        win_dpi = user32.GetDpiForWindow(hwnd)
        if win_dpi == 0:
            win_dpi = user32.GetDpiForSystem()
        scale = round(win_dpi / 96.0, 4)

        # Query DPI Awareness Context
        ctx = user32.GetWindowDpiAwarenessContext(hwnd)
        awareness = user32.GetAwarenessFromDpiAwarenessContext(ctx)

        awareness_names = {
            -1: "DPI_AWARENESS_INVALID",
            0: "DPI_AWARENESS_UNAWARE (DPI-Virtualization Active)",
            1: "DPI_AWARENESS_SYSTEM_AWARE",
            2: "DPI_AWARENESS_PER_MONITOR_AWARE (V1 or V2)",
        }
        desc = awareness_names.get(awareness, f"UNKNOWN ({awareness})")

        # Window Bounding Box
        rect = RECT()
        user32.GetWindowRect(hwnd, ctypes.byref(rect))
        raw_bounds = rect.as_tuple()

        # Coordinate Normalization Formula: X_adj = X_raw / S, Y_adj = Y_raw / S
        adj_bounds = self.normalize_bounding_box(raw_bounds, scale)

        return WindowProcessDpiMeta(
            hwnd=hwnd,
            title=title,
            pid=pid.value,
            dpi=win_dpi,
            scale_factor=scale,
            awareness_id=awareness,
            awareness_desc=desc,
            bounds_raw=raw_bounds,
            bounds_adjusted=adj_bounds,
        )

    # --------------------------------------------------------------------------
    # Step 2: Coordinate Normalization Formula & DPI Awareness Enforcement
    # --------------------------------------------------------------------------
    @staticmethod
    def normalize_coordinates(x_raw: float, y_raw: float, scale_factor: float) -> Tuple[float, float]:
        """
        Coordinate Normalization Formula:
        If S > 1.0:
            X_adjusted = X_raw / S
            Y_adjusted = Y_raw / S
        """
        if scale_factor <= 0:
            scale_factor = 1.0
        return (round(x_raw / scale_factor, 2), round(y_raw / scale_factor, 2))

    @staticmethod
    def scale_to_raw_coordinates(x_adj: float, y_adj: float, scale_factor: float) -> Tuple[int, int]:
        """
        Reverse normalization for physical hardware dispatch:
            X_raw = round(X_adj * S)
            Y_raw = round(Y_adj * S)
        """
        return (int(round(x_adj * scale_factor)), int(round(y_adj * scale_factor)))

    @staticmethod
    def normalize_bounding_box(bounds_raw: Tuple[int, int, int, int], scale_factor: float) -> Tuple[float, float, float, float]:
        """
        Normalizes [left, top, right, bottom] bounding box via 1 / S.
        """
        l, t, r, b = bounds_raw
        if scale_factor <= 0:
            scale_factor = 1.0
        return (
            round(l / scale_factor, 2),
            round(t / scale_factor, 2),
            round(r / scale_factor, 2),
            round(b / scale_factor, 2),
        )

    def enforce_dpi_awareness_compat(self, executable_path: str) -> bool:
        """
        Ensures the OS compatibility layer enforces Per-Monitor High-DPI awareness
        for the game executable via Windows AppCompat registry flags.
        """
        try:
            import winreg
            key_path = r"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"
            with winreg.CreateKey(winreg.HKEY_CURRENT_USER, key_path) as key:
                # ~ HIGHDPIAWARE forces Per-Monitor DPI awareness
                winreg.SetValueEx(key, executable_path, 0, winreg.REG_SZ, "~ HIGHDPIAWARE")
            return True
        except Exception as e:
            print(f"[Warn] Could not set AppCompatFlags registry: {e}")
            return False

    def enforce_windowed_fullscreen(self, hwnd: int) -> bool:
        """
        Forces window to borderless windowed-fullscreen matching primary display,
        bypassing window border calculation bugs in scaled DPI modes.
        """
        display = self.query_display_settings()
        GWL_STYLE = -16
        WS_POPUP = 0x80000000
        WS_VISIBLE = 0x10000000

        # Set style to popup (borderless)
        user32.SetWindowLongPtrW(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE)
        # Position at (0, 0) with full display dimensions
        res = user32.SetWindowPos(
            hwnd,
            ctypes.c_void_p(0), # HWND_TOP
            0, 0,
            display.screen_width, display.screen_height,
            SWP_FRAMECHANGED | SWP_SHOWWINDOW
        )
        return bool(res)

    # --------------------------------------------------------------------------
    # Step 3: Layout Overflow Detection & Fallback Strategies
    # --------------------------------------------------------------------------
    def detect_overflow(self, hwnd: int, display: Optional[DisplayMetrics] = None) -> OverflowCheckResult:
        """
        Evaluates whether the game window or dialog overflows past the primary screen matrix.
        """
        if display is None:
            display = self.query_display_settings()

        rect = RECT()
        user32.GetWindowRect(hwnd, ctypes.byref(rect))

        left = rect.left
        top = rect.top
        right = rect.right
        bottom = rect.bottom
        width = rect.width
        height = rect.height

        clip_left = max(0, -left)
        clip_top = max(0, -top)
        clip_right = max(0, right - display.screen_width)
        clip_bottom = max(0, bottom - display.screen_height)

        is_overflowing = (clip_left > 0 or clip_top > 0 or clip_right > 0 or clip_bottom > 0)

        # Centering delta:
        # Target centered X = (W_screen - W_win) // 2
        # Target centered Y = (H_screen - H_win) // 2
        target_x = max(0, (display.screen_width - width) // 2)
        target_y = max(0, (display.screen_height - height) // 2)

        delta_x = target_x - left
        delta_y = target_y - top

        return OverflowCheckResult(
            is_overflowing=is_overflowing,
            clip_left=clip_left,
            clip_top=clip_top,
            clip_right=clip_right,
            clip_bottom=clip_bottom,
            delta_x=delta_x,
            delta_y=delta_y,
        )

    def fallback_recenter_window(self, hwnd: int, delta_x: int, delta_y: int) -> bool:
        """
        Fallback Strategy A1: Programmatic title bar repositioning by (delta_x, delta_y).
        """
        rect = RECT()
        user32.GetWindowRect(hwnd, ctypes.byref(rect))
        new_x = rect.left + delta_x
        new_y = rect.top + delta_y

        res = user32.SetWindowPos(
            hwnd,
            ctypes.c_void_p(0),
            new_x, new_y, 0, 0,
            SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE
        )
        return bool(res)

    def fallback_simulate_title_drag(self, hwnd: int, delta_x: int, delta_y: int, steps: int = 10) -> None:
        """
        Fallback Strategy A2: Simulates mouse drag across the window's title bar coordinates.
        """
        rect = RECT()
        user32.GetWindowRect(hwnd, ctypes.byref(rect))

        # Title bar coordinate inside client or non-client area
        start_x = rect.left + (rect.width // 2)
        start_y = rect.top + 15

        # Move mouse and drag
        lparam_start = (start_y << 16) | (start_x & 0xFFFF)
        user32.PostMessageW(hwnd, WM_MOUSEMOVE, 0, lparam_start)
        time.sleep(0.1)
        user32.PostMessageW(hwnd, WM_LBUTTONDOWN, 0x0001, lparam_start)
        time.sleep(0.1)

        for step in range(1, steps + 1):
            curr_x = start_x + int((delta_x * step) / steps)
            curr_y = start_y + int((delta_y * step) / steps)
            lp = (curr_y << 16) | (curr_x & 0xFFFF)
            user32.PostMessageW(hwnd, WM_MOUSEMOVE, 0x0001, lp)
            time.sleep(0.03)

        end_x = start_x + delta_x
        end_y = start_y + delta_y
        lparam_end = (end_y << 16) | (end_x & 0xFFFF)
        time.sleep(0.1)
        user32.PostMessageW(hwnd, WM_LBUTTONUP, 0, lparam_end)

    def fallback_keyboard_move(self, hwnd: int, delta_x: int, delta_y: int) -> None:
        """
        Fallback Strategy B1: Alt+Space -> Move (SC_MOVE) -> Arrow nudge -> Enter.
        """
        # Trigger System Menu Move command
        user32.SendMessageW(hwnd, WM_SYSCOMMAND, SC_MOVE, 0)
        time.sleep(0.2)

        # Arrow key steps (approx 10px per arrow hit in Windows standard window move)
        x_steps = abs(delta_x) // 10
        y_steps = abs(delta_y) // 10
        x_key = VK_RIGHT if delta_x > 0 else VK_LEFT
        y_key = VK_DOWN if delta_y > 0 else VK_UP

        for _ in range(x_steps):
            self._send_key(hwnd, x_key)
            time.sleep(0.02)

        for _ in range(y_steps):
            self._send_key(hwnd, y_key)
            time.sleep(0.02)

        # Commit move via Enter
        self._send_key(hwnd, VK_RETURN)

    def fallback_tab_focus_cycle(self, hwnd: int, cycles: int = 5) -> None:
        """
        Fallback Strategy B2: Tab focus cycling to bring clipped action buttons into view.
        """
        for _ in range(cycles):
            self._send_key(hwnd, VK_TAB)
            time.sleep(0.15)

    def _send_key(self, hwnd: int, vk_code: int) -> None:
        down_lparam = 0x00000001
        up_lparam = 0xC0000001
        user32.PostMessageW(hwnd, WM_KEYDOWN, vk_code, down_lparam)
        time.sleep(0.05)
        user32.PostMessageW(hwnd, WM_KEYUP, vk_code, up_lparam)

    def find_windows_by_title(self, substring: str) -> List[Tuple[int, str]]:
        """Find visible windows matching a title substring."""
        matches = []
        def enum_cb(hwnd, lparam):
            if user32.IsWindowVisible(hwnd):
                length = user32.GetWindowTextLengthW(hwnd)
                if length > 0:
                    buf = ctypes.create_unicode_buffer(length + 1)
                    user32.GetWindowTextW(hwnd, buf, length + 1)
                    if substring.lower() in buf.value.lower():
                        matches.append((hwnd, buf.value))
            return True

        WNDENUMPROC = ctypes.WINFUNCTYPE(ctypes.c_bool, wintypes.HWND, wintypes.LPARAM)
        user32.EnumWindows(WNDENUMPROC(enum_cb), 0)
        return matches

    def capture_window_bitmap(self, hwnd: int) -> Optional[Any]:
        """
        Captures window visual bounds using GDI and PrintWindow (PW_RENDERFULLCONTENT).
        Returns a PIL Image if Pillow is available.
        """
        rect = RECT()
        user32.GetWindowRect(hwnd, ctypes.byref(rect))
        w = rect.width
        h = rect.height
        if w <= 0 or h <= 0:
            return None

        hwnd_dc = user32.GetWindowDC(hwnd)
        mem_dc = gdi32.CreateCompatibleDC(hwnd_dc)
        bitmap = gdi32.CreateCompatibleBitmap(hwnd_dc, w, h)
        old_bmp = gdi32.SelectObject(mem_dc, bitmap)

        # PrintWindow with PW_RENDERFULLCONTENT
        success = user32.PrintWindow(hwnd, mem_dc, PW_RENDERFULLCONTENT)
        if not success:
            # Fallback to standard PrintWindow
            success = user32.PrintWindow(hwnd, mem_dc, 0)

        img = None
        if success:
            try:
                from PIL import Image
                # Extract bitmap bytes
                class BITMAPINFOHEADER(ctypes.Structure):
                    _fields_ = [
                        ('biSize', wintypes.DWORD),
                        ('biWidth', wintypes.LONG),
                        ('biHeight', wintypes.LONG),
                        ('biPlanes', wintypes.WORD),
                        ('biBitCount', wintypes.WORD),
                        ('biCompression', wintypes.DWORD),
                        ('biSizeImage', wintypes.DWORD),
                        ('biXPelsPerMeter', wintypes.LONG),
                        ('biYPelsPerMeter', wintypes.LONG),
                        ('biClrUsed', wintypes.DWORD),
                        ('biClrImportant', wintypes.DWORD),
                    ]

                bih = BITMAPINFOHEADER()
                bih.biSize = ctypes.sizeof(BITMAPINFOHEADER)
                bih.biWidth = w
                bih.biHeight = -h # top-down
                bih.biPlanes = 1
                bih.biBitCount = 32
                bih.biCompression = 0 # BI_RGB

                buf = ctypes.create_string_buffer(w * h * 4)
                gdi32.GetDIBits(mem_dc, bitmap, 0, h, buf, ctypes.byref(bih), 0) # DIB_RGB_COLORS
                img = Image.frombuffer('RGBA', (w, h), buf, 'raw', 'BGRA', 0, 1)
            except Exception as e:
                print(f"[Warn] Image conversion failed: {e}")

        # Cleanup GDI handles
        gdi32.SelectObject(mem_dc, old_bmp)
        gdi32.DeleteObject(bitmap)
        gdi32.DeleteDC(mem_dc)
        user32.ReleaseDC(hwnd, hwnd_dc)
        return img

    # --------------------------------------------------------------------------
    # Step 4: Verification via Screenshot Bounds & Element Containment
    # --------------------------------------------------------------------------
    def verify_window_and_elements_fit(
        self,
        hwnd: int,
        critical_elements_raw: Optional[List[Dict[str, Any]]] = None
    ) -> Dict[str, Any]:
        """
        Takes snapshot of window bounds and verifies all critical UI buttons and borders
        fit entirely within the primary display matrix without clipping.
        """
        display = self.query_display_settings()
        meta = self.inspect_window_dpi_metadata(hwnd)
        overflow = self.detect_overflow(hwnd, display)

        element_verifications = []
        all_elements_fit = True

        if critical_elements_raw:
            win_l, win_t, win_r, win_b = meta.bounds_raw
            scale = meta.scale_factor

            for elem in critical_elements_raw:
                elem_id = elem.get("id", "element")
                # Expected relative or screen coordinates
                raw_x = elem.get("x", 0)
                raw_y = elem.get("y", 0)
                raw_w = elem.get("width", 50)
                raw_h = elem.get("height", 20)

                # Normalized coordinates
                adj_x, adj_y = self.normalize_coordinates(raw_x, raw_y, scale)
                adj_w, adj_h = self.normalize_coordinates(raw_w, raw_h, scale)

                # Absolute element bounds in physical screen space
                elem_screen_l = win_l + raw_x
                elem_screen_t = win_t + raw_y
                elem_screen_r = elem_screen_l + raw_w
                elem_screen_b = elem_screen_t + raw_h

                elem_clipped = (
                    elem_screen_l < 0 or
                    elem_screen_t < 0 or
                    elem_screen_r > display.screen_width or
                    elem_screen_b > display.screen_height or
                    elem_screen_r > win_r or
                    elem_screen_b > win_b
                )

                if elem_clipped:
                    all_elements_fit = False

                element_verifications.append({
                    "id": elem_id,
                    "raw_rect": (elem_screen_l, elem_screen_t, elem_screen_r, elem_screen_b),
                    "adjusted_coords": (adj_x, adj_y, adj_w, adj_h),
                    "clipped": elem_clipped,
                })

        verdict = (not overflow.is_overflowing) and all_elements_fit

        return {
            "verdict": "PASS" if verdict else "FAIL",
            "display": {
                "dpi": display.dpi,
                "scale_factor": display.scale_factor,
                "resolution": f"{display.screen_width}x{display.screen_height}",
            },
            "window": {
                "hwnd": meta.hwnd,
                "title": meta.title,
                "dpi": meta.dpi,
                "awareness": meta.awareness_desc,
                "bounds_raw": meta.bounds_raw,
                "bounds_adjusted": meta.bounds_adjusted,
            },
            "overflow": {
                "is_overflowing": overflow.is_overflowing,
                "clip_margins": {
                    "left": overflow.clip_left,
                    "top": overflow.clip_top,
                    "right": overflow.clip_right,
                    "bottom": overflow.clip_bottom,
                },
                "recommended_delta": (overflow.delta_x, overflow.delta_y),
            },
            "elements": element_verifications,
        }

# ==============================================================================
# Self-Test Runner
# ==============================================================================

if __name__ == "__main__":
    print("=" * 70)
    print("  WINDOWS HIGH-DPI UI AUTOMATION & QA AGENT - DIAGNOSTIC SUITE")
    print("=" * 70)

    agent = WindowsDpiQAAgent(force_agent_per_monitor_v2=True)
    display = agent.query_display_settings()

    print(f"\n[1. Display Pre-Check]")
    print(f"  - Active System DPI  : {display.dpi} ({int(display.scale_factor * 100)}%)")
    print(f"  - System Scale (S)   : {display.scale_factor}")
    print(f"  - Primary Resolution : {display.screen_width} x {display.screen_height}")
    print(f"  - Virtual Screen     : {display.virtual_width} x {display.virtual_height}")

    # Coordinate Normalization Test
    print(f"\n[2. Coordinate Normalization Verification]")
    sample_raw_clicks = [(1920, 1080), (1280, 720), (2560, 1440), (800, 600)]
    for raw_x, raw_y in sample_raw_clicks:
        adj_x, adj_y = agent.normalize_coordinates(raw_x, raw_y, display.scale_factor)
        recovered_x, recovered_y = agent.scale_to_raw_coordinates(adj_x, adj_y, display.scale_factor)
        print(f"  - Raw ({raw_x:>4}, {raw_y:>4}) / S({display.scale_factor}) => Adjusted: ({adj_x:>7.2f}, {adj_y:>7.2f}) => Round-trip: ({recovered_x}, {recovered_y})")

    # Inspect foreground or active windows
    print(f"\n[3. Window Metadata & Layout Verification]")
    fg_hwnd = user32.GetForegroundWindow()
    if fg_hwnd:
        meta = agent.inspect_window_dpi_metadata(fg_hwnd)
        print(f"  - Foreground HWND    : {meta.hwnd} ('{meta.title}')")
        print(f"  - Process ID         : {meta.pid}")
        print(f"  - Window DPI & Scale : {meta.dpi} (S = {meta.scale_factor})")
        print(f"  - DPI Awareness      : {meta.awareness_desc}")
        print(f"  - Raw Bounds         : {meta.bounds_raw}")
        print(f"  - Adjusted Bounds    : {meta.bounds_adjusted}")

        # Test sample critical UI elements inside window
        sample_buttons = [
            {"id": "btn_options", "x": 100, "y": 200, "width": 180, "height": 45},
            {"id": "btn_start_game", "x": 100, "y": 260, "width": 180, "height": 45},
            {"id": "btn_quit", "x": 100, "y": 320, "width": 180, "height": 45},
        ]
        result = agent.verify_window_and_elements_fit(fg_hwnd, sample_buttons)
        print(f"\n[4. QA Verification Verdict]")
        print(f"  - Overflow Status    : {'OVERFLOWING' if result['overflow']['is_overflowing'] else 'WITHIN BOUNDS'}")
        print(f"  - Clip Margins (L,T,R,B): {result['overflow']['clip_margins']}")
        print(f"  - Overall Verdict    : {result['verdict']}")
    else:
        print("  - No active foreground window detected.")

    print("\n" + "=" * 70)
