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
    private Text gameOverCreatedText;
    private Text gameOverScoreText;
    private Text gameOverRecordText;
    private Text toastText;
    private Image nextBallImage;
    private Image nextBallGlow;
    private Image nextBallFrame;
    private Image goalBackground;
    private Image gameOverLargestImage;
    private Image gameOverLargestGlow;
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
        scoreText.text = $"Score\n{score}";
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
        var goalColor = CosmicBodyConfig.GetGlowColor(level);
        goalColor.a = 0.28f;
        if (goalBackground != null)
        {
            goalBackground.color = goalColor;
        }
    }

    public void SetNextBall(int level)
    {
        nextBallImage.sprite = CircleSpriteCache.Circle;
        nextBallImage.color = CircleSpriteCache.GetBallColor(level);
        nextBallImage.transform.localScale = Vector3.one * Mathf.Lerp(0.84f, 1.18f, Mathf.Clamp01((level - 1) / 4f));

        var glowColor = CosmicBodyConfig.GetGlowColor(level);
        glowColor.a = 0.32f;
        nextBallGlow.color = glowColor;
        nextBallFrame.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.28f);
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

    public void ShowGameOver(int score, int bestScore, int largestLevel, int bestLargestLevel, bool newBestScore, bool newBestLargest, string motivationLine)
    {
        var largestName = CosmicBodyConfig.GetShortName(largestLevel);
        var largestColor = CosmicBodyConfig.GetBaseColor(largestLevel);
        var glowColor = CosmicBodyConfig.GetGlowColor(largestLevel);
        glowColor.a = 0.34f;

        gameOverCreatedText.text = $"You created:\n{largestName}";
        gameOverLargestImage.color = largestColor;
        gameOverLargestGlow.color = glowColor;
        gameOverScoreText.text =
            $"Score: {score}\n" +
            $"Best: {bestScore}\n\n" +
            $"Largest: {CosmicBodyConfig.GetShortName(largestLevel)}\n" +
            $"Best Largest: {CosmicBodyConfig.GetShortName(bestLargestLevel)}";
        gameOverRecordText.text = GetGameOverRecordLine(newBestScore, newBestLargest, motivationLine);
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

        CreateGoalPanel(canvasObject.transform);
        scoreText = CreateText(canvasObject.transform, "Score", new Vector2(34f, -32f), TextAnchor.UpperLeft, 34, Color.white);
        scoreText.rectTransform.sizeDelta = new Vector2(230f, 92f);
        scoreText.lineSpacing = 0.86f;

        largestText = CreateText(canvasObject.transform, "Largest", new Vector2(34f, -128f), TextAnchor.UpperLeft, 26, new Color(0.78f, 0.84f, 0.92f, 0.92f));
        largestText.rectTransform.sizeDelta = new Vector2(360f, 46f);

        bestText = CreateText(canvasObject.transform, "Best", new Vector2(-34f, -32f), TextAnchor.UpperRight, 28, new Color(0.78f, 0.84f, 0.92f, 0.9f));
        bestText.rectTransform.sizeDelta = new Vector2(280f, 52f);

        CreateNextPreview(canvasObject.transform);
        CreateDiscoveryToast(canvasObject.transform);
        CreateGameOverPanel(canvasObject.transform, controller);
    }

    private void CreateGoalPanel(Transform canvas)
    {
        var goalObject = new GameObject("Goal Panel");
        goalObject.transform.SetParent(canvas);

        var rect = goalObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -204f);
        rect.sizeDelta = new Vector2(690f, 74f);

        goalBackground = goalObject.AddComponent<Image>();
        goalBackground.sprite = CircleSpriteCache.Square;
        goalBackground.color = new Color(1f, 0.84f, 0.36f, 0.16f);
        goalBackground.raycastTarget = false;

        goalText = CreateText(goalObject.transform, "Goal", Vector2.zero, TextAnchor.MiddleCenter, 34, new Color(1f, 0.94f, 0.68f));
        goalText.rectTransform.anchorMin = Vector2.zero;
        goalText.rectTransform.anchorMax = Vector2.one;
        goalText.rectTransform.offsetMin = Vector2.zero;
        goalText.rectTransform.offsetMax = Vector2.zero;
        goalText.rectTransform.sizeDelta = Vector2.zero;
    }

    private void CreateNextPreview(Transform canvas)
    {
        var label = CreateText(canvas, "Next Label", new Vector2(0f, -34f), TextAnchor.UpperCenter, 26, new Color(0.78f, 0.84f, 0.92f, 0.92f));
        label.text = "Next Ball";
        label.rectTransform.sizeDelta = new Vector2(220f, 40f);

        var previewRoot = new GameObject("Next Ball Preview Root");
        previewRoot.transform.SetParent(canvas);
        var rootRect = previewRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0f, -112f);
        rootRect.sizeDelta = new Vector2(122f, 122f);

        var glowObject = new GameObject("Next Ball Glow");
        glowObject.transform.SetParent(previewRoot.transform);
        var glowRect = glowObject.AddComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.offsetMin = new Vector2(-16f, -16f);
        glowRect.offsetMax = new Vector2(16f, 16f);
        nextBallGlow = glowObject.AddComponent<Image>();
        nextBallGlow.sprite = CircleSpriteCache.Circle;
        nextBallGlow.color = new Color(0.4f, 0.9f, 1f, 0.24f);
        nextBallGlow.raycastTarget = false;

        var frameObject = new GameObject("Next Ball Frame");
        frameObject.transform.SetParent(previewRoot.transform);
        var frameRect = frameObject.AddComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.offsetMin = new Vector2(-4f, -4f);
        frameRect.offsetMax = new Vector2(4f, 4f);
        nextBallFrame = frameObject.AddComponent<Image>();
        nextBallFrame.sprite = CircleSpriteCache.Circle;
        nextBallFrame.color = new Color(0.58f, 0.92f, 1f, 0.18f);
        nextBallFrame.raycastTarget = false;

        var previewObject = new GameObject("Next Ball Preview");
        previewObject.transform.SetParent(canvas);
        var rect = previewObject.AddComponent<RectTransform>();
        rect.anchorMin = rootRect.anchorMin;
        rect.anchorMax = rootRect.anchorMax;
        rect.anchoredPosition = rootRect.anchoredPosition;
        rect.sizeDelta = new Vector2(98f, 98f);

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
        background.color = new Color(0f, 0f, 0f, 0.72f);

        var title = CreateText(gameOverPanel.transform, "Game Over Title", new Vector2(0f, 420f), TextAnchor.MiddleCenter, 56, new Color(0.82f, 0.88f, 0.98f, 0.9f));
        title.text = "Game Over";
        title.rectTransform.sizeDelta = new Vector2(760f, 82f);

        CreateGameOverLargestPreview(gameOverPanel.transform);

        gameOverCreatedText = CreateText(gameOverPanel.transform, "Game Over Created", new Vector2(0f, 210f), TextAnchor.MiddleCenter, 48, Color.white);
        gameOverCreatedText.rectTransform.sizeDelta = new Vector2(780f, 150f);
        gameOverCreatedText.lineSpacing = 0.9f;

        gameOverScoreText = CreateText(gameOverPanel.transform, "Game Over Score", new Vector2(0f, -26f), TextAnchor.MiddleCenter, 34, new Color(0.9f, 0.94f, 1f));
        gameOverScoreText.rectTransform.sizeDelta = new Vector2(760f, 260f);
        gameOverScoreText.lineSpacing = 1.02f;

        gameOverRecordText = CreateText(gameOverPanel.transform, "Game Over Record", new Vector2(0f, -198f), TextAnchor.MiddleCenter, 34, new Color(1f, 0.84f, 0.5f));
        gameOverRecordText.rectTransform.sizeDelta = new Vector2(760f, 78f);

        var buttonObject = new GameObject("Play Again Button");
        buttonObject.transform.SetParent(gameOverPanel.transform);
        var buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -350f);
        buttonRect.sizeDelta = new Vector2(440f, 110f);

        var buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.12f, 0.64f, 1f, 0.95f);

        var button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(() =>
        {
            SoundManager.Play(SoundEvent.ButtonClick);
            controller.Restart();
        });

        var buttonText = CreateText(buttonObject.transform, "Play Again Text", Vector2.zero, TextAnchor.MiddleCenter, 38, Color.white);
        buttonText.text = "Play Again";
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;

        gameOverPanel.SetActive(false);
    }

    private void CreateGameOverLargestPreview(Transform parent)
    {
        var glowObject = new GameObject("Game Over Largest Glow");
        glowObject.transform.SetParent(parent);
        var glowRect = glowObject.AddComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0.5f, 0.5f);
        glowRect.anchorMax = new Vector2(0.5f, 0.5f);
        glowRect.anchoredPosition = new Vector2(0f, 318f);
        glowRect.sizeDelta = new Vector2(178f, 178f);
        gameOverLargestGlow = glowObject.AddComponent<Image>();
        gameOverLargestGlow.sprite = CircleSpriteCache.Circle;
        gameOverLargestGlow.raycastTarget = false;

        var bodyObject = new GameObject("Game Over Largest Body");
        bodyObject.transform.SetParent(parent);
        var bodyRect = bodyObject.AddComponent<RectTransform>();
        bodyRect.anchorMin = glowRect.anchorMin;
        bodyRect.anchorMax = glowRect.anchorMax;
        bodyRect.anchoredPosition = glowRect.anchoredPosition;
        bodyRect.sizeDelta = new Vector2(118f, 118f);
        gameOverLargestImage = bodyObject.AddComponent<Image>();
        gameOverLargestImage.sprite = CircleSpriteCache.Circle;
        gameOverLargestImage.raycastTarget = false;
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

    private static string GetGameOverRecordLine(bool newBestScore, bool newBestLargest, string fallback)
    {
        if (newBestLargest)
        {
            return "New Largest Ball Record!";
        }

        if (newBestScore)
        {
            return "New Best Score!";
        }

        return fallback;
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
