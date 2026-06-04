using UnityEngine;

public sealed class DangerLine : MonoBehaviour
{
    private const float GameOverHoldSeconds = 2f;

    private GameController controller;
    private SpriteRenderer visual;
    private float lineY;
    private float dangerTimer;

    public void Initialize(GameController gameController, float y, float width)
    {
        controller = gameController;
        lineY = y;
        transform.position = new Vector3(0f, lineY, 0f);

        visual = gameObject.AddComponent<SpriteRenderer>();
        visual.sprite = CircleSpriteCache.Square;
        visual.color = GetSafeColor();
        visual.sortingOrder = 3;
        transform.localScale = new Vector3(width, 0.035f, 1f);
    }

    private void Update()
    {
        if (controller == null || controller.IsGameOver)
        {
            return;
        }

        var hasValidBallAboveLine = false;

        foreach (var ball in controller.ActiveBalls)
        {
            if (ball == null)
            {
                continue;
            }

            if (ball.IsEligibleForDanger(lineY))
            {
                hasValidBallAboveLine = true;
                break;
            }
        }

        dangerTimer = hasValidBallAboveLine ? dangerTimer + Time.deltaTime : 0f;
        UpdateVisualState();

        if (dangerTimer >= GameOverHoldSeconds)
        {
            controller.TriggerGameOver();
        }
    }

    private void UpdateVisualState()
    {
        var t = Mathf.Clamp01(dangerTimer / GameOverHoldSeconds);
        if (t <= 0f)
        {
            visual.color = GetSafeColor();
            return;
        }

        var pulse = Mathf.Sin(Time.time * 16f) * 0.5f + 0.5f;
        if (t < 0.65f)
        {
            visual.color = Color.Lerp(new Color(1f, 0.62f, 0.18f, 0.6f), new Color(1f, 0.38f, 0.12f, 0.85f), t);
            return;
        }

        visual.color = Color.Lerp(new Color(1f, 0.15f, 0.12f, 0.82f), new Color(1f, 0.05f, 0.04f, 1f), pulse);
    }

    private static Color GetSafeColor()
    {
        return new Color(1f, 0.17f, 0.17f, 0.22f);
    }
}
