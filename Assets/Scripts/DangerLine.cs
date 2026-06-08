using System.Collections;
using UnityEngine;

public sealed class DangerLine : MonoBehaviour
{
    private const float GameOverHoldSeconds = 2f;
    private const float WarningThreshold = 0.28f;
    private const float CriticalThreshold = 0.68f;
    private const int ShimmerBandCount = 4;

    private GameController controller;
    private SpriteRenderer visual;
    private SpriteRenderer glow;
    private SpriteRenderer outerGlow;
    private SpriteRenderer zone;
    private SpriteRenderer dimField;
    private SpriteRenderer flash;
    private SpriteRenderer[] shimmerBands;
    private TextMesh label;
    private float lineY;
    private float lineWidth;
    private float dangerTimer;
    private bool warningSoundPlayed;
    private bool gameOverVisualLocked;

    public void Initialize(GameController gameController, float y, float width)
    {
        controller = gameController;
        lineY = y;
        lineWidth = width;
        transform.position = new Vector3(0f, lineY, 0f);

        var zoneObject = new GameObject("Critical Zone Field");
        zoneObject.transform.SetParent(transform);
        zoneObject.transform.localPosition = new Vector3(0f, -0.24f, 0f);
        zoneObject.transform.localRotation = Quaternion.identity;
        zoneObject.transform.localScale = new Vector3(1f, 28f, 1f);

        zone = zoneObject.AddComponent<SpriteRenderer>();
        zone.sprite = CircleSpriteCache.Square;
        zone.color = new Color(1f, 0.05f, 0.24f, 0.01f);
        zone.sortingOrder = 7;
        var zoneMaterial = RuntimeMaterials.CreateEnergyBeam(new Color(1f, 0.15f, 0.36f), 0.42f, 0.7f);
        if (zoneMaterial != null)
        {
            zone.material = zoneMaterial;
        }

        var dimFieldObject = new GameObject("Critical Zone Container Dim");
        dimFieldObject.transform.SetParent(transform);
        dimFieldObject.transform.localPosition = new Vector3(0f, -0.36f, 0f);
        dimFieldObject.transform.localRotation = Quaternion.identity;
        dimFieldObject.transform.localScale = new Vector3(1f, 72f, 1f);

        dimField = dimFieldObject.AddComponent<SpriteRenderer>();
        dimField.sprite = CircleSpriteCache.Square;
        dimField.color = new Color(0.02f, 0f, 0.04f, 0f);
        dimField.sortingOrder = 6;

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
        var glowMaterial = RuntimeMaterials.CreateEnergyBeam(new Color(1f, 0.12f, 0.3f), 0.85f, 1.4f);
        if (glowMaterial != null)
        {
            glow.material = glowMaterial;
        }

        var outerGlowObject = new GameObject("Danger Line Outer Energy Glow");
        outerGlowObject.transform.SetParent(transform);
        outerGlowObject.transform.localPosition = Vector3.zero;
        outerGlowObject.transform.localRotation = Quaternion.identity;
        outerGlowObject.transform.localScale = new Vector3(1f, 8.6f, 1f);

        outerGlow = outerGlowObject.AddComponent<SpriteRenderer>();
        outerGlow.sprite = CircleSpriteCache.Square;
        outerGlow.color = new Color(1f, 0.04f, 0.24f, 0.01f);
        outerGlow.sortingOrder = 7;

        shimmerBands = new SpriteRenderer[ShimmerBandCount];
        for (var i = 0; i < shimmerBands.Length; i++)
        {
            var shimmerObject = new GameObject($"Critical Zone Energy Shimmer {i + 1}");
            shimmerObject.transform.SetParent(transform);
            shimmerObject.transform.localRotation = Quaternion.identity;
            shimmerObject.transform.localScale = new Vector3(0.18f, 1.4f, 1f);

            var shimmer = shimmerObject.AddComponent<SpriteRenderer>();
            shimmer.sprite = CircleSpriteCache.Square;
            shimmer.color = new Color(1f, 0.82f, 0.96f, 0f);
            shimmer.sortingOrder = 10;
            shimmerBands[i] = shimmer;
        }

        var flashObject = new GameObject("Danger Line Game Over Flash");
        flashObject.transform.SetParent(transform);
        flashObject.transform.localPosition = Vector3.zero;
        flashObject.transform.localRotation = Quaternion.identity;
        flashObject.transform.localScale = new Vector3(1f, 44f, 1f);

        flash = flashObject.AddComponent<SpriteRenderer>();
        flash.sprite = CircleSpriteCache.Square;
        flash.color = new Color(1f, 0.76f, 0.92f, 0f);
        flash.sortingOrder = 12;

        var labelObject = new GameObject("Critical Zone Label");
        labelObject.transform.SetParent(transform);
        labelObject.transform.localPosition = new Vector3(0f, 0.13f, -0.2f);
        labelObject.transform.localRotation = Quaternion.identity;

        label = labelObject.AddComponent<TextMesh>();
        label.text = "DANGER";
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
            SetGameOverFlashVisual();
            controller.TriggerGameOver();
        }
    }

    private void UpdateVisualState()
    {
        var t = Mathf.Clamp01(dangerTimer / GameOverHoldSeconds);
        var statePulseSpeed = t >= CriticalThreshold ? 15.5f : t >= WarningThreshold ? 4.8f : 1.8f;
        var pulse = Mathf.Sin(Time.time * statePulseSpeed) * 0.5f + 0.5f;
        var slowPulse = Mathf.Sin(Time.time * 2.1f) * 0.5f + 0.5f;
        var lineThickness = Mathf.Lerp(0.026f, 0.058f, t) + (t >= WarningThreshold ? pulse * Mathf.Lerp(0.004f, 0.012f, t) : 0f);
        transform.localScale = new Vector3(lineWidth, lineThickness, 1f);

        if (t <= 0f)
        {
            visual.color = GetSafeColor();
            glow.color = new Color(0.72f, 0.18f, 0.34f, 0.012f + slowPulse * 0.006f);
            outerGlow.color = new Color(1f, 0.04f, 0.22f, 0.006f);
            zone.color = new Color(1f, 0.05f, 0.24f, 0.004f + slowPulse * 0.003f);
            dimField.color = new Color(0.02f, 0f, 0.04f, 0f);
            label.text = string.Empty;
            label.color = new Color(1f, 0.38f, 0.52f, 0f);
            UpdateShimmerBands(0f, 0f, pulse);
            return;
        }

        if (t < WarningThreshold)
        {
            var earlyT = Mathf.Clamp01(t / WarningThreshold);
            visual.color = Color.Lerp(GetSafeColor(), new Color(1f, 0.2f, 0.36f, 0.28f), earlyT);
            glow.color = Color.Lerp(new Color(1f, 0.08f, 0.24f, 0.018f), new Color(1f, 0.12f, 0.32f, 0.075f), earlyT);
            outerGlow.color = Color.Lerp(new Color(1f, 0.04f, 0.22f, 0.008f), new Color(1f, 0.05f, 0.26f, 0.035f), earlyT);
            zone.color = Color.Lerp(new Color(1f, 0.05f, 0.24f, 0.01f), new Color(1f, 0.05f, 0.24f, 0.036f), earlyT);
            dimField.color = new Color(0.02f, 0f, 0.04f, earlyT * 0.018f);
            label.text = string.Empty;
            label.color = new Color(1f, 0.38f, 0.52f, 0f);
            UpdateShimmerBands(0.03f * earlyT, 0.24f, pulse);
            return;
        }

        if (t < CriticalThreshold)
        {
            var warningT = Mathf.Clamp01((t - WarningThreshold) / (CriticalThreshold - WarningThreshold));
            var warningPulse = Mathf.Lerp(0.72f, 1f, pulse);
            visual.color = Color.Lerp(new Color(1f, 0.26f, 0.38f, 0.36f), new Color(1f, 0.16f, 0.34f, 0.72f), warningT) * warningPulse;
            glow.color = Color.Lerp(new Color(1f, 0.16f, 0.38f, 0.1f), new Color(1f, 0.08f, 0.28f, 0.24f), warningT + pulse * 0.08f);
            outerGlow.color = Color.Lerp(new Color(1f, 0.06f, 0.28f, 0.05f), new Color(1f, 0.1f, 0.34f, 0.18f), warningT + pulse * 0.08f);
            zone.color = Color.Lerp(new Color(1f, 0.05f, 0.24f, 0.05f), new Color(1f, 0.08f, 0.28f, 0.16f), warningT + pulse * 0.08f);
            dimField.color = Color.Lerp(new Color(0.02f, 0f, 0.04f, 0.03f), new Color(0.04f, 0f, 0.03f, 0.075f), warningT);
            label.text = "Critical Zone";
            label.color = Color.Lerp(new Color(1f, 0.42f, 0.56f, 0.36f), new Color(1f, 0.54f, 0.62f, 0.76f), warningT);
            UpdateShimmerBands(Mathf.Lerp(0.08f, 0.16f, warningT), Mathf.Lerp(0.45f, 0.78f, warningT), pulse);
            return;
        }

        label.text = "CRITICAL ZONE";
        visual.color = Color.Lerp(new Color(1f, 0.08f, 0.28f, 0.85f), new Color(1f, 0.42f, 0.72f, 1f), pulse);
        glow.color = Color.Lerp(new Color(1f, 0.06f, 0.28f, 0.28f), new Color(1f, 0.32f, 0.68f, 0.48f), pulse);
        outerGlow.color = Color.Lerp(new Color(1f, 0.02f, 0.2f, 0.18f), new Color(1f, 0.24f, 0.66f, 0.38f), pulse);
        zone.color = Color.Lerp(new Color(1f, 0.04f, 0.22f, 0.2f), new Color(1f, 0.18f, 0.5f, 0.34f), pulse);
        dimField.color = Color.Lerp(new Color(0.03f, 0f, 0.035f, 0.08f), new Color(0.06f, 0f, 0.04f, 0.15f), pulse);
        label.color = Color.Lerp(new Color(1f, 0.56f, 0.68f, 0.82f), new Color(1f, 0.86f, 0.92f, 1f), pulse);
        UpdateShimmerBands(0.22f, 1f, pulse);
    }

    private void SetGameOverFlashVisual()
    {
        if (gameOverVisualLocked)
        {
            return;
        }

        gameOverVisualLocked = true;
        visual.color = new Color(1f, 0.78f, 0.9f, 1f);
        glow.color = new Color(1f, 0.38f, 0.72f, 0.64f);
        outerGlow.color = new Color(1f, 0.22f, 0.64f, 0.58f);
        zone.color = new Color(1f, 0.18f, 0.5f, 0.42f);
        dimField.color = new Color(0.04f, 0f, 0.04f, 0.18f);
        flash.color = new Color(1f, 0.76f, 0.92f, 0.58f);
        label.text = "CRITICAL ZONE";
        label.color = new Color(1f, 0.94f, 0.98f, 1f);
        UpdateShimmerBands(0.34f, 1f, 1f);
        StartCoroutine(GameOverFlashRoutine());
    }

    private void UpdateShimmerBands(float alpha, float travelStrength, float pulse)
    {
        if (shimmerBands == null)
        {
            return;
        }

        for (var i = 0; i < shimmerBands.Length; i++)
        {
            var band = shimmerBands[i];
            var phase = Mathf.Repeat(Time.time * Mathf.Lerp(0.18f, 0.86f, travelStrength) + i * 0.27f, 1f);
            var x = Mathf.Lerp(-0.47f, 0.47f, phase);
            var localPulse = Mathf.Sin(Time.time * (4.5f + travelStrength * 8f) + i * 1.7f) * 0.5f + 0.5f;

            band.transform.localPosition = new Vector3(x, 0f, -0.1f);
            band.transform.localScale = new Vector3(Mathf.Lerp(0.08f, 0.24f, localPulse), Mathf.Lerp(0.9f, 2.4f, travelStrength), 1f);
            band.color = new Color(1f, Mathf.Lerp(0.5f, 0.9f, pulse), 0.92f, alpha * Mathf.Lerp(0.38f, 1f, localPulse));
        }
    }

    private static Color GetSafeColor()
    {
        return new Color(1f, 0.14f, 0.34f, 0.065f);
    }

    private IEnumerator GameOverFlashRoutine()
    {
        var elapsed = 0f;
        const float duration = 0.36f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            flash.color = new Color(1f, 0.76f, 0.92f, Mathf.Lerp(0.58f, 0f, Mathf.SmoothStep(0f, 1f, t)));
            yield return null;
        }

        flash.color = new Color(1f, 0.76f, 0.92f, 0f);
    }
}
