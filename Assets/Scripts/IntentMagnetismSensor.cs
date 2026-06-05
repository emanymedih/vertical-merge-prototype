using System.Collections;
using UnityEngine;

public sealed class IntentMagnetismSensor : MonoBehaviour
{
    private const float ActiveSeconds = 1.2f;
    private const float MaxHorizontalSpeed = 0.55f;
    private const float NudgeAcceleration = 1.9f;

    private Ball owner;
    private CircleCollider2D triggerCollider;
    private Coroutine activeRoutine;

    public void Initialize(Ball ball, float radius)
    {
        owner = ball;
        triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = radius;
        triggerCollider.enabled = false;
    }

    public void Activate()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(ActiveRoutine());
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (owner == null || owner.IsMerging || owner.IsPreMerging || owner.Body == null || !triggerCollider.enabled)
        {
            return;
        }

        var target = other.GetComponent<Ball>();
        if (target == null || target == owner || target.IsMerging || target.IsPreMerging || target.Level != owner.Level)
        {
            return;
        }

        var offset = target.transform.position.x - owner.transform.position.x;
        if (Mathf.Abs(offset) < 0.02f)
        {
            return;
        }

        var velocity = owner.Body.linearVelocity;
        var targetVelocityX = Mathf.Clamp(offset * NudgeAcceleration, -MaxHorizontalSpeed, MaxHorizontalSpeed);
        velocity.x = Mathf.MoveTowards(velocity.x, targetVelocityX, NudgeAcceleration * Time.fixedDeltaTime);
        owner.Body.linearVelocity = velocity;
    }

    private IEnumerator ActiveRoutine()
    {
        triggerCollider.enabled = true;
        yield return new WaitForSeconds(ActiveSeconds);
        triggerCollider.enabled = false;
        activeRoutine = null;
    }
}
