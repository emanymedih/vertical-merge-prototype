# Readability + Result Screen Polish Pass v1

## Goal

Make the game easier to read during play and make the end of a run feel like a result,
not a dry failure screen. The player should quickly understand the current goal, next
ball, danger state, pressure floor, and what they achieved after Game Over.

## What Changed

- Game Over presentation is now framed as `Run Complete`.
- Result details sit inside a translucent sci-fi result card.
- The field behind the result card is dimmed but still visible as run context.
- Largest object preview is shown as a non-physical trophy inside the result card.
- Result message priority is: new best score, new largest record, near miss, close run, retry.
- Play Again is larger, closer to the result, and gently pulses after the screen appears.
- Goal HUD is a compact glass panel with two-line `NEXT GOAL` copy.
- Next Ball HUD has its own compact panel and readable preview.
- Pressure floor visuals are softer and more transparent, with a light energy-wave edge.
- Danger line hides warning text in safe state and only shows readable warning text under pressure.

## Why Run Complete

The end of a run should reinforce progress:

- `You created: Black Hole`
- `Almost reached Galaxy Core`
- `New Best Score`

The player still understands the run ended, but the emotional focus is achievement and
revenge, not punishment.

## Result Card

The result card is a dark translucent navy/black panel with subtle cyan edge glow. It
keeps the piled objects visible behind a dim overlay while giving all result text a
stable readable surface.

Composition:

1. `Run Complete`
2. Largest object trophy preview
3. `You created: {ObjectName}`
4. Score / Best / Largest / Best Largest
5. Motivation or record message
6. `Play Again`

## HUD Readability

- Goal is smaller and cleaner so it does not dominate the field.
- Next Ball is visually separated from the background and from Goal.
- Score, Best, and Largest remain compact.
- No gameplay data or rules changed.

## Danger And Pressure Visuals

- Danger line safe state is intentionally quiet.
- Warning and critical states become brighter and show text only when relevant.
- Pressure floor is now more like a gravitational field than a solid debug block.
- Pressure speed, delay, relief, and Game Over logic are unchanged.

## Out Of Scope

- No new mechanics.
- No physics changes.
- No scoring changes.
- No spawn distribution changes.
- No merge rule changes.
- No pressure floor logic changes.
- No danger timer or Game Over logic changes.
- No shop, ads, revive, currency, levels, missions, skins, leaderboard, accounts, analytics, or external assets.

## Manual Checklist

1. Play until Game Over.
2. Confirm the result screen is readable within 2 seconds.
3. Confirm the largest object preview is visible.
4. Confirm `You created: {ObjectName}` is the main emotional message.
5. Confirm `Play Again` is the obvious next action.
6. Press `Play Again` and confirm a new run starts immediately.
7. Confirm Goal is readable during play.
8. Confirm Next Ball is readable during play.
9. Confirm Goal and Next Ball do not overlap.
10. Confirm pressure floor does not look like a solid debug rectangle.
11. Confirm danger text does not hang around in safe state.
12. Confirm Danger Line still warns before Game Over.
13. Confirm Pressure Floor still rises and relief still works after merges.
14. Confirm portrait layout does not clip text or button placement.
15. Check Console for new errors.
