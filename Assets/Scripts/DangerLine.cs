using System.Collections.Generic;
using UnityEngine;

public sealed class DangerLine : MonoBehaviour
{
    private const float GameOverHoldSeconds = 2f;

    private readonly Dictionary<int, float> dangerTimers = new Dictionary<int, float>();
    private readonly List<int> idsToRemove = new List<int>();
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

        idsToRemove.Clear();
        foreach (var id in dangerTimers.Keys)
        {
            idsToRemove.Add(id);
        }

        foreach (var ball in controller.ActiveBalls)
        {
            if (ball == null)
            {
                continue;
            }

            var id = ball.GetInstanceID();
            if (ball.IsEligibleForDanger(lineY))
            {
                dangerTimers[id] = dangerTimers.TryGetValue(id, out var timer) ? timer + Time.deltaTime : Time.deltaTime;
                idsToRemove.Remove(id);

                if (dangerTimers[id] >= GameOverHoldSeconds)
                {
                    controller.TriggerGameOver();
                    return;
                }
            }
        }

        for (var i = 0; i < idsToRemove.Count; i++)
        {
            dangerTimers.Remove(idsToRemove[i]);
        }
    }
}
