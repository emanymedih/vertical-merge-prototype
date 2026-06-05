using System.Collections;
using UnityEngine;

public sealed class BlackHoleGravityField : MonoBehaviour
{
    private const float GravityRadius = 2.4f;
    private const float PullAcceleration = 0.72f;
    private const float MinDistance = 0.28f;
    private const float AbsorptionRadius = 2.65f;
    private const float AbsorptionCycleSeconds = 15f;
    private const float AbsorptionWarningSeconds = 3f;
    private const float AbsorptionDurationSeconds = 0.82f;
    private const float TargetRefreshSeconds = 0.22f;
    private const int MaxAbsorbLevel = 5;

    private Ball source;
    private GameController controller;
    private SpriteRenderer outerAura;
    private SpriteRenderer innerAura;
    private SpriteRenderer chargeAura;
    private SpriteRenderer targetMarker;
    private SpriteRenderer beamRenderer;
    private Ball absorptionTarget;
    private float cycleStartedAt;
    private float nextTargetRefreshAt;
    private bool isAbsorbing;

    public void Initialize(Ball sourceBall, GameController gameController)
    {
        source = sourceBall;
        controller = gameController;
        cycleStartedAt = Time.time;
        CreateAura();
    }

    private void FixedUpdate()
    {
        if (source == null || controller == null || controller.IsGameOver)
        {
            return;
        }

        foreach (var target in controller.ActiveBalls)
        {
            if (!CanAttract(target))
            {
                continue;
            }

            var offset = (Vector2)transform.position - (Vector2)target.transform.position;
            var distance = offset.magnitude;
            if (distance <= MinDistance || distance > GravityRadius)
            {
                continue;
            }

            var falloff = 1f - Mathf.Clamp01(distance / GravityRadius);
            var levelFactor = GetLevelPullFactor(target.Level);
            var acceleration = PullAcceleration * falloff * falloff * levelFactor;
            target.TryApplyExternalForce(offset.normalized * acceleration * target.Mass);
        }
    }

    private void Update()
    {
        if (source == null || controller == null || controller.IsGameOver)
        {
            SetWarningVisuals(false, 0f);
            return;
        }

        if (outerAura == null || innerAura == null || chargeAura == null)
        {
            return;
        }

        var cycleElapsed = Mathf.Clamp(Time.time - cycleStartedAt, 0f, AbsorptionCycleSeconds);
        var cycleProgress = Mathf.Clamp01(cycleElapsed / AbsorptionCycleSeconds);
        var warningProgress = Mathf.InverseLerp(AbsorptionCycleSeconds - AbsorptionWarningSeconds, AbsorptionCycleSeconds, cycleElapsed);
        var isWarning = warningProgress > 0f;

        if (!isAbsorbing && isWarning && Time.time >= nextTargetRefreshAt)
        {
            nextTargetRefreshAt = Time.time + TargetRefreshSeconds;
            absorptionTarget = FindAbsorptionTarget();
            if (absorptionTarget != null && warningProgress < 0.18f)
            {
                SoundManager.Play(SoundEvent.BlackHoleWarning);
            }
        }

        UpdateAura(cycleProgress, warningProgress);
        UpdateTargetWarning(isWarning, warningProgress);

        if (!isAbsorbing && cycleElapsed >= AbsorptionCycleSeconds)
        {
            StartCoroutine(AbsorbCurrentTargetRoutine());
        }
    }

    private void UpdateAura(float cycleProgress, float warningProgress)
    {
        var pulse = Mathf.Sin(Time.time * 3.8f) * 0.5f + 0.5f;
        var warningPulse = Mathf.Sin(Time.time * Mathf.Lerp(5.2f, 9.4f, warningProgress)) * 0.5f + 0.5f;
        var chargeScale = Mathf.Lerp(1.05f, 1.42f, cycleProgress);

        outerAura.transform.localScale = Vector3.one * Mathf.Lerp(2.15f, 2.36f + warningProgress * 0.28f, Mathf.Max(pulse, warningPulse * warningProgress));
        innerAura.transform.localScale = new Vector3(Mathf.Lerp(1.48f, 1.62f + warningProgress * 0.22f, pulse), Mathf.Lerp(0.44f, 0.5f + warningProgress * 0.08f, pulse), 1f);
        chargeAura.transform.localScale = Vector3.one * chargeScale;

        outerAura.color = new Color(0.42f, 0.15f, 0.82f, Mathf.Lerp(0.08f, 0.16f + warningProgress * 0.16f, pulse));
        innerAura.color = new Color(0.8f, 0.42f, 1f, Mathf.Lerp(0.34f, 0.56f + warningProgress * 0.22f, pulse));
        chargeAura.color = new Color(0.72f, 0.22f, 1f, Mathf.Lerp(0.04f, 0.34f, cycleProgress) + warningPulse * warningProgress * 0.2f);
    }

