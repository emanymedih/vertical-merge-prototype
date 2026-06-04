using UnityEngine;

public sealed class DangerLine : MonoBehaviour
{
    private const float GameOverHoldSeconds = 2f;
    private const float WarningThreshold = 0.28f;
    private const float CriticalThreshold = 0.68f;

    private GameController controller;
    private SpriteRenderer visual;
    private SpriteRenderer glow;
    private SpriteRenderer zone;
    private TextMesh label;
    private float lineY;
    private float dangerTimer;
    private bool warningSoundPlayed;

    public void Initialize(GameController gameController, float y, float width)
    {
        controller = gameController;
        lineY = y;
        transform.position = new Vector3(0f, lineY, 0f);

        var zoneObject = new GameObject("Critical Zone Field");
        zoneObject.transform.SetParent(transform);
        zoneObject.transform.localPosition = new Vector3(0f, -0.24f, 0f);
        zoneObject.transform.localRotation = Quaternion.identity;
        zoneObject.transform.localScale = new Vector3(1f, 12f, 1f);

        zone = zoneObject.AddComponent<SpriteRenderer>();
        zone.sprite = CircleSpriteCache.Square;
        zone.color = new Color(1f, 0.05f, 0.24f, 0.01f);
        zone.sortingOrder = 7;

        visual = gameObject.AddComponent<SpriteRenderer>();
        visual.sprite = CircleSpriteCache.Square;
        visual.color = GetSafeColor();
        visual.sortingOrder = 9;
        transform.localScale = new Vector3(width, 0.035f, 1f);

        var glowObject = new GameObject("Danger Line Glow");
        glowObject.transform.SetParent(transform);
        glowObject.transform.localPosition = Vector3.zero;
        glowObject.transform.localRotation = Quaternion.identity;
        glowObject.transform.localScale = new Vector3(1f, 3.2f, 1f);

        glow = glowObject.AddComponent<SpriteRenderer>();
        glow.sprite = CircleSpriteCache.Square;
        glow.color = new Color(1f, 0.08f, 0.22f, 0.018f);
        glow.sortingOrder = 8;

        var labelObject = new GameObject("Critical Zone Label");
        labelObject.transform.SetParent(transform);
        labelObject.transform.localPosition = new Vector3(0f, 0.13f, -0.2f);
        labelObject.transform.localRotation = Quaternion.identity;

        label = labelObject.AddComponent<TextMesh>();
        label.text = "CRITICAL ZONE";
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 48;
        label.characterSize = 0.035f;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.color = new Color(1f, 0.38f, 0.52f, 0.12f);

        var labelRenderer = labelObject.GetComponent<MeshRenderer>();
        labelRenderer.sortingOrder = 10;
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
        controller.SetDangerPressure(Mathf.Clamp01(dangerTimer / GameOverHoldSeconds));

        var pressure = Mathf.Clamp01(dangerTimer / GameOverHoldSeconds);
        if (pressure >= WarningThreshold && !warningSoundPlayed)
        {
            warningSoundPlayed = true;
            SoundManager.Play(SoundEvent.DangerWarning);
            OnboardingController.Instance?.SetDangerActive(true);
        }
        else if (dangerTimer <= 0f)
        {
            warningSoundPlayed = false;
            OnboardingController.Instance?.SetDangerActive(false);
        }

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
            glow.color = new Color(0.9f, 0.08f, 0.24f, 0.018f);
            zone.color = new Color(1f, 0.05f, 0.24f, 0.01f);
            label.color = new Color(1f, 0.38f, 0.52f, 0.12f);
            return;
        }

        if (t < WarningThreshold)
        {
            var earlyT = Mathf.Clamp01(t / WarningThreshold);
            visual.color = Color.Lerp(GetSafeColor(), new Color(1f, 0.2f, 0.36f, 0.28f), earlyT);
            glow.color = Color.Lerp(new Color(1f, 0.08f, 0.24f, 0.018f), new Color(1f, 0.12f, 0.32f, 0.08f), earlyT);
            zone.color = Color.Lerp(new Color(1f, 0.05f, 0.24f, 0.01f), new Color(1f, 0.05f, 0.24f, 0.04f), earlyT);
            label.color = Color.Lerp(new Color(1f, 0.38f, 0.52f, 0.12f), new Color(1f, 0.44f, 0.58f, 0.28f), earlyT);
            return;
        }

        var pulse = Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f;
        if (t < CriticalThreshold)
        {
            var warningT = Mathf.Clamp01((t - WarningThreshold) / (CriticalThreshold - WarningThreshold));
            visual.color = Color.Lerp(new Color(1f, 0.26f, 0.38f, 0.36f), new Color(1f, 0.16f, 0.34f, 0.72f), warningT);
            glow.color = Color.Lerp(new Color(1f, 0.16f, 0.38f, 0.1f), new Color(1f, 0.08f, 0.28f, 0.22f), warningT);
            zone.color = Color.Lerp(new Color(1f, 0.05f, 0.24f, 0.05f), new Color(1f, 0.08f, 0.28f, 0.16f), warningT);
            label.color = Color.Lerp(new Color(1f, 0.42f, 0.56f, 0.36f), new Color(1f, 0.54f, 0.62f, 0.76f), warningT);
            return;
        }

        visual.color = Color.Lerp(new Color(1f, 0.08f, 0.28f, 0.85f), new Color(1f, 0.42f, 0.72f, 1f), pulse);
        glow.color = Color.Lerp(new Color(1f, 0.06f, 0.28f, 0.28f), new Color(1f, 0.32f, 0.68f, 0.48f), pulse);
        zone.color = Color.Lerp(new Color(1f, 0.04f, 0.22f, 0.2f), new Color(1f, 0.18f, 0.5f, 0.34f), pulse);
        label.color = Color.Lerp(new Color(1f, 0.56f, 0.68f, 0.82f), new Color(1f, 0.86f, 0.92f, 1f), pulse);
    }

    private static Color GetSafeColor()
    {
        return new Color(1f, 0.14f, 0.34f, 0.065f);
    }
}
