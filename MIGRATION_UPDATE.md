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
  - Transform and kinematics mapping: `moveto`, `setAngularVelocity`, hierarchical `mount` / `dismount` with recursive transform propagation (`updateMountPosition`, `updateMountedChildren`) to support multi-object attachments like unit icons with decorative golden frames.
  - Vector math parity helpers: `t2dVectorDistance`, `t2dVectorCompare`.
  - Smooth animation controller hooks (`playAnimation`, `setFrame`, `setImageMap`).
  - Timed visual fade controllers (`startFadeIn`, `startFadeOut`).

### 5. OpenAL Audio Engine & Fully Qualified Asset IDs
- **Audio Profile Modernization:** Replaced obsolete `AudioProfile` / `AudioDescription` script blocks with standalone `<AudioAsset>` definitions in `Archon/data/audio/`.
- **SFX Routing Pipeline (`playArchonSound`):**
  - Updated audio playback across all gameplay scripts to route through fully qualified module asset IDs (`"Archon:<AssetName>"`), resolving silent failures in legacy `alxPlay()` calls.
  - Restored combat impact sounds (`fxBeenHit`), attack swings (`fxSword`), creature movement footsteps (`fxFootStep`), death sequences (`fxDie`), countdown ticks (`ambient4Audio`), and looping background music (`ambient1Audio`).

### 6. Framebuffer Multiply Blending for Intro Logo
- **Multiplicative Blending:** Configured the intro screen's dynamic gold-scrolling logo to use standard multiplicative blending:
  - **Pass 1:** Renders the logo (`gamelonlogo.png`) on `SceneLayer 1`.
  - **Pass 2:** Overlays the animated gold texture scroller (`jee1.png`) on `SceneLayer 0` configured with `BlendMode="1"`, `SrcBlendFactor="DST_COLOR"`, and `DstBlendFactor="ZERO"` (Multiply mode).
  - Produces crisp masking where the wavy gold texture modulates exclusively through the logo shape and typography against a void-black background.

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
- **Unit Selection Golden Frame & Mouse Alignment:**
  - Restored the golden border frame (`$grid[%i]`, `gridsImageMap` frame 0) around the dragged creature icon by propagating transform updates down the mount hierarchy to front layer 0.
  - Adjusted `$mouseObj::mount::offset` from `"7 7"` to `"3.5 -3.5"`, correcting for Torque2D 4.0's Box2D Y-up coordinate system and ensuring the dragged creature icon is closely aligned with the mouse cursor coordinates.
  - Synchronized `$mouseObj::object` position and updated mounted children in real-time across `onMouseMove` and `onMouseDragged` events.
  - Enhanced `cancelCharacterPlacement` fallback to snap both the dragged unit icon and its golden frame cleanly back to the origin tray slot (`$gridback[%obj.index]`) at layers 6 and 5.
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

## 🛠️ Subsystem Architecture & Modular Refactoring