    private void UpdateTargetWarning(bool isWarning, float warningProgress)
    {
        if (!isWarning || absorptionTarget == null || !CanAbsorb(absorptionTarget))
        {
            SetWarningVisuals(false, 0f);
            return;
        }

        var pulse = Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f;
        var targetScale = BallConfig.GetDiameter(absorptionTarget.Level) * Mathf.Lerp(1.12f, 1.34f, pulse);
        targetMarker.enabled = true;
        targetMarker.transform.position = absorptionTarget.transform.position;
        targetMarker.transform.localScale = Vector3.one * targetScale;
        targetMarker.color = new Color(0.86f, 0.35f, 1f, Mathf.Lerp(0.1f, 0.38f, warningProgress) + pulse * 0.08f);

        var start = (Vector2)transform.position;
        var end = (Vector2)absorptionTarget.transform.position;
        var midpoint = (start + end) * 0.5f;
        var direction = end - start;
        var distance = direction.magnitude;

        beamRenderer.enabled = true;
        beamRenderer.transform.position = new Vector3(midpoint.x, midpoint.y, transform.position.z);
        beamRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        beamRenderer.transform.localScale = new Vector3(distance, Mathf.Lerp(0.018f, 0.044f, warningProgress), 1f);
        beamRenderer.color = new Color(0.82f, 0.28f, 1f, Mathf.Lerp(0.06f, 0.34f, warningProgress) + pulse * 0.08f);
    }

    private void SetWarningVisuals(bool visible, float alpha)
    {
        if (targetMarker != null)
        {
            targetMarker.enabled = visible;
            if (visible)
            {
                targetMarker.color = new Color(0.86f, 0.35f, 1f, alpha);
            }
        }

        if (beamRenderer != null)
        {
            beamRenderer.enabled = visible;
            if (visible)
            {
                beamRenderer.color = new Color(0.82f, 0.28f, 1f, alpha);
            }
        }
    }

    private bool CanAttract(Ball target)
    {
        return target != null
            && target != source
            && !target.IsMerging
            && target.Level < 9;
    }

    private bool CanAbsorb(Ball target)
    {
        if (target == null || target == source || target.IsMerging || target.IsPreMerging || target.Level > MaxAbsorbLevel)
        {
            return false;
        }

        var distance = Vector2.Distance(transform.position, target.transform.position);
        return distance > MinDistance && distance <= AbsorptionRadius;
    }

    private Ball FindAbsorptionTarget()
    {
        Ball bestTarget = null;
        var bestScore = float.MinValue;

        foreach (var candidate in controller.ActiveBalls)
        {
            if (!CanAbsorb(candidate))
            {
                continue;
            }

            var distance = Vector2.Distance(transform.position, candidate.transform.position);
            var levelPreference = Mathf.InverseLerp(MaxAbsorbLevel, 1f, candidate.Level) * 0.35f;
            var distanceScore = 1f - Mathf.Clamp01(distance / AbsorptionRadius);
            var score = distanceScore + levelPreference;

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = candidate;
            }
        }

