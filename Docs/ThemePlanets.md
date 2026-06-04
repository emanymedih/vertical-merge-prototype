# Planets Theme v1

## Product reason

The game theme is Planets / Cosmic Bodies.

This makes the merge chain easier to understand than abstract L1-L10 balls: the player drops cosmic bodies, merges identical ones, creates stronger objects, and tries to survive inside the gravity container.

## Level chain

| Level | Display Name | Short Name | Description |
| --- | --- | --- | --- |
| L1 | Asteroid | Asteroid | Small rocky body |
| L2 | Moon | Moon | Small satellite |
| L3 | Small Planet | Planet | Young planet |
| L4 | Blue Planet | Blue Planet | Water world |
| L5 | Gas Giant | Gas Giant | Massive gas planet |
| L6 | Star | Star | Burning star |
| L7 | Red Giant | Red Giant | Expanding star |
| L8 | Neutron Star | Neutron Star | Dense stellar core |
| L9 | Black Hole | Black Hole | Gravity well |
| L10 | Galaxy Core | Galaxy Core | Cosmic center |

## UI naming rules

- Runtime logic keeps using integer levels.
- Player-facing UI should use cosmic names for goals, discoveries, largest body, and best largest body.
- Short UI format uses the short name, for example `Goal: Create Red Giant`.
- Debug or internal logic may still use L1-L10.
- Levels above L10 should show a safe fallback such as `Cosmic Body L11`.

## Visual color direction

The stage 2 visual identity pass is tracked in `Docs/VisualIdentityPlan.md`.

- Asteroid: gray/brown.
- Moon: pale gray/blue.
- Small Planet: green/blue.
- Blue Planet: bright blue.
- Gas Giant: orange/yellow.
- Star: yellow/white.
- Red Giant: red/orange.
- Neutron Star: cyan/white.
- Black Hole: very dark purple/black with a readable glow.
- Galaxy Core: bright magenta/gold cosmic color.

## Out of scope

- New mechanics.
- New modes or levels.
- Shop, ads, monetization, daily rewards, streaks, accounts, or analytics.
- Collection screen or skin system.
- Full art pass, heavy particles, sound, or external assets.
- Changes to physics, scoring, spawn distribution, merge logic, danger line, or restart.
