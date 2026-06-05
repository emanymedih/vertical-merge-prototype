# Black Hole Event Horizon Pass v1

## Goal

Make `Black Hole` feel like a living arena event, not only a large late-game body.
The player should feel a soft time pressure: if they wait too long, the Black Hole
will consume one nearby small or mid-level body.

## Changed

- L9 `Black Hole` now runs a 15-second Event Horizon cycle.
- During the normal cycle it keeps the existing gentle gravity pull.
- During the final 3 seconds, its aura charges up and a nearby target gets a purple
  warning marker and beam.
- At the end of the cycle it absorbs one eligible nearby body with a short spiral
  shrink animation.
- Sound hooks were added for `BlackHoleWarning` and `BlackHoleAbsorb`.

## Absorption Rules

- Eligible targets are L1-L5 only.
- L6-L10 are not absorbed.
- The Black Hole never absorbs itself or another Black Hole.
- Absorption does not award score.
- Absorption does not trigger merge rules.
- Absorption does not delete multiple bodies in one pulse.
- If no eligible target is nearby, the pulse fires visually and the cycle restarts.

## Design Intent

The Event Horizon is closer to a pressure-zone reminder than a bonus system. It gives
the player a reason to move faster, while still being fair because the target is shown
before it is consumed.

The mechanic should create thoughts like:

- "I should use that Moon before the Black Hole takes it."
- "The Black Hole helped clear the field, but it cost me a possible merge."
- "I created something powerful, and now the arena behaves differently."

## Not Changed

- No scoring table changes.
- No merge rule changes.
- No spawn distribution changes.
- No pressure floor logic changes.
- No danger line game over logic changes.
- No restart logic changes.
- No shop, ads, revive, currency, quests, skins, leaderboard, accounts, or analytics.

## Manual Checks

1. Create or editor-spawn a Black Hole with `Ctrl+Shift+B`.
2. Confirm nearby bodies still drift gently toward it.
3. Wait for the 15-second cycle.
4. Confirm the final warning phase is visible but not noisy.
5. Confirm only one nearby L1-L5 body is absorbed.
6. Confirm L6+ bodies are not absorbed.
7. Confirm no score is awarded for absorption.
8. Confirm merges still work normally near the Black Hole.
9. Confirm absorption does not create an immediate unfair Game Over.
10. Confirm the cycle restarts after absorption.
