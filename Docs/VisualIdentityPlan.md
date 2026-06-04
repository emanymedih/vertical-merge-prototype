# Visual Identity Plan v1

## Stage 2: Cosmic bodies

Each body keeps the same physics level and collider, but gains simple procedural child visuals:

| Level | Body | Visual identity |
| --- | --- | --- |
| L1 | Asteroid | Rocky brown body with dark crater spots |
| L2 | Moon | Pale moon body with cleaner crater marks |
| L3 | Small Planet | Green planet with blue land/water spots |
| L4 | Blue Planet | Bright blue body with light atmosphere glow and surface spots |
| L5 | Gas Giant | Orange body with horizontal atmospheric bands |
| L6 | Star | Yellow body with glow and bright inner core |
| L7 | Red Giant | Red/orange body with stronger glow and warm core |
| L8 | Neutron Star | Cyan-white compact body with cold glow, ring, and core |
| L9 | Black Hole | Dark body with visible purple glow and accretion ring |
| L10 | Galaxy Core | Bright magenta/gold body with glow, ring, and inner core |

All detail layers are visual-only child objects with no colliders.

## Stage 3: Cosmic arena

The container is styled as a gravitational chamber:

- dark space background with subtle procedural stars;
- darker physical wall renderers;
- cyan inner energy boundaries;
- glowing gravity base;
- subtle chamber shadow;
- danger line styled as a critical energy line;
- drop guide styled as a faint cyan energy beam.

The existing wall, bottom, danger, merge, score, and restart logic stay unchanged.

## Out of scope

- New mechanics, levels, modes, shop, ads, rewards, skins, missions, accounts, analytics, sound, or haptics.
- External art assets or heavy VFX.
- Physics tuning, scoring changes, spawn distribution changes, merge rule changes, danger timing changes, or restart changes.
