using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public sealed class PressureFloor : MonoBehaviour
{
    private const float InitialDelaySeconds = 8f;
    private const float BaseRiseSpeed = 0.026f;
    private const float RiseAcceleration = 0.000025f;
    private const float MaxRiseSpeed = 0.045f;
    private const float SurfaceThickness = 0.16f;

    private GameController controller;
    private Rigidbody2D body;
    private BoxCollider2D surfaceCollider;
    private SpriteRenderer surfaceRenderer;
    private SpriteRenderer fillRenderer;
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
        fillRenderer.color = new Color(0.12f, 0.55f, 0.95f, 0.24f);
        fillRenderer.sortingOrder = 1;

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
        surfaceRenderer.color = Color.Lerp(
            new Color(0.24f, 0.76f, 1f, 0.62f),
            new Color(1f, 0.25f, 0.16f, 0.86f),
            dangerT);
    }
}
