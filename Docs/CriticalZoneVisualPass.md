# Critical Zone Visual Pass

## Goal

Make the danger line read like a live pressure indicator instead of a debug line. The player should feel that space is running out before the timer reaches Game Over.

## Changed

- Safe state is nearly invisible with only a weak transparent glow.
- Warning state grows brighter and begins a slower pulse.
- Critical state adds faster pulsing, wider glow, field haze, subtle container dimming, and moving energy shimmer bands.
- Game Over triggers a short flash, then leaves the line in a bright stabilized state behind the result UI.
- The visual layers use runtime sprites/materials only; no new external assets.

## Not Changed

- No change to `GameOverHoldSeconds`.
- No change to which balls count as dangerous.
- No change to score, pressure floor, physics, spawn, merge, or restart logic.

## Manual Check

1. In Play Mode, let a stable ball cross the danger line.
2. Confirm Safe is quiet when nothing is threatening.
3. Confirm Warning pulses without feeling like constant panic.
4. Confirm Critical is visibly more urgent and easier to read.
5. Confirm Game Over still triggers after the same hold time.
6. Confirm the result screen remains readable after the flash.
