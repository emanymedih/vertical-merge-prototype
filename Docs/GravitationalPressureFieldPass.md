# Gravitational Pressure Field Pass v1

## Goal

Keep the existing rising pressure mechanic, but remove the visual reading of "water"
or liquid inside the cosmic arena. The field should feel like gravitational compression
closing the playable space from below.

## Changed

- `PressureFloor` visuals now use a gravitational pressure field theme.
- The lower area is a dark translucent gravity field, not a solid blue block.
- The top edge is a glowing compression wave front.
- Additional subtle compression ripples sit inside the field.
- Colors move from water-blue toward purple/cyan gravity, then warmer warning colors
  as the field approaches danger.

## Not Changed

- No pressure timing changes.
- No pressure rise speed changes.
- No merge relief changes.
- No danger line game over changes.
- No collider or physics behavior changes.
- No new assets.

## Manual Checks

1. Start Play Mode and wait for the pressure field to rise.
2. Confirm it reads as cosmic/gravitational pressure, not water.
3. Confirm balls remain readable behind/above the field.
4. Confirm the top compression wave is visible but not noisy.
5. Confirm the warning color intensifies as it approaches danger.
6. Confirm merges still reduce pressure as before.
7. Confirm Game Over timing is unchanged.
