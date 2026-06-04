using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public sealed class PressureFloor : MonoBehaviour
{
    private const float InitialDelaySeconds = 8f;
    private const float BaseRiseSpeed = 0.038f;
    private const float RiseAcceleration = 0.00004f;
    private const float MaxRiseSpeed = 0.065f;
    private const float SurfaceThickness = 0.16f;

    private GameController controller;
    private Rigidbody2D body;
    private BoxCollider2D surfaceCollider;
    private SpriteRenderer surfaceRenderer;
    private SpriteRenderer fillRenderer;
    private SpriteRenderer lowerFillRenderer;
    private SpriteRenderer topGlowRenderer;
    private float startY;
    private float currentY;
    private float bottomY;
    private float dangerY;
    private float width;
    private float elapsed;

    public void Initialize(GameController gameController, ContainerBounds bounds)
    {
        controller = gameController;
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
        surfaceRenderer.color = new Color(0.24f, 0.76f, 1f, 0.72f);
        surfaceRenderer.sortingOrder = 4;

        var fill = new GameObject("Pressure Fill");
        fillRenderer = fill.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = CircleSpriteCache.Square;
        fillRenderer.color = new Color(0.12f, 0.55f, 0.95f, 0.12f);
        fillRenderer.sortingOrder = 1;

        var lowerFill = new GameObject("Pressure Lower Field");
        lowerFillRenderer = lowerFill.AddComponent<SpriteRenderer>();
        lowerFillRenderer.sprite = CircleSpriteCache.Square;
        lowerFillRenderer.color = new Color(0.04f, 0.28f, 0.72f, 0.16f);
        lowerFillRenderer.sortingOrder = 1;

        var topGlow = new GameObject("Pressure Energy Wave");
        topGlowRenderer = topGlow.AddComponent<SpriteRenderer>();
        topGlowRenderer.sprite = CircleSpriteCache.Square;
        topGlowRenderer.color = new Color(0.32f, 0.9f, 1f, 0.36f);
        topGlowRenderer.sortingOrder = 5;

        SetPositionImmediately();
        UpdateVisual();
    }

    public void ApplyMergeRelief(int mergedLevel)
    {
        var relief = Mathf.Clamp(0.05f + mergedLevel * 0.012f, 0.07f, 0.18f);
        currentY = Mathf.Max(startY, currentY - relief);
        SetPositionImmediately();
        UpdateVisual();
    }

    private void FixedUpdate()
    {
        if (controller == null || controller.IsGameOver)
        {
            return;
        }

        elapsed += Time.fixedDeltaTime;
        if (elapsed < InitialDelaySeconds)
        {
            return;
        }

        var activeSeconds = elapsed - InitialDelaySeconds;
        var riseSpeed = Mathf.Min(BaseRiseSpeed + activeSeconds * RiseAcceleration, MaxRiseSpeed);
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
        fillRenderer.transform.position = new Vector3(0f, bottomY + fillHeight * 0.5f, 0f);
        fillRenderer.transform.localScale = new Vector3(width, fillHeight, 1f);

        var dangerT = Mathf.InverseLerp(bottomY, dangerY, currentY);
        var lowerHeight = Mathf.Max(0.02f, fillHeight * 0.58f);
        lowerFillRenderer.transform.position = new Vector3(0f, bottomY + lowerHeight * 0.5f, 0f);
        lowerFillRenderer.transform.localScale = new Vector3(width, lowerHeight, 1f);

        topGlowRenderer.transform.position = new Vector3(0f, currentY + 0.035f, 0f);
        topGlowRenderer.transform.localScale = new Vector3(width, 0.065f, 1f);

        fillRenderer.color = Color.Lerp(
            new Color(0.08f, 0.45f, 0.9f, 0.09f),
            new Color(0.95f, 0.18f, 0.12f, 0.17f),
            dangerT);
        lowerFillRenderer.color = Color.Lerp(
            new Color(0.03f, 0.22f, 0.62f, 0.14f),
            new Color(0.72f, 0.08f, 0.08f, 0.24f),
            dangerT);
        topGlowRenderer.color = Color.Lerp(
            new Color(0.32f, 0.9f, 1f, 0.34f),
            new Color(1f, 0.32f, 0.2f, 0.46f),
            dangerT);
        surfaceRenderer.color = Color.Lerp(
            new Color(0.24f, 0.76f, 1f, 0.46f),
            new Color(1f, 0.25f, 0.16f, 0.68f),
            dangerT);
    }
}
