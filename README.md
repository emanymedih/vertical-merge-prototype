# Vertical Merge Prototype

Minimal Unity 2D iOS prototype for a portrait physics merge game.

## Current Scope

- One scene: `Assets/Scenes/GameScene.unity`
- Runtime-generated circular balls and container visuals
- Planets / Cosmic Bodies theme naming documented in `Docs/ThemePlanets.md`
- Procedural cosmic body and arena visual direction documented in `Docs/VisualIdentityPlan.md`
- Classic arcade readability and manual run-test pass documented in `Docs/ClassicArcadeCorePass.md`
- Drag horizontally near the top and release to drop
- Same-level balls merge into the next level
- Score and best score
- Danger line game over after sustained overfill
- Restart button
- Simple merge pop, particles, camera shake, and iOS haptic placeholder

## Excluded

- Ads
- In-app purchases
- Levels
- Shop
- Skins
- Login
- Analytics
- Sound
- Main menu

## Next Development Plan

1. Validate the prototype in Unity Play Mode.
2. Tune physics, ball sizes, spawn levels, and danger-line timing.
3. Add focused automated or manual regression checks for merge, score, restart, and game over.
4. Improve mobile input feel and safe-area layout.
5. Prepare an iOS build target and test on device.
6. Add polish only after the core loop feels reliable.

## Future Ideas

Future mechanics are tracked in `Docs/DesignNotes.md`. They should stay out of the prototype until the base loop feels good.
