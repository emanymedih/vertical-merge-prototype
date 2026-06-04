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
    private int nextLevel;
    private bool isDragging;
    private float nextDropTime;
    private Ball lastDroppedBall;
    private Vector2 spawnPosition;

    public int NextLevel => nextLevel;

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
        GenerateNextBall();
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
        lastDroppedBall = controller.SpawnBall(nextLevel, spawnPosition);
        lastDroppedBall.PlayPop();
        SoundManager.Play(SoundEvent.Drop);
        OnboardingController.Instance?.RegisterDrop();
        nextDropTime = Time.time + MinimumDropInterval;
        GenerateNextBall();
    }

    private void GenerateNextBall()
    {
        nextLevel = controller.GetNextSpawnLevel();
        spawnPosition.x = ClampSpawnX(spawnPosition.x);
        if (GameUi.Instance != null)
        {
            GameUi.Instance.SetNextBall(nextLevel);
        }
    }

    private void UpdatePreview()
    {
        SetPreviewVisible(true);
        var canDrop = CanDrop();
        previewRenderer.transform.position = spawnPosition;
        previewRenderer.transform.localScale = Vector3.one * Ball.GetDiameter(nextLevel);
        var color = CircleSpriteCache.GetBallColor(nextLevel);
        color.a = canDrop ? (isDragging ? 0.82f : 0.58f) : 0.3f;
        previewRenderer.color = color;

        previewGlowRenderer.transform.localScale = Vector3.one * 1.22f;
        var glowColor = CosmicBodyConfig.GetGlowColor(nextLevel);
        glowColor.a = canDrop ? (isDragging ? 0.34f : 0.16f) : 0.06f;
        previewGlowRenderer.color = glowColor;

        var guideHeight = Mathf.Max(0.1f, spawnPosition.y - guideBottomY);
        guideRenderer.transform.position = new Vector3(spawnPosition.x, guideBottomY + guideHeight * 0.5f, 0f);
        guideRenderer.transform.localScale = new Vector3(0.022f, guideHeight, 1f);
        var guideAlpha = canDrop ? (isDragging ? 0.42f : 0.16f) : 0.06f;
        guideRenderer.color = new Color(dropGuideColor.r, dropGuideColor.g, dropGuideColor.b, guideAlpha);

        guideGlowRenderer.color = new Color(dropGuideColor.r, dropGuideColor.g, dropGuideColor.b, guideAlpha * 0.32f);
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
        var radius = BallConfig.GetRadius(nextLevel);
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
        if (!isDragging || !CanDrop())
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
            var anticipationRange = BallConfig.GetRadius(nextLevel) + ball.Radius + anticipationHorizontalPadding;
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
        return ball != null && !ball.IsMerging && ball.Level == nextLevel;
    }
}
