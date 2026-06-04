using System.Collections;
using UnityEngine;

public sealed class GameEffects : MonoBehaviour
{
    private CameraShake cameraShake;

    public void Initialize(Camera mainCamera)
    {
        cameraShake = mainCamera.gameObject.AddComponent<CameraShake>();
    }

    public void PlayMerge(Vector2 position, int level)
    {
        StartCoroutine(MergeFlashRoutine(position, level));
        PlayParticles(position, level);

        if (level >= 5)
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
        var endScale = startScale * 1.55f;
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

    private void PlayParticles(Vector2 position, int level)
    {
        var particleObject = new GameObject("Merge Particles");
        particleObject.transform.position = position;

        var particles = particleObject.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.duration = 0.35f;
        main.loop = false;
        main.startLifetime = 0.32f;
        main.startSpeed = 1.8f + level * 0.08f;
        main.startSize = 0.08f;
        main.startColor = CircleSpriteCache.GetBallColor(level);
        main.maxParticles = 24;

        var emission = particles.emission;
        emission.enabled = false;

        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.25f;

        particles.Emit(Mathf.Clamp(8 + level * 2, 10, 24));
        Destroy(particleObject, 1f);
    }
}
