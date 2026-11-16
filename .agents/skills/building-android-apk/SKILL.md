---
name: building-android-apk
description: Use when building, updating, or troubleshooting Android Debug/Release APKs from engine/compilers/android-studio in this repository.
---

# Building Android APKs (Torque2D / Archonita)

Follow this playbook whenever building or modifying the Android APK target in `engine/compilers/android-studio`.

## 1. Environment & Toolchain Preconditions

- **Project root:** `engine/compilers/android-studio` (AGP `8.6.0` in `settings.gradle`, Gradle `8.7` in `gradle/wrapper/gradle-wrapper.properties`).
- **SDK / NDK / ABI (`app/build.gradle`):**
  - `compileSdk 34`, `minSdk 21`, `targetSdk 34`
  - `ndkVersion '25.2.9519653'`
  - `abiFilters 'arm64-v8a'` (the only ABI with prebuilt `libfreetype.a`; `libopenal.so` is compiled from source by CMake)
  - Native build drives repo-root `CMakeLists.txt` with `-DANDROID_STL=c++_static` and `-std=c++17`.
- **JDK requirement (critical on hosts with JDK 25+):**
  - Gradle `8.7` fails Groovy buildscript compilation under OpenJDK 25 (`Unsupported class file major version 69`). On Windows setups where `JAVA_HOME` defaults to `C:\Program Files\Android\Android Studio\jbr` (OpenJDK 25), set `JAVA_HOME` in the build command to a JDK 17/21 installation first (for example `C:\Users\lkarh\.gradle\jdks\jetbrains_s_r_o_-21-amd64-windows.2` — check `$env:USERPROFILE\.gradle\jdks` if the hash/folder differs).
- **Windows terminal sandbox:**
  - `.\gradlew.bat` working-directory setup, Gradle daemon, and `%LOCALAPPDATA%\Android\Sdk` access fail inside Standard Sandbox Mode (`errno 5`). Attempt sandboxed if required by tool rules, then rerun the identical command (`toolSummary` unchanged) with `BypassSandbox: true`. Keep read-only `git` / Zip / asset checks sandboxed.

## 2. Build Commands

From PowerShell at the repository root (prefer `--offline` first if caches are warm; drop `--offline` if `:app:lintVitalAnalyzeRelease` needs uncached artifacts such as `com.android.tools.lint:lint-gradle:31.6.0`):

```powershell
$env:JAVA_HOME = 'C:\Users\lkarh\.gradle\jdks\jetbrains_s_r_o_-21-amd64-windows.2'
Push-Location engine/compilers/android-studio
.\gradlew.bat clean assembleDebug assembleRelease
Pop-Location
```

- **Stage assets only (fast check):** `.\gradlew.bat clean :app:copyGame`
- **Native compile check only:** `.\gradlew.bat :app:externalNativeBuildDebug`

### Expected Outputs

- Debug APK (debug-signed): `engine/compilers/android-studio/app/build/outputs/apk/debug/app-debug.apk`
- Release APK (unsigned unless a signing config is added): `engine/compilers/android-studio/app/build/outputs/apk/release/app-release-unsigned.apk`

## 3. Architecture & Guardrails (Do Not Break)

1. **Dual `main.cs` entrypoints:**
   - Leave repo-root `main.cs` untouched for desktop (`./editor/main.cs`).
   - Android boots `engine/compilers/android-studio/app/src/main/android_boot/main.cs`, which `:app:copyGame` stages into `app/src/main/game/main.cs` (`$Scripts::ignoreDSOs = true`, scans `./library` + `./Archon`, loads `AppCore`, defines `onExit()` and `androidBackButton(%val)`).
   - Never add a `main.cs` inside `app/src/main/assets/` — `sourceSets.main.assets.srcDirs` merges both `src/main/assets` and `src/main/game`.
