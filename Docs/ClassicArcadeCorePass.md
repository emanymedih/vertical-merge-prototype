# Classic Arcade Core Pass

## Goal

Make the prototype feel closer to a classic one-screen arcade puzzle loop:
read the board fast, see the next possible merge, feel tension before danger,
enjoy merges as the main reward, and want one more run after Game Over.

## Classic Principles

- Readability first: the player should identify matching objects in about one second.
- Small reachable goal: the next merge opportunity should be visible without direct guidance.
- Tension rhythm: calm, risk, save, relief, then new risk.
- Reward the core action: merge feedback must feel better at higher levels.
- Fast retry: Game Over should show progress and make Play Again the obvious action.
- Fair loss: the player should feel the loss came from their choices, not a hidden penalty.

## Changed In This Pass

- Procedural cosmic body visuals now have readability rims and stronger high-level silhouettes.
- Black Hole has a brighter rim and accretion-ring treatment so it remains visible on the dark arena.
- Star, Red Giant, and Neutron Star have more distinct core/ring/band treatments.
- Similar-object resonance uses the body's cosmic glow color and a stronger emphasized target pulse.
- Danger line safe/warning/critical states are less sudden and less noisy in safe state.
- Merge feedback separates normal, L6+, and L8+ moments with stronger but short feedback.
- Game Over puts more weight on the run result than on the failure label.
- Game Over logs run duration, score, largest object, and merge count to the Unity Console.

## Out Of Scope

- No shop, ads, rewarded ads, in-app purchases, daily rewards, streaks, push notifications, accounts, analytics SDK, leaderboard, map, missions, quests, characters, story, new modes, or external assets.
- No scoring table rewrite.
- No physics tuning rewrite.
- No spawn distribution rewrite.
- No merge rule rewrite.
- No heavy particles or long blocking VFX.

## Manual Test

Play 10 full runs and record the result:

| Run | Duration | Score | Largest | Wanted Play Again? | Loss Felt Fair? | Merges Felt Good? | Goal Was Clear? | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 |  |  |  |  |  |  |  |  |
| 2 |  |  |  |  |  |  |  |  |
| 3 |  |  |  |  |  |  |  |  |
| 4 |  |  |  |  |  |  |  |  |
| 5 |  |  |  |  |  |  |  |  |
| 6 |  |  |  |  |  |  |  |  |
| 7 |  |  |  |  |  |  |  |  |
| 8 |  |  |  |  |  |  |  |  |
| 9 |  |  |  |  |  |  |  |  |
| 10 |  |  |  |  |  |  |  |  |

Target rhythm:

- First novice run: 1-3 minutes.
- Normal run after understanding: 3-6 minutes.
- Good run: 7-10 minutes.

After the 10 runs, decide whether the next pass should tune container size, ball sizes,
risk pressure, or spawn difficulty. Do not tune those before the test unless an obvious
fairness bug appears.

## Play Mode Checklist

1. Project compiles without errors.
2. Game starts.
3. Drop works.
4. Merge works.
5. Score updates.
6. Goal updates.
7. Discovery toast appears.
8. Danger line warns before Game Over.
9. Newly spawned or briefly bouncing balls do not cause false Game Over.
10. Game Over emphasizes the created object, not only defeat.
11. Play Again immediately starts a new run.
12. Similar-object resonance helps but does not aim for the player.
13. High merge feedback is noticeable and short.
14. Chain and Saved messages do not spam.
15. Portrait UI stays readable.
16. Console has no new errors.
