# Helper Star Pass

## Goal

Add a rare pleasant mid-session event that can change a difficult field without turning progression into a lottery. The Helper Star upgrades one stable low/mid-level cosmic body and creates a short emotional peak: the run can still turn around.

## Mechanic

- The Helper Star is a runtime event, not a normal spawn payload.
- It appears after the run has lasted long enough, then enters a long cooldown.
- A session can contain 0-3 Helper Star events.
- Short runs are not guaranteed to see it.
- The target is randomly chosen from active, stable bodies from L2 to L5.
- Bodies that are merging, pre-merging, anomaly-targeted, recently spawned, moving too fast, or outside the chamber are excluded.

## Upgrade Rules

- Standard result: target level +1.
- Rare result: target level +2.
- The result is capped at L6 Star.
- The event cannot directly create Neutron Star, Black Hole, or Galaxy Core.
- The event does not award score; it changes board state and progression only.

## Visual Feel

- A small glowing Helper Star appears inside the chamber.
- It softly hovers, projects a gold/cyan energy beam, and highlights the selected target.
- On activation, the target is replaced by the upgraded cosmic body with a short birth pop.
- The effect uses procedural sprites, runtime materials, and pooled particles; no external assets are required.

## Balance Guardrails

- Frequency depends on session duration, not just drop count.
- Long cooldown prevents several stars in a row.
- Limiting targets to L2-L5 protects high-level merge mastery.
- Capping the result at L6 keeps Black Hole and Galaxy Core meaningful.

## Manual Check

1. Play a run longer than one minute and verify the Helper Star can appear.
2. Confirm it only targets L2-L5 stable bodies.
3. Confirm it never upgrades beyond L6.
4. Confirm score is not added by the event.
5. Confirm Goal/Largest update if the event creates a new highest body.
6. Confirm the star does not appear during opening demo or after Game Over.
7. In Unity Editor, use `Ctrl+Shift+H` to force-test the event.
