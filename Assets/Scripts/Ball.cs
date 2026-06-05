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
    private SpriteRenderer popGlow;
    private Color baseColor;
    private bool isMerging;
    private Vector3 targetScale;

    public int MergeId { get; private set; }
    public int Level { get; private set; }
    public float SpawnedAt { get; private set; }
    public float Radius => transform.lossyScale.x * 0.5f;
    public float Mass => body != null ? body.mass : BallConfig.GetMass(Level);
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
        if (Level == 9)
        {
            gameObject.AddComponent<BlackHoleGravityField>().Initialize(this, controller);
        }

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

    public void PlayDropPop()
    {
        StopAllCoroutines();
        StartCoroutine(DropPopRoutine());
    }

    public void PlayMergeBirth(int level)
    {
        StopAllCoroutines();
        StartCoroutine(MergeBirthRoutine(level));
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
        var glowColor = CosmicBodyConfig.GetGlowColor(Level);
        var tintColor = Color.Lerp(glowColor, Color.white, emphasized ? 0.28f : 0.12f);
        spriteRenderer.color = Color.Lerp(baseColor, tintColor, clampedStrength * brightness);

        var pulse = Mathf.Sin(Time.time * (emphasized ? 8.5f : 6.5f)) * 0.5f + 0.5f;
        var glowAlpha = Mathf.Lerp(0.14f, emphasized ? 0.42f : 0.26f, pulse) * clampedStrength;
        var maxGlowScale = emphasized ? resonanceEmphasizedMaxScale : resonanceNormalMaxScale;
        var glowScale = Mathf.Lerp(1.14f, maxGlowScale, pulse) + clampedStrength * 0.05f;

        glowColor.a = glowAlpha;
        resonanceGlow.color = glowColor;
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

    public void TryApplyExternalForce(Vector2 force)
    {
        if (isMerging || body == null)
        {
            return;
        }

        body.AddForce(force, ForceMode2D.Force);
    }

    public bool TryStartBlackHoleAbsorption(Vector2 blackHoleCenter, float duration)
    {
        if (isMerging || body == null)
        {
            return false;
        }

        StopAllCoroutines();
        StartCoroutine(BlackHoleAbsorptionRoutine(blackHoleCenter, duration));
        return true;
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

    private IEnumerator DropPopRoutine()
    {
        EnsurePopGlow();
        popGlow.enabled = true;

        var glowColor = CosmicBodyConfig.GetGlowColor(Level);
        var elapsed = 0f;
        const float duration = 0.14f;
        var squashScale = new Vector3(targetScale.x * 1.08f, targetScale.y * 0.88f, targetScale.z);
        var stretchScale = new Vector3(targetScale.x * 0.94f, targetScale.y * 1.12f, targetScale.z);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            if (t < 0.42f)
            {
                var localT = AnimationEasing.EaseOutCubic(t / 0.42f);
                transform.localScale = Vector3.Lerp(targetScale, squashScale, localT);
            }
            else
            {
                var localT = AnimationEasing.EaseOutBack((t - 0.42f) / 0.58f);
                transform.localScale = Vector3.Lerp(squashScale, stretchScale, localT);
            }

            glowColor.a = Mathf.Lerp(0.34f, 0f, t);
            popGlow.color = glowColor;
            popGlow.transform.localScale = Vector3.one * Mathf.Lerp(1.12f, 1.42f, t);
            yield return null;
        }

        elapsed = 0f;
        const float settleDuration = 0.08f;
        var startScale = transform.localScale;
        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            var t = AnimationEasing.EaseOutCubic(elapsed / settleDuration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
        popGlow.enabled = false;
    }

    private IEnumerator MergeBirthRoutine(int level)
    {
        EnsurePopGlow();
        popGlow.enabled = true;

        var glowColor = CosmicBodyConfig.GetGlowColor(level);
        var duration = level >= 8 ? 0.3f : level >= 6 ? 0.26f : 0.22f;
        var elapsed = 0f;
        var startScale = targetScale * 0.55f;
        var overshootScale = targetScale * (level >= 8 ? 1.22f : level >= 6 ? 1.17f : 1.12f);
        transform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            if (t < 0.64f)
            {
                var localT = AnimationEasing.EaseOutBack(t / 0.64f);
                transform.localScale = Vector3.LerpUnclamped(startScale, overshootScale, localT);
            }
            else
            {
                var localT = AnimationEasing.EaseOutCubic((t - 0.64f) / 0.36f);
                transform.localScale = Vector3.Lerp(overshootScale, targetScale, localT);
            }

            glowColor.a = Mathf.Lerp(level >= 6 ? 0.42f : 0.28f, 0f, t);
            popGlow.color = glowColor;
            popGlow.transform.localScale = Vector3.one * Mathf.Lerp(1.1f, level >= 8 ? 1.72f : 1.46f, t);
            yield return null;
        }

        transform.localScale = targetScale;
        popGlow.enabled = false;
    }

    private IEnumerator BlackHoleAbsorptionRoutine(Vector2 blackHoleCenter, float duration)
    {
        isMerging = true;
        ClearResonance();

        if (circleCollider != null)
        {
            circleCollider.enabled = false;
        }

        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
        }

        var startPosition = (Vector2)transform.position;
        var startScale = transform.localScale;
        var sideVector = Vector2.Perpendicular(blackHoleCenter - startPosition).normalized;
        if (sideVector.sqrMagnitude <= 0.001f)
        {
            sideVector = Vector2.right;
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            var eased = Mathf.SmoothStep(0f, 1f, t);
            var spiral = Mathf.Sin(t * Mathf.PI * 2.4f) * (1f - eased) * Radius * 0.42f;
            var position = Vector2.Lerp(startPosition, blackHoleCenter, eased) + sideVector * spiral;

            transform.position = new Vector3(position.x, position.y, transform.position.z);
            transform.localScale = Vector3.Lerp(startScale, Vector3.one * 0.04f, eased);
            transform.Rotate(0f, 0f, Mathf.Lerp(220f, 780f, t) * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void EnsurePopGlow()
    {
        if (popGlow != null)
        {
            return;
        }

        var glowObject = new GameObject("Pop Glow");
        glowObject.transform.SetParent(transform);
        glowObject.transform.localPosition = Vector3.zero;
        glowObject.transform.localRotation = Quaternion.identity;
        glowObject.transform.localScale = Vector3.one * 1.16f;

        popGlow = glowObject.AddComponent<SpriteRenderer>();
        popGlow.sprite = CircleSpriteCache.Circle;
        popGlow.sortingOrder = spriteRenderer.sortingOrder - 2;
        popGlow.color = new Color(1f, 1f, 1f, 0f);
        popGlow.enabled = false;
    }
}
