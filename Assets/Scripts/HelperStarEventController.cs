using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HelperStarEventController : MonoBehaviour
{
    private const int MinTargetLevel = 2;
    private const int MaxTargetLevel = 5;
    private const int MaxResultLevel = 6;
    private const int MaxEventsPerRun = 3;
    private const float MinRunSeconds = 58f;
    private const float InitialDelayMin = 62f;
    private const float InitialDelayMax = 94f;
    private const float CooldownMin = 82f;
    private const float CooldownMax = 126f;
    private const float RareUpgradeChance = 0.16f;
    private const float StableAgeSeconds = 1.25f;
    private const float StableVelocity = 0.55f;
    private const float StableAngularVelocity = 75f;
    private const float ActivationSeconds = 1.18f;

    private GameController controller;
    private ContainerBounds bounds;
    private GameObject visualRoot;
    private SpriteRenderer starRenderer;
    private SpriteRenderer glowRenderer;
    private LineRenderer beamRenderer;
    private int eventsThisRun;
    private bool isRunningEvent;

    public void Initialize(GameController gameController, ContainerBounds containerBounds)
    {
        controller = gameController;
        bounds = containerBounds;
        BuildVisuals();
        StartCoroutine(EventLoop());
    }

    private IEnumerator EventLoop()
    {
        yield return new WaitForSeconds(Random.Range(InitialDelayMin, InitialDelayMax));

        while (controller != null && !controller.IsGameOver && eventsThisRun < MaxEventsPerRun)
        {
            if (!controller.IsOpeningDemoActive && controller.RunSeconds >= MinRunSeconds && HasEligibleTargets())
            {
                yield return StartCoroutine(ActivateRoutine());
                eventsThisRun++;
                yield return new WaitForSeconds(Random.Range(CooldownMin, CooldownMax));
            }
            else
            {
                yield return new WaitForSeconds(7f);
            }
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (controller == null || controller.IsGameOver || isRunningEvent)
        {
            return;
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.H))
        {
            StartCoroutine(ActivateRoutine());
        }
    }
#endif

    private IEnumerator ActivateRoutine()
    {
        var target = PickTarget();
        if (target == null)
        {
            yield break;
        }

        isRunningEvent = true;
        SoundManager.Play(SoundEvent.HelperStarAppear);

        var rareUpgrade = Random.value < RareUpgradeChance;
        var delta = rareUpgrade ? 2 : 1;
        var clampedResult = Mathf.Min(target.Level + delta, MaxResultLevel);
        rareUpgrade = clampedResult - target.Level > 1;
        delta = rareUpgrade ? 2 : 1;

        var sourcePosition = GetSourcePosition(target);
        ShowVisuals(sourcePosition);
        target.SetResonance(1f, true);

        var elapsed = 0f;
        var startPosition = sourcePosition;
        var beamColor = rareUpgrade ? new Color(1f, 0.8f, 0.22f, 1f) : new Color(0.7f, 1f, 0.88f, 1f);
        while (elapsed < ActivationSeconds)
        {
            if (!IsTargetStillEligible(target))
            {
                if (target != null)
                {
                    target.ClearResonance();
                }

                HideVisuals();
                isRunningEvent = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / ActivationSeconds);
            var eased = AnimationEasing.EaseInOutSine(t);
            var targetPosition = (Vector2)target.transform.position + Vector2.up * Mathf.Max(0.38f, target.Radius * 0.72f);
            var hover = Vector2.up * Mathf.Sin(Time.time * 10.5f) * 0.055f;
            var starPosition = Vector2.Lerp(startPosition, targetPosition, eased * 0.78f) + hover;

            visualRoot.transform.position = starPosition;
            var pulse = Mathf.Sin(Time.time * (rareUpgrade ? 11.5f : 8.5f)) * 0.5f + 0.5f;
            visualRoot.transform.localScale = Vector3.one * Mathf.Lerp(0.82f, rareUpgrade ? 1.14f : 1.04f, pulse);

            beamColor.a = Mathf.Lerp(0.34f, rareUpgrade ? 0.82f : 0.64f, pulse);
            beamRenderer.startColor = beamColor;
            beamRenderer.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, beamColor.a * 0.38f);
            beamRenderer.SetPosition(0, starPosition);
            beamRenderer.SetPosition(1, target.transform.position);
            target.SetResonance(Mathf.Lerp(0.55f, 1f, pulse), true);
            yield return null;
        }

        var upgradePosition = target != null ? (Vector2)target.transform.position : (Vector2)visualRoot.transform.position;
        if (target != null)
        {
            target.ClearResonance();
        }

        var upgraded = controller.TryApplyHelperStarUpgrade(target, delta, visualRoot.transform.position, rareUpgrade);
        if (upgraded)
        {
            yield return StartCoroutine(FadeOutRoutine(upgradePosition, rareUpgrade));
        }

        HideVisuals();
        isRunningEvent = false;
    }

    private bool HasEligibleTargets()
    {
        return PickTarget() != null;
    }

    private Ball PickTarget()
    {
        if (controller == null)
        {
            return null;
        }

        var candidates = new List<Ball>();
        var balls = controller.ActiveBalls;
        for (var i = 0; i < balls.Count; i++)
        {
            var ball = balls[i];
            if (IsTargetStillEligible(ball))
            {
                candidates.Add(ball);
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    private bool IsTargetStillEligible(Ball ball)
    {
        if (ball == null || ball.Body == null || ball.IsMerging || ball.IsPreMerging || ball.IsAnomalyTargeted)
        {
            return false;
        }

        if (ball.Level < MinTargetLevel || ball.Level > MaxTargetLevel)
        {
            return false;
        }

        if (Time.time - ball.SpawnedAt < StableAgeSeconds)
        {
            return false;
        }

        if (ball.Body.linearVelocity.sqrMagnitude > StableVelocity * StableVelocity)
        {
            return false;
        }

        if (Mathf.Abs(ball.Body.angularVelocity) > StableAngularVelocity)
        {
            return false;
        }

        var position = ball.transform.position;
        return position.x - ball.Radius >= bounds.Left
            && position.x + ball.Radius <= bounds.Right
            && position.y - ball.Radius >= bounds.Bottom
            && position.y + ball.Radius <= bounds.Top;
    }

    private Vector2 GetSourcePosition(Ball target)
    {
        var targetX = target != null ? target.transform.position.x : 0f;
        var side = Random.value < 0.5f ? -1f : 1f;
        var x = Mathf.Clamp(targetX + side * Random.Range(0.95f, 1.55f), bounds.Left + 0.45f, bounds.Right - 0.45f);
        var y = Mathf.Clamp(bounds.Top - Random.Range(0.72f, 1.12f), bounds.Bottom + 1.2f, bounds.Top - 0.42f);
        return new Vector2(x, y);
    }

    private void BuildVisuals()
    {
        visualRoot = new GameObject("Helper Star Visual");
        visualRoot.transform.SetParent(transform);
        visualRoot.SetActive(false);

        glowRenderer = CreateCircle("Helper Star Glow", 42, new Color(1f, 0.78f, 0.26f, 0.28f), 0.62f);
        starRenderer = CreateCircle("Helper Star Core", 43, new Color(1f, 0.95f, 0.55f, 0.98f), 0.24f);

        var beamObject = new GameObject("Helper Star Beam");
        beamObject.transform.SetParent(visualRoot.transform);
        beamRenderer = beamObject.AddComponent<LineRenderer>();
        beamRenderer.positionCount = 2;
        beamRenderer.useWorldSpace = true;
        beamRenderer.widthMultiplier = 0.035f;
        beamRenderer.sortingOrder = 41;
        beamRenderer.numCapVertices = 6;
        beamRenderer.material = CreateBeamMaterial();
    }

    private SpriteRenderer CreateCircle(string objectName, int sortingOrder, Color color, float scale)
    {
        var circleObject = new GameObject(objectName);
        circleObject.transform.SetParent(visualRoot.transform);
        circleObject.transform.localPosition = Vector3.zero;
        circleObject.transform.localScale = Vector3.one * scale;

        var renderer = circleObject.AddComponent<SpriteRenderer>();
        renderer.sprite = CircleSpriteCache.Circle;
        renderer.sortingOrder = sortingOrder;
        renderer.color = color;
        var glowMaterial = RuntimeMaterials.CreateAtmosphereGlow(color, sortingOrder == 42 ? 1.28f : 0.72f, sortingOrder == 42 ? 2.8f : 4.5f);
        if (glowMaterial != null)
        {
            renderer.material = glowMaterial;
        }

        return renderer;
    }

    private static Material CreateBeamMaterial()
    {
        var material = RuntimeMaterials.CreateEnergyBeam(new Color(1f, 0.84f, 0.32f), 1.18f, 4.2f);
        if (material != null)
        {
            return material;
        }

        var fallbackShader = Shader.Find("Sprites/Default");
        return fallbackShader != null ? new Material(fallbackShader) : null;
    }

    private void ShowVisuals(Vector2 sourcePosition)
    {
        visualRoot.transform.position = sourcePosition;
        visualRoot.transform.localScale = Vector3.one * 0.6f;
        visualRoot.SetActive(true);
        beamRenderer.enabled = true;
    }

    private IEnumerator FadeOutRoutine(Vector2 position, bool rareUpgrade)
    {
        var elapsed = 0f;
        const float duration = 0.2f;
        var startScale = visualRoot.transform.localScale;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            var currentPosition = (Vector2)visualRoot.transform.position;
            visualRoot.transform.position = Vector2.Lerp(currentPosition, position, AnimationEasing.EaseOutCubic(t));
            visualRoot.transform.localScale = Vector3.Lerp(startScale, Vector3.one * (rareUpgrade ? 1.45f : 1.12f), t);

            var coreColor = starRenderer.color;
            var glowColor = glowRenderer.color;
            coreColor.a = Mathf.Lerp(0.96f, 0f, t);
            glowColor.a = Mathf.Lerp(0.34f, 0f, t);
            starRenderer.color = coreColor;
            glowRenderer.color = glowColor;
            yield return null;
        }
    }

    private void HideVisuals()
    {
        if (starRenderer != null)
        {
            starRenderer.color = new Color(1f, 0.95f, 0.55f, 0.98f);
        }

        if (glowRenderer != null)
        {
            glowRenderer.color = new Color(1f, 0.78f, 0.26f, 0.28f);
        }

        if (beamRenderer != null)
        {
            beamRenderer.enabled = false;
        }

        if (visualRoot != null)
        {
            visualRoot.SetActive(false);
        }
    }
}
