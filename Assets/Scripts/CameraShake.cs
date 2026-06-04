using System.Collections;
using UnityEngine;

public sealed class CameraShake : MonoBehaviour
{
    private Coroutine shakeRoutine;
    private Vector3 basePosition;

    private void Awake()
    {
        basePosition = transform.localPosition;
    }

    public void Shake(float duration, float strength)
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            transform.localPosition = basePosition;
        }

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var fade = 1f - Mathf.Clamp01(elapsed / duration);
            transform.localPosition = basePosition + (Vector3)Random.insideUnitCircle * strength * fade;
            yield return null;
        }

        transform.localPosition = basePosition;
        shakeRoutine = null;
    }
}
