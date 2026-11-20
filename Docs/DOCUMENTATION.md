# Chicken Invaders — Project Documentation

Complete overview of the Unity 2D coursework project as implemented: architecture, gameplay, scenes, menus, visuals, and tuning.

---

## 1. Project summary

**Chicken Invaders** is a vertical top-down shooter. The player flies a ship, shoots chickens and asteroids, collects weapon gifts, and uses bombs. There are **3 levels**, each made of ordered **rounds (waves)**. Clearing Level 3 shows a Victory screen; losing all lives shows Game Over with Retry.

| Item | Value |
|------|--------|
| Engine | Unity 6000.6.4f1 (Unity 6) |
| Pipeline | URP 2D |
| Input | New Input System |
| Style | 2D sprites + TMP HUD + OnGUI menus with visual-pack art |

---

## 2. Architecture

### 2.1 Design principles

- **Data-driven:** levels and waves live in ScriptableObjects (`LevelDataSO`, `WaveDataSO`, `WeaponDataSO`).
- **Runtime state separate from config:** `GameState` holds lives, score, bombs, weapon power, current level/round.
- **Singleton managers** with `DontDestroyOnLoad` for cross-scene flow.
- **Event bus (`GameEvents`)** so UI/VFX react without hard references to gameplay internals.
- **Object pooling** for bullets, eggs, enemies, asteroids, pickups.
- **Visual layer** loads art from `Resources/ChickenArt` and applies scale/sprites without changing core combat logic.

### 2.2 Core flow

```
MainMenu
   → LevelManager.LoadLevel / StartNewGame
      → Scene Level1|2|3
         → WaveManager plays LevelDataSO.waves in order
            → WaveSpawner (Chicken or Asteroid)
         → Round clear → next round
         → Last round clear → LevelCompleted
            → (Level 1–2) stage banner → next level scene
            → (Level 3) stage banner → Victory menu
         → Lives = 0 → Game Over menu (Retry / Main Menu)
```

### 2.3 Persistent systems (`GameBootstrap`)

Created automatically after scene load if missing:

| Object | Role |
|--------|------|
| `GameManager` | Owns `GameState`; raises events on changes |
| `LevelManager` | Scene transitions, new game, restart level |
| `PauseMenuController` | Esc pause in gameplay scenes |
| `GameOverMenuController` | Game Over UI + Retry |
| `VictoryMenuController` | Victory UI + Play Again |

Also resets `Time.timeScale = 1` so pause never leaves the game frozen (important with Disable Domain Reload).

### 2.4 Script folders

```
Assets/Scripts/
  Core/          GameManager, GameState, GameEvents, GameBootstrap
  Data/          LevelDataSO, WaveDataSO, WeaponDataSO
  Input/         PlayerInputReader
  Player/        PlayerMovement, PlayerHealth
  Weapons/       PlayerWeapon, Projectile, BombController, IDamageable
  Enemies/       EnemyController, EggProjectile, AsteroidController
  Waves/         WaveManager, WaveSpawner, FormationUtility
  Levels/        LevelManager, LevelSceneController, MainMenuController
  UI/            Pause, GameOver, Victory menus
  Pickups/       WeaponGiftPickup, WeaponGiftSpawner
  Pooling/       PoolManager, ObjectPool, IPoolable
  Visual/        Assets, HUD, sizes, parallax, VFX, animators
```

### 2.5 Key classes

| Class | Responsibility |
|-------|----------------|
| `GameState` | Serializable progress (level, round, lives, score, bombs, weapon power) |
| `GameManager` | Mutates state; publishes `GameEvents` |
| `GameEvents` | Static events: lives/score/bombs/weapon/round/level, death, round/level/game complete |
| `LevelManager` | Load levels, wait for stage banner, advance or raise victory |
| `WaveManager` | Runs ordered waves for the active `LevelDataSO` |
| `WaveSpawner` | Spawns chickens or asteroids; tracks pending + active until round clear |
| `EnemyController` | Entry path → formation → optional dive; egg drops; gift chance |
| `AsteroidController` | Falls down; damageable; leaves screen or dies |
| `PlayerWeapon` | Fires from current `WeaponDataSO` by power level 1–3 |
| `BombController` | Clears/damages enemies on X; costs one bomb |
| `PoolManager` | Get/return pooled prefabs |
| `VisualAssets` | `Resources.Load` helpers for ChickenArt sprites |
| `GameplayHudUI` | TMP HUD + designed round/stage banners |
| `ForcedVisualScale` / `VisualSizeSettings` | Consistent entity sizes |