        return bestTarget;
    }

    private IEnumerator AbsorbCurrentTargetRoutine()
    {
        isAbsorbing = true;
        absorptionTarget = FindAbsorptionTarget();

        if (absorptionTarget != null && CanAbsorb(absorptionTarget))
        {
            SoundManager.Play(SoundEvent.BlackHoleAbsorb);
            absorptionTarget.TryStartBlackHoleAbsorption(transform.position, AbsorptionDurationSeconds);
            yield return PulseAbsorptionAuraRoutine();
        }
        else
        {
            yield return PulseEmptyAuraRoutine();
        }

        absorptionTarget = null;
        cycleStartedAt = Time.time;
        nextTargetRefreshAt = 0f;
        isAbsorbing = false;
        SetWarningVisuals(false, 0f);
    }

    private IEnumerator PulseAbsorptionAuraRoutine()
    {
        var elapsed = 0f;
        while (elapsed < AbsorptionDurationSeconds)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / AbsorptionDurationSeconds);
            var pulse = Mathf.Sin(t * Mathf.PI);
            outerAura.transform.localScale = Vector3.one * Mathf.Lerp(2.48f, 2.9f, pulse);
            innerAura.transform.localScale = new Vector3(Mathf.Lerp(1.72f, 2.08f, pulse), Mathf.Lerp(0.54f, 0.68f, pulse), 1f);
            chargeAura.transform.localScale = Vector3.one * Mathf.Lerp(1.38f, 1.86f, pulse);
            yield return null;
        }
    }

    private IEnumerator PulseEmptyAuraRoutine()
    {
        var elapsed = 0f;
        const float duration = 0.35f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var pulse = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
            outerAura.transform.localScale = Vector3.one * Mathf.Lerp(2.28f, 2.54f, pulse);
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (targetMarker != null)
        {
            Destroy(targetMarker.gameObject);
        }

        if (beamRenderer != null)
        {
            Destroy(beamRenderer.gameObject);
        }
    }

    private static float GetLevelPullFactor(int level)
    {
        if (level <= 3)
        {
            return 1f;
        }

        if (level <= 5)
        {
            return 0.58f;
        }

        return 0.24f;
    }

    private void CreateAura()
    {
        var outerObject = new GameObject("Black Hole Gravity Aura");
        outerObject.transform.SetParent(transform);
        outerObject.transform.localPosition = Vector3.zero;
        outerObject.transform.localRotation = Quaternion.identity;
        outerObject.transform.localScale = Vector3.one * 2.2f;
        outerAura = outerObject.AddComponent<SpriteRenderer>();
        outerAura.sprite = CircleSpriteCache.Circle;
        outerAura.sortingOrder = 2;
        outerAura.color = new Color(0.42f, 0.15f, 0.82f, 0.12f);

        var innerObject = new GameObject("Black Hole Accretion Pull");
        innerObject.transform.SetParent(transform);
        innerObject.transform.localPosition = Vector3.zero;
        innerObject.transform.localRotation = Quaternion.identity;
        innerObject.transform.localScale = new Vector3(1.55f, 0.46f, 1f);
        innerAura = innerObject.AddComponent<SpriteRenderer>();
        innerAura.sprite = CircleSpriteCache.Circle;
        innerAura.sortingOrder = 18;
        innerAura.color = new Color(0.8f, 0.42f, 1f, 0.44f);

        var chargeObject = new GameObject("Black Hole Event Horizon Charge");
        chargeObject.transform.SetParent(transform);
        chargeObject.transform.localPosition = Vector3.zero;
        chargeObject.transform.localRotation = Quaternion.identity;
        chargeObject.transform.localScale = Vector3.one * 1.08f;
        chargeAura = chargeObject.AddComponent<SpriteRenderer>();
        chargeAura.sprite = CircleSpriteCache.Circle;
        chargeAura.sortingOrder = 19;
        chargeAura.color = new Color(0.72f, 0.22f, 1f, 0.04f);

        var markerObject = new GameObject("Black Hole Absorption Target Marker");
        markerObject.transform.position = transform.position;
        markerObject.transform.rotation = Quaternion.identity;
        targetMarker = markerObject.AddComponent<SpriteRenderer>();
        targetMarker.sprite = CircleSpriteCache.Circle;
        targetMarker.sortingOrder = 24;
        targetMarker.enabled = false;

        var beamObject = new GameObject("Black Hole Absorption Beam");
        beamObject.transform.position = transform.position;
        beamObject.transform.rotation = Quaternion.identity;
        beamRenderer = beamObject.AddComponent<SpriteRenderer>();
        beamRenderer.sprite = CircleSpriteCache.Square;
        beamRenderer.sortingOrder = 23;
        beamRenderer.enabled = false;
    }
}
