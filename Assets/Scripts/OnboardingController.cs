using UnityEngine;
using UnityEngine.UI;

public sealed class OnboardingController : MonoBehaviour
{
    private const string OnboardingCompletedKey = "MergePrototypeOnboardingCompleted";
    private const int MaxDropsBeforeAutoComplete = 4;

    private Canvas canvas;
    private GameObject promptObject;
    private Text promptText;
    private Image promptBackground;
    private int dropCount;
    private bool completed;
    private bool isAiming;
    private bool dangerActive;

    public static OnboardingController Instance { get; private set; }

    public static OnboardingController Build()
    {
        var onboardingObject = new GameObject("Onboarding Controller");
        return onboardingObject.AddComponent<OnboardingController>();
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(OnboardingCompletedKey);
        PlayerPrefs.Save();
    }

    public void ResumeAfterOpeningDemo()
    {
        if (completed)
        {
            return;
        }

        isAiming = false;
        dangerActive = false;
        ShowPrompt("Drag to aim");
    }

    public void BeginAiming()
    {
        if (completed)
        {
            return;
        }

        isAiming = true;
        ShowPrompt("Release to drop");
    }

    public void RegisterDrop()
    {
        if (completed)
        {
            return;
        }

        isAiming = false;
        dropCount++;
        ShowPrompt(dropCount >= 2 ? "Merge same objects" : "Merge same objects");

        if (dropCount >= MaxDropsBeforeAutoComplete)
        {
            Complete();
        }
    }

    public void RegisterMerge()
    {
        if (completed)
        {
            return;
        }

        ShowPrompt("Nice merge");
        Complete();
    }

    public void SetDangerActive(bool active)
    {
        if (completed)
        {
            return;
        }

        dangerActive = active;
        if (dangerActive)
        {
            ShowPrompt("Don't cross the danger line");
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        completed = PlayerPrefs.HasKey(OnboardingCompletedKey);
        CreateUi();

        if (completed || OpeningDemoController.ShouldPlayOpeningDemo())
        {
            promptObject.SetActive(false);
            return;
        }

        ShowPrompt("Drag to aim");
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.O))
        {
            ResetProgress();
            completed = false;
            dropCount = 0;
            isAiming = false;
            dangerActive = false;
            ShowPrompt("Drag to aim");
        }

        if (completed || dangerActive || isAiming || dropCount > 0)
        {
            return;
        }

        ShowPrompt("Drag to aim");
    }

    private void CreateUi()
    {
        var canvasObject = new GameObject("Onboarding Canvas");
        canvasObject.transform.SetParent(transform);

        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 1f;

        promptObject = new GameObject("Onboarding Prompt");
        promptObject.transform.SetParent(canvasObject.transform);

        var promptRect = promptObject.AddComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 1f);
        promptRect.anchorMax = new Vector2(0.5f, 1f);
        promptRect.anchoredPosition = new Vector2(0f, -330f);
        promptRect.sizeDelta = new Vector2(610f, 72f);

        promptBackground = promptObject.AddComponent<Image>();
        promptBackground.color = new Color(0.03f, 0.07f, 0.11f, 0.76f);
        promptBackground.raycastTarget = false;

        var textObject = new GameObject("Onboarding Text");
        textObject.transform.SetParent(promptObject.transform);

        var textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        promptText = textObject.AddComponent<Text>();
        promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        promptText.fontSize = 30;
        promptText.alignment = TextAnchor.MiddleCenter;
        promptText.color = new Color(0.9f, 0.96f, 1f, 0.96f);
        promptText.raycastTarget = false;
    }

    private void ShowPrompt(string message)
    {
        if (promptObject == null)
        {
            return;
        }

        promptText.text = message;
        promptBackground.color = dangerActive
            ? new Color(0.22f, 0.04f, 0.08f, 0.82f)
            : new Color(0.03f, 0.07f, 0.11f, 0.76f);
        promptObject.SetActive(true);
    }

    private void Complete()
    {
        completed = true;
        PlayerPrefs.SetInt(OnboardingCompletedKey, 1);
        PlayerPrefs.Save();

        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }
}