### 2.6 Events (high level)

| Event | Typical listeners |
|-------|-------------------|
| `OnScoreChanged` / `OnLivesChanged` / `OnBombCountChanged` / … | HUD |
| `OnRoundCompleted` | Round-complete banner |
| `OnLevelCompleted` | Stage-complete banner + `LevelManager` transition |
| `OnGameCompleted` | `VictoryMenuController` |
| `OnPlayerDied` | `GameOverMenuController` |

---

## 3. Scenes and levels

### 3.1 Scenes

| Scene | Purpose |
|-------|---------|
| `MainMenu` | Title, New Game, Level 1–3 select |
| `Level1` | Earth Orbit |
| `Level2` | Asteroid Belt |
| `Level3` | Deep Space |
| `SampleScene` | Redirects to MainMenu via bootstrap |

Build Settings should include MainMenu + Level1–3.

### 3.2 Level data (`Assets/ScriptableObjects/Levels/`)

Each `LevelDataSO` has a wave list. Approximate content:

| Level | Theme | Wave pattern (summary) |
|------:|-------|-------------------------|
| 1 | Earth Orbit | Normal chickens → hard chickens → **asteroids** → chickens → boss placeholder |
| 2 | Asteroid Belt | Armored → hard armored → **asteroids** → armored → **hard asteroids** → hard armored → boss placeholder |
| 3 | Deep Space | Alien chickens / hard aliens, **asteroids**, **hard asteroids**, boss placeholder |

Wave assets live under `Assets/ScriptableObjects/Waves/`.

### 3.3 Wave types

- **Chicken** — formation spawn (Grid / V / Compact), eggs, optional dive.
- **Asteroid** — sequential falling rocks; round ends when all are destroyed or leave the screen.

Difficulty multipliers on a wave asset can scale health, speed, spawn interval, and count.

---

## 4. Play settings (defaults)

Defined mainly in `GameState` and wave assets.

### 4.1 Player start stats

| Stat | Default |
|------|--------:|
| Lives | 3 |
| Bombs | 2 |
| Weapon power | 1 (min 1, max 3) |
| Score | 0 |
| Start level (New Game) | 1 |
| Start round | 1 |

Change bombs/lives in `Assets/Scripts/Core/GameState.cs` (`DefaultBombs`, `DefaultLives`).  
`GameManager.InitializeNewGame()` resets state and forces `DefaultBombs` so stale scene Inspector values cannot override.

### 4.2 Controls

| Action | Binding |
|--------|---------|
| Move | WASD or Arrow keys |
| Fire | Space (hold to auto-fire) |
| Bomb | X |
| Pause / Resume | Esc |

Implemented in `PlayerInputReader` (Input Action Asset or runtime fallback bindings).

### 4.3 Combat rules (summary)

- **Lasers:** pooled projectiles; fire rate / pattern from `WeaponDataSO` for current power.
- **Weapon gifts:** chicken death can drop a gift; collect to raise power by 1 (cap 3).
- **Bomb:** spends 1 bomb; strong damage to chickens/eggs/asteroids; bosses take a % of max HP.
- **Damage to player:** contact with Enemy, Egg, or Asteroid → lose a life.
- **Game Over:** lives reach 0 → pause time → Game Over menu.
- **Retry:** same level, round 1, stats reset (`RestartCurrentLevel`).
- **Play Again (Victory):** new game from Level 1.
- **Stage complete:** designed banner from HUD, then next level (or Victory after Level 3).

### 4.4 Asteroid round tuning (current)

| Asset | Count | Speed | Spawn interval |
|-------|------:|------:|---------------:|
| `Wave_Asteroids` | 20 | 2.0 | 0.75 s |
| `Wave_Asteroids_Hard` | 24 (+ multipliers) | 2.3 | 0.60 s |

Longer than early prototypes so asteroid rounds take more play time to clear.

### 4.5 Visual sizes

Edit `Assets/Scripts/Visual/VisualSizeSettings.cs`:

| Entity | Scale |
|--------|------:|
| Player | 1.50 |
| Chicken | 1.00 |
| Egg | 0.65 |
| Gift | 0.65 |
| Laser | 0.55 |
| Asteroid | 0.60 |

