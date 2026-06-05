using UnityEngine;

public sealed class BallSpawner : MonoBehaviour
{
    private const float MinimumDropInterval = 0.45f;

    [SerializeField] private float resonanceStrength = 0.64f;
    [SerializeField] private float anticipationStrength = 1f;
    [SerializeField] private float anticipationHorizontalPadding = 0.35f;
    [SerializeField] private Color dropGuideColor = new Color(0.24f, 0.84f, 1f, 1f);

    private Camera gameCamera;
    private GameController controller;
    private SpriteRenderer previewRenderer;
    private SpriteRenderer previewGlowRenderer;
    private SpriteRenderer guideRenderer;
    private SpriteRenderer guideGlowRenderer;
    private float leftWall;
    private float rightWall;
    private float spawnY;
    private float guideBottomY;
    private SpawnPayload nextSpawn;
    private bool isDragging;
    private float nextDropTime;
    private float previewGeneratedAt;
    private Ball lastDroppedBall;
    private Vector2 spawnPosition;

    public int NextLevel => nextSpawn.Level;

    public void Initialize(Camera cameraToUse, GameController gameController, float leftWall, float rightWall, float bottomY, float topY)
    {
        gameCamera = cameraToUse;
        controller = gameController;
        this.leftWall = leftWall;
        this.rightWall = rightWall;
        spawnY = topY;
        guideBottomY = bottomY + 0.35f;

        var preview = new GameObject("Next Ball World Preview");
        preview.transform.SetParent(transform);
        previewRenderer = preview.AddComponent<SpriteRenderer>();
        previewRenderer.sprite = CircleSpriteCache.Circle;
        previewRenderer.sortingOrder = 20;

        var previewGlow = new GameObject("Ghost Ball Glow");
        previewGlow.transform.SetParent(preview.transform);
        previewGlow.transform.localPosition = Vector3.zero;
        previewGlow.transform.localRotation = Quaternion.identity;
        previewGlow.transform.localScale = Vector3.one * 1.22f;
        previewGlowRenderer = previewGlow.AddComponent<SpriteRenderer>();
        previewGlowRenderer.sprite = CircleSpriteCache.Circle;
        previewGlowRenderer.sortingOrder = 19;

        var guide = new GameObject("Drop Guide");
        guide.transform.SetParent(transform);
        guideRenderer = guide.AddComponent<SpriteRenderer>();
        guideRenderer.sprite = CircleSpriteCache.Square;
        guideRenderer.sortingOrder = 10;

        var guideGlow = new GameObject("Drop Guide Glow");
        guideGlow.transform.SetParent(guide.transform);
        guideGlow.transform.localPosition = Vector3.zero;
        guideGlow.transform.localRotation = Quaternion.identity;
        guideGlow.transform.localScale = new Vector3(3.4f, 1f, 1f);
        guideGlowRenderer = guideGlow.AddComponent<SpriteRenderer>();
        guideGlowRenderer.sprite = CircleSpriteCache.Square;
        guideGlowRenderer.sortingOrder = 9;

        spawnPosition = new Vector2(0f, spawnY);
        GenerateNextSpawn();
        UpdatePreview();
    }

    private void Update()
    {
        if (controller == null || controller.IsGameOver || controller.IsInputLocked)
        {
            SetPreviewVisible(false);
            ClearResonance();
            return;
        }

        HandlePointer();
        UpdatePreview();
        UpdateResonance();
    }

    private void HandlePointer()
    {
        if (TryGetPointer(out var screenPosition, out var pressed, out var released))
        {
            var worldPosition = gameCamera.ScreenToWorldPoint(screenPosition);
            spawnPosition.x = ClampSpawnX(worldPosition.x);
        }

        if (pressed)
        {
            isDragging = true;
            OnboardingController.Instance?.BeginAiming();
        }

        if (isDragging && released)
        {
            if (CanDrop())
            {
                DropCurrentBall();
            }

            isDragging = false;
        }
    }

