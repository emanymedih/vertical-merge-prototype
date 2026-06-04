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
    private Image goalBorder;
    private Image gameOverLargestImage;
    private Image gameOverLargestGlow;
    private Image playAgainButtonImage;
    private Image toastBackground;
    private GameObject gameOverPanel;
    private GameObject toastObject;
    private Transform gameOverLargestPreviewRoot;
    private Coroutine toastRoutine;
    private Coroutine playAgainPulseRoutine;

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
        goalText.text = $"NEXT GOAL\n{verb} {CosmicBodyConfig.GetShortName(level)}";
        var goalColor = CosmicBodyConfig.GetGlowColor(level);
        goalColor.a = 0.16f;
        if (goalBackground != null)
        {
            goalBackground.color = new Color(0.02f, 0.05f, 0.08f, 0.62f);
        }

        if (goalBorder != null)
        {
            goalBorder.color = new Color(goalColor.r, goalColor.g, goalColor.b, 0.26f);
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

        gameOverCreatedText.text = $"You created:\n{largestName}";
        BuildResultPreview(largestLevel);
        gameOverScoreText.text =
            $"Score: {score}\n" +
            $"Best: {bestScore}\n\n" +
            $"Largest: {CosmicBodyConfig.GetShortName(largestLevel)}\n" +
            $"Best Largest: {CosmicBodyConfig.GetShortName(bestLargestLevel)}";
        gameOverRecordText.text = GetGameOverRecordLine(newBestScore, newBestLargest, largestLevel, motivationLine);
        gameOverPanel.SetActive(true);

        if (playAgainPulseRoutine != null)
        {
            StopCoroutine(playAgainPulseRoutine);
        }

        playAgainPulseRoutine = StartCoroutine(PlayAgainPulseRoutine());
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
        rect.anchoredPosition = new Vector2(0f, -238f);
        rect.sizeDelta = new Vector2(560f, 82f);

        goalBackground = goalObject.AddComponent<Image>();
        goalBackground.sprite = CircleSpriteCache.Square;
        goalBackground.color = new Color(0.02f, 0.05f, 0.08f, 0.62f);
        goalBackground.raycastTarget = false;

        var borderObject = new GameObject("Goal Border Glow");
        borderObject.transform.SetParent(goalObject.transform);
        var borderRect = borderObject.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-3f, -3f);
        borderRect.offsetMax = new Vector2(3f, 3f);
        goalBorder = borderObject.AddComponent<Image>();
        goalBorder.sprite = CircleSpriteCache.Square;
        goalBorder.color = new Color(0.32f, 0.9f, 1f, 0.22f);
        goalBorder.raycastTarget = false;

        goalText = CreateText(goalObject.transform, "Goal", Vector2.zero, TextAnchor.MiddleCenter, 28, new Color(0.92f, 0.98f, 1f));
        goalText.rectTransform.anchorMin = Vector2.zero;
        goalText.rectTransform.anchorMax = Vector2.one;
        goalText.rectTransform.offsetMin = Vector2.zero;
        goalText.rectTransform.offsetMax = Vector2.zero;
        goalText.rectTransform.sizeDelta = Vector2.zero;
        goalText.lineSpacing = 0.82f;
    }

    private void CreateNextPreview(Transform canvas)
    {
        var panelObject = new GameObject("Next Ball Panel");
        panelObject.transform.SetParent(canvas);
        var panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -104f);
        panelRect.sizeDelta = new Vector2(176f, 174f);

        var panelBackground = panelObject.AddComponent<Image>();
        panelBackground.sprite = CircleSpriteCache.Square;
        panelBackground.color = new Color(0.015f, 0.04f, 0.07f, 0.58f);
        panelBackground.raycastTarget = false;

        var panelBorderObject = new GameObject("Next Panel Glow");
        panelBorderObject.transform.SetParent(panelObject.transform);
        var panelBorderRect = panelBorderObject.AddComponent<RectTransform>();
        panelBorderRect.anchorMin = Vector2.zero;
        panelBorderRect.anchorMax = Vector2.one;
        panelBorderRect.offsetMin = new Vector2(-3f, -3f);
        panelBorderRect.offsetMax = new Vector2(3f, 3f);
        var panelBorder = panelBorderObject.AddComponent<Image>();
        panelBorder.sprite = CircleSpriteCache.Square;
        panelBorder.color = new Color(0.28f, 0.86f, 1f, 0.18f);
        panelBorder.raycastTarget = false;

        var label = CreateText(panelObject.transform, "Next Label", new Vector2(0f, 58f), TextAnchor.MiddleCenter, 24, new Color(0.78f, 0.9f, 1f, 0.92f));
        label.text = "NEXT";
        label.rectTransform.sizeDelta = new Vector2(140f, 34f);

        var previewRoot = new GameObject("Next Ball Preview Root");
        previewRoot.transform.SetParent(panelObject.transform);
        var rootRect = previewRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0f, -100f);
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
        previewObject.transform.SetParent(panelObject.transform);
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
        background.color = new Color(0f, 0f, 0f, 0.58f);
        background.raycastTarget = false;

        var cardObject = new GameObject("Result Card");
        cardObject.transform.SetParent(gameOverPanel.transform);
        var cardRect = cardObject.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = new Vector2(0f, 32f);
        cardRect.sizeDelta = new Vector2(760f, 1260f);

        var cardBackground = cardObject.AddComponent<Image>();
        cardBackground.sprite = CircleSpriteCache.Square;
        cardBackground.color = new Color(0.015f, 0.028f, 0.045f, 0.86f);
        cardBackground.raycastTarget = false;
        CreateCardBorder(cardObject.transform, cardRect.sizeDelta);

        var title = CreateText(cardObject.transform, "Run Complete Title", new Vector2(0f, 520f), TextAnchor.MiddleCenter, 40, new Color(0.84f, 0.94f, 1f, 0.92f));
        title.text = "Run Complete";
        title.rectTransform.sizeDelta = new Vector2(660f, 62f);

        CreateGameOverLargestPreview(cardObject.transform);

        gameOverCreatedText = CreateText(cardObject.transform, "Game Over Created", new Vector2(0f, 214f), TextAnchor.MiddleCenter, 50, Color.white);
        gameOverCreatedText.rectTransform.sizeDelta = new Vector2(660f, 148f);
        gameOverCreatedText.lineSpacing = 0.9f;

        gameOverScoreText = CreateText(cardObject.transform, "Game Over Score", new Vector2(0f, 4f), TextAnchor.MiddleCenter, 32, new Color(0.9f, 0.95f, 1f));
        gameOverScoreText.rectTransform.sizeDelta = new Vector2(660f, 252f);
        gameOverScoreText.lineSpacing = 1.02f;

        gameOverRecordText = CreateText(cardObject.transform, "Game Over Record", new Vector2(0f, -208f), TextAnchor.MiddleCenter, 34, new Color(1f, 0.84f, 0.5f));
        gameOverRecordText.rectTransform.sizeDelta = new Vector2(660f, 82f);

        var buttonObject = new GameObject("Play Again Button");
        buttonObject.transform.SetParent(cardObject.transform);
        var buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -440f);
        buttonRect.sizeDelta = new Vector2(500f, 118f);

        playAgainButtonImage = buttonObject.AddComponent<Image>();
        playAgainButtonImage.sprite = CircleSpriteCache.Square;
        playAgainButtonImage.color = new Color(0.12f, 0.64f, 1f, 0.98f);

        var button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(() =>
        {
            SoundManager.Play(SoundEvent.ButtonClick);
            controller.Restart();
        });

        var buttonText = CreateText(buttonObject.transform, "Play Again Text", Vector2.zero, TextAnchor.MiddleCenter, 42, Color.white);
        buttonText.text = "Play Again";
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;

        gameOverPanel.SetActive(false);
    }

    private void CreateGameOverLargestPreview(Transform parent)
    {
        var previewRootObject = new GameObject("Largest Trophy Preview");
        previewRootObject.transform.SetParent(parent);
        var previewRootRect = previewRootObject.AddComponent<RectTransform>();
        previewRootRect.anchorMin = new Vector2(0.5f, 0.5f);
        previewRootRect.anchorMax = new Vector2(0.5f, 0.5f);
        previewRootRect.anchoredPosition = new Vector2(0f, 372f);
        previewRootRect.sizeDelta = new Vector2(210f, 210f);
        gameOverLargestPreviewRoot = previewRootObject.transform;

        var glowObject = new GameObject("Game Over Largest Glow");
        glowObject.transform.SetParent(previewRootObject.transform);
        var glowRect = glowObject.AddComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0.5f, 0.5f);
        glowRect.anchorMax = new Vector2(0.5f, 0.5f);
        glowRect.anchoredPosition = Vector2.zero;
        glowRect.sizeDelta = new Vector2(178f, 178f);
        gameOverLargestGlow = glowObject.AddComponent<Image>();
        gameOverLargestGlow.sprite = CircleSpriteCache.Circle;
        gameOverLargestGlow.raycastTarget = false;

        var bodyObject = new GameObject("Game Over Largest Body");
        bodyObject.transform.SetParent(previewRootObject.transform);
        var bodyRect = bodyObject.AddComponent<RectTransform>();
        bodyRect.anchorMin = glowRect.anchorMin;
        bodyRect.anchorMax = glowRect.anchorMax;
        bodyRect.anchoredPosition = Vector2.zero;
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

    private void BuildResultPreview(int level)
    {
        ClearResultPreviewAccents();

        var metadata = CosmicBodyConfig.Get(level);
        var glowColor = metadata.GlowColor;
        glowColor.a = level >= 8 ? 0.48f : 0.34f;
        gameOverLargestGlow.color = glowColor;
        gameOverLargestGlow.rectTransform.sizeDelta = new Vector2(188f, 188f);

        gameOverLargestImage.color = metadata.BaseColor;
        gameOverLargestImage.rectTransform.sizeDelta = new Vector2(124f, 124f);

        switch (metadata.VisualType)
        {
            case CosmicVisualType.GasGiant:
                AddPreviewAccent("Trophy Band", Vector2.zero, new Vector2(96f, 12f), metadata.DetailColor, 0.44f, CircleSpriteCache.Square);
                AddPreviewAccent("Trophy Band", new Vector2(0f, -22f), new Vector2(72f, 9f), metadata.DetailColor, 0.3f, CircleSpriteCache.Square);
                break;
            case CosmicVisualType.Star:
                AddPreviewAccent("Trophy Core", Vector2.zero, new Vector2(52f, 52f), metadata.DetailColor, 0.64f, CircleSpriteCache.Circle);
                AddPreviewAccent("Trophy Hot Core", Vector2.zero, new Vector2(25f, 25f), Color.white, 0.76f, CircleSpriteCache.Circle);
                break;
            case CosmicVisualType.RedGiant:
                AddPreviewAccent("Trophy Core", Vector2.zero, new Vector2(52f, 52f), metadata.DetailColor, 0.56f, CircleSpriteCache.Circle);
                AddPreviewAccent("Trophy Band", new Vector2(0f, -18f), new Vector2(92f, 12f), metadata.DetailColor, 0.28f, CircleSpriteCache.Square);
                break;
            case CosmicVisualType.NeutronStar:
                AddPreviewAccent("Trophy Ring", Vector2.zero, new Vector2(152f, 34f), metadata.GlowColor, 0.52f, CircleSpriteCache.Circle);
                AddPreviewAccent("Trophy Core", Vector2.zero, new Vector2(42f, 42f), Color.white, 0.84f, CircleSpriteCache.Circle);
                break;
            case CosmicVisualType.BlackHole:
                AddPreviewAccent("Trophy Outer Ring", Vector2.zero, new Vector2(166f, 52f), metadata.DetailColor, 0.48f, CircleSpriteCache.Circle);
                AddPreviewAccent("Trophy Energy Ring", Vector2.zero, new Vector2(148f, 42f), metadata.GlowColor, 0.86f, CircleSpriteCache.Circle);
                AddPreviewAccent("Trophy Core", Vector2.zero, new Vector2(74f, 74f), Color.black, 0.94f, CircleSpriteCache.Circle);
                break;
            case CosmicVisualType.GalaxyCore:
                AddPreviewAccent("Trophy Ring", Vector2.zero, new Vector2(152f, 44f), metadata.DetailColor, 0.62f, CircleSpriteCache.Circle);
                AddPreviewAccent("Trophy Core", Vector2.zero, new Vector2(58f, 58f), metadata.DetailColor, 0.48f, CircleSpriteCache.Circle);
                AddPreviewAccent("Trophy Hot Core", Vector2.zero, new Vector2(28f, 28f), Color.white, 0.86f, CircleSpriteCache.Circle);
                break;
        }
    }

    private void ClearResultPreviewAccents()
    {
        if (gameOverLargestPreviewRoot == null)
        {
            return;
        }

        for (var i = gameOverLargestPreviewRoot.childCount - 1; i >= 0; i--)
        {
            var child = gameOverLargestPreviewRoot.GetChild(i);
            if (child.name.StartsWith("Trophy "))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void AddPreviewAccent(string name, Vector2 position, Vector2 size, Color color, float alpha, Sprite sprite)
    {
        var accentObject = new GameObject(name);
        accentObject.transform.SetParent(gameOverLargestPreviewRoot);
        var rect = accentObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var image = accentObject.AddComponent<Image>();
        image.sprite = sprite;
        color.a = alpha;
        image.color = color;
        image.raycastTarget = false;
    }

    private void CreateCardBorder(Transform parent, Vector2 size)
    {
        CreateCardLine(parent, "Result Card Top Glow", new Vector2(0f, size.y * 0.5f - 2f), new Vector2(size.x, 4f));
        CreateCardLine(parent, "Result Card Bottom Glow", new Vector2(0f, -size.y * 0.5f + 2f), new Vector2(size.x, 4f));
        CreateCardLine(parent, "Result Card Left Glow", new Vector2(-size.x * 0.5f + 2f, 0f), new Vector2(4f, size.y));
        CreateCardLine(parent, "Result Card Right Glow", new Vector2(size.x * 0.5f - 2f, 0f), new Vector2(4f, size.y));
    }

    private static void CreateCardLine(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var lineObject = new GameObject(name);
        lineObject.transform.SetParent(parent);
        var rect = lineObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var image = lineObject.AddComponent<Image>();
        image.sprite = CircleSpriteCache.Square;
        image.color = new Color(0.24f, 0.82f, 1f, 0.2f);
        image.raycastTarget = false;
    }

    private IEnumerator PlayAgainPulseRoutine()
    {
        yield return new WaitForSeconds(1f);

        var buttonTransform = playAgainButtonImage.transform;
        var baseScale = Vector3.one;
        while (gameOverPanel.activeSelf)
        {
            var pulse = Mathf.Sin(Time.time * 4.2f) * 0.5f + 0.5f;
            buttonTransform.localScale = Vector3.one * Mathf.Lerp(1f, 1.035f, pulse);
            playAgainButtonImage.color = Color.Lerp(new Color(0.12f, 0.64f, 1f, 0.96f), new Color(0.28f, 0.82f, 1f, 1f), pulse);
            yield return null;
        }

        buttonTransform.localScale = baseScale;
        playAgainPulseRoutine = null;
    }

    private static string GetGameOverRecordLine(bool newBestScore, bool newBestLargest, int largestLevel, string fallback)
    {
        if (newBestScore)
        {
            return "New Best Score!";
        }

        if (newBestLargest)
        {
            return $"New Largest Record: {CosmicBodyConfig.GetShortName(largestLevel)}!";
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
