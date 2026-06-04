using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public sealed class Ball : MonoBehaviour
{
    private const float SpawnGraceSeconds = 1.25f;
    private const float SettledVelocity = 0.55f;
    private const float SettledAngularVelocity = 75f;
    private const float NextDropVelocity = 1.05f;
    private const float ForcedNextDropSeconds = 1.35f;
    private const float BalancedStackX = 0.09f;
    private const float StackNudgeForce = 0.72f;
    private const float StackNudgeTorque = 0.16f;

    [SerializeField] private float popBaseOvershoot = 1.13f;
    [SerializeField] private float popMaxOvershoot = 1.22f;
    [SerializeField] private float resonanceNormalBrightness = 0.2f;
    [SerializeField] private float resonanceEmphasizedBrightness = 0.34f;
    [SerializeField] private float resonanceNormalMaxScale = 1.2f;
    [SerializeField] private float resonanceEmphasizedMaxScale = 1.28f;

    private GameController controller;
    private Rigidbody2D body;
    private CircleCollider2D circleCollider;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer resonanceGlow;
    private Color baseColor;
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

        targetScale = Vector3.one * BallConfig.GetDiameter(Level);
        transform.localScale = targetScale;

        body.useAutoMass = false;
        body.gravityScale = BallConfig.GetGravityScale(Level);
        body.mass = BallConfig.GetMass(Level);
        body.linearDamping = BallConfig.GetLinearDamping(Level);
        body.angularDamping = BallConfig.GetAngularDamping(Level);
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        circleCollider.radius = 0.5f;
        circleCollider.sharedMaterial = PhysicsMaterials.GetBallMaterial(Level);

        spriteRenderer.sprite = CircleSpriteCache.Circle;
        baseColor = CircleSpriteCache.GetBallColor(Level);
        spriteRenderer.color = baseColor;
        spriteRenderer.sortingOrder = 5 + Level;

        var bodyVisual = gameObject.AddComponent<CosmicBodyVisual>();
        bodyVisual.Initialize(Level, spriteRenderer);

        controller.RegisterBall(this);
    }

    public static float GetDiameter(int level)
    {
        return BallConfig.GetDiameter(level);
    }

    public bool IsReadyForNextDrop(float spawnY)
    {
        if (isMerging || body == null)
        {
            return true;
        }

        var age = Time.time - SpawnedAt;
        if (age >= ForcedNextDropSeconds)
        {
            return true;
        }

        var hasLeftSpawnArea = transform.position.y + Radius < spawnY - 0.85f;
        var slowedEnough = body.linearVelocity.sqrMagnitude <= NextDropVelocity * NextDropVelocity;
        return hasLeftSpawnArea && slowedEnough;
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

    public void PlayPop(float intensity = 1f)
    {
        StopAllCoroutines();
        StartCoroutine(PopRoutine(intensity));
    }

    public void SetResonance(float strength, bool emphasized)
    {
        if (isMerging || spriteRenderer == null)
        {
            return;
        }

        EnsureResonanceGlow();

        var clampedStrength = Mathf.Clamp01(strength);
        var brightness = emphasized ? resonanceEmphasizedBrightness : resonanceNormalBrightness;
        spriteRenderer.color = Color.Lerp(baseColor, Color.white, clampedStrength * brightness);

        var pulse = Mathf.Sin(Time.time * (emphasized ? 8.5f : 6.5f)) * 0.5f + 0.5f;
        var glowAlpha = Mathf.Lerp(0.12f, emphasized ? 0.34f : 0.22f, pulse) * clampedStrength;
        var maxGlowScale = emphasized ? resonanceEmphasizedMaxScale : resonanceNormalMaxScale;
        var glowScale = Mathf.Lerp(1.14f, maxGlowScale, pulse) + clampedStrength * 0.05f;

        resonanceGlow.color = new Color(1f, 1f, 1f, glowAlpha);
        resonanceGlow.transform.localScale = Vector3.one * glowScale;
        resonanceGlow.enabled = true;
    }

    public void ClearResonance()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = baseColor;
        }

        if (resonanceGlow != null)
        {
            resonanceGlow.enabled = false;
        }
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
            if (collision.relativeVelocity.sqrMagnitude > 0.18f)
            {
                SoundManager.Play(SoundEvent.CollisionSoft);
            }
            return;
        }

        if (MergeId > other.MergeId)
        {
            return;
        }

        controller.MergeBalls(this, other);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isMerging || body == null)
        {
            return;
        }

        var other = collision.collider.GetComponent<Ball>();
        if (other == null || other.isMerging || transform.position.y <= other.transform.position.y)
        {
            return;
        }

        var xOffset = transform.position.x - other.transform.position.x;
        if (Mathf.Abs(xOffset) > BalancedStackX)
        {
            return;
        }

        var direction = xOffset >= 0f ? 1f : -1f;
        if (Mathf.Abs(xOffset) < 0.012f)
        {
            direction = MergeId > other.MergeId ? 1f : -1f;
        }

        var levelFactor = Mathf.Clamp01(Level / 8f);
        var force = Mathf.Lerp(StackNudgeForce, StackNudgeForce * 0.45f, levelFactor);
        body.AddForce(Vector2.right * direction * force, ForceMode2D.Force);
        body.AddTorque(-direction * StackNudgeTorque, ForceMode2D.Force);
    }

    private void OnDestroy()
    {
        if (controller != null)
        {
            controller.UnregisterBall(this);
        }
    }

    private void EnsureResonanceGlow()
    {
        if (resonanceGlow != null)
        {
            return;
        }

        var glowObject = new GameObject("Resonance Glow");
        glowObject.transform.SetParent(transform);
        glowObject.transform.localPosition = Vector3.zero;
        glowObject.transform.localRotation = Quaternion.identity;
        glowObject.transform.localScale = Vector3.one * 1.16f;

        resonanceGlow = glowObject.AddComponent<SpriteRenderer>();
        resonanceGlow.sprite = CircleSpriteCache.Circle;
        resonanceGlow.sortingOrder = spriteRenderer.sortingOrder - 1;
        resonanceGlow.color = new Color(1f, 1f, 1f, 0f);
        resonanceGlow.enabled = false;
    }

    private IEnumerator PopRoutine(float intensity)
    {
        var elapsed = 0f;
        const float duration = 0.16f;
        var startScale = targetScale * 0.5f;
        var overshootScale = targetScale * Mathf.Lerp(popBaseOvershoot, popMaxOvershoot, Mathf.Clamp01((intensity - 1f) * 5f));

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
