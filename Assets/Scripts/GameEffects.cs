using System.Collections;
using UnityEngine;

public sealed class GameEffects : MonoBehaviour
{
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

        if (level >= 6)
        {
            cameraShake.Shake(0.12f, Mathf.Min(0.035f + level * 0.006f, 0.09f));
        }
    }

    private IEnumerator MergeFlashRoutine(Vector2 position, int level)
    {
        var flash = new GameObject("Merge Pop");
        flash.transform.position = position;

        var renderer = flash.AddComponent<SpriteRenderer>();
        renderer.sprite = CircleSpriteCache.Circle;
        renderer.color = CircleSpriteCache.GetBallColor(level);
        renderer.sortingOrder = 30;

        var startScale = Vector3.one * Ball.GetDiameter(level);
        var endScale = startScale * (level >= 6 ? 1.8f : 1.55f);
        var elapsed = 0f;
        const float duration = 0.22f;

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
        textMesh.fontSize = level >= 6 ? 58 : 44;
        textMesh.characterSize = 0.045f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;

        var renderer = scoreObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 50;

        var startPosition = scoreObject.transform.position;
        var endPosition = startPosition + Vector3.up * 0.55f;
        var startScale = Vector3.one * (level >= 6 ? 1.2f : 1f);
        var elapsed = 0f;
        const float duration = 0.75f;

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
        var main = particles.main;
        main.duration = 0.35f;
        main.loop = false;
        main.startLifetime = 0.32f;
        main.startSpeed = 1.7f + level * 0.09f;
        main.startSize = level >= 6 ? 0.11f : 0.08f;
        main.startColor = CircleSpriteCache.GetBallColor(level);
        main.maxParticles = level >= 6 ? 32 : 24;

        var emission = particles.emission;
        emission.enabled = false;

        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.25f;

        particles.Emit(Mathf.Clamp(8 + level * 2, 10, level >= 6 ? 32 : 24));
        Destroy(particleObject, 1f);
    }
}
