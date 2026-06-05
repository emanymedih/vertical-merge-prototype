using System.Collections.Generic;
using UnityEngine;

public sealed class MagneticTensionController : MonoBehaviour
{
    private const int MinimumLevel = 5;
    private const float CheckIntervalSeconds = 0.12f;
    private const float NearMergePadding = 0.1f;
    private const float PullForce = 0.16f;
    private const float MaxPairDistanceMultiplier = 2.35f;
    private const float BlockerRadiusPadding = 0.18f;

    private readonly HashSet<Ball> highlightedBalls = new HashSet<Ball>();
    private readonly List<Ball> previousHighlights = new List<Ball>();
    private GameController controller;
    private float nextCheckAt;

    public void Initialize(GameController gameController)
    {
        controller = gameController;
    }

    private void FixedUpdate()
    {
        if (controller == null || controller.IsGameOver || Time.time < nextCheckAt)
        {
            return;
        }

        nextCheckAt = Time.time + CheckIntervalSeconds;
        ScanPairs();
    }

    private void ScanPairs()
    {
        previousHighlights.Clear();
        previousHighlights.AddRange(highlightedBalls);
        highlightedBalls.Clear();

        var balls = controller.ActiveBalls;
        for (var i = 0; i < balls.Count; i++)
        {
            var first = balls[i];
            if (!CanMagnetize(first))
            {
                continue;
            }

            for (var j = i + 1; j < balls.Count; j++)
            {
                var second = balls[j];
                if (!CanMagnetize(second) || first.Level != second.Level)
                {
                    continue;
                }

                if (!IsNearMissPair(first, second, balls))
                {
                    continue;
                }

                ApplyTension(first, second);
            }
        }

        for (var i = 0; i < previousHighlights.Count; i++)
        {
            var ball = previousHighlights[i];
            if (ball != null && !highlightedBalls.Contains(ball))
            {
                ball.SetMagneticTension(0f);
            }
        }
    }

    private static bool CanMagnetize(Ball ball)
    {
        return ball != null && !ball.IsMerging && !ball.IsPreMerging && ball.Level >= MinimumLevel;
    }

    private static bool IsNearMissPair(Ball first, Ball second, IReadOnlyList<Ball> balls)
    {
        var firstPosition = (Vector2)first.transform.position;
        var secondPosition = (Vector2)second.transform.position;
        var offset = secondPosition - firstPosition;
        var distance = offset.magnitude;
        var expectedMergeDistance = first.Radius + second.Radius;
        var maxDistance = expectedMergeDistance * MaxPairDistanceMultiplier;

        if (distance <= expectedMergeDistance + first.Radius * NearMergePadding || distance > maxDistance)
        {
            return false;
        }

        return HasSmallBlockerBetween(first, second, balls, firstPosition, secondPosition);
    }

    private static bool HasSmallBlockerBetween(Ball first, Ball second, IReadOnlyList<Ball> balls, Vector2 firstPosition, Vector2 secondPosition)
    {
        var segment = secondPosition - firstPosition;
        var segmentLengthSqr = segment.sqrMagnitude;
        if (segmentLengthSqr <= 0.001f)
        {
            return false;
        }

        for (var i = 0; i < balls.Count; i++)
        {
            var blocker = balls[i];
            if (blocker == null || blocker == first || blocker == second || blocker.Level > 2 || blocker.IsMerging || blocker.IsPreMerging)
            {
                continue;
            }

            var blockerPosition = (Vector2)blocker.transform.position;
            var t = Mathf.Clamp01(Vector2.Dot(blockerPosition - firstPosition, segment) / segmentLengthSqr);
            if (t <= 0.08f || t >= 0.92f)
            {
                continue;
            }

            var closest = firstPosition + segment * t;
            var allowedDistance = blocker.Radius + BlockerRadiusPadding;
            if (Vector2.Distance(blockerPosition, closest) <= allowedDistance)
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyTension(Ball first, Ball second)
    {
        var firstPosition = (Vector2)first.transform.position;
        var secondPosition = (Vector2)second.transform.position;
        var direction = (secondPosition - firstPosition).normalized;
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        first.TryApplyExternalForce(direction * PullForce * first.Mass);
        second.TryApplyExternalForce(-direction * PullForce * second.Mass);
        first.SetMagneticTension(0.72f);
        second.SetMagneticTension(0.72f);
        highlightedBalls.Add(first);
        highlightedBalls.Add(second);
    }
}
