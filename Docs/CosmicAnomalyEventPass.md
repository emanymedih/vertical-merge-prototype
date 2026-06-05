# Cosmic Anomaly Event Pass v1

## Goal

Add a mid-session arena event so the game does not wait until late-game Black Hole
creation before the field feels alive. The anomaly should create a small burst of
pressure: it can help clear clutter, but it can also steal a useful small body before
the player merges it.

## Changed

- Added `CosmicAnomalyEventController` as a runtime arena event.
- First anomaly appears after roughly 45-75 seconds.
- Later anomalies use a 48-66 second cooldown.
- An anomaly starts with a short warning/charge phase.
- While active, it gently pulls nearby bodies.
- It can absorb up to two nearby L1-L4 bodies per event.
- It does not absorb L5+ bodies.
- It has no collider and no Rigidbody2D.
- Added sound hooks for `CosmicAnomalyWarning` and `CosmicAnomalyAbsorb`.
- Editor QA shortcut: `Ctrl+Shift+A` triggers an anomaly immediately in Play Mode.

## Design Intent

This event is not a bonus system and not a new merge level. It is a temporary arena
disturbance, similar in spirit to a shrinking pressure zone: the player sees the field
charging and has a reason to make the next decision faster.

Expected player thoughts:

- "That Moon might get taken, I should use it now."
- "The anomaly cleared some trash, but it also changed my plan."
- "The arena can surprise me before the final Black Hole."

## Rules

- No score is awarded for anomaly absorption.
- No merge rules are changed.
- No spawn distribution is changed.
- No danger line or pressure floor logic is changed.
- The anomaly does not appear as a mergeable ball.
- The anomaly does not affect opening demo logic.

## Manual Checks

1. Start Play Mode and wait 45-75 seconds, or press `Ctrl+Shift+A` in the Unity Editor.
2. Confirm a purple anomaly warning appears inside the arena, not near the spawn line.
3. Confirm nearby small bodies drift gently toward the anomaly.
4. Confirm up to two L1-L4 bodies can be absorbed with a short spiral/shrink animation.
5. Confirm L5+ bodies are not absorbed.
6. Confirm absorption gives no score and does not trigger merge/discovery.
7. Confirm the event disappears after a short active window.
8. Confirm normal drop/merge/danger/game over behavior still works.
9. Confirm the event does not leave markers/beams visible after Game Over.
