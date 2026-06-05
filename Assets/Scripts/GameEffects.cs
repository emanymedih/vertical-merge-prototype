using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GameEffects : MonoBehaviour
{
    [SerializeField] private int highImpactLevel = 6;
    [SerializeField] private int peakImpactLevel = 8;
    [SerializeField] private float baseMergeFlashDuration = 0.22f;
    [SerializeField] private float floatingScoreDuration = 0.75f;
    [SerializeField] private float highLevelShakeDuration = 0.12f;
    [SerializeField] private float peakLevelShakeDuration = 0.16f;

    private CameraShake cameraShake;
    private readonly Queue<SpriteRenderer> circleRendererPool = new Queue<SpriteRenderer>();
    private readonly Queue<TextMesh> floatingScorePool = new Queue<TextMesh>();
    private readonly Queue<ParticleSystem> particlePool = new Queue<ParticleSystem>();

    public void Initialize(Camera mainCamera)
    {
        cameraShake = mainCamera.gameObject.AddComponent<CameraShake>();
    }

    public void PlayMerge(Vector2 position, int level, int score)
    {
        var feel = CosmicBodyFeelDatabase.Get(level);
        StartCoroutine(MergeFlashRoutine(position, level, feel));
        if (score > 0)
        {
            StartCoroutine(FloatingScoreRoutine(position, score, level, feel));
        }

        PlayParticles(position, level, feel);

        if (level >= highImpactLevel)
        {
            var shakeStrength = feel.ShakeStrength;
            cameraShake.Shake(level >= peakImpactLevel ? peakLevelShakeDuration : highLevelShakeDuration, shakeStrength);
        }
    }

    public void PlayCriticalMerge(Vector2 position, int level, int score)
    {
        PlayMerge(position, level, score);
        StartCoroutine(SpecialPulseRoutine(position, level, new Color(0.92f, 0.98f, 1f), new Color(1f, 0.46f, 0.92f), 0.34f, 3.2f));
        StartCoroutine(FloatingMessageRoutine(position + Vector2.up * 0.36f, "Critical Merge!", new Color(1f, 0.82f, 1f), 0.86f));
        cameraShake?.Shake(0.18f, Mathf.Max(0.12f, CosmicBodyFeelDatabase.Get(level).ShakeStrength * 1.24f));
    }

    public void PlayCometImpact(Vector2 position, int targetLevel)
    {
        StartCoroutine(SpecialPulseRoutine(position, Mathf.Max(2, targetLevel), new Color(0.8f, 1f, 1f), new Color(0.22f, 0.86f, 1f), 0.26f, 2.35f));
        StartCoroutine(FloatingMessageRoutine(position + Vector2.up * 0.28f, "Comet Save!", new Color(0.76f, 1f, 1f), 0.72f));
        PlayParticles(position, Mathf.Max(2, targetLevel), CosmicBodyFeelDatabase.Get(Mathf.Max(2, targetLevel)));
        cameraShake?.Shake(0.1f, 0.055f);
    }

    public void PlayAnomalyEvaded(Vector2 position, int bonusScore, int multiplier)
    {
        StartCoroutine(SpecialPulseRoutine(position, 4, new Color(0.76f, 1f, 0.62f), new Color(1f, 0.84f, 0.28f), 0.24f, 2.25f));
        StartCoroutine(FloatingMessageRoutine(position + Vector2.up * 0.34f, $"Evaded! x{multiplier} +{bonusScore}", new Color(1f, 0.9f, 0.42f), 0.92f));
        PlayEvadeSparks(position);
        cameraShake?.Shake(0.08f, 0.035f);
    }

    public void PlayAnomalyConsumed(Vector2 position)
    {
        StartCoroutine(SpecialPulseRoutine(position, 4, new Color(0.16f, 0.02f, 0.22f), new Color(0.72f, 0.28f, 1f), 0.24f, 1.7f));
        PlayConsumeParticles(position);
    }

    public void PlayStressRelief()
    {
        StartCoroutine(FloatingMessageRoutine(new Vector2(0f, 1.1f), "Saved!", new Color(0.72f, 1f, 1f), 0.68f));
    }

    private IEnumerator MergeFlashRoutine(Vector2 position, int level, CosmicBodyFeel feel)
    {
        var coreRenderer = GetCircleRenderer("Merge Core Flash", 30);
        var coreFlash = coreRenderer.gameObject;
        coreFlash.transform.position = position;
        coreRenderer.sprite = CircleSpriteCache.Circle;
        var flashColor = Color.Lerp(CircleSpriteCache.GetBallColor(level), CosmicBodyConfig.GetGlowColor(level), level >= highImpactLevel ? 0.45f : 0.2f);
        coreRenderer.color = flashColor;

        var ringRenderer = GetCircleRenderer("Merge Shockwave Ring", 29);
        var ring = ringRenderer.gameObject;
        ring.transform.position = position;
        ringRenderer.sprite = CircleSpriteCache.Circle;
        ringRenderer.color = CosmicBodyConfig.GetGlowColor(level);

        var startScale = Vector3.one * Ball.GetDiameter(level);
        var coreEndScale = startScale * feel.FlashScale;
        var ringStartScale = startScale * 0.82f;
        var ringEndScale = startScale * feel.RingScale;
        var elapsed = 0f;
        var duration = level >= peakImpactLevel ? 0.28f : level >= highImpactLevel ? 0.25f : baseMergeFlashDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            var coreT = AnimationEasing.EaseOutCubic(t);
            var ringT = AnimationEasing.EaseOutCubic(t);
            coreFlash.transform.localScale = Vector3.Lerp(startScale * 0.54f, coreEndScale, coreT);
            ring.transform.localScale = Vector3.Lerp(ringStartScale, ringEndScale, ringT);

            var coreColor = coreRenderer.color;
            coreColor.a = Mathf.Lerp(level >= highImpactLevel ? 0.9f : 0.72f, 0f, coreT);
            coreRenderer.color = coreColor;

            var ringColor = ringRenderer.color;
            ringColor.a = Mathf.Lerp(level >= peakImpactLevel ? 0.66f : level >= highImpactLevel ? 0.48f : 0.32f, 0f, t);
            ringRenderer.color = ringColor;
            yield return null;
        }

        ReturnCircleRenderer(coreRenderer);
        ReturnCircleRenderer(ringRenderer);
    }

    private IEnumerator SpecialPulseRoutine(Vector2 position, int level, Color coreColor, Color ringColor, float duration, float ringScale)
    {
        var coreRenderer = GetCircleRenderer("Special Event Core Flash", 32);
        var ringRenderer = GetCircleRenderer("Special Event Shockwave", 31);

        coreRenderer.transform.position = position;
        ringRenderer.transform.position = position;
        coreRenderer.sprite = CircleSpriteCache.Circle;
        ringRenderer.sprite = CircleSpriteCache.Circle;

        var startScale = Vector3.one * Ball.GetDiameter(level) * 0.6f;
        var coreEndScale = Vector3.one * Ball.GetDiameter(level) * 1.6f;
        var ringEndScale = Vector3.one * Ball.GetDiameter(level) * ringScale;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            var eased = AnimationEasing.EaseOutCubic(t);

            coreRenderer.transform.localScale = Vector3.Lerp(startScale, coreEndScale, eased);
            ringRenderer.transform.localScale = Vector3.Lerp(startScale * 0.8f, ringEndScale, eased);

            coreColor.a = Mathf.Lerp(0.78f, 0f, eased);
            ringColor.a = Mathf.Lerp(0.56f, 0f, t);
            coreRenderer.color = coreColor;
            ringRenderer.color = ringColor;
            yield return null;
        }

        ReturnCircleRenderer(coreRenderer);
        ReturnCircleRenderer(ringRenderer);
    }

    private IEnumerator FloatingScoreRoutine(Vector2 position, int score, int level, CosmicBodyFeel feel)
    {
        var textMesh = GetFloatingScore();
        var scoreObject = textMesh.gameObject;
        scoreObject.transform.position = new Vector3(position.x, position.y + Ball.GetDiameter(level) * 0.45f, -0.2f);

        textMesh.text = $"+{score}";
        textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textMesh.fontSize = GetFloatingScoreFontSize(level);
        textMesh.characterSize = 0.045f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = level >= peakImpactLevel
            ? new Color(0.9f, 1f, 1f)
            : level >= highImpactLevel ? new Color(1f, 0.92f, 0.62f) : Color.white;

        var renderer = scoreObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 50;

        var startPosition = scoreObject.transform.position;
        var endPosition = startPosition + Vector3.up * (level >= highImpactLevel ? 0.74f : 0.58f);
        var baseScale = Vector3.one * GetFloatingScoreScale(level);
        var startScale = baseScale * 0.55f;
        var popScale = baseScale * (level >= peakImpactLevel ? 1.34f : level >= highImpactLevel ? 1.22f : 1.14f);
        var elapsed = 0f;
        var duration = floatingScoreDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            scoreObject.transform.position = Vector3.Lerp(startPosition, endPosition, AnimationEasing.EaseOutCubic(t));
            if (t < 0.26f)
            {
                scoreObject.transform.localScale = Vector3.LerpUnclamped(startScale, popScale, AnimationEasing.EaseOutBack(t / 0.26f));
            }
            else
            {
                scoreObject.transform.localScale = Vector3.Lerp(popScale, baseScale * 0.92f, AnimationEasing.EaseInOutSine((t - 0.26f) / 0.74f));
            }

            var color = textMesh.color;
            color.a = Mathf.Lerp(1f, 0f, Mathf.SmoothStep(0.62f, 1f, t));
            textMesh.color = color;
            yield return null;
        }

        ReturnFloatingScore(textMesh);
    }

    private IEnumerator FloatingMessageRoutine(Vector2 position, string message, Color color, float duration)
    {
        var textMesh = GetFloatingScore();
        var textObject = textMesh.gameObject;
        textObject.transform.position = new Vector3(position.x, position.y, -0.3f);

        textMesh.text = message;
        textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textMesh.fontSize = 46;
        textMesh.characterSize = 0.045f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;

        var renderer = textObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 52;

        var startPosition = textObject.transform.position;
        var endPosition = startPosition + Vector3.up * 0.54f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            textObject.transform.position = Vector3.Lerp(startPosition, endPosition, AnimationEasing.EaseOutCubic(t));
            textObject.transform.localScale = Vector3.LerpUnclamped(Vector3.one * 0.72f, Vector3.one * 1.08f, AnimationEasing.EaseOutBack(Mathf.Clamp01(t / 0.22f)));

            var textColor = textMesh.color;
            textColor.a = Mathf.Lerp(1f, 0f, Mathf.SmoothStep(0.62f, 1f, t));
            textMesh.color = textColor;
            yield return null;
        }

        ReturnFloatingScore(textMesh);
    }

    private void PlayParticles(Vector2 position, int level, CosmicBodyFeel feel)
    {
        var particles = GetParticles();
        var particleObject = particles.gameObject;
        particleObject.transform.position = position;

        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = particles.main;
        main.playOnAwake = false;
        main.duration = 0.35f;
        main.loop = false;
        main.startLifetime = level >= peakImpactLevel ? 0.42f : level >= highImpactLevel ? 0.36f : 0.3f;
        main.startSpeed = level >= peakImpactLevel ? 2.35f + level * 0.08f : level >= highImpactLevel ? 2f + level * 0.08f : 1.62f + level * 0.08f;
        main.startSize = level >= peakImpactLevel ? 0.15f : level >= highImpactLevel ? 0.12f : 0.075f;
        main.startColor = Color.Lerp(CircleSpriteCache.GetBallColor(level), CosmicBodyConfig.GetGlowColor(level), 0.35f);
        main.maxParticles = Mathf.Max(feel.ParticlesCount, 12);

        var emission = particles.emission;
        emission.enabled = false;

        var velocityLimit = particles.limitVelocityOverLifetime;
        velocityLimit.enabled = true;
        velocityLimit.limit = 3.4f;
        velocityLimit.dampen = 0.58f;
        velocityLimit.drag = level >= highImpactLevel ? 1.5f : 1.1f;

        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.25f;

        particles.Emit(Mathf.Clamp(feel.ParticlesCount, 10, Mathf.Max(feel.ParticlesCount, 12)));
        StartCoroutine(ReturnParticlesRoutine(particles, Mathf.Max(main.startLifetime.constantMax, 0.6f) + 0.2f));
    }

    private void PlayEvadeSparks(Vector2 position)
    {
        var particles = GetParticles();
        particles.gameObject.transform.position = position;
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = particles.main;
        main.playOnAwake = false;
        main.duration = 0.26f;
        main.loop = false;
        main.startLifetime = 0.34f;
        main.startSpeed = 2.65f;
        main.startSize = 0.095f;
        main.startColor = new Color(1f, 0.88f, 0.34f, 1f);
        main.maxParticles = 32;

        var emission = particles.emission;
        emission.enabled = false;

        var velocityLimit = particles.limitVelocityOverLifetime;
        velocityLimit.enabled = true;
        velocityLimit.limit = 3.2f;
        velocityLimit.dampen = 0.68f;
        velocityLimit.drag = 1.7f;

        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.18f;

        particles.Emit(28);
        StartCoroutine(ReturnParticlesRoutine(particles, 0.68f));
    }

    private void PlayConsumeParticles(Vector2 position)
    {
        var particles = GetParticles();
        particles.gameObject.transform.position = position;
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = particles.main;
        main.playOnAwake = false;
        main.duration = 0.28f;
        main.loop = false;
        main.startLifetime = 0.46f;
        main.startSpeed = 1.55f;
        main.startSize = 0.12f;
        main.startColor = new Color(0.62f, 0.22f, 1f, 0.94f);
        main.maxParticles = 36;

        var emission = particles.emission;
        emission.enabled = false;

        var velocityLimit = particles.limitVelocityOverLifetime;
        velocityLimit.enabled = true;
        velocityLimit.limit = 1.8f;
        velocityLimit.dampen = 0.72f;
        velocityLimit.drag = 2.2f;

        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.12f;

        particles.Emit(30);
        StartCoroutine(ReturnParticlesRoutine(particles, 0.82f));
    }

    private float GetFlashScale(int level)
    {
        if (level >= peakImpactLevel)
        {
            return 2.12f;
        }

        return level >= highImpactLevel ? 1.82f : 1.5f;
    }

    private float GetRingScale(int level)
    {
        if (level >= peakImpactLevel)
        {
            return 2.85f;
        }

        return level >= highImpactLevel ? 2.35f : 1.86f;
    }

    private int GetFloatingScoreFontSize(int level)
    {
        if (level >= peakImpactLevel)
        {
            return 74;
        }

        return level >= highImpactLevel ? 60 : 42;
    }

    private float GetFloatingScoreScale(int level)
    {
        if (level >= peakImpactLevel)
        {
            return 1.48f;
        }

        return level >= highImpactLevel ? 1.24f : 1f;
    }

    private SpriteRenderer GetCircleRenderer(string objectName, int sortingOrder)
    {
        SpriteRenderer renderer;
        if (circleRendererPool.Count > 0)
        {
            renderer = circleRendererPool.Dequeue();
            renderer.gameObject.name = objectName;
        }
        else
        {
            var effectObject = new GameObject(objectName);
            renderer = effectObject.AddComponent<SpriteRenderer>();
        }

        renderer.gameObject.SetActive(true);
        renderer.sortingOrder = sortingOrder;
        renderer.transform.localRotation = Quaternion.identity;
        return renderer;
    }

    private void ReturnCircleRenderer(SpriteRenderer renderer)
    {
        renderer.gameObject.SetActive(false);
        circleRendererPool.Enqueue(renderer);
    }

    private TextMesh GetFloatingScore()
    {
        TextMesh textMesh;
        if (floatingScorePool.Count > 0)
        {
            textMesh = floatingScorePool.Dequeue();
        }
        else
        {
            var scoreObject = new GameObject("Floating Score");
            textMesh = scoreObject.AddComponent<TextMesh>();
        }

        textMesh.gameObject.SetActive(true);
        return textMesh;
    }

    private void ReturnFloatingScore(TextMesh textMesh)
    {
        textMesh.gameObject.SetActive(false);
        floatingScorePool.Enqueue(textMesh);
    }

    private ParticleSystem GetParticles()
    {
        ParticleSystem particles;
        if (particlePool.Count > 0)
        {
            particles = particlePool.Dequeue();
        }
        else
        {
            var particleObject = new GameObject("Merge Particles");
            particles = particleObject.AddComponent<ParticleSystem>();
        }

        particles.gameObject.SetActive(true);
        return particles;
    }

    private IEnumerator ReturnParticlesRoutine(ParticleSystem particles, float delay)
    {
        yield return new WaitForSeconds(delay);
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particles.gameObject.SetActive(false);
        particlePool.Enqueue(particles);
    }
}
