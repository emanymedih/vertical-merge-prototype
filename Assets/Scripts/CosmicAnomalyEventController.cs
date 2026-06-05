using System.Collections;
using UnityEngine;

public sealed class CosmicAnomalyEventController : MonoBehaviour
{
    private const float FirstEventMinDelay = 45f;
    private const float FirstEventMaxDelay = 75f;
    private const float CooldownMinSeconds = 48f;
    private const float CooldownMaxSeconds = 66f;
    private const float WarningSeconds = 2.6f;
    private const float ActiveSeconds = 9.5f;
    private const float PullRadius = 2.15f;
    private const float AbsorptionRadius = 2.05f;
    private const float PullAcceleration = 0.58f;
    private const float AbsorptionDurationSeconds = 0.68f;
    private const float MinDistance = 0.22f;
    private const int MaxAbsorbLevel = 4;
    private const int MaxAbsorptionsPerEvent = 2;

    private GameController controller;
    private ContainerBounds bounds;
    private Vector2 anomalyPosition;
    private SpriteRenderer outerRing;
    private SpriteRenderer innerRing;
    private SpriteRenderer core;
    private SpriteRenderer targetMarker;
    private SpriteRenderer beamRenderer;
    private Ball currentTarget;
    private bool isActive;
    private bool isWarning;
    private float warningStartedAt;
    private float activeStartedAt;

    public void Initialize(GameController gameController, ContainerBounds containerBounds)
    {
        controller = gameController;
        bounds = containerBounds;
        CreateVisuals();
        HideVisuals();
        StartCoroutine(EventLoopRoutine());
    }

    private void FixedUpdate()
    {
        if (!isActive || controller == null || controller.IsGameOver)
        {
            return;
        }

        var balls = controller.ActiveBalls;
        for (var i = 0; i < balls.Count; i++)
        {
            var ball = balls[i];
            if (!CanPull(ball))
            {
                continue;
            }

            var offset = anomalyPosition - (Vector2)ball.transform.position;
            var distance = offset.magnitude;
            if (distance <= MinDistance || distance > PullRadius)
            {
                continue;
            }

            var falloff = 1f - Mathf.Clamp01(distance / PullRadius);
            var levelFactor = GetLevelPullFactor(ball.Level);
            var acceleration = PullAcceleration * falloff * falloff * levelFactor;
            ball.TryApplyExternalForce(offset.normalized * acceleration * ball.Mass);
        }
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

        if (!isActive && !isWarning)
        {
            return;
        }

        if (controller == null || controller.IsGameOver)
        {
            isActive = false;
            isWarning = false;
            HideVisuals();
            return;
        }

        UpdateAnomalyVisuals();
        UpdateTargetWarning();
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
        currentTarget = null;
        isWarning = true;
        isActive = false;
        warningStartedAt = Time.time;
        SetVisualsVisible(true);
        SoundManager.Play(SoundEvent.CosmicAnomalyWarning);

        while (Time.time - warningStartedAt < WarningSeconds)
        {
            if (controller == null || controller.IsGameOver)
            {
                HideVisuals();
                yield break;
            }

            yield return null;
        }

        isWarning = false;
        isActive = true;
        activeStartedAt = Time.time;

        var absorptions = 0;
        var nextAbsorptionAt = Time.time + 2.8f;
        while (Time.time - activeStartedAt < ActiveSeconds)
        {
            if (controller == null || controller.IsGameOver)
            {
                HideVisuals();
                yield break;
            }

            if (absorptions < MaxAbsorptionsPerEvent && Time.time >= nextAbsorptionAt)
            {
                currentTarget = FindAbsorptionTarget();
                if (currentTarget != null && CanAbsorb(currentTarget))
                {
                    SoundManager.Play(SoundEvent.CosmicAnomalyAbsorb);
                    currentTarget.TryStartBlackHoleAbsorption(anomalyPosition, AbsorptionDurationSeconds);
                    absorptions++;
                    yield return PulseCoreRoutine();
                }

                nextAbsorptionAt = Time.time + 3.2f;
            }

            yield return null;
        }

        isActive = false;
        isWarning = false;
        currentTarget = null;
        HideVisuals();
    }

