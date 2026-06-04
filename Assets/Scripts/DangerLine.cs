using System.Collections.Generic;
using UnityEngine;

public sealed class DangerLine : MonoBehaviour
{
    private const float GameOverHoldSeconds = 2f;

    private readonly Dictionary<Ball, float> dangerTimers = new Dictionary<Ball, float>();
    private readonly List<Ball> ballsToRemove = new List<Ball>();
    private GameController controller;
    private float lineY;

    public void Initialize(GameController gameController, float y, float width)
    {
        controller = gameController;
        lineY = y;
        transform.position = new Vector3(0f, lineY, 0f);

        var visual = gameObject.AddComponent<SpriteRenderer>();
        visual.sprite = CircleSpriteCache.Square;
        visual.color = new Color(1f, 0.17f, 0.17f, 0.82f);
        visual.sortingOrder = 3;
        transform.localScale = new Vector3(width, 0.035f, 1f);
    }

    private void Update()
    {
        if (controller == null || controller.IsGameOver)
        {
            return;
        }

        ballsToRemove.Clear();
        foreach (var trackedBall in dangerTimers.Keys)
        {
            ballsToRemove.Add(trackedBall);
        }

        foreach (var ball in controller.ActiveBalls)
        {
            if (ball == null)
            {
                continue;
            }

            if (ball.IsEligibleForDanger(lineY))
            {
                dangerTimers[ball] = dangerTimers.TryGetValue(ball, out var timer) ? timer + Time.deltaTime : Time.deltaTime;
                ballsToRemove.Remove(ball);

                if (dangerTimers[ball] >= GameOverHoldSeconds)
                {
                    controller.TriggerGameOver();
                    return;
                }
            }
        }

        for (var i = 0; i < ballsToRemove.Count; i++)
        {
            dangerTimers.Remove(ballsToRemove[i]);
        }
    }
}
