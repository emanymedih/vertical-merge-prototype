# Animation Feel Pass v1

## Goal

Make the core loop feel more responsive and rewarding with short snappy arcade
animations. Drop should feel immediate, merge should feel like the main reward, and
high-level merges should read as special events without slowing the game down.

## Changed

- Added code easing helpers for procedural animation.
- Drop balls now use a short squash/stretch pop and glow blink.
- World ghost preview breathes softly while aiming.
- Next preview pops in after each drop.
- Merge-created balls now use a dedicated birth animation.
- Merge effects now use a core flash plus expanding shockwave ring.
- Floating score has a quicker pop, eased rise, and softer fade.
- High merges L6+ and peak merges L8+ get stronger short-lived visual feedback.
- Toast moments like `Chain!`, `Great Chain!`, `Saved!`, and discovery now pop/fade.

## Not Changed

- No physics changes.
- No scoring changes.
- No spawn distribution changes.
- No merge rule changes.
- No pressure or danger logic changes.
- No Animator Controller or animation assets.
- No external assets.

## Manual Checks

1. Drop a few balls and confirm release remains instant.
2. Confirm ghost preview breathes only as a visual layer.
3. Merge L1-L5 and confirm flash/ring/score are readable.
4. Merge L6+ and L8+ and confirm feedback is stronger but short.
5. Trigger `Chain!` / `Great Chain!` and confirm toast animation is clear.
6. Confirm Black Hole and Cosmic Anomaly absorption still work.
7. Confirm no lingering flash/ring/score/toast objects remain after Game Over.
