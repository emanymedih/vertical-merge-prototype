using UnityEngine;

public sealed class BallSpawner : MonoBehaviour
{
    private Camera gameCamera;
    private GameController controller;
    private SpriteRenderer previewRenderer;
    private SpriteRenderer guideRenderer;
    private float leftWall;
    private float rightWall;
    private float spawnY;
    private float guideBottomY;
    private int nextLevel;
    private bool isDragging;
    private float nextDropTime;
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

        var guide = new GameObject("Drop Guide");
        guide.transform.SetParent(transform);
        guideRenderer = guide.AddComponent<SpriteRenderer>();
        guideRenderer.sprite = CircleSpriteCache.Square;
        guideRenderer.sortingOrder = 4;

        spawnPosition = new Vector2(0f, spawnY);
        GenerateNextBall();
        UpdatePreview();
    }

    private void Update()
    {
        if (controller == null || controller.IsGameOver)
        {
            SetPreviewVisible(false);
            return;
        }

        HandlePointer();
        UpdatePreview();
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
        }

        if (isDragging && released)
        {
            if (Time.time >= nextDropTime)
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
        var ball = controller.SpawnBall(nextLevel, spawnPosition);
        ball.PlayPop();
        nextDropTime = Time.time + 0.25f;
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
        previewRenderer.transform.position = spawnPosition;
        previewRenderer.transform.localScale = Vector3.one * Ball.GetDiameter(nextLevel);
        var color = CircleSpriteCache.GetBallColor(nextLevel);
        color.a = 0.72f;
        previewRenderer.color = color;

        var guideHeight = Mathf.Max(0.1f, spawnPosition.y - guideBottomY);
        guideRenderer.transform.position = new Vector3(spawnPosition.x, guideBottomY + guideHeight * 0.5f, 0f);
        guideRenderer.transform.localScale = new Vector3(0.035f, guideHeight, 1f);
        guideRenderer.color = new Color(color.r, color.g, color.b, isDragging ? 0.35f : 0.18f);
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

        if (guideRenderer != null)
        {
            guideRenderer.enabled = visible;
        }
    }
}
