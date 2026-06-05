# Early Pacing + Black Hole Function Pass v1

## Goal

Make the first minute feel less slow and make `Black Hole` behave like a real cosmic
milestone instead of only being a large object. The player should reach meaningful
planet stages faster and feel that creating a Black Hole changes the physical field.

## Changed

- Early spawn pacing is more generous during the first 60-90 seconds.
- The first fresh-player session gets a stronger boost for the first 12 controllable
  spawn requests.
- Direct spawn still never goes above L4.
- L1-L4 naming is clearer:
  - L1 `Meteor`
  - L2 `Moon`
  - L3 `Verdant Planet`
  - L4 `Ocean Planet`
- L3/L4 visuals now have stronger procedural planet details.
- L9 `Black Hole` now has a local gravity field.
- L9 visuals have a stronger accretion/aura treatment.
- Follow-up release pass adds a timed Event Horizon absorption cycle; see
  `Docs/BlackHoleEventHorizonPass.md`.
- Editor QA shortcut: `Ctrl+Shift+B` spawns a Black Hole in Play Mode for gravity testing.

## Black Hole Gravity

Black Hole gravity is intentionally gentle:

- radius is local, around the Black Hole;
- L1-L3 bodies are pulled the most;
- L4-L5 bodies are pulled less;
- L6-L8 bodies are only lightly affected;
- L9/L10 are not affected;
- no bodies are deleted, absorbed, scored, or transformed;
- merge rules are unchanged.

This should feel like a high-level physical reward and risk, not like the game taking
control away from the player.

## Not Changed

- No scoring table changes.
- No merge rule changes.
- No physics size/mass/damping table changes.
- No pressure floor logic changes.
- No danger line Game Over logic changes.
- No restart logic changes.
- No shop, ads, revive, currency, quests, skins, leaderboard, accounts, or analytics.

## Manual Checks

1. Reset first-session state with `Ctrl+Shift+O`, restart Play, and confirm opening demo still works.
2. Confirm first controlled minute reaches L3/L4 faster than before.
3. Confirm direct spawns do not exceed L4.
4. Confirm L1-L4 names and visuals are clear in Goal, Next, field, and Result screen.
5. Create or editor-spawn a Black Hole with `Ctrl+Shift+B`.
6. Confirm nearby small bodies drift toward it gently.
7. Confirm large bodies are not violently pulled.
8. Confirm Black Hole does not delete or absorb bodies.
9. Confirm normal merges still work near Black Hole.
10. Confirm Black Hole gravity does not create immediate unfair Game Over.
11. Fill the 10-run balance table again after this pass.
