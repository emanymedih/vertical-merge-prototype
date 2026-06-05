# Atmospheric Shader Pass

## Goal

Make the first glance feel more atmospheric without adding external assets, URP, DOTween, or heavy VFX.

## What Changed

- Added lightweight ShaderLab shaders for procedural nebula haze, soft additive glow, and animated energy beams.
- Applied nebula overlay to the arena background, including the wallpaper-backed chamber.
- Applied energy/glow materials to chamber side boundaries and gravity platform highlights.
- Applied soft glow materials to cosmic body rims, glows, and rings.
- Added a grade-scaled aura layer: L1-L2 stay subtle, L3+ get visible pulsing glow, and high-grade bodies gain stronger atmospheric glow without changing physics size.
- Applied the energy beam material to anomaly target beams.

## Constraints

- Built for the current built-in 2D runtime-generated scene.
- No physics, scoring, spawn distribution, merge rules, pressure logic, or restart logic changes.
- Shaders are optional at runtime: if `Shader.Find` fails, renderers keep their normal sprite material fallback.

## Manual Check

1. Open Play Mode and confirm the arena has visible atmospheric depth.
2. Confirm balls remain readable and merge targets are not hidden by glow.
3. Confirm side boundaries/platform feel energized but do not overpower the field.
4. Trigger anomaly with `Ctrl+Shift+A` and verify beam remains visible.
5. Check Console for shader or C# errors.
