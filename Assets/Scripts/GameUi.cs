using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class GameUi : MonoBehaviour
{
    private Text scoreText;
    private Text bestText;
    private Text gameOverScoreText;
    private Image nextBallImage;
    private GameObject gameOverPanel;

    public static GameUi Instance { get; private set; }

    public static GameUi Build(GameController controller)
    {
        var uiObject = new GameObject("Game UI");
        var ui = uiObject.AddComponent<GameUi>();
        ui.Create(controller);
        return ui;
    }

    public void SetScore(int score)
    {
        scoreText.text = $"Score {score}";
    }

    public void SetBestScore(int bestScore)
    {
        bestText.text = $"Best {bestScore}";
    }

    public void SetNextBall(int level)
    {
        nextBallImage.sprite = CircleSpriteCache.Circle;
        nextBallImage.color = CircleSpriteCache.GetBallColor(level);
        nextBallImage.transform.localScale = Vector3.one * Mathf.Lerp(0.72f, 1.15f, Mathf.Clamp01((level - 1) / 4f));
    }

    public void ShowGameOver(int score, int bestScore)
    {
        gameOverScoreText.text = $"Score {score}\nBest {bestScore}";
        gameOverPanel.SetActive(true);
    }

    private void Create(GameController controller)
    {
        Instance = this;

        var canvasObject = new GameObject("Canvas");
        canvasObject.transform.SetParent(transform);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<GraphicRaycaster>();

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 1f;

        EnsureEventSystem();

        scoreText = CreateText(canvasObject.transform, "Score", new Vector2(40f, -34f), TextAnchor.UpperLeft, 44, Color.white);
        bestText = CreateText(canvasObject.transform, "Best", new Vector2(-40f, -34f), TextAnchor.UpperRight, 34, new Color(0.86f, 0.9f, 0.96f));

        CreateNextPreview(canvasObject.transform);
        CreateGameOverPanel(canvasObject.transform, controller);
    }

    private void CreateNextPreview(Transform canvas)
    {
        var label = CreateText(canvas, "Next Label", new Vector2(0f, -34f), TextAnchor.UpperCenter, 30, new Color(0.86f, 0.9f, 0.96f));
        label.text = "Next";

        var previewObject = new GameObject("Next Ball Preview");
        previewObject.transform.SetParent(canvas);
        var rect = previewObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -105f);
        rect.sizeDelta = new Vector2(86f, 86f);

        nextBallImage = previewObject.AddComponent<Image>();
        nextBallImage.raycastTarget = false;
    }

    private void CreateGameOverPanel(Transform canvas, GameController controller)
    {
        gameOverPanel = new GameObject("Game Over Panel");
        gameOverPanel.transform.SetParent(canvas);

        var panelRect = gameOverPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var background = gameOverPanel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.68f);

        var title = CreateText(gameOverPanel.transform, "Game Over Title", new Vector2(0f, 170f), TextAnchor.MiddleCenter, 76, Color.white);
        title.text = "Game Over";

        gameOverScoreText = CreateText(gameOverPanel.transform, "Game Over Score", new Vector2(0f, 20f), TextAnchor.MiddleCenter, 40, new Color(0.9f, 0.94f, 1f));

        var buttonObject = new GameObject("Restart Button");
        buttonObject.transform.SetParent(gameOverPanel.transform);
        var buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -160f);
        buttonRect.sizeDelta = new Vector2(340f, 94f);

        var buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.18f, 0.58f, 0.92f);

        var button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(controller.Restart);

        var buttonText = CreateText(buttonObject.transform, "Restart Text", Vector2.zero, TextAnchor.MiddleCenter, 38, Color.white);
        buttonText.text = "Restart";
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;

        gameOverPanel.SetActive(false);
    }

    private Text CreateText(Transform parent, string name, Vector2 anchoredPosition, TextAnchor alignment, int fontSize, Color color)
    {
        var textObject = new GameObject(name);
        textObject.transform.SetParent(parent);

        var rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = GetAnchorForAlignment(alignment);
        rect.anchorMax = rect.anchorMin;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(460f, 110f);

        var text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private static Vector2 GetAnchorForAlignment(TextAnchor alignment)
    {
        switch (alignment)
        {
            case TextAnchor.UpperLeft:
                return new Vector2(0f, 1f);
            case TextAnchor.UpperCenter:
                return new Vector2(0.5f, 1f);
            case TextAnchor.UpperRight:
                return new Vector2(1f, 1f);
            case TextAnchor.MiddleLeft:
                return new Vector2(0f, 0.5f);
            case TextAnchor.MiddleRight:
                return new Vector2(1f, 0.5f);
            case TextAnchor.LowerLeft:
                return new Vector2(0f, 0f);
            case TextAnchor.LowerCenter:
                return new Vector2(0.5f, 0f);
            case TextAnchor.LowerRight:
                return new Vector2(1f, 0f);
            default:
                return new Vector2(0.5f, 0.5f);
        }
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }
}
