# Balance Test Template v1

Use this after the Productization Pass visual and UX changes are in place.

## Target rhythm

- First run for a new player: 2-4 minutes.
- First merge: 10-20 seconds.
- First Planet: 30-60 seconds.
- Normal run: 3-6 minutes.
- Strong run: 7-10 minutes.

## How to run the test

1. Open `Assets/Scenes/GameScene.unity`.
2. Press Play.
3. For a true first-session test, press `Ctrl+Shift+O`, stop Play, then start Play again.
4. Play until `Run Complete`.
5. Copy the run values from the Unity Console line that starts with `Run complete:`.
6. Press `Play Again` immediately and continue until 10 runs are logged.
7. Do not tune anything during the 10-run set. Record first, decide later.

Console log format:

```text
Run complete: duration=MM:SS, score=1234, largest=Planet, mergeCount=12, firstMerge=00:14, firstPlanet=00:48, firstSessionPacing=True
```

Use `firstSessionPacing=True` only for the first fresh-player run. Later runs should usually be evaluated as normal runs.

## 10-run log

| Run | Time | First Merge | First Planet | Score | Largest | Merge Count | Wanted Replay? | Loss Felt Fair? | Notes |
| --- | --- | --- | --- | ---: | --- | ---: | --- | --- | --- |
| 1 |  |  |  |  |  |  |  |  |  |
| 2 |  |  |  |  |  |  |  |  |  |
| 3 |  |  |  |  |  |  |  |  |  |
| 4 |  |  |  |  |  |  |  |  |  |
| 5 |  |  |  |  |  |  |  |  |  |
| 6 |  |  |  |  |  |  |  |  |  |
| 7 |  |  |  |  |  |  |  |  |  |
| 8 |  |  |  |  |  |  |  |  |  |
| 9 |  |  |  |  |  |  |  |  |  |
| 10 |  |  |  |  |  |  |  |  |  |

## Tuning rules

- If runs are too short, consider a later balance pass for larger container space, smaller bodies, safer spawn distribution, or slower pressure.
- If runs are too long, consider a later balance pass for larger bodies, faster pressure, more risky spawn distribution, or tighter danger recovery.
- If first merge is later than 20 seconds, increase early L2 availability before changing physics.
- If first Planet is later than 60 seconds, review early L2/L3 pacing before changing scoring or merge rules.
- Do not tune physics from one run. Use the full 10-run pattern.

## Productization questions

- Is it clear within 10 seconds what to do?
- Does merging feel satisfying enough to repeat?
- Does Game Over make Play Again feel like the obvious next action?
- Did the player understand why the run ended?
- Did the largest created body feel memorable?
