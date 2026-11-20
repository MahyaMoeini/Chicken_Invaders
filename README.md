# Chicken Invaders

A vertical 2D chicken shooter inspired by *Chicken Invaders*, built in **Unity 6** (URP 2D) with C#.

## Requirements

- Unity **6000.6.4f1** (or compatible Unity 6)
- Universal Render Pipeline (2D)
- New Input System

## Play

1. Open the project in Unity Hub.
2. Add scenes to Build Settings: `MainMenu`, `Level1`, `Level2`, `Level3` (or use **Chicken Invaders → Fix Build Settings Scenes** if available).
3. Open `Assets/Scenes/MainMenu.unity` and press Play.
4. **New Game** starts Level 1; level buttons jump to a stage.

### Controls

| Action | Keys |
|--------|------|
| Move | WASD / Arrow keys |
| Fire | Space (hold) |
| Bomb | X |
| Pause | Esc |

## What's included

- 3 levels with chicken waves, asteroid rounds, and boss placeholders
- Weapon power levels 1–3, bombs, lives, score
- Object pooling, ScriptableObject wave/level data, event-driven HUD
- Visual pack (sprites, HUD banners, menus, VFX, parallax)

## Documentation

See **[Docs/DOCUMENTATION.md](Docs/DOCUMENTATION.md)** for architecture, gameplay settings, scenes, menus, and how systems connect.

