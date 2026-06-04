using UnityEngine;

public sealed class BallSpawner : MonoBehaviour
{
    private Camera gameCamera;
    private GameController controller;
    private SpriteRenderer previewRenderer;
    private float leftLimit;
    private float rightLimit;
    private float spawnY;
    private int nextLevel;
    private bool isDragging;
    private Vector2 spawnPosition;

    public int NextLevel => nextLevel;

    public void Initialize(Camera cameraToUse, GameController gameController, float leftWall, float rightWall, float topY)
    {
        gameCamera = cameraToUse;
        controller = gameController;
        leftLimit = leftWall + 0.6f;
        rightLimit = rightWall - 0.6f;
        spawnY = topY;

        var preview = new GameObject("Next Ball World Preview");
        preview.transform.SetParent(transform);
        previewRenderer = preview.AddComponent<SpriteRenderer>();
        previewRenderer.sprite = CircleSpriteCache.Circle;
        previewRenderer.sortingOrder = 20;

        spawnPosition = new Vector2(0f, spawnY);
        GenerateNextBall();
        UpdatePreview();
    }

    private void Update()
    {
        if (controller == null || controller.IsGameOver)
        {
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
            spawnPosition.x = Mathf.Clamp(worldPosition.x, leftLimit, rightLimit);
        }

        if (pressed)
        {
            isDragging = true;
        }

        if (isDragging && released)
        {
            DropCurrentBall();
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
        GenerateNextBall();
    }

    private void GenerateNextBall()
    {
        nextLevel = Random.Range(1, 4);
        if (GameUi.Instance != null)
        {
            GameUi.Instance.SetNextBall(nextLevel);
        }
    }

    private void UpdatePreview()
    {
        previewRenderer.transform.position = spawnPosition;
        previewRenderer.transform.localScale = Vector3.one * Ball.GetDiameter(nextLevel);
        var color = CircleSpriteCache.GetBallColor(nextLevel);
        color.a = 0.72f;
        previewRenderer.color = color;
    }
}
