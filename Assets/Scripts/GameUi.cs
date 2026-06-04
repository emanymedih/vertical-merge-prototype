using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class GameUi : MonoBehaviour
{
    [SerializeField] private float defaultToastDuration = 1.15f;
    [SerializeField] private float discoveryToastDuration = 1.7f;
    [SerializeField] private float highLevelDiscoveryToastDuration = 2f;

    private Text scoreText;
    private Text bestText;
    private Text largestText;
    private Text goalText;
    private Text gameOverScoreText;
    private Text toastText;
    private Image nextBallImage;
    private Image toastBackground;
    private GameObject gameOverPanel;
    private GameObject toastObject;
    private Coroutine toastRoutine;

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

    public void SetLargestLevel(int level)
    {
        largestText.text = $"Largest: {CosmicBodyConfig.GetShortName(level)}";
    }

    public void SetGoalLevel(int level, bool alreadyDiscovered)
    {
        var verb = alreadyDiscovered ? "Create" : "Discover";
        goalText.text = $"Goal: {verb} {CosmicBodyConfig.GetShortName(level)}";
    }

    public void SetNextBall(int level)
    {
        nextBallImage.sprite = CircleSpriteCache.Circle;
        nextBallImage.color = CircleSpriteCache.GetBallColor(level);
        nextBallImage.transform.localScale = Vector3.one * Mathf.Lerp(0.72f, 1.15f, Mathf.Clamp01((level - 1) / 4f));
    }

    public void ShowDiscoveryToast(int level)
    {
        var color = level >= 6
            ? new Color(0.18f, 0.12f, 0.04f, 0.9f)
            : new Color(0.07f, 0.09f, 0.11f, 0.86f);
        ShowToast($"Discovered: {CosmicBodyConfig.GetShortName(level)}", level >= 6 ? highLevelDiscoveryToastDuration : discoveryToastDuration, color);
    }

    public void ShowMomentMessage(string message)
    {
        ShowToast(message, defaultToastDuration, new Color(0.07f, 0.09f, 0.11f, 0.86f));
    }

    public void ShowGameOver(int score, int bestScore, int largestLevel, int bestLargestLevel, string motivationLine)
    {
        gameOverScoreText.text =
            $"Score: {score}\n" +
            $"Best: {bestScore}\n\n" +
            $"Largest: {CosmicBodyConfig.GetShortName(largestLevel)}\n" +
            $"Best Largest: {CosmicBodyConfig.GetShortName(bestLargestLevel)}\n\n" +
            motivationLine;
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
        largestText = CreateText(canvasObject.transform, "Largest", new Vector2(40f, -94f), TextAnchor.UpperLeft, 30, new Color(0.86f, 0.9f, 0.96f));
        bestText = CreateText(canvasObject.transform, "Best", new Vector2(-40f, -34f), TextAnchor.UpperRight, 34, new Color(0.86f, 0.9f, 0.96f));
        goalText = CreateText(canvasObject.transform, "Goal", new Vector2(0f, -198f), TextAnchor.UpperCenter, 34, new Color(1f, 0.92f, 0.58f));
        largestText.rectTransform.sizeDelta = new Vector2(460f, 54f);
        goalText.rectTransform.sizeDelta = new Vector2(660f, 58f);

        CreateNextPreview(canvasObject.transform);
        CreateDiscoveryToast(canvasObject.transform);
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

        var title = CreateText(gameOverPanel.transform, "Game Over Title", new Vector2(0f, 260f), TextAnchor.MiddleCenter, 76, Color.white);
        title.text = "Game Over";
        title.rectTransform.sizeDelta = new Vector2(760f, 110f);

        gameOverScoreText = CreateText(gameOverPanel.transform, "Game Over Score", new Vector2(0f, 20f), TextAnchor.MiddleCenter, 36, new Color(0.9f, 0.94f, 1f));
        gameOverScoreText.rectTransform.sizeDelta = new Vector2(760f, 390f);
        gameOverScoreText.lineSpacing = 1.08f;

        var buttonObject = new GameObject("Play Again Button");
        buttonObject.transform.SetParent(gameOverPanel.transform);
        var buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -270f);
        buttonRect.sizeDelta = new Vector2(380f, 96f);

        var buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.18f, 0.58f, 0.92f);

        var button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(controller.Restart);

        var buttonText = CreateText(buttonObject.transform, "Play Again Text", Vector2.zero, TextAnchor.MiddleCenter, 38, Color.white);
        buttonText.text = "Play Again";
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;

        gameOverPanel.SetActive(false);
    }

    private void CreateDiscoveryToast(Transform canvas)
    {
        toastObject = new GameObject("Discovery Toast");
        toastObject.transform.SetParent(canvas);

        var toastRect = toastObject.AddComponent<RectTransform>();
        toastRect.anchorMin = new Vector2(0.5f, 1f);
        toastRect.anchorMax = new Vector2(0.5f, 1f);
        toastRect.anchoredPosition = new Vector2(0f, -285f);
        toastRect.sizeDelta = new Vector2(620f, 74f);

        toastBackground = toastObject.AddComponent<Image>();
        toastBackground.color = new Color(0.07f, 0.09f, 0.11f, 0.86f);

        toastText = CreateText(toastObject.transform, "Discovery Toast Text", Vector2.zero, TextAnchor.MiddleCenter, 30, Color.white);
        toastText.rectTransform.anchorMin = Vector2.zero;
        toastText.rectTransform.anchorMax = Vector2.one;
        toastText.rectTransform.offsetMin = Vector2.zero;
        toastText.rectTransform.offsetMax = Vector2.zero;

        toastObject.SetActive(false);
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

    private void ShowToast(string message, float duration, Color backgroundColor)
    {
        if (toastRoutine != null)
        {
            StopCoroutine(toastRoutine);
        }

        toastRoutine = StartCoroutine(ToastRoutine(message, duration, backgroundColor));
    }

    private IEnumerator ToastRoutine(string message, float duration, Color backgroundColor)
    {
        toastText.text = message;
        toastBackground.color = backgroundColor;
        toastObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        toastObject.SetActive(false);
        toastRoutine = null;
    }
}
