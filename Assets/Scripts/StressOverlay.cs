using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class StressOverlay : MonoBehaviour
{
    private const float StackWarningRange = 1.45f;

    private GameController controller;
    private PressureFloor pressureFloor;
    private float dangerY;
    private Image fullOverlay;
    private Image topEdge;
    private Image bottomEdge;
    private Image leftEdge;
    private Image rightEdge;
    private float reliefSuppressUntil;
    private float nextHapticPulseAt;

    public static StressOverlay Instance { get; private set; }

    public static StressOverlay Build(GameController gameController, PressureFloor floor, float dangerLineY)
    {
        var overlayObject = new GameObject("Stress Overlay");
        var overlay = overlayObject.AddComponent<StressOverlay>();
        overlay.Initialize(gameController, floor, dangerLineY);
        return overlay;
    }

    public void Initialize(GameController gameController, PressureFloor floor, float dangerLineY)
    {
        Instance = this;
        controller = gameController;
        pressureFloor = floor;
        dangerY = dangerLineY;

        var canvasObject = new GameObject("Stress Overlay Canvas");
        canvasObject.transform.SetParent(transform);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -10;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 1f;

        fullOverlay = CreatePanel(canvasObject.transform, "Stress Wash", Vector2.zero, Vector2.zero, new Color(0.2f, 0f, 0f, 0f), true);
        topEdge = CreateEdge(canvasObject.transform, "Stress Top Edge", new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(1080f, 160f));
        bottomEdge = CreateEdge(canvasObject.transform, "Stress Bottom Edge", new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(1080f, 160f));
        leftEdge = CreateEdge(canvasObject.transform, "Stress Left Edge", new Vector2(0f, 0.5f), new Vector2(58f, 0f), new Vector2(116f, 1920f));
        rightEdge = CreateEdge(canvasObject.transform, "Stress Right Edge", new Vector2(1f, 0.5f), new Vector2(-58f, 0f), new Vector2(116f, 1920f));
    }

    public void PlayRelief()
    {
        reliefSuppressUntil = Time.time + 0.65f;
        StartCoroutine(ReliefFlashRoutine());
    }

    private void Update()
    {
        if (controller == null || controller.IsGameOver)
        {
            SetStressVisual(0f);
            return;
        }

        var stress = CalculateStress();
        if (Time.time < reliefSuppressUntil)
        {
            stress = 0f;
        }

        SetStressVisual(stress);
        if (stress >= 0.72f && Time.time >= nextHapticPulseAt)
        {
            nextHapticPulseAt = Time.time + Mathf.Lerp(0.82f, 0.34f, stress);
            Haptics.LightImpact();
            SoundManager.Play(SoundEvent.StressHeartbeat);
        }
    }

    private float CalculateStress()
    {
        var highestTop = controller.GetHighestBallTopY();
        var stackStress = highestTop < -100f ? 0f : Mathf.Clamp01(1f - ((dangerY - highestTop) / StackWarningRange));
        var pressureStress = pressureFloor != null ? pressureFloor.PressureProgress : 0f;
        return Mathf.Clamp01(Mathf.Max(controller.DangerPressure, stackStress * 0.72f, pressureStress * 0.48f));
    }

    private void SetStressVisual(float stress)
    {
        var pulse = Mathf.Sin(Time.time * Mathf.Lerp(3.2f, 7.4f, stress)) * 0.5f + 0.5f;
        var washAlpha = Mathf.Lerp(0f, 0.16f, stress) + pulse * stress * 0.035f;
        fullOverlay.color = new Color(0.26f, 0.02f, 0.02f, washAlpha);

        var edgeAlpha = Mathf.Lerp(0f, 0.34f, stress) + pulse * stress * 0.08f;
        var edgeColor = new Color(1f, 0.08f, 0.1f, edgeAlpha);
        topEdge.color = edgeColor;
        bottomEdge.color = edgeColor;
        leftEdge.color = edgeColor;
        rightEdge.color = edgeColor;
    }

    private IEnumerator ReliefFlashRoutine()
    {
        var elapsed = 0f;
        const float duration = 0.28f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            var alpha = Mathf.Lerp(0.18f, 0f, AnimationEasing.EaseOutCubic(t));
            fullOverlay.color = new Color(0.45f, 0.95f, 1f, alpha);
            yield return null;
        }
    }

    private static Image CreateEdge(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var image = CreatePanel(parent, name, position, size, new Color(1f, 0.08f, 0.1f, 0f), false);
        image.rectTransform.anchorMin = anchor;
        image.rectTransform.anchorMax = anchor;
        return image;
    }

    private static Image CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool stretch)
    {
        var panelObject = new GameObject(name);
        panelObject.transform.SetParent(parent);
        var rect = panelObject.AddComponent<RectTransform>();
        if (stretch)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        var image = panelObject.AddComponent<Image>();
        image.sprite = CircleSpriteCache.Square;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }
}