    private bool TryGetPointer(out Vector2 screenPosition, out bool pressed, out bool released)
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            screenPosition = touch.position;
            pressed = touch.phase == TouchPhase.Began;
            released = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
            return true;
        }

        screenPosition = Input.mousePosition;
        pressed = Input.GetMouseButtonDown(0);
        released = Input.GetMouseButtonUp(0);
        return Input.GetMouseButton(0) || pressed || released;
    }

    private void DropCurrentBall()
    {
        ClearResonance();
        controller.BeginDropWindow();
        if (nextSpawn.IsComet)
        {
            controller.SpawnComet(spawnPosition);
            lastDroppedBall = null;
        }
        else
        {
            lastDroppedBall = controller.SpawnBall(nextSpawn.Level, spawnPosition);
            lastDroppedBall.PlayDropPop();
            lastDroppedBall.EnableIntentMagnetism();
        }

        SoundManager.Play(SoundEvent.Drop);
        OnboardingController.Instance?.RegisterDrop();
        nextDropTime = Time.time + MinimumDropInterval;
        GenerateNextSpawn();
    }

    private void GenerateNextSpawn()
    {
        nextSpawn = controller.GetNextSpawnPayload();
        previewGeneratedAt = Time.time;
        spawnPosition.x = ClampSpawnX(spawnPosition.x);
        if (GameUi.Instance != null)
        {
            GameUi.Instance.SetNextSpawn(nextSpawn);
        }
    }

    private void UpdatePreview()
    {
        SetPreviewVisible(true);
        var canDrop = CanDrop();
        var popInT = Mathf.Clamp01((Time.time - previewGeneratedAt) / 0.18f);
        var popIn = Mathf.Lerp(0.72f, 1f, AnimationEasing.EaseOutBack(popInT));
        var breathing = Mathf.Sin(Time.time * (isDragging ? 7.2f : 3.6f)) * 0.5f + 0.5f;
        var breathScale = canDrop ? Mathf.Lerp(1f, isDragging ? 1.055f : 1.025f, breathing) : 0.96f;
        previewRenderer.transform.position = spawnPosition;
        previewRenderer.transform.localScale = Vector3.one * nextSpawn.Diameter * popIn * breathScale;
        var color = nextSpawn.GetPreviewColor();
        color.a = canDrop ? (isDragging ? 0.82f : 0.58f) : 0.3f;
        previewRenderer.color = color;

        previewGlowRenderer.transform.localScale = Vector3.one * Mathf.Lerp(1.18f, isDragging ? 1.42f : 1.28f, breathing) * popIn;
        var glowColor = nextSpawn.GetGlowColor();
        glowColor.a = canDrop ? (isDragging ? Mathf.Lerp(0.22f, 0.42f, breathing) : Mathf.Lerp(0.12f, 0.2f, breathing)) : 0.06f;
        previewGlowRenderer.color = glowColor;

        var guideHeight = Mathf.Max(0.1f, spawnPosition.y - guideBottomY);
        guideRenderer.transform.position = new Vector3(spawnPosition.x, guideBottomY + guideHeight * 0.5f, 0f);
        guideRenderer.transform.localScale = new Vector3(Mathf.Lerp(0.018f, 0.028f, isDragging ? breathing : 0f), guideHeight, 1f);
        var guideAlpha = canDrop ? (isDragging ? Mathf.Lerp(0.28f, 0.48f, breathing) : 0.14f) : 0.06f;
        guideRenderer.color = new Color(dropGuideColor.r, dropGuideColor.g, dropGuideColor.b, guideAlpha);

        guideGlowRenderer.color = new Color(dropGuideColor.r, dropGuideColor.g, dropGuideColor.b, guideAlpha * (isDragging ? 0.44f : 0.24f));
    }

    private bool CanDrop()
    {
        if (Time.time < nextDropTime)
        {
            return false;
        }

        return lastDroppedBall == null || lastDroppedBall.IsReadyForNextDrop(spawnY);
    }

    private float ClampSpawnX(float worldX)
    {
        var radius = nextSpawn.Radius;
        return Mathf.Clamp(worldX, leftWall + radius, rightWall - radius);
    }

    private void SetPreviewVisible(bool visible)
    {
        if (previewRenderer != null)
        {
            previewRenderer.enabled = visible;
        }

        if (previewGlowRenderer != null)
        {
            previewGlowRenderer.enabled = visible;
        }

        if (guideRenderer != null)
        {
            guideRenderer.enabled = visible;
        }

        if (guideGlowRenderer != null)
        {
            guideGlowRenderer.enabled = visible;
        }
    }

    private void UpdateResonance()
    {
        if (!isDragging || !CanDrop() || nextSpawn.IsComet)
        {
            ClearResonance();
            return;
        }

        Ball emphasizedBall = null;
        var closestOffset = float.MaxValue;

        foreach (var ball in controller.ActiveBalls)
        {
            if (!CanResonate(ball))
            {
                continue;
            }

            var xOffset = Mathf.Abs(ball.transform.position.x - spawnPosition.x);
            var anticipationRange = nextSpawn.Radius + ball.Radius + anticipationHorizontalPadding;
            if (xOffset <= anticipationRange && xOffset < closestOffset)
            {
                closestOffset = xOffset;
                emphasizedBall = ball;
            }
        }

        foreach (var ball in controller.ActiveBalls)
        {
            if (!CanResonate(ball))
            {
                ball?.ClearResonance();
                continue;
            }

            var emphasized = ball == emphasizedBall;
            ball.SetResonance(emphasized ? anticipationStrength : resonanceStrength, emphasized);
        }
    }

    private void ClearResonance()
    {
        if (controller == null)
        {
            return;
        }

        foreach (var ball in controller.ActiveBalls)
        {
            if (ball != null)
            {
                ball.ClearResonance();
            }
        }
    }

    private bool CanResonate(Ball ball)
    {
        return ball != null && !ball.IsMerging && ball.Level == nextSpawn.Level;
    }
}
