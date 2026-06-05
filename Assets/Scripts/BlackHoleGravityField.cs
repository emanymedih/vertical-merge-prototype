using UnityEngine;

public sealed class BlackHoleGravityField : MonoBehaviour
{
    private const float GravityRadius = 2.4f;
    private const float PullAcceleration = 0.72f;
    private const float MinDistance = 0.28f;

    private Ball source;
    private GameController controller;
    private SpriteRenderer outerAura;
    private SpriteRenderer innerAura;

    public void Initialize(Ball sourceBall, GameController gameController)
    {
        source = sourceBall;
        controller = gameController;
        CreateAura();
    }

    private void FixedUpdate()
    {
        if (source == null || controller == null || controller.IsGameOver)
        {
            return;
        }

        foreach (var target in controller.ActiveBalls)
        {
            if (!CanAttract(target))
            {
                continue;
            }

            var offset = (Vector2)transform.position - (Vector2)target.transform.position;
            var distance = offset.magnitude;
            if (distance <= MinDistance || distance > GravityRadius)
            {
                continue;
            }

            var falloff = 1f - Mathf.Clamp01(distance / GravityRadius);
            var levelFactor = GetLevelPullFactor(target.Level);
            var acceleration = PullAcceleration * falloff * falloff * levelFactor;
            target.TryApplyExternalForce(offset.normalized * acceleration * target.Mass);
        }
    }

    private void Update()
    {
        if (outerAura == null || innerAura == null)
        {
            return;
        }

        var pulse = Mathf.Sin(Time.time * 3.8f) * 0.5f + 0.5f;
        outerAura.transform.localScale = Vector3.one * Mathf.Lerp(2.15f, 2.36f, pulse);
        innerAura.transform.localScale = new Vector3(Mathf.Lerp(1.48f, 1.62f, pulse), Mathf.Lerp(0.44f, 0.5f, pulse), 1f);

        outerAura.color = new Color(0.42f, 0.15f, 0.82f, Mathf.Lerp(0.08f, 0.16f, pulse));
        innerAura.color = new Color(0.8f, 0.42f, 1f, Mathf.Lerp(0.34f, 0.56f, pulse));
    }

    private bool CanAttract(Ball target)
    {
        return target != null
            && target != source
            && !target.IsMerging
            && target.Level < 9;
    }

    private static float GetLevelPullFactor(int level)
    {
        if (level <= 3)
        {
            return 1f;
        }

        if (level <= 5)
        {
            return 0.58f;
        }

        return 0.24f;
    }

    private void CreateAura()
    {
        var outerObject = new GameObject("Black Hole Gravity Aura");
        outerObject.transform.SetParent(transform);
        outerObject.transform.localPosition = Vector3.zero;
        outerObject.transform.localRotation = Quaternion.identity;
        outerObject.transform.localScale = Vector3.one * 2.2f;
        outerAura = outerObject.AddComponent<SpriteRenderer>();
        outerAura.sprite = CircleSpriteCache.Circle;
        outerAura.sortingOrder = 2;
        outerAura.color = new Color(0.42f, 0.15f, 0.82f, 0.12f);

        var innerObject = new GameObject("Black Hole Accretion Pull");
        innerObject.transform.SetParent(transform);
        innerObject.transform.localPosition = Vector3.zero;
        innerObject.transform.localRotation = Quaternion.identity;
        innerObject.transform.localScale = new Vector3(1.55f, 0.46f, 1f);
        innerAura = innerObject.AddComponent<SpriteRenderer>();
        innerAura.sprite = CircleSpriteCache.Circle;
        innerAura.sortingOrder = 18;
        innerAura.color = new Color(0.8f, 0.42f, 1f, 0.44f);
    }
}