Applied via `ForcedVisualScale` / prefab visual applier.

### 4.6 Movement

- Player move speed default: **8** (`PlayerMovement`).
- Position clamped to camera view with padding.

---

## 5. Menus and UI

### 5.1 Main Menu (`MainMenuController`)

Designed panel + title banner:

- **New Game** → Level 1, reset stats  
- **Level 1 / 2 / 3** → load that stage (no full reset unless New Game)

### 5.2 Pause (`PauseMenuController`)

Esc in Level1–3: Resume / Main Menu. Blocked while Game Over or Victory is open.

### 5.3 Game Over (`GameOverMenuController`)

Designed Game Over banner:

- **Retry** — same level, round 1, fresh stats  
- **Main Menu**

### 5.4 Victory (`VictoryMenuController`)

After Level 3 stage banner delay:

- **Play Again** — new game from Level 1  
- **Main Menu**

### 5.5 HUD (`GameplayHudUI`)

TMP: score, lives, bombs, level, round.  
Designed banners: round complete, **stage complete** (all levels including 3), then Victory menu on final clear.

Old plain `GUI.Box` “Level Completed!” text from `LevelManager` was removed so it does not overlap the designed banners.

---

## 6. Visual pack

Art is organized under:

- `Assets/Art/` — source sprites  
- `Assets/Resources/ChickenArt/` — runtime loads (`VisualAssets`)

Includes: player ship, lasers, chickens (normal / armored / alien), bosses, eggs, asteroids, pickups, backgrounds, UI buttons/banners, VFX frames.

Supporting systems:

- `LevelVisualBootstrap` / `ParallaxBackground` — per-level backgrounds locked to camera  
- `ChickenVisualController` / `PlayerVisualController` — animation / damaged look  
- `VfxSpawner`, `ScreenShake`, `SimpleSpriteAnimator`  
- Prefab applier hooks so pooled objects get correct sprites/scale

---

## 7. Pooling

`PoolManager` + `IPoolable`:

- Spawn via `Get(prefab)`, return when dead/despawned.
- Used for projectiles, eggs, enemies, asteroids, gifts.
- Pools reset on level transitions / retry to avoid leftovers.

---

## 8. How to change common things

| Goal | Where |
|------|--------|
| Start bombs / lives | `GameState.DefaultBombs` / `DefaultLives` |
| Asteroid length | `Wave_Asteroids` / `Wave_Asteroids_Hard` count & `spawnInterval` |
| Level wave order | `Level1.asset` / `Level2.asset` / `Level3.asset` wave lists |
| Weapon feel | `WeaponDataSO` assets (fire rate, projectile) |
| Entity size | `VisualSizeSettings` |
| Banner / button art | `Resources/ChickenArt/UI/` |
| Stage banner duration before next level | `LevelManager.levelCompleteDisplaySeconds` (default 2s) |

---

## 9. Implementation phases (course mapping)

| Phase | Focus | Status |
|------:|-------|--------|
| 1 | Architecture, SOs, GameState, events, pooling | Done |
| 2 | Player movement + New Input System | Done |
| 3 | Weapons, bombs, damage, pickups | Done |
| 4 | Waves, chickens, asteroids, round flow | Done |
| 5 | Scenes, menus, pause, game over, victory | Done |
| Extra | Visual pack, HUD polish, size tuning, longer asteroid rounds | Done |

---

## 10. Known editor tips

- Prefer **Stop Play completely** after changing defaults (DDOL + Disable Domain Reload can keep old runtime values).
- Scene-serialized `bombCount` on `GameManager` can lag behind code; `InitializeNewGame` forces `DefaultBombs`.
- Ensure level scenes are in **Build Settings** or level loads will fail.

---

## 11. File map (quick)

| Path | Contents |
|------|----------|
| `Assets/Scripts/` | All gameplay C# |
| `Assets/ScriptableObjects/` | Level / Wave / Weapon assets |
| `Assets/Scenes/` | MainMenu, Level1–3 |
| `Assets/Prefabs/` | Player, enemies, projectiles, pickups |
| `Assets/Resources/ChickenArt/` | Runtime visual pack |
| `Docs/DOCUMENTATION.md` | This document |
| `README.md` | Short GitHub overview |
