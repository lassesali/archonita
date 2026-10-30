# Archonita (2006 Legacy Branch)

## 📜 Overview
**Archonita** is a 2D turn-based tactical board game originally built in 2006, inspired by the 1983 classic *Archon: The Light and the Dark*. It was developed on the **Torque Game Builder (TGB)** engine (later known as Torque 2D). 

This branch preserves the original 2006 codebase, featuring its unique blend of tactical grid positioning, dynamic real-time dice combat, and a hybrid Finnish-English coding architecture.

---

## ⚙️ Engine & Platform Details
The game runs on the GarageGames TGB engine ecosystem from circa 2006:
* **Runtime:** `TGB.exe` (Native 32-bit Windows C++ executable).
* **Graphics:** Legacy OpenGL with Direct3D translation wrappers (`glu2d3d.dll`, `opengl2d3d.dll`).
* **Audio:** OpenAL hardware/software pipeline.
* **Scripting:** TorqueScript (`.cs`), executed just-in-time or via compiled bytecode (`.cs.dso`).

---

## 📂 Project Structure
The workspace follows TGB's standard modular package structure, with all core logic housed in the `Archon/` module:

* `main.cs` - Engine bootstrapper and CLI parser.
* `TGB.exe` - The main engine executable.
* **`Archon/`** - Core Game Module
  * `data/` - Contains all `audio` (SFX, ambient), `images` (spritesheets, UI), and `levels` (scene graph `.t2d` files like `board.t2d` and `mainmenu.t2d`).
  * `gui/` - Torque canvas scripts defining the viewport.
  * `managed/` - Datablocks for sprite animations, image maps, and audio profiles.
  * `gameScripts/` - The heart of the game logic.
    * `exec.cs` - The monolithic rules engine managing the board, AI, and combat loop.
    * `complexMouse.cs` - Ray-cast picking and drag-and-drop mechanics.
    * `units.cs`, `hero.cs`, `nappula.cs` - Entity definitions and sprite callbacks.

---

## 🧠 Core Game Systems

### 1. Dual State Machine Architecture
Archonita is driven by two distinct state machine layers initialized in `exec.cs`:

**Global Game State (`$game...State`)**
Manages the macro-flow of the application, transitioning smoothly from `$gameIntroState` to `$gamePreBattleState` (deployment phase), and finally into the `$gameBattleState` (active tactical combat) and `$gameEndState`.

**Unit Animation State (`$olento...State`)**
Every unit sprite (*"nappula"*) relies on an internal visual state machine:
* `$olentoStandState` (Idle)
* `$olentoWalkState` (Movement interpolated across the board)
* `$olentoAttackState` (Weapon swings/projectile cast)
* `$olentoBeenHitState` (Flinch reaction)
* `$olentoDieState` (Tipping over upon reaching 0 HP)

### 2. Tactical Board & Data Models
The battlefield is defined as a strict $4 \times 4$ coordinate plane where $X \in [0..3]$ and $Y \in [0..3]$. 
* **`$board.nappulat[X, Y]`**: Tracks the active unit ID on a specific tile.
* **`$board.obstacles[X, Y]`**: Collision matrix defining passable floors ($0$), environmental rocks ($1$-$3$), or occupied tiles ($255$).

Players draft from distinct archetypes, grouped by faction:
* **Player (Group 0):** Warrior, Archer, Cleric, Paladin.
* **AI (Group 1):** Skeleton Warrior, Skeleton Archer, Dread Zombie, Ancient Ghost.

### 3. Combat & Turn Scheduling
Turns are managed via a dynamic, interleaved queue (`jaaNappuloidenVuorot`). The engine pools all living units from both sides and interleaves them (e.g., `P1 AI1 P2 AI2`), ensuring an alternating flow of initiative. 

**Dice-Roll Resolution:**
When an attack is initiated, the game executes a visual dice simulation (`rollDicesRedAttacks`, `rollDicesRedDefends`):
* Attackers spawn red dice equal to their `attack` stat; defenders spawn blue dice equal to their `defence`.
* Dice rotate dynamically on-screen. Hits are scored on rolls of $0$ or $2$, while blocks are scored on $3$. 
* If Attack Successes > Defense Blocks, the target takes damage, triggering a blood particle effect and the `$olentoBeenHitState`.

### 4. Artificial Intelligence
The AI evaluates its turn via a 3-tier hierarchical script (`katsoMihinRuutuunVoiLiikkua`):
1. **Opportunistic Attack:** Immediately strikes any enemy unit within melee or ranged line-of-sight.
2. **Threat Assessment:** If no target is present, it constructs a dynamic danger matrix (`$board.vaara[X, Y]`), scoring tiles based on their proximity to player ranged units.
3. **Evasive Move:** Selects the cardinal direction offering the lowest danger rating and executes a movement to advance or retreat.

---

## ⚠️ Legacy Code Quirks
As a preserved 2006 codebase built during a short game jam, the architecture contains several historical quirks:
* **The God-Script:** The `exec.cs` file is a monolithic ~2,000 line script that handles board creation, AI, turn scheduling, and dice math all at once.
* **Bilingual Nomenclature:** The codebase heavily utilizes a mix of English and Finnish variables (e.g., `nappula` for piece, `sankari` for hero, `vaara` for danger, mixed with `Attack`, `Defence`, and `Sprite`). 
* **Global State:** Extensive use of global TorqueScript variables (`$board`, `$nappulaSprite`, `$noppa`) drives the game logic. 
