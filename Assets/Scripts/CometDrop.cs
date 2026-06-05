using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public sealed class CometDrop : MonoBehaviour
{
    public const float Radius = 0.42f;
    public const float Diameter = Radius * 2f;

    private GameController controller;
    private Rigidbody2D body;
    private CircleCollider2D circleCollider;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer glowRenderer;
    private bool hasResolved;

    public void Initialize(GameController gameController)
    {
        controller = gameController;
        body = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        transform.localScale = Vector3.one * Diameter;

        body.useAutoMass = false;
        body.mass = 0.9f;
        body.gravityScale = 1.08f;
        body.linearDamping = 0.015f;
        body.angularDamping = 0.08f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        circleCollider.radius = 0.5f;
        circleCollider.sharedMaterial = PhysicsMaterials.GetBallMaterial(2);

        spriteRenderer.sprite = CircleSpriteCache.Circle;
        spriteRenderer.color = new Color(0.36f, 0.92f, 1f, 0.94f);
        spriteRenderer.sortingOrder = 18;

        var glowObject = new GameObject("Comet Glow");
        glowObject.transform.SetParent(transform);
        glowObject.transform.localPosition = Vector3.zero;
        glowObject.transform.localRotation = Quaternion.identity;
        glowObject.transform.localScale = Vector3.one * 1.36f;
        glowRenderer = glowObject.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = CircleSpriteCache.Circle;
        glowRenderer.sortingOrder = 17;
        glowRenderer.color = new Color(0.72f, 0.96f, 1f, 0.28f);

        StartCoroutine(LifetimeRoutine());
    }

    private void Update()
    {
        if (hasResolved || glowRenderer == null)
        {
            return;
        }

        var pulse = Mathf.Sin(Time.time * 8.2f) * 0.5f + 0.5f;
        glowRenderer.transform.localScale = Vector3.one * Mathf.Lerp(1.24f, 1.56f, pulse);
        glowRenderer.color = new Color(0.72f, 0.96f, 1f, Mathf.Lerp(0.18f, 0.42f, pulse));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasResolved || controller == null || controller.IsGameOver)
        {
            return;
        }

        var ball = collision.collider.GetComponent<Ball>();
        if (ball == null || !CanAffect(ball))
        {
            return;
        }

        hasResolved = true;
        var contactPoint = collision.contactCount > 0 ? collision.GetContact(0).point : (Vector2)transform.position;
        controller.ResolveCometImpact(this, ball, contactPoint);
    }

    private static bool CanAffect(Ball ball)
    {
        return ball != null && !ball.IsMerging && !ball.IsPreMerging && ball.Level < 9;
    }

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(7f);
        if (!hasResolved)
        {
            Destroy(gameObject);
        }
    }
}
