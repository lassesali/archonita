# Archonita — Migration Update Patch Notes

---

## 📜 Origins & Legacy

**Archonita** was originally conceived, developed, and published by **Gamelon Productions** for the renowned **Suomipelit.com Pikkukesäkilpailu 2006** game jam in June 2006. Built within the creative constraints of a short game jam, the game pioneered a hybrid gameplay formula: tactical turn-based grid positioning reminiscent of classic board games like *Archon*, merged with dynamic real-time dice rolls, spellcasting, unit recruitment, and card-based strategy.

Initially engineered on GarageGames' early 32-bit **Torque Game Builder 1.1** (TGB / TGE2D), the original codebase established the iconic Finnish-language domain architecture that still powers the game under the hood (`sankari`, `nappula`, `olento`, `lisaaUusiNappulaLaudalle`, `sekuntiLaskuri`, `valittuRuutu`).

Twenty years later, this **Migration Update** bridges two decades of engine evolution, porting Archonita from legacy Windows XP-era tech to the modern, open-source, 64-bit **Torque2D 4.0 (EA4)** engine runtime.

---

## ⚙️ Engine & Backend Migration

### 1. 64-Bit Modern Architecture & Runtime Overhaul
- **Binary Modernization:** Transitioned from the legacy 32-bit `TGB.exe` to a native 64-bit `Torque2D.exe` runtime, ensuring complete stability on modern Windows 10/11 x64 systems and resolving legacy address space limitations.
- **Modular Architecture (`module.taml`):** Converted the project into a self-contained module (`Archon`, version 1.0.0) adhering to Torque2D 4.0 module lifecycle specifications (`create` / `destroy` callbacks), cleanly registered under the engine's `ModuleDatabase`.

### 2. Scene Graph to TAML Serialization
- **Declarative Level Architecture:** Migrated all legacy scene graph files (`.t2d`) into modern XML/TAML Scene files (`.scene.taml`):
  - `intro.scene.taml`
  - `mainmenu.scene.taml`
  - `createhero.scene.taml`
  - `board.scene.taml`
  - `help.scene.taml`
  - `credits.scene.taml`
- **Camera & Coordinate System Conversion:**
  - Standardized coordinate transformations from legacy TGB units to T2D 4.0 orthographic camera viewports (`100 75`).
  - Adapted world coordinates across the Y-axis to match Torque2D 4.0 scene space conventions while preserving exact board tile alignments via world-space mapping (`boardToWorld` and `boardToWorldUnit`).

### 3. Unified Asset Management (`AssetDatabase`)
- **Image & Animation Assets:**
  - Deprecated legacy `t2dImageMapDatablock` and `t2dAnimationDatablock` script declarations.
  - Converted over 70 image maps into declarative `<ImageAsset>` descriptors and 160+ sprite frame sequences into `<AnimationAsset>` definitions with explicit frame timings, cyclic states, and sprite sheet slicing.
- **Particle Systems:**
  - Reconstructed legacy `.eff` particle configurations into `<ParticleAsset>` descriptors (e.g., `magic_trail.asset.taml`, `bubbles.asset.taml`, `blood.asset.taml`).
  - Restored proper texture bindings (`particles1ImageMap`, `smallbubble`) to eliminate fallback placeholders and recover crisp starbursts and rising bubble emitters.

### 4. TorqueScript 2.0 Compatibility Layer (`t2d_compat.cs`)
- Implemented a specialized, high-performance compatibility bridge to preserve the original 2006 gameplay logic while translating obsolete API calls to modern Torque2D classes:
  - `Sprite` and `Scroller` abstraction wrapping legacy `t2dStaticSprite`, `t2dAnimatedSprite`, and `t2dScroller`.
  - Transform and kinematics mapping: `moveto`, `setAngularVelocity`, `mount`, `unmount`.
  - Vector math parity helpers: `t2dVectorDistance`, `t2dVectorCompare`.
  - Smooth animation controller hooks (`playAnimation`, `setFrame`, `setImageMap`).
  - Timed visual fade controllers (`startFadeIn`, `startFadeOut`).

### 5. OpenAL Audio Engine & Fully Qualified Asset IDs
- **Audio Profile Modernization:** Replaced obsolete `AudioProfile` / `AudioDescription` script blocks with standalone `<AudioAsset>` definitions in `Archon/data/audio/`.
- **SFX Routing Pipeline (`playArchonSound`):**
  - Updated audio playback across all gameplay scripts to route through fully qualified module asset IDs (`"Archon:<AssetName>"`), resolving silent failures in legacy `alxPlay()` calls.
  - Restored combat impact sounds (`fxBeenHit`), attack swings (`fxSword`), creature movement footsteps (`fxFootStep`), death sequences (`fxDie`), countdown ticks (`ambient4Audio`), and looping background music (`ambient1Audio`).

