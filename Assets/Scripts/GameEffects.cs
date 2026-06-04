using System.Collections;
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

    public void Initialize(Camera mainCamera)
    {
        cameraShake = mainCamera.gameObject.AddComponent<CameraShake>();
    }

    public void PlayMerge(Vector2 position, int level, int score)
    {
        StartCoroutine(MergeFlashRoutine(position, level));
        StartCoroutine(FloatingScoreRoutine(position, score, level));
        PlayParticles(position, level);

        if (level >= highImpactLevel)
        {
            var shakeStrength = level >= peakImpactLevel
                ? Mathf.Min(0.052f + level * 0.008f, 0.11f)
                : Mathf.Min(0.03f + level * 0.005f, 0.065f);
            cameraShake.Shake(level >= peakImpactLevel ? peakLevelShakeDuration : highLevelShakeDuration, shakeStrength);
        }
    }

    private IEnumerator MergeFlashRoutine(Vector2 position, int level)
    {
        var flash = new GameObject("Merge Pop");
        flash.transform.position = position;

        var renderer = flash.AddComponent<SpriteRenderer>();
        renderer.sprite = CircleSpriteCache.Circle;
        var flashColor = Color.Lerp(CircleSpriteCache.GetBallColor(level), CosmicBodyConfig.GetGlowColor(level), level >= highImpactLevel ? 0.45f : 0.2f);
        renderer.color = flashColor;
        renderer.sortingOrder = 30;

        var startScale = Vector3.one * Ball.GetDiameter(level);
        var endScale = startScale * GetFlashScale(level);
        var elapsed = 0f;
        var duration = baseMergeFlashDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            flash.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            var color = renderer.color;
            color.a = 1f - t;
            renderer.color = color;
            yield return null;
        }

        Destroy(flash);
    }

    private IEnumerator FloatingScoreRoutine(Vector2 position, int score, int level)
    {
        var scoreObject = new GameObject("Floating Score");
        scoreObject.transform.position = new Vector3(position.x, position.y + Ball.GetDiameter(level) * 0.45f, -0.2f);

        var textMesh = scoreObject.AddComponent<TextMesh>();
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
        var endPosition = startPosition + Vector3.up * 0.55f;
        var startScale = Vector3.one * GetFloatingScoreScale(level);
        var elapsed = 0f;
        var duration = floatingScoreDuration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            scoreObject.transform.position = Vector3.Lerp(startPosition, endPosition, Mathf.SmoothStep(0f, 1f, t));
            scoreObject.transform.localScale = Vector3.Lerp(startScale, Vector3.one * 0.92f, t);

            var color = textMesh.color;
            color.a = 1f - t;
            textMesh.color = color;
            yield return null;
        }

        Destroy(scoreObject);
    }

    private void PlayParticles(Vector2 position, int level)
    {
        var particleObject = new GameObject("Merge Particles");
        particleObject.transform.position = position;

        var particles = particleObject.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = particles.main;
        main.playOnAwake = false;
        main.duration = 0.35f;
        main.loop = false;
        main.startLifetime = 0.32f;
        main.startSpeed = 1.7f + level * 0.09f;
        main.startSize = level >= peakImpactLevel ? 0.13f : level >= highImpactLevel ? 0.11f : 0.08f;
        main.startColor = Color.Lerp(CircleSpriteCache.GetBallColor(level), CosmicBodyConfig.GetGlowColor(level), 0.35f);
        main.maxParticles = level >= peakImpactLevel ? 36 : level >= highImpactLevel ? 30 : 22;

        var emission = particles.emission;
        emission.enabled = false;

        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.25f;

        var maxBurst = level >= peakImpactLevel ? 36 : level >= highImpactLevel ? 30 : 22;
        particles.Emit(Mathf.Clamp(8 + level * 2, 10, maxBurst));
        Destroy(particleObject, 1f);
    }

    private float GetFlashScale(int level)
    {
        if (level >= peakImpactLevel)
        {
            return 2.12f;
        }

        return level >= highImpactLevel ? 1.82f : 1.5f;
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
}