### 1. Monolithic Script Decomposition (`exec.cs`)
- Decomposed the legacy 2,000+ line monolithic `exec.cs` god-script into four dedicated, single-responsibility `ScriptObject` subsystems adhering to [TORQUE_SCRIPT.md](file:///M:/Archonita/games/TORQUE_SCRIPT.md):
  - **`BoardManager.cs`**: Handles board geometry calculations, obstacle generation, coordinate translations (`boardToWorld`, `boardToWorldUnit`), legal placement validation, and creature tray lifecycle management.
  - **`TurnManager.cs`**: Manages the core game state machine (`$gameIntroState`, `$gameMainMenuState`, `$gamePreBattleState`, `$gameBattleState`, `$gameCardSelectState`), turn progression, round countdown timers, and phase transitions.
  - **`CombatEngine.cs`**: Encapsulates dynamic combat dice rolls, hero damage calculations, spell/ability dispatch, creature hit reactions, and death sequence resolution.
  - **`TacticalAI.cs`**: Implements AI opponent decision trees, strategic evaluation of board tiles, candidate movement sorting, offensive attacks, and defensive reaction mechanics.
- `exec.cs` has been repurposed as a clean, lightweight module loader orchestrating subsystem execution.

---

## 🎨 Dynamic High-Resolution Asset Scaling

### 1. Automated 512px High-Resolution Adaptation
- Implemented **`AssetResolutionManager.cs`** to provide dynamic, backwards-compatible support for high-resolution 512px assets (`heroicons.png`, `icons.png`, `credits.png`):
  - Automatically queries image dimensions at runtime via `AssetDatabase`.
  - Dynamically recalculates cell geometry (`CellWidth`, `CellHeight`, `CellCountX`, `CellCountY`) for upscaled asset sheets (e.g. adapting `heroiconsImageMap` from 128x128 to 512x512).
  - Preserves gameplay coordinate transformations and mount offsets while rendering razor-sharp icons on high-DPI and modern 1080p/4K displays.

---

## 🛡️ Input Lifecycle & Crash-To-Desktop (CTD) Prevention

### 1. Elimination of Case-Insensitive Infinite Self-Recursion
- Resolved critical crash-to-desktop (`0xC0000005` Access Violation / `0xC00000FD` Stack Overflow) caused by case-insensitive function lookup in TorqueScript:
  - Removed self-recursive alias definitions where lowercase wrappers overwrote PascalCase functions (`mainmenufadeout` calling `mainMenuFadeOut`, `cardscreenfadeout` calling `cardScreenFadeOut`, `SelectHeroScreen` calling `selectHeroScreen`, `creditsfadeout` calling `creditsFadeOut`).
  - Corrected circular fade forwarding in `t2d_compat.cs` and added `isObject()` guards across all fade operations in `fading.cs`.

### 2. Robust Intro Skipping & Mouse Release Safety
- Added `skipIntro()` lifecycle handler to [`Archon/gameScripts/intro.cs`](file:///M:/Archonita/games/Archon/gameScripts/intro.cs):
  - Cancels pending fade timers (`$introFadeSchedule`) and menu transition timers (`$introMenuSchedule`).
  - Immediately halts ambient audio (`$introMusic`) and transitions smoothly to `mainmenuScreen()`.
- Added a 350 ms debounce guard in `sceneWindow2D::onMouseDown` to prevent rapid double-clicks on the intro screen from prematurely triggering main menu options.
- Guarded `sceneWindow2D::onMouseUp` in [`Archon/gameScripts/complexMouse.cs`](file:///M:/Archonita/games/Archon/gameScripts/complexMouse.cs):
  - Prevented execution of uninstantiated hero/tile queries during intro and non-battle states.
  - Protected all screen transitions against re-entrant click spam (`$mainMenuTransitioning`, `$cardSelectTransitioning`).

### 3. Pre-Battle Unit Placement & Input Synchronization
- **Unassigned Variable Resolution in Placement Logic:**
  - Resolved bug in `sceneWindow2D::placeSelectedCharacter` ([`Archon/gameScripts/complexMouse.cs`](file:///M:/Archonita/games/Archon/gameScripts/complexMouse.cs)) where `%targetRuutu` attempted to call `getValittuRuutu()` on an unassigned `%sankari` variable rather than `%hero` (`HeroGroup.getObject(0)`), causing target tile validation to fail unconditionally and snapping selected units back into the recruitment tray.
- **Dynamic Board Geometry Bounds Checking:**
  - Replaced hardcoded dimensions (`0 <= x < 4`, `2 <= y < 4`) in `placeSelectedCharacter` and `cancelCharacterPlacement` with dynamic parameters from `BoardConfig` (`width`, `playerPlacementMinY`, `playerPlacementMaxY`).
- **Input Pipeline Synchronization:**
  - Ensured `mouseOverGreenGrid(%mod, %worldPos)` is evaluated immediately upon mouse release (`onMouseUp`) and secondary click (`onMouseDown`) before invoking `placeSelectedCharacter`, allowing seamless unit placement via both drag-and-drop and click-to-pick workflows.
  - Verified full placement lifecycle: creature points deduction, recruitment tray reflow, obstacle map reservation (`$board.obstacles[x, y] = 255`), piece map registration (`$board.pieces[x, y]`), and unit sprite instantiation with correct layer and directional animation (`warriorStoppedAnimation_n`).

---

## 🌐 Nomenclature Modernization & Bilingual Standardization

### 1. Pure English Canonical API & Backward-Compatible Wrappers
- Standardized core gameplay functions and class methods to pure English adhering to standard C# / TorqueScript naming conventions (camelCase functions/methods, PascalCase classes):
  - **`BoardManager.cs`**:
    - Added canonical pure English stubs: `addNewPieceToBoard()`, `selectedPiece()`, `updatePointsGridsIcons()`, `printDebug()`.
    - Maintained backward-compatible aliases: `createSetupNappulat()`, `deleteSetupNappulat()`, `updateOlentoikonit()`, `updateOlentopisteet()`, `updatePisteetGridsIcons()`, `lisaaUusiNappulaLaudalle()`, `selectedNappula()`, `luoAttackGrid()`, `luoLiikuGrid()`, `piilotaGrids()`, `debuggi()`.
  - **`complexMouse.cs`**:
    - Replaced `setNappiReleasedOff()` with `setButtonReleasedOff()` while keeping backward-compatible wrapper.
    - Updated active piece selection query from `game.getNappulaSelected()` to `game.getSelectedPiece()`.
  - **`createhero.cs`**:
    - Standardized `checkSelectedCard()` as canonical method with `tarkastavalittukortti()` wrapper.
  - **`mainmenu.cs`**:
    - Standardized `stopBubbleEffect()` as canonical method with `lopetasaippuakuplat()` wrapper.
  - **`gameObject.cs`**:
    - Verified canonical English state methods: `setSelectedPiece()`, `getSelectedPiece()`, `setActivePlayer()`, `getActivePlayer()`, `setActiveOpponent()`, `getActiveOpponent()`, `setTurn()`, `getTurn()`, `addTurn()`, `setTurnQueue()`, `getTurnQueue()`, `setGameTimer()`, `getGameTimer()`, `addGameTimer()`, `subGameTimer()`, `setPieceCount()`, `getPieceCount()`, `addPieceCount()`.
  - **`hero.cs`**:
    - Verified canonical English hero methods: `addCreaturePoints()`, `subCreaturePoints()`, `getCreaturePoints()`, `addPieceCount()`, `subPieceCount()`, `getPieceCount()`, `setSelectedTile()`, `getSelectedTile()`, `setSelectedTileX()`, `getSelectedTileX()`, `setSelectedTileY()`, `getSelectedTileY()`, `setHeroClass()`, `getHeroClass()`.

### 2. Class Method Bindings Modernization (`Piece` & `Card`)
- Migrated legacy `Nappula` and `Kortti` class bindings in [`Archon/gameScripts/nappula.cs`](file:///M:/Archonita/games/Archon/gameScripts/nappula.cs) to modern `Piece` and `Card` classes:
  - Canonical methods: `Piece::onPositionTarget`, `Piece::onMoveToComplete`, `Piece::onAnimationEnd`.
  - Canonical methods: `Card::onMoveToComplete`, `Card::onPositionTarget`.
  - Configured spawned sprites in `BoardManager.cs` and `TurnManager.cs` to instantiate `class = Piece; superclass = Nappula;` and `class = Card; superclass = Kortti;`, ensuring zero regressions for legacy event dispatchers.

### 3. Decorative Dice Idle Rotation Animation
- Resolved missing idle rotation animation on the decorative red and blue dice in the top right corner during the creature placement phase (`$gamePreBattleState`):
  - **[`board.scene.taml`](file:///M:/Archonita/games/Archon/data/levels/board.scene.taml)**: Configured `AngularVelocity="10"` and `Angle="340"` on `diceBlue` for continuous anti-clockwise rotation; configured `AngularVelocity="-10"` on `diceRed` for continuous clockwise rotation.
  - **[`board.t2d`](file:///M:/Archonita/games/Archon/data/levels/board.t2d)**: Synchronized `AngularVelocity`, `Angle`, and `AutoRotation` fields on named `diceBlue` and `diceRed` sprite definitions.
  - **[`BoardManager.cs`](file:///M:/Archonita/games/Archon/gameScripts/BoardManager.cs)**: Added explicit angular velocity initialization in `BoardManager::showBoardScreen()` to guarantee active rotation upon entering the board.
  - **[`t2d_compat.cs`](file:///M:/Archonita/games/Archon/gameScripts/t2d_compat.cs)**: Added `setAutoRotation(%val)` compatibility mapping on `SceneObject` and `Sprite` translating legacy TGB degrees-per-second auto-rotation into modern Torque2D angular velocity (`setAngularVelocity(-%val)`).

### 4. Ornate 3D Title Logo & Fullscreen Main Menu Background
- **Title Logo Replacement**: Replaced legacy flat blue "ARCHONITA" text at the top of the title screen with the ornate, metallic 3D "ARCHONITA" logo:
  - Added [`archonita_title_new.png`](file:///M:/Archonita/games/Archon/data/images/archonita_title_new.png) and registered [`archonita_title_newImageMap.asset.taml`](file:///M:/Archonita/games/Archon/data/images/archonita_title_newImageMap.asset.taml).
  - Cleared Cell 1 from [`texts.png`](file:///M:/Archonita/games/Archon/data/images/texts.png) to eliminate redundant legacy text rasterization.
  - Positioned and scaled the 3D logo in [`mainmenu.scene.taml`](file:///M:/Archonita/games/Archon/data/levels/mainmenu.scene.taml) and [`mainmenu.t2d`](file:///M:/Archonita/games/Archon/data/levels/mainmenu.t2d) (`Position="0 25.5"`, `Size="75 18.75"`), blending shadows naturally into the dark forest canopy.
- **Fullscreen Background Expansion**:
  - Reconfigured [`startpic.png`](file:///M:/Archonita/games/Archon/data/images/startpic.png) sprite (`kuva`) in [`mainmenu.scene.taml`](file:///M:/Archonita/games/Archon/data/levels/mainmenu.scene.taml) and [`mainmenu.t2d`](file:///M:/Archonita/games/Archon/data/levels/mainmenu.t2d) from `100 57.8` to `Size="100 75"`.
  - Background now completely fills the 4:3 screen viewport without letterboxing or black bars at the top and bottom.

### 5. Seamless Background Music & Indefinite Looping
- Fixed background music cutting out when transitioning from the intro logo screen into the main menu and board game screen:
  - Created [`AudioManager`](file:///M:/Archonita/games/Archon/gameScripts/AudioManager.cs) `ScriptObject` with managed lifecycle in [`Archon/main.cs`](file:///M:/Archonita/games/Archon/main.cs).
  - Implemented `startBackgroundMusic()`, `isMusicPlaying()`, and watchdog looping schedule `checkBackgroundMusicLoop()`.
  - Removed premature `alxStop` calls in [`intro.cs`](file:///M:/Archonita/games/Archon/gameScripts/intro.cs) and [`mainmenu.cs`](file:///M:/Archonita/games/Archon/gameScripts/mainmenu.cs), maintaining uninterrupted playback through intro, main menu, hero selection, and board gameplay.
  - Linked `stopBackgroundMusic()` to clean game shutdown in `endGame()`.

### 6. Main Menu Version Watermark UI
- Added dynamic version watermark anchored to the viewport bottom-right corner:
  - Created external configuration files [`version.json`](file:///M:/Archonita/games/version.json) and [`version.env`](file:///M:/Archonita/games/version.env) storing channel (`Early Access`), build number (`101`), and version string (`2.0.1-ea4`).
  - Implemented [`VersionConfig`](file:///M:/Archonita/games/Archon/gameScripts/versionConfig.cs) `ScriptObject` to read and parse external configuration via `FileObject` into `$App::Channel`, `$App::Build`, and `$App::Version`.
  - Configured `MainMenuWatermark` container with `HorizSizing="left"` and `VertSizing="top"` on `mainScreenGui`.
  - Implemented crisp pure white typography (`fontColor="255 255 255 255"`, `fontType="Arial"`) with paired 1px drop shadows for contrast against artwork.
  - Corrected GUI z-ordering with `mainScreenGui.pushToBack(MainMenuWatermark)` so text renders on top of `sceneWindow2D`.

### 7. TorqueScript Source Code Bytecode Protection & Release Automation
- Enhanced [`make_dist.ps1`](file:///M:/Archonita/games/make_dist.ps1) distribution automation to protect proprietary source code:
  - Generates a temporary compilation script setting `$pref::saveDSO = true;` with a recursive `findFirstFile("*.cs")` loop invoking `compile(%file)` for all scripts.
  - Executes `Torque2D.exe` in the target directory to compile all project scripts into `.dso` bytecode files.
  - Creates release bootstrapper `main.dso` and updates [`Play Archonita.bat`](file:///M:/Archonita/games/dist/Play%20Archonita.bat) to launch `Archonita.exe main.dso`.
  - Configures release runtime with `$Scripts::ignoreDSOs = false;` to execute compiled DSO bytecode.
  - Explicitly purges all raw `.cs` files across the distribution folder (`Get-ChildItem -Path $DestinationDir -Filter *.cs -Recurse | Remove-Item -Force`), leaving 0 raw `.cs` files in release.

### 8. UI Hexagon Sprites Anchoring & Action Selection State
- Resolved critical input bug where clicking any of the three tactical action hexagon sprites (Swords, Shield, Star) permanently anchored the sprite to the mouse cursor:
  - **Draggable Exclusion:** In [`Archon/gameScripts/complexMouse.cs`](file:///M:/Archonita/games/Archon/gameScripts/complexMouse.cs), intercepted clicks on action icons (`isIconDefence`, `isIconMagic`, `isIconAttack`) in `sceneWindow2D::onMouseDown`, explicitly excluding them from placement mounting (`!isActionIcon(%obj)`) and preventing them from mounting to `$mouseObj::object`.
  - **Action Selection & State Toggling:** Added `setSelectedAction` and `getSelectedAction` to the [`game`](file:///M:/Archonita/games/Archon/gameScripts/gameObject.cs) singleton object. Implemented `onActionIconClick` in `complexMouse.cs` to register selection and toggling of active tactical actions (1 = Defence, 2 = Magic, 3 = Attack), executing immediate defence resolution during battle (`doAction()`) and providing audio click feedback.
  - **Static Coordinate Anchor Enforcement:** Created `resetActionIconsAnchor()` and `updateActionIconsVisualState()` in `complexMouse.cs`, guaranteeing the hexagon sprites remain strictly fixed at their static UI coordinates (`iconDefence2` at `28.3 -8.1`, `iconMagic3` at `40 -15.1`, and `iconAttack2` at `40 -1.35`). Moving the cursor across the screen maintains their anchored positions without tracking the cursor.
  - **Dynamic Highlight & Hover Feedback:** Highlighted active or hovered action hexagons (`18x18`, alpha `1.0`, layer `30`) while keeping unselected hexagons in resting state (`16x16`, alpha `0.7`, layer `31`).
  - **Scene Metadata Modernization:** Updated `isSelectable` to `"0"` / `"False"` on `iconDefence2`, `iconMagic3`, and `iconAttack2` in both [`Archon/data/levels/board.scene.taml`](file:///M:/Archonita/games/Archon/data/levels/board.scene.taml) and [`Archon/data/levels/board.t2d`](file:///M:/Archonita/games/Archon/data/levels/board.t2d).

---

> [!NOTE]
> All original Finnish domain functions, gameplay rules, and board behaviors from the 2006 release have been preserved in full conformity with [TORQUE_SCRIPT.md](file:///M:/Archonita/games/TORQUE_SCRIPT.md).
