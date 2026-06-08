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
- Merge relief is 15% stronger than the previous mechanical value through a tunable
  `mergeReliefMultiplier`.
- Relief now animates downward smoothly and adds a short recovery glow.
- L4+ merge relief can show `Space Recovered`, with cooldown to avoid message spam.

## Not Changed

- No pressure timing changes.
- No pressure rise speed changes.
- No danger line game over changes.
- No collider or physics behavior changes.
- No new assets.

## Manual Checks

1. Start Play Mode and wait for the pressure field to rise.
2. Confirm it reads as cosmic/gravitational pressure, not water.
3. Confirm balls remain readable behind/above the field.
4. Confirm the top compression wave is visible but not noisy.
5. Confirm the warning color intensifies as it approaches danger.
6. Confirm merges reduce pressure a little more clearly than before.
7. Confirm Game Over timing is unchanged.
8. Confirm repeated merges cannot move the pressure floor below its starting limit.