    private void UpdateAnomalyVisuals()
    {
        var warningProgress = isWarning ? Mathf.Clamp01((Time.time - warningStartedAt) / WarningSeconds) : 1f;
        var activeProgress = isActive ? Mathf.Clamp01((Time.time - activeStartedAt) / ActiveSeconds) : 0f;
        var pulse = Mathf.Sin(Time.time * (isWarning ? 7.2f : 5.4f)) * 0.5f + 0.5f;
        var charge = isWarning ? Mathf.Lerp(0.35f, 1f, warningProgress) : Mathf.Lerp(1f, 0.72f, activeProgress);

        outerRing.transform.position = anomalyPosition;
        innerRing.transform.position = anomalyPosition;
        core.transform.position = anomalyPosition;

        outerRing.transform.localScale = Vector3.one * Mathf.Lerp(1.35f, 2.35f, charge + pulse * 0.12f);
        innerRing.transform.localScale = new Vector3(Mathf.Lerp(0.98f, 1.42f, charge), Mathf.Lerp(0.32f, 0.48f, pulse), 1f);
        core.transform.localScale = Vector3.one * Mathf.Lerp(0.34f, 0.52f, pulse);

        outerRing.color = new Color(0.52f, 0.18f, 1f, Mathf.Lerp(0.08f, 0.28f, charge));
        innerRing.color = new Color(0.86f, 0.42f, 1f, Mathf.Lerp(0.2f, 0.56f, charge));
        core.color = new Color(0.06f, 0.01f, 0.1f, Mathf.Lerp(0.62f, 0.9f, charge));
    }

    private void UpdateTargetWarning()
    {
        if (!isActive)
        {
            SetTargetVisuals(false);
            return;
        }

        currentTarget = FindAbsorptionTarget();
        if (currentTarget == null)
        {
            SetTargetVisuals(false);
            return;
        }

        var pulse = Mathf.Sin(Time.time * 8.8f) * 0.5f + 0.5f;
        targetMarker.enabled = true;
        targetMarker.transform.position = currentTarget.transform.position;
        targetMarker.transform.localScale = Vector3.one * BallConfig.GetDiameter(currentTarget.Level) * Mathf.Lerp(1.08f, 1.24f, pulse);
        targetMarker.color = new Color(0.8f, 0.28f, 1f, Mathf.Lerp(0.12f, 0.32f, pulse));

        var start = anomalyPosition;
        var end = (Vector2)currentTarget.transform.position;
        var direction = end - start;
        var midpoint = (start + end) * 0.5f;
        beamRenderer.enabled = true;
        beamRenderer.transform.position = new Vector3(midpoint.x, midpoint.y, 0f);
        beamRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        beamRenderer.transform.localScale = new Vector3(direction.magnitude, Mathf.Lerp(0.014f, 0.032f, pulse), 1f);
        beamRenderer.color = new Color(0.82f, 0.22f, 1f, Mathf.Lerp(0.06f, 0.24f, pulse));
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

    private Ball FindAbsorptionTarget()
    {
        Ball bestTarget = null;
        var bestScore = float.MinValue;
        var balls = controller.ActiveBalls;

        for (var i = 0; i < balls.Count; i++)
        {
            var ball = balls[i];
            if (!CanAbsorb(ball))
            {
                continue;
            }

            var distance = Vector2.Distance(anomalyPosition, ball.transform.position);
            var distanceScore = 1f - Mathf.Clamp01(distance / AbsorptionRadius);
            var levelPreference = Mathf.InverseLerp(MaxAbsorbLevel, 1f, ball.Level) * 0.22f;
            var score = distanceScore + levelPreference;

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = ball;
            }
        }

        return bestTarget;
    }

    private bool CanPull(Ball ball)
    {
        return ball != null && !ball.IsMerging && ball.Level <= 6;
    }

    private bool CanAbsorb(Ball ball)
    {
        if (ball == null || ball.IsMerging || ball.IsPreMerging || ball.Level > MaxAbsorbLevel)
        {
            return false;
        }

        var distance = Vector2.Distance(anomalyPosition, ball.transform.position);
        return distance > MinDistance && distance <= AbsorptionRadius;
    }

    private static float GetLevelPullFactor(int level)
    {
        if (level <= 2)
        {
            return 1f;
        }

        if (level <= 4)
        {
            return 0.72f;
        }

        return 0.36f;
    }

    private void CreateVisuals()
    {
        outerRing = CreateRenderer("Cosmic Anomaly Outer Ring", CircleSpriteCache.Circle, 21);
        innerRing = CreateRenderer("Cosmic Anomaly Inner Ring", CircleSpriteCache.Circle, 22);
        core = CreateRenderer("Cosmic Anomaly Core", CircleSpriteCache.Circle, 23);
        targetMarker = CreateRenderer("Cosmic Anomaly Target Marker", CircleSpriteCache.Circle, 25);
        beamRenderer = CreateRenderer("Cosmic Anomaly Beam", CircleSpriteCache.Square, 24);
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

    private void SetVisualsVisible(bool visible)
    {
        outerRing.enabled = visible;
        innerRing.enabled = visible;
        core.enabled = visible;
    }

    private void SetTargetVisuals(bool visible)
    {
        targetMarker.enabled = visible;
        beamRenderer.enabled = visible;
    }

    private void HideVisuals()
    {
        SetVisualsVisible(false);
        SetTargetVisuals(false);
    }
}
