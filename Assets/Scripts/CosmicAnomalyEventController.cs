using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CosmicAnomalyEventController : MonoBehaviour
{
    private const float FirstEventMinDelay = 45f;
    private const float FirstEventMaxDelay = 75f;
    private const float CooldownMinSeconds = 48f;
    private const float CooldownMaxSeconds = 66f;
    private const float TargetScanRadius = 5f;
    private const float TelegraphSeconds = 3.5f;
    private const float AbsorptionDurationSeconds = 0.68f;
    private const float KnockoutDistance = 2f;
    private const int MaxTargetLevel = 4;
    private const int MaxTargetsPerEvent = 2;

    private readonly List<TargetLock> targetLocks = new List<TargetLock>();
    private GameController controller;
    private GameEffects effects;
    private ContainerBounds bounds;
    private Vector2 anomalyPosition;
    private SpriteRenderer outerRing;
    private SpriteRenderer innerRing;
    private SpriteRenderer core;
    private bool isEventActive;
    private float eventStartedAt;
    private float nextTickAt;

    public static CosmicAnomalyEventController Instance { get; private set; }

    public void Initialize(GameController gameController, ContainerBounds containerBounds, GameEffects gameEffects)
    {
        Instance = this;
        controller = gameController;
        effects = gameEffects;
        bounds = containerBounds;
        CreateVisuals();
        HideVisuals();
        StartCoroutine(EventLoopRoutine());
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.A))
        {
            StopAllCoroutines();
            StartCoroutine(PlayAnomalyRoutine());
            StartCoroutine(EventLoopRoutine(Random.Range(CooldownMinSeconds, CooldownMaxSeconds)));
        }