### 6. Multi-Pass Framebuffer Alpha Blending Restoration
- **Inverted Multiplicative Blending:** Faithfully restored the 2006 intro screen's dynamic gold-scrolling logo:
  - **Pass 1:** Renders the authentic inverted monochrome logo mask (`gamelonlogo.png`) on `SceneLayer 1`.
  - **Pass 2:** Overlays the animated gold texture scroller (`jee1.png`) on `SceneLayer 0` configured with `BlendMode="1"`, `SrcBlendFactor="ONE_MINUS_DST_COLOR"`, and `DstBlendFactor="ZERO"`.
  - Produces crisp, transparent masking where the wavy gold texture shines exclusively through the map icon and typography against a void-black background.

---

## 💾 Player Data Transfer & State Management

### 1. Game State Machine Modernization
- Reinforced the global turn and game lifecycle state machine (`$gameIntroState`, `$gameMenuState`, `$gamePreBattleState`, `$gameBattleState`, `$gameEndState`).
- Eliminated state desynchronization between scene loads, ensuring clean transitions from hero creation to the tactical board.

### 2. Character Data & Attribute Integrity
- **Hero Group Persistence:** Preserved full data integrity for player and AI hero singletons across transitions:
  - Class progression (`sankarivalittu`: 1 = Warrior, 2 = Wizard).
  - Resource pools: Creature Points (`olentopisteet`) and Mana (`mana`).
  - Profession attributes (`ammatti`), action allowances, and hero portrait icon states.
- **Unit Roster State:**
  - Automated translation of unit datablocks across all 8 fantasy archetypes: *Warrior*, *Archer*, *Cleric*, *Paladin*, *Skeleton Warrior*, *Skeleton Archer*, *Dread Zombie*, and *Ancient Ghost*.
  - Maintained distinct direction vectors and per-state animations (*Stopped*, *Walking*, *Attack*, *Magic*, *BeenHit*, *TippingOver*).

### 3. Strict Board Placement & Boundary Validation
- **Grid Boundary Enforcement:**
  - Implemented boundary validation intercepting creature drops in `lisaaUusiNappulaLaudalle` to strictly restrict placement to legal coordinates ($0 \le x < 4, 0 \le y < 4$).
  - Added occupancy and obstacle collision guards preventing unit stacking.
- **Snap-Back Rejection Fallback:**
  - Invalid drops or placement on non-green tiles instantly reject transform updates and safely snap the creature back to its originating tray slot.
- **Unit Tray Lifecycle Management:**
  - Re-anchored the lower creature tray to coordinate bounds (`Y = -28 / -30`), eliminating the regression where placement icons drifted to the screen top.
  - Added automatic safe deletion of all 12 tray slots upon transitioning from preparation into active combat (`InitGameForBattle`).

---

## 🚀 Performance Upgrades

### 1. Rendering & Graphics Pipeline
| Component | 2006 Legacy (TGB 1.1) | 2026 Migration (Torque2D 4.0 EA4) |
| :--- | :--- | :--- |
| **Architecture** | 32-bit x86 | Native 64-bit x86_64 |
| **Graphics API** | Fixed-function legacy OpenGL | Modern OpenGL 4.6 Core / Compatibility |
| **Resolution Support** | Fixed 4:3 (800x600, 1024x768) | Multi-resolution windowed & widescreen display scaling |
| **Layer Rendering** | CPU-sorted 32-layer Z-buffer | SceneLayer depth batching ($0$ front to $31$ back) |
| **Frame Rates** | Clamped VSync / software timer | Smooth vsync uncoupled logic ticks |

### 2. Input Handling & Screen Picking
- Modernized mouse coordinate picking via `sceneWindow2D.pickPoint`, providing zero-latency collision detection against board tiles, creatures, and GUI control cards.
- Refined drag-and-drop mechanics with state caching to prevent ghost clicks or stuck drag states.

### 3. Asset Loading & Memory Efficiency
- Switched to declarative TAML parsing, cutting scene initialization and transition times.
- Implemented automatic caching of compiled TorqueScript objects (`.dso`), reducing repeat script parsing overhead.
- Optimized texture memory by consolidating loose images into dedicated asset manifests.

### 4. Audio Subsystem Optimization
- Direct hardware-accelerated OpenAL streaming for background music, minimizing CPU memory overhead during continuous loops.
- Instantaneous one-shot sound buffer reuse for high-frequency combat audio effects.

---

> [!NOTE]
> All original Finnish domain functions, gameplay rules, and board behaviors from the 2006 release have been preserved in full conformity with [TORQUE_SCRIPT.md](file:///M:/Archonita/games/TORQUE_SCRIPT.md).
