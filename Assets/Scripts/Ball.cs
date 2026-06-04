using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public sealed class Ball : MonoBehaviour
{
    private const float SpawnGraceSeconds = 1.25f;
    private const float SettledVelocity = 0.55f;
    private const float SettledAngularVelocity = 75f;

    private GameController controller;
    private Rigidbody2D body;
    private CircleCollider2D circleCollider;
    private SpriteRenderer spriteRenderer;
    private bool isMerging;
    private Vector3 targetScale;

    public int MergeId { get; private set; }
    public int Level { get; private set; }
    public float SpawnedAt { get; private set; }
    public float Radius => transform.lossyScale.x * 0.5f;
    public bool IsMerging => isMerging;

    public void Initialize(int level, GameController gameController)
    {
        Level = level;
        controller = gameController;
        MergeId = controller.GetNextBallId();
        SpawnedAt = Time.time;

        body = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        targetScale = Vector3.one * GetDiameter(Level);
        transform.localScale = targetScale;

        var physicsT = GetPhysicsT(Level);
        body.useAutoMass = false;
        body.gravityScale = Mathf.Lerp(1.0f, 1.08f, physicsT);
        body.mass = CalculateMass(Level);
        body.linearDamping = Mathf.Lerp(0.025f, 0.12f, physicsT);
        body.angularDamping = Mathf.Lerp(0.08f, 0.32f, physicsT);
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        circleCollider.radius = 0.5f;
        circleCollider.density = Mathf.Lerp(0.65f, 1.55f, physicsT);
        circleCollider.sharedMaterial = PhysicsMaterials.GetBallMaterial(Level);

        spriteRenderer.sprite = CircleSpriteCache.Circle;
        spriteRenderer.color = CircleSpriteCache.GetBallColor(Level);
        spriteRenderer.sortingOrder = 5 + Level;

        controller.RegisterBall(this);
    }

    public static float GetDiameter(int level)
    {
        return Mathf.Min(0.72f + (level - 1) * 0.12f, 1.85f);
    }

    private static float CalculateMass(int level)
    {
        var radius = GetDiameter(level) * 0.5f;
        var area = Mathf.PI * radius * radius;
        var density = Mathf.Lerp(0.65f, 1.55f, GetPhysicsT(level));
        return Mathf.Clamp(area * density, 0.22f, 5.2f);
    }

    private static float GetPhysicsT(int level)
    {
        return Mathf.Clamp01((level - 1) / 9f);
    }

    public bool IsEligibleForDanger(float dangerLineY)
    {
        if (isMerging || Time.time - SpawnedAt < SpawnGraceSeconds)
        {
            return false;
        }

        if (body == null)
        {
            return false;
        }

        var settled = body.linearVelocity.sqrMagnitude <= SettledVelocity * SettledVelocity
            && Mathf.Abs(body.angularVelocity) <= SettledAngularVelocity;

        return settled && transform.position.y + Radius >= dangerLineY;
    }

    public void MarkMerging()
    {
        isMerging = true;
        if (circleCollider != null)
        {
            circleCollider.enabled = false;
        }
    }

    public void PlayPop()
    {
        StopAllCoroutines();
        StartCoroutine(PopRoutine());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isMerging || controller == null || controller.IsGameOver)
        {
            return;
        }

        var other = collision.collider.GetComponent<Ball>();
        if (other == null || other == this || other.isMerging || other.Level != Level)
        {
            return;
        }

        if (MergeId > other.MergeId)
        {
            return;
        }

        controller.MergeBalls(this, other);
    }

    private void OnDestroy()
    {
        if (controller != null)
        {
            controller.UnregisterBall(this);
        }
    }

    private IEnumerator PopRoutine()
    {
        var elapsed = 0f;
        const float duration = 0.16f;
        var startScale = targetScale * 0.5f;
        var overshootScale = targetScale * 1.13f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, overshootScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(overshootScale, targetScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