#endif

        if (!isEventActive)
        {
            return;
        }

        if (controller == null || controller.IsGameOver)
        {
            HideVisuals();
            isEventActive = false;
            return;
        }

        var progress = Mathf.Clamp01((Time.time - eventStartedAt) / TelegraphSeconds);
        UpdateAnomalyVisuals(progress);
        UpdateTargetVisuals(progress);
        CheckRescueConditions();
        PlayTickFeedback(progress);
    }

    public void RegisterTargetMerged(Ball ball)
    {
        ResolveTarget(ball, true);
    }

    public void RegisterTargetDisrupted(Ball ball)
    {
        ResolveTarget(ball, true);
    }

    private IEnumerator EventLoopRoutine(float initialDelay = -1f)
    {
        var delay = initialDelay > 0f ? initialDelay : Random.Range(FirstEventMinDelay, FirstEventMaxDelay);
        yield return new WaitForSeconds(delay);

        while (true)
        {
            while (controller == null || controller.IsGameOver || controller.IsOpeningDemoActive)
            {
                yield return null;
            }

            yield return PlayAnomalyRoutine();
            yield return new WaitForSeconds(Random.Range(CooldownMinSeconds, CooldownMaxSeconds));
        }
    }

    private IEnumerator PlayAnomalyRoutine()
    {
        anomalyPosition = PickAnomalyPosition();
        PickTargets();
        if (targetLocks.Count == 0)
        {
            HideVisuals();
            yield break;
        }

        isEventActive = true;
        eventStartedAt = Time.time;
        nextTickAt = Time.time;
        SetVisualsVisible(true);
        SoundManager.Play(SoundEvent.CosmicAnomalyWarning);

        while (Time.time - eventStartedAt < TelegraphSeconds)
        {
            if (controller == null || controller.IsGameOver)
            {
                HideVisuals();
                yield break;
            }

            yield return null;
        }

        isEventActive = false;
        for (var i = 0; i < targetLocks.Count; i++)
        {
            var targetLock = targetLocks[i];
            if (targetLock.Resolved || !CanAbsorb(targetLock.Target))
            {
                continue;
            }

            targetLock.Target.SetAnomalyTargeted(false);
            SoundManager.Play(SoundEvent.CosmicAnomalyAbsorb);
            targetLock.Target.TryStartBlackHoleAbsorption(anomalyPosition, AbsorptionDurationSeconds);
            effects?.PlayAnomalyConsumed(anomalyPosition);
            yield return PulseCoreRoutine();
        }

        HideVisuals();
    }

    private void PickTargets()
    {
        ClearTargets(true);
        var candidates = new List<Ball>();
        foreach (var ball in controller.ActiveBalls)
        {
            if (CanTarget(ball) && Vector2.Distance(anomalyPosition, ball.transform.position) <= TargetScanRadius)
            {
                candidates.Add(ball);
            }
        }

        candidates.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
        var count = Mathf.Min(Random.value < 0.42f ? 2 : 1, Mathf.Min(MaxTargetsPerEvent, candidates.Count));
        for (var i = 0; i < count; i++)
        {
            var index = Mathf.Min(i + Random.Range(0, Mathf.Min(3, candidates.Count - i)), candidates.Count - 1);
            var target = candidates[index];
            candidates.RemoveAt(index);
            target.SetAnomalyTargeted(true);
            targetLocks.Add(CreateTargetLock(target, targetLocks.Count));
        }
    }

    private TargetLock CreateTargetLock(Ball target, int index)
    {
        var marker = CreateRenderer($"Anomaly Target Marker {index + 1}", CircleSpriteCache.Circle, 26);
        var beam = CreateBeamRenderer($"Anomaly Target Beam {index + 1}", 24);
        beam.transform.SetParent(transform);
        var timerBack = CreateRenderer($"Anomaly Timer Back {index + 1}", CircleSpriteCache.Circle, 25);
        var timerFill = CreateRenderer($"Anomaly Timer Fill {index + 1}", CircleSpriteCache.Circle, 27);

        return new TargetLock
        {
            Target = target,
            InitialPosition = target.transform.position,
            Marker = marker,
            Beam = beam,
            TimerBack = timerBack,
            TimerFill = timerFill
        };
    }

    private void CheckRescueConditions()
    {
        for (var i = 0; i < targetLocks.Count; i++)
        {
            var targetLock = targetLocks[i];
            if (targetLock.Resolved)
            {
                continue;
            }

            if (targetLock.Target == null)
            {
                targetLock.Resolved = true;
                continue;
            }

            var movedDistance = Vector2.Distance(targetLock.InitialPosition, targetLock.Target.transform.position);
            if (movedDistance >= KnockoutDistance)
            {
                ResolveTarget(targetLock.Target, true);
            }
        }
    }

    private void ResolveTarget(Ball ball, bool rescued)
    {
        for (var i = 0; i < targetLocks.Count; i++)
        {
            var targetLock = targetLocks[i];
            if (targetLock.Resolved || targetLock.Target != ball)
            {
                continue;
            }

            targetLock.Resolved = true;
            var position = ball != null ? (Vector2)ball.transform.position : targetLock.InitialPosition;
            var level = ball != null ? ball.Level : 1;
            if (ball != null)
            {
                ball.SetAnomalyTargeted(false);
            }

            SetTargetVisuals(targetLock, false);
            if (rescued)
            {
                controller.GrantAnomalyRescueBonus(position, level);
            }
        }
    }

    private void UpdateAnomalyVisuals(float progress)
    {
        var pulse = Mathf.Sin(Time.time * 7.2f) * 0.5f + 0.5f;
        var charge = Mathf.Lerp(0.28f, 1f, AnimationEasing.EaseOutCubic(progress));

        outerRing.transform.position = anomalyPosition;
        innerRing.transform.position = anomalyPosition;
        core.transform.position = anomalyPosition;

        outerRing.transform.localScale = Vector3.one * Mathf.Lerp(1.24f, 2.26f, charge + pulse * 0.08f);
        innerRing.transform.localScale = new Vector3(Mathf.Lerp(0.88f, 1.36f, charge), Mathf.Lerp(0.28f, 0.48f, pulse), 1f);
        core.transform.localScale = Vector3.one * Mathf.Lerp(0.32f, 0.54f, pulse);

        outerRing.color = new Color(0.78f, 0.08f, 0.28f, Mathf.Lerp(0.06f, 0.32f, charge));
        innerRing.color = new Color(1f, 0.22f, 0.36f, Mathf.Lerp(0.16f, 0.58f, charge));
        core.color = new Color(0.08f, 0.01f, 0.08f, Mathf.Lerp(0.62f, 0.9f, charge));
    }

    private void UpdateTargetVisuals(float progress)
    {
        var remaining = 1f - progress;
        var pulse = Mathf.Sin(Time.time * 9.5f) * 0.5f + 0.5f;

        for (var i = 0; i < targetLocks.Count; i++)
        {
            var targetLock = targetLocks[i];
            if (targetLock.Resolved || targetLock.Target == null)
            {
                SetTargetVisuals(targetLock, false);
                continue;
            }

            var targetPosition = (Vector2)targetLock.Target.transform.position;
            SetTargetVisuals(targetLock, true);

            var diameter = BallConfig.GetDiameter(targetLock.Target.Level);
            targetLock.Marker.transform.position = targetPosition;
            targetLock.Marker.transform.localScale = Vector3.one * diameter * Mathf.Lerp(1.16f, 1.34f, pulse);
            targetLock.Marker.color = new Color(1f, 0.14f, 0.24f, Mathf.Lerp(0.22f, 0.48f, pulse));

            targetLock.TimerBack.transform.position = targetPosition + Vector2.up * (diameter * 0.74f);
            targetLock.TimerBack.transform.localScale = Vector3.one * diameter * 0.46f;
            targetLock.TimerBack.color = new Color(0.08f, 0.01f, 0.02f, 0.5f);

            targetLock.TimerFill.transform.position = targetLock.TimerBack.transform.position;
            targetLock.TimerFill.transform.localScale = Vector3.one * diameter * Mathf.Lerp(0.06f, 0.46f, remaining);
            targetLock.TimerFill.color = new Color(1f, Mathf.Lerp(0.12f, 0.62f, remaining), 0.12f, 0.72f);

            var beamColor = new Color(1f, 0.08f, 0.18f, Mathf.Lerp(0.18f, 0.42f, pulse));
            targetLock.Beam.positionCount = 2;
            targetLock.Beam.SetPosition(0, new Vector3(anomalyPosition.x, anomalyPosition.y, -0.15f));
            targetLock.Beam.SetPosition(1, new Vector3(targetPosition.x, targetPosition.y, -0.15f));
            targetLock.Beam.startWidth = Mathf.Lerp(0.018f, 0.042f, pulse);
            targetLock.Beam.endWidth = Mathf.Lerp(0.034f, 0.072f, pulse);
            targetLock.Beam.startColor = new Color(beamColor.r, beamColor.g, beamColor.b, beamColor.a * 0.56f);
            targetLock.Beam.endColor = beamColor;
        }
    }

    private void PlayTickFeedback(float progress)
    {
        if (Time.time < nextTickAt)
        {
            return;
        }

        nextTickAt = Time.time + Mathf.Lerp(0.52f, 0.16f, progress);
        SoundManager.Play(SoundEvent.AnomalyTick);
        Haptics.LightImpact();
    }

    private IEnumerator PulseCoreRoutine()
    {
        var elapsed = 0f;
        while (elapsed < AbsorptionDurationSeconds)
        {
            elapsed += Time.deltaTime;
            var pulse = Mathf.Sin(Mathf.Clamp01(elapsed / AbsorptionDurationSeconds) * Mathf.PI);
            core.transform.localScale = Vector3.one * Mathf.Lerp(0.52f, 0.74f, pulse);
            outerRing.transform.localScale = Vector3.one * Mathf.Lerp(2.25f, 2.75f, pulse);
            yield return null;
        }
    }

    private Vector2 PickAnomalyPosition()
    {
        var horizontalPadding = 0.9f;
        var x = Random.Range(bounds.Left + horizontalPadding, bounds.Right - horizontalPadding);
        var minY = bounds.Bottom + 1.55f;
        var maxY = Mathf.Min(bounds.DangerY - 1.05f, bounds.Top - 3.1f);
        if (maxY <= minY)
        {
            maxY = minY + 0.5f;
        }

        var y = Random.Range(minY, maxY);
        return new Vector2(x, y);
    }

    private bool CanTarget(Ball ball)
    {
        return ball != null && !ball.IsMerging && !ball.IsPreMerging && !ball.IsAnomalyTargeted && ball.Level <= MaxTargetLevel;
    }

    private bool CanAbsorb(Ball ball)
    {
        return ball != null && !ball.IsMerging && !ball.IsPreMerging && ball.Level <= MaxTargetLevel;
    }

    private void CreateVisuals()
    {
        outerRing = CreateRenderer("Cosmic Anomaly Outer Ring", CircleSpriteCache.Circle, 21);
        innerRing = CreateRenderer("Cosmic Anomaly Inner Ring", CircleSpriteCache.Circle, 22);
        core = CreateRenderer("Cosmic Anomaly Core", CircleSpriteCache.Circle, 23);
    }

    private SpriteRenderer CreateRenderer(string objectName, Sprite sprite, int sortingOrder)
    {
        var visual = new GameObject(objectName);
        visual.transform.SetParent(transform);
        visual.transform.position = Vector3.zero;
        visual.transform.rotation = Quaternion.identity;

        var renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        renderer.enabled = false;
        return renderer;
    }

    private static LineRenderer CreateBeamRenderer(string objectName, int sortingOrder)
    {
        var visual = new GameObject(objectName);
        var renderer = visual.AddComponent<LineRenderer>();
        visual.transform.position = Vector3.zero;
        visual.transform.rotation = Quaternion.identity;

        renderer.positionCount = 2;
        renderer.useWorldSpace = true;
        renderer.numCapVertices = 4;
        renderer.numCornerVertices = 2;
        renderer.sortingOrder = sortingOrder;
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        var material = RuntimeMaterials.CreateEnergyBeam(new Color(1f, 0.1f, 0.22f, 0.86f), 1.1f, 3.2f);
        if (material != null)
        {
            renderer.material = material;
        }

        renderer.enabled = false;
        return renderer;
    }

    private void SetVisualsVisible(bool visible)
    {
        outerRing.enabled = visible;
        innerRing.enabled = visible;
        core.enabled = visible;
    }

    private static void SetTargetVisuals(TargetLock targetLock, bool visible)
    {
        targetLock.Marker.enabled = visible;
        targetLock.Beam.enabled = visible;
        targetLock.TimerBack.enabled = visible;
        targetLock.TimerFill.enabled = visible;
    }

    private void ClearTargets(bool destroyVisuals)
    {
        for (var i = 0; i < targetLocks.Count; i++)
        {
            var targetLock = targetLocks[i];
            if (targetLock.Target != null)
            {
                targetLock.Target.SetAnomalyTargeted(false);
            }

            SetTargetVisuals(targetLock, false);
            if (destroyVisuals)
            {
                Destroy(targetLock.Marker.gameObject);
                Destroy(targetLock.Beam.gameObject);
                Destroy(targetLock.TimerBack.gameObject);
                Destroy(targetLock.TimerFill.gameObject);
            }
        }

        targetLocks.Clear();
    }

    private void HideVisuals()
    {
        SetVisualsVisible(false);
        ClearTargets(true);
    }

    private sealed class TargetLock
    {
        public Ball Target;
        public Vector2 InitialPosition;
        public SpriteRenderer Marker;
        public LineRenderer Beam;
        public SpriteRenderer TimerBack;
        public SpriteRenderer TimerFill;
        public bool Resolved;
    }
}