2. **Asset staging (`:app:copyGame` uses `Sync`):**
   - `copyGame` uses explicit `include`s (`Archon/**`, `library/AppCore/**`, `library/Audio/**`, `library/themes/**`, `version.json`, `version.env`) and excludes desktop/dev clutter (`**/*.dso`, `**/*.bak`, `**/*.jbf`, `**/Thumbs.db`, `**/preferences.cs`, `Archon/**/1280/**`, `Archon/**/800/**`, `Archon/resolutionpatch_*.zip`) so `editor/`, `toybox/`, `PlanetX/`, `dist/`, `tests/`, `shots/`, `*.exe`, and `*.pdb` stay out of the APK.
   - `copyGame` pre-seeds `library/themes/Base.taml` -> `themes/Base.taml` and `library/AppCore/gui/images/cursors/*.png` -> `themes/cursors/Base/` because Android APK assets are read-only and `AppCore::loadThemes()` would otherwise try `pathCopy` / `TAMLWrite`.
   - Keep `Sync` (not `Copy`) so stale excluded files are automatically purged from `src/main/game/`.
3. **Preserve JNI package & namespace:**
   - Keep `namespace 'com.garagegames.torque2d'` (`app/build.gradle`) and the `com.garagegames.torque2d` Java package unchanged — `engine/source/platformAndroid/T2DActivity.cpp` hardcodes `"com.garagegames.torque2d.FontManager"` via JNI `loadClass`.
   - Note: Keep `.gitignore` root-anchored (`/Torque2D`, `/Torque2D_DEBUG`) so `.../com/garagegames/torque2d/*.java` remains tracked on case-insensitive filesystems.
4. **Android font fallback, display sizing, and preferences:**
   - `FontManager.java` caches `defaultFontPath` (`assets/fonts/Roboto-Regular.ttf`) and falls back across `Roboto`, `Noto Sans`, `Droid Sans`, and `defaultFontPath` so desktop font requests (`Arial`, `Lucida Console`) do not return `null`.
   - `Archon/main.cs` and `Archon/gameScripts/ResolutionManager.cs` guard `$platform $= "Android"` to preserve the physical device surface size/aspect ratio, skip `setScreenMode`, and read/write `getUserDataDirectory() @ "/preferences.cs"` (Android APK `Platform::isFile` returns `true` for any asset path containing `.`, and writes outside user data/cache fail).
5. **If modifying C++ engine code or shaders before building:**
   - Register any new engine files in `cmake/EngineSources.cmake` or `cmake/PlatformSources.cmake`.
   - Follow `.agents/rules/opengl-migration.md` (GLES `320 es` parity, no raw DSA, mandatory VAO, `glslang` validation).
6. **APK asset & script paths are case-sensitive:**
   - Android `AssetManager` opens paths inside APK ZIP entries with exact case (unlike Windows NTFS). Keep `exec(...)` paths and asset references exact-case (for example `./gameScripts/complexMouse.cs`, not `complexmouse.cs`). Also dismiss the Java `SplashScreen` dialog (`hideSplashScreen()` / non-focusable UI-thread dismissal in `SplashScreen.java`) so native input/loop is not frozen behind `assets/splash.png`.

## 4. Required Post-Build Verification

1. **Inspect both APKs directly (ZIP verification):**
   - Confirm `lib/arm64-v8a/libtorque2d.so`, `lib/arm64-v8a/libopenal.so`, `classes*.dex`, `assets/main.cs`, `assets/themes/Base.taml`, `assets/themes/cursors/Base/defaultCursor.png`, `assets/fonts/Roboto-Regular.ttf`, and `assets/Archon/module.taml` are present.
   - Confirm `assets/editor/`, `assets/toybox/`, `assets/PlanetX/`, `assets/dist/`, `*.exe`, `*.pdb`, `*.dso`, `*.jbf`, `Archon/preferences.cs`, and `Archon/resolutionpatch_*.zip` are absent.
2. **Validate `.cs` changes:** If you modified TorqueScript files (`Archon/**`, `library/**`, `android_boot/main.cs`), review them against `TORQUE_SCRIPT.md` (`checking-torquescript-conventions` skill).
3. **Report scope honestly:** Unless tested on an `arm64-v8a` device or emulator (and desktop tests if C++/shared scripts changed), state clearly that runtime behavior on Android hardware was not executed.
