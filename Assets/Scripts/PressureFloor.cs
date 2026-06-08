using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public sealed class PressureFloor : MonoBehaviour
{
    private const float FirstSessionInitialDelaySeconds = 5f;
    private const float FirstSessionBaseRiseSpeed = 0.048f;
    private const float FirstSessionMaxRiseSpeed = 0.078f;
    private const float StandardInitialDelaySeconds = 7f;
    private const float StandardBaseRiseSpeed = 0.042f;
    private const float StandardMaxRiseSpeed = 0.07f;
    private const float RiseAcceleration = 0.00004f;
    private const float SurfaceThickness = 0.16f;
    private const int CompressionWaveCount = 4;

    [SerializeField] private float mergeReliefMultiplier = 1.15f;
    [SerializeField] private float mergeReliefBase = 0.05f;
    [SerializeField] private float mergeReliefPerLevel = 0.012f;
    [SerializeField] private float minMergeRelief = 0.07f;
    [SerializeField] private float maxMergeRelief = 0.18f;
    [SerializeField] private float reliefAnimationSeconds = 0.18f;
    [SerializeField] private float reliefGlowSeconds = 0.34f;

    private GameController controller;
    private Rigidbody2D body;
    private BoxCollider2D surfaceCollider;
    private SpriteRenderer surfaceRenderer;
    private SpriteRenderer fillRenderer;
    private SpriteRenderer lowerFillRenderer;
    private SpriteRenderer topGlowRenderer;
    private SpriteRenderer horizonCoreRenderer;
    private SpriteRenderer[] compressionWaveRenderers;
    private float startY;
    private float currentY;
    private float bottomY;
    private float dangerY;
    private float width;
    private float elapsed;
    private float initialDelaySeconds;
    private float baseRiseSpeed;
    private float maxRiseSpeed;
    private bool reliefAnimating;
    private float reliefStartY;
    private float reliefTargetY;
    private float reliefAnimationElapsed;
    private float reliefGlowUntil;

    public float PressureProgress => Mathf.Clamp01(Mathf.InverseLerp(bottomY, dangerY, currentY));

    public void Initialize(GameController gameController, ContainerBounds bounds, bool firstSessionPacingActive)
    {
        controller = gameController;
        initialDelaySeconds = firstSessionPacingActive ? FirstSessionInitialDelaySeconds : StandardInitialDelaySeconds;
        baseRiseSpeed = firstSessionPacingActive ? FirstSessionBaseRiseSpeed : StandardBaseRiseSpeed;
        maxRiseSpeed = firstSessionPacingActive ? FirstSessionMaxRiseSpeed : StandardMaxRiseSpeed;

        bottomY = bounds.Bottom - 0.35f;
        startY = bottomY - 0.55f;
        currentY = startY;
        dangerY = bounds.DangerY;
        width = bounds.Width;

        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        surfaceCollider = GetComponent<BoxCollider2D>();
        surfaceCollider.size = Vector2.one;
        surfaceCollider.sharedMaterial = PhysicsMaterials.WallMaterial;

        surfaceRenderer = GetComponent<SpriteRenderer>();
        surfaceRenderer.sprite = CircleSpriteCache.Square;
        surfaceRenderer.color = new Color(0.58f, 0.32f, 1f, 0.44f);
        surfaceRenderer.sortingOrder = 4;

        fillRenderer = CreateVisualLayer("Gravity Compression Haze", 1, new Color(0.22f, 0.12f, 0.55f, 0.08f));
        lowerFillRenderer = CreateVisualLayer("Deep Gravity Pressure Field", 1, new Color(0.04f, 0.02f, 0.16f, 0.18f));
        topGlowRenderer = CreateVisualLayer("Compression Wave Front Glow", 5, new Color(0.52f, 0.9f, 1f, 0.3f));
        horizonCoreRenderer = CreateVisualLayer("Compression Wave Front Core", 6, new Color(0.84f, 0.58f, 1f, 0.48f));

        compressionWaveRenderers = new SpriteRenderer[CompressionWaveCount];
        for (var i = 0; i < CompressionWaveCount; i++)
        {
            compressionWaveRenderers[i] = CreateVisualLayer($"Gravity Compression Ripple {i + 1}", 3, new Color(0.55f, 0.38f, 1f, 0.08f));
        }

        SetPositionImmediately();
        UpdateVisual();
    }

    public bool ApplyMergeRelief(int mergedLevel)
    {
        var relief = GetMergeRelief(mergedLevel);
        var nextY = Mathf.Max(startY, currentY - relief);
        if (nextY >= currentY - 0.001f)
        {
            return false;
        }

        reliefStartY = currentY;
        reliefTargetY = nextY;
        reliefAnimationElapsed = 0f;
        reliefAnimating = true;
        reliefGlowUntil = Time.time + reliefGlowSeconds;
        return true;
    }

    private void FixedUpdate()
    {
        if (controller == null || controller.IsGameOver)
        {
            return;
        }

        elapsed += Time.fixedDeltaTime;
        if (elapsed < initialDelaySeconds)
        {
            return;
        }

        if (reliefAnimating)
        {
            reliefAnimationElapsed += Time.fixedDeltaTime;
            var t = reliefAnimationSeconds <= 0f ? 1f : Mathf.Clamp01(reliefAnimationElapsed / reliefAnimationSeconds);
            currentY = Mathf.Lerp(reliefStartY, reliefTargetY, Mathf.SmoothStep(0f, 1f, t));
            if (t >= 1f)
            {
                reliefAnimating = false;
                currentY = reliefTargetY;
            }

            MoveWithPhysics();
            UpdateVisual();
            return;
        }

        var activeSeconds = elapsed - initialDelaySeconds;
        var riseSpeed = Mathf.Min(baseRiseSpeed + activeSeconds * RiseAcceleration, maxRiseSpeed);
        currentY += riseSpeed * Time.fixedDeltaTime;

        MoveWithPhysics();
        UpdateVisual();

        if (currentY >= dangerY)
        {
            controller.TriggerGameOver();
        }
    }

    private void SetPositionImmediately()
    {
        var position = new Vector2(0f, currentY);
        transform.position = position;
        if (body != null)
        {
            body.position = position;
        }
    }

    private void MoveWithPhysics()
    {
        var position = new Vector2(0f, currentY);
        if (body != null)
        {
            body.MovePosition(position);
        }
        else
        {
            transform.position = position;
        }
    }

    private void UpdateVisual()
    {
        transform.localScale = new Vector3(width, SurfaceThickness, 1f);

        var fillHeight = Mathf.Max(0.02f, currentY - bottomY);
        var dangerT = Mathf.InverseLerp(bottomY, dangerY, currentY);
        var pulse = Mathf.Sin(Time.time * Mathf.Lerp(2.4f, 4.8f, dangerT)) * 0.5f + 0.5f;
        var threatPulse = Mathf.Sin(Time.time * Mathf.Lerp(4.2f, 7.4f, dangerT)) * 0.5f + 0.5f;
        var reliefGlow = Mathf.Clamp01((reliefGlowUntil - Time.time) / Mathf.Max(0.01f, reliefGlowSeconds));

        fillRenderer.transform.position = new Vector3(0f, bottomY + fillHeight * 0.5f, 0f);
        fillRenderer.transform.localScale = new Vector3(width, fillHeight, 1f);

        var lowerHeight = Mathf.Max(0.02f, fillHeight * 0.58f);
        lowerFillRenderer.transform.position = new Vector3(0f, bottomY + lowerHeight * 0.5f, 0f);
        lowerFillRenderer.transform.localScale = new Vector3(width, lowerHeight, 1f);

        topGlowRenderer.transform.position = new Vector3(0f, currentY + 0.035f, 0f);
        topGlowRenderer.transform.localScale = new Vector3(width, Mathf.Lerp(0.06f, 0.105f, dangerT + pulse * 0.08f) + reliefGlow * 0.065f, 1f);

        horizonCoreRenderer.transform.position = new Vector3(0f, currentY + 0.055f, 0f);
        horizonCoreRenderer.transform.localScale = new Vector3(width * (Mathf.Lerp(0.92f, 1.02f, pulse) + reliefGlow * 0.04f), 0.022f + reliefGlow * 0.018f, 1f);

        for (var i = 0; i < compressionWaveRenderers.Length; i++)
        {
            var wave = compressionWaveRenderers[i];
            var waveT = (i + 1f) / (compressionWaveRenderers.Length + 1f);
            var waveY = Mathf.Lerp(bottomY + 0.12f, currentY - 0.1f, waveT);
            var wavePulse = Mathf.Sin(Time.time * 2.6f + i * 1.4f) * 0.5f + 0.5f;
            wave.transform.position = new Vector3(0f, waveY, 0f);
            wave.transform.localScale = new Vector3(width * Mathf.Lerp(0.68f, 0.96f, wavePulse), Mathf.Lerp(0.012f, 0.026f, dangerT), 1f);
            wave.color = Color.Lerp(
                new Color(0.52f, 0.32f, 1f, 0.035f + wavePulse * 0.025f),
                new Color(1f, 0.26f, 0.16f, 0.08f + wavePulse * 0.055f),
                dangerT);
        }

        fillRenderer.color = Color.Lerp(
            new Color(0.2f, 0.12f, 0.58f, 0.055f),
            new Color(0.72f, 0.12f, 0.08f, 0.13f),
            dangerT);
        lowerFillRenderer.color = Color.Lerp(
            new Color(0.035f, 0.018f, 0.14f, 0.16f),
            new Color(0.34f, 0.025f, 0.04f, 0.24f),
            dangerT);
        var topGlowColor = Color.Lerp(
            new Color(0.52f, 0.9f, 1f, 0.24f + pulse * 0.08f),
            new Color(1f, 0.32f, 0.18f, 0.38f + threatPulse * 0.14f),
            dangerT);
        topGlowColor = Color.Lerp(topGlowColor, new Color(0.66f, 1f, 0.88f, 0.58f), reliefGlow);
        topGlowRenderer.color = topGlowColor;

        var horizonCoreColor = Color.Lerp(
            new Color(0.84f, 0.58f, 1f, 0.42f + pulse * 0.12f),
            new Color(1f, 0.72f, 0.36f, 0.6f + threatPulse * 0.16f),
            dangerT);
        horizonCoreColor = Color.Lerp(horizonCoreColor, new Color(0.92f, 1f, 0.72f, 0.72f), reliefGlow);
        horizonCoreRenderer.color = horizonCoreColor;
        surfaceRenderer.color = Color.Lerp(
            new Color(0.42f, 0.22f, 0.84f, 0.26f),
            new Color(1f, 0.18f, 0.12f, 0.46f),
            dangerT);
    }

    private float GetMergeRelief(int mergedLevel)
    {
        var currentRelief = Mathf.Clamp(mergeReliefBase + mergedLevel * mergeReliefPerLevel, minMergeRelief, maxMergeRelief);
        return currentRelief * Mathf.Max(0f, mergeReliefMultiplier);
    }

    private static SpriteRenderer CreateVisualLayer(string layerName, int sortingOrder, Color color)
    {
        var layer = new GameObject(layerName);
        var renderer = layer.AddComponent<SpriteRenderer>();
        renderer.sprite = CircleSpriteCache.Square;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }
}
