# Variable Reward + Anomaly Agency Pass v1

## Goal

Add controlled mid-run events that make a six-minute session feel less flat: rare critical merges, a rescue comet, stress feedback near danger, light intent magnetism, and an anomaly event the player can interrupt.

## What Changed

- Critical merge: a normal merge has a 2.5% chance to create `Level + 2`, capped at `Galaxy Core / L10`. It is disabled during opening demo.
- Comet drop: if the player has not merged for 30+ seconds and danger/pressure is rising, the next spawn can become a glowing Comet with a 10% chance and 60 second cooldown.
- Comet impact: the Comet destroys one valid object, gives small pressure relief, and gives no score.
- Run Complete: result card now includes a pulsing silhouette of the next goal, for example `Galaxy Core is waiting...`.
- Stress overlay: screen edges react to danger pressure, high stack position, and pressure floor progress; `Saved!` resets the stress feedback.
- Intent magnetism: a freshly dropped normal body gets subtle horizontal assistance toward same-level bodies for 1.2 seconds.
- Anomaly agency: Cosmic Anomaly locks 1-2 targets from L1-L4, telegraphs for 3.5 seconds, and lets the player rescue targets by knock-out movement or merge escape.

## Design Defaults

- Critical merge score uses the actually created level.
- Critical merge silently marks the skipped intermediate level as discovered, so goal progression stays readable.
- Comet is a rescue tool, not a reward economy source.
- Anomaly `Evaded!` gives a one-shot x2 rescue bonus based on 50% of the next-level score for the rescued target, shown as `Evaded! x2`.
- Sound remains hook-only in this pass. No audio assets or Unity Audio module dependency are added.
- No DOTween, Post Processing package, external assets, SDKs, ads, revive, shop, leaderboard, or meta systems.

## Manual Test

1. Play for at least 10 minutes and confirm normal drop, pre-merge, merge, score, goal, pressure floor, danger line, restart, Black Hole gravity, and anomaly still work.
2. Trigger editor anomaly shortcut `Ctrl+Shift+A` and verify target lock, beam, countdown, rescue by moving a target, rescue by merge, and absorption if rescue fails.
3. Verify `Evaded!` appears only for a real rescue and adds a one-shot bonus.
4. Verify Comet appears only under pressure after a long no-merge window, destroys one valid object, gives no score, and gives relief.
5. Verify critical merge can create `Level + 2`, never above `Galaxy Core`, and does not break Discovery or Run Complete.
6. Verify stress overlay is almost invisible in safe state, grows during danger, and resets on `Saved!`.
7. Verify intent magnetism is subtle and does not look like auto-aim.
8. Verify Run Complete shows largest object plus next-goal silhouette.
9. Check Console for new errors.
