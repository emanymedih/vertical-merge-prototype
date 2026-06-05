# Pre-Merge Satisfaction Pass v1

## Goal

Make merge feel more anticipated and rewarding. Same-level bodies should briefly
charge before merging, then release with stronger data-driven effects, haptics, and
audio hooks.

## Changed

- Same-level collision now starts a short `0.16s` pre-merge charge instead of merging immediately.
- Pre-merge bodies use a temporary `FixedJoint2D`, squash/jitter, and glow.
- Merge release uses `CosmicBodyFeelProfile` data with fallback values.
- Audio profile data stores clip ids/pitch as no-op hooks until the Unity Audio module is enabled.
- Merge flash, ring, floating score, and particles are pooled.
- Particles use burst emission with velocity damping for a fast-start/ease-out feel.
- Haptics now expose `LightImpact`, `HeavyImpact`, and `SuccessPattern` without `Handheld.Vibrate`.
- iOS haptics are routed through a native `UIImpactFeedbackGenerator` bridge.
- `MagneticTensionController` gently pulls L5+ near-miss pairs when an L1/L2 blocker sits between them.

## Not Changed

- No merge rule changes.
- No scoring changes.
- No spawn distribution changes.
- No pressure or danger logic changes.
- No DOTween or Nice Vibrations dependency.
- Gameplay balls are not object-pooled in this pass.

## Manual Checks

1. Merge two L1 bodies and confirm the short charge is visible.
2. Confirm charged bodies do not fly apart before merge.
3. Confirm opening demo still completes.
4. Confirm Black Hole/Cosmic Anomaly do not absorb pre-merging bodies.
5. Confirm L6+, L8+, and L10 merges have stronger feedback.
6. Confirm Editor haptics are no-op and iOS does not use `Handheld.Vibrate`.
7. Confirm pooled VFX do not linger after Game Over.
8. Create an L5+ near miss with a small blocker and confirm subtle magnetic pull/glow.
