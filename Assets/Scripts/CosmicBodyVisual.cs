using UnityEngine;

public sealed class CosmicBodyVisual : MonoBehaviour
{
    [SerializeField] private float glowScale = 1.18f;
    [SerializeField] private float strongGlowScale = 1.28f;
    [SerializeField] private float ringScaleX = 1.34f;
    [SerializeField] private float ringScaleY = 0.34f;
    [SerializeField] private float spotAlpha = 0.72f;
    [SerializeField] private float bandAlpha = 0.46f;

    private SpriteRenderer mainRenderer;
    private Transform visualRoot;

    public void Initialize(int level, SpriteRenderer rendererToUse)
    {
        mainRenderer = rendererToUse;
        var metadata = CosmicBodyConfig.Get(level);
        mainRenderer.color = metadata.BaseColor;

        visualRoot = new GameObject("Cosmic Visual").transform;
        visualRoot.SetParent(transform);
        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;

        BuildVisual(metadata);
    }

    private void BuildVisual(CosmicBodyMetadata metadata)
    {
        AddGradeAura(metadata);
        AddReadabilityRim(metadata);

        switch (metadata.VisualType)
        {
            case CosmicVisualType.Asteroid:
                AddSpots(metadata.DetailColor, new[] { new Vector2(-0.18f, 0.12f), new Vector2(0.18f, -0.08f), new Vector2(0.02f, -0.24f) }, new[] { 0.18f, 0.14f, 0.1f });
                AddBand(new Vector2(0.12f, 0.18f), 0.42f, 0.045f, metadata.GlowColor, 0.18f);
                break;
            case CosmicVisualType.Moon:
                AddSpots(metadata.DetailColor, new[] { new Vector2(-0.16f, 0.16f), new Vector2(0.18f, 0.02f), new Vector2(-0.02f, -0.22f) }, new[] { 0.16f, 0.11f, 0.13f });
                break;
            case CosmicVisualType.Planet:
                AddGlow(metadata.GlowColor, 0.16f, glowScale);
                AddSpots(metadata.DetailColor, new[] { new Vector2(-0.16f, 0.05f), new Vector2(0.18f, -0.12f), new Vector2(0.08f, 0.2f) }, new[] { 0.22f, 0.18f, 0.11f });
                AddBand(new Vector2(0f, -0.06f), 0.72f, 0.055f, Color.Lerp(metadata.DetailColor, Color.white, 0.18f), 0.24f);
                break;
            case CosmicVisualType.BluePlanet:
                AddGlow(metadata.GlowColor, 0.24f, glowScale + 0.04f);
                AddSpots(metadata.DetailColor, new[] { new Vector2(-0.12f, 0.16f), new Vector2(0.2f, -0.08f), new Vector2(-0.24f, -0.16f) }, new[] { 0.19f, 0.15f, 0.1f });
                AddBand(new Vector2(0f, 0.1f), 0.82f, 0.05f, metadata.DetailColor, 0.3f);
                AddBand(new Vector2(0f, -0.18f), 0.62f, 0.045f, Color.Lerp(metadata.DetailColor, Color.blue, 0.16f), 0.22f);
                break;
            case CosmicVisualType.GasGiant:
                AddBand(new Vector2(0f, 0.17f), 0.78f, 0.08f, metadata.DetailColor, 0.34f);
                AddBand(new Vector2(0f, -0.04f), 0.86f, 0.1f, Color.Lerp(metadata.DetailColor, Color.white, 0.2f), bandAlpha);
                AddBand(new Vector2(0f, -0.24f), 0.64f, 0.07f, metadata.DetailColor, 0.28f);
                break;
            case CosmicVisualType.Star:
                AddGlow(metadata.GlowColor, 0.34f, strongGlowScale);
                AddCore(metadata.DetailColor, 0.44f, 0.5f);
                AddCore(Color.white, 0.2f, 0.72f, 3);
                break;
            case CosmicVisualType.RedGiant:
                AddGlow(metadata.GlowColor, 0.44f, strongGlowScale + 0.14f);
                AddCore(metadata.DetailColor, 0.36f, 0.56f);
                AddBand(new Vector2(0f, 0.12f), 0.72f, 0.09f, metadata.DetailColor, 0.2f);
                AddBand(new Vector2(0f, -0.16f), 0.66f, 0.08f, Color.Lerp(metadata.DetailColor, Color.red, 0.22f), 0.3f);
                break;
            case CosmicVisualType.NeutronStar:
                AddGlow(metadata.GlowColor, 0.48f, strongGlowScale + 0.02f);
                AddCore(metadata.DetailColor, 0.3f, 0.88f);
                AddRing(metadata.GlowColor, 0.5f, 1.18f, 0.2f);
                AddBand(Vector2.zero, 0.9f, 0.045f, metadata.DetailColor, 0.42f);
                break;
            case CosmicVisualType.BlackHole:
                AddGlow(metadata.GlowColor, 0.5f, strongGlowScale + 0.24f);
                AddRing(metadata.DetailColor, 0.46f, ringScaleX * 1.08f, ringScaleY * 1.18f);
                AddRing(metadata.GlowColor, 0.94f, ringScaleX * 1.08f, ringScaleY);
                AddRing(Color.Lerp(metadata.GlowColor, Color.white, 0.25f), 0.32f, ringScaleX * 1.42f, ringScaleY * 0.6f);
                AddCore(metadata.DetailColor, 0.62f, 0.28f, 1);
                AddCore(Color.black, 0.5f, 0.9f, 3);
                break;
            case CosmicVisualType.GalaxyCore:
                AddGlow(metadata.GlowColor, 0.46f, strongGlowScale + 0.14f);
                AddRing(metadata.DetailColor, 0.6f, 1.22f, 0.28f);
                AddCore(metadata.DetailColor, 0.48f, 0.34f);
                AddCore(Color.white, 0.24f, 0.82f, 3);
                break;
        }
    }

    private void AddGradeAura(CosmicBodyMetadata metadata)
    {
        var gradeT = Mathf.Clamp01((metadata.Level - 1f) / (BallConfig.MaxConfiguredLevel - 1f));
        var visibleGradeT = Mathf.Clamp01((metadata.Level - 3f) / (BallConfig.MaxConfiguredLevel - 3f));
        var glowColor = Color.Lerp(metadata.GlowColor, Color.white, metadata.Level >= 8 ? 0.14f : 0.04f);
        var auraAlpha = metadata.Level < 3 ? Mathf.Lerp(0.035f, 0.06f, gradeT) : Mathf.Lerp(0.18f, 0.58f, visibleGradeT);
        var auraScale = metadata.Level < 3 ? Mathf.Lerp(1.1f, 1.16f, gradeT) : Mathf.Lerp(1.28f, 1.72f, visibleGradeT);
        var intensity = metadata.Level < 3 ? 0.48f : Mathf.Lerp(1.15f, 2.35f, visibleGradeT);
        var pulseSpeed = metadata.Level < 3 ? 0.55f : Mathf.Lerp(1.2f, 2.8f, visibleGradeT);
        AddCircle("Grade Aura", Vector2.zero, auraScale, WithAlpha(glowColor, auraAlpha), mainRenderer.sortingOrder - 4, true, intensity, pulseSpeed);
    }

    private void AddReadabilityRim(CosmicBodyMetadata metadata)
    {
        var rimColor = Color.Lerp(metadata.GlowColor, Color.white, metadata.Level >= 8 ? 0.18f : 0.08f);
        var alpha = metadata.Level >= 6 ? 0.32f : 0.22f;
        var gradeT = Mathf.Clamp01((metadata.Level - 1f) / (BallConfig.MaxConfiguredLevel - 1f));
        AddCircle("Readability Rim", Vector2.zero, 1.045f, WithAlpha(rimColor, alpha), mainRenderer.sortingOrder - 1, true, Mathf.Lerp(0.7f, 1.18f, gradeT), 1.1f + gradeT);
    }

    private void AddSpots(Color color, Vector2[] positions, float[] sizes)
    {
        var spotColor = color;
        spotColor.a = spotAlpha;
        for (var i = 0; i < positions.Length; i++)
        {
            AddCircle("Surface Spot", positions[i], sizes[i], spotColor, mainRenderer.sortingOrder + 1);
        }
    }

    private void AddGlow(Color color, float alpha, float scale)
    {
        AddCircle("Body Glow", Vector2.zero, scale, WithAlpha(color, Mathf.Max(alpha, 0.24f)), mainRenderer.sortingOrder - 2, true, 1.25f, 1.6f);
    }

    private void AddCore(Color color, float scale, float alpha, int sortingOffset = 2)
    {
        AddCircle("Body Core", Vector2.zero, scale, WithAlpha(color, alpha), mainRenderer.sortingOrder + sortingOffset);
    }

    private void AddRing(Color color, float alpha, float scaleX, float scaleY)
    {
        AddShape("Energy Ring", Vector2.zero, new Vector2(scaleX, scaleY), WithAlpha(color, alpha), mainRenderer.sortingOrder + 1, CircleSpriteCache.Circle, true);
    }

    private void AddBand(Vector2 position, float width, float height, Color color, float alpha)
    {
        AddShape("Atmosphere Band", position, new Vector2(width, height), WithAlpha(color, alpha), mainRenderer.sortingOrder + 1, CircleSpriteCache.Square);
    }

    private void AddCircle(string name, Vector2 position, float scale, Color color, int sortingOrder, bool useGlowMaterial = false, float glowIntensity = 0.88f, float pulseSpeed = -1f)
    {
        AddShape(name, position, new Vector2(scale, scale), color, sortingOrder, CircleSpriteCache.Circle, useGlowMaterial, glowIntensity, pulseSpeed);
    }

    private void AddShape(string name, Vector2 position, Vector2 scale, Color color, int sortingOrder, Sprite sprite, bool useGlowMaterial = false, float glowIntensity = 0.88f, float pulseSpeed = -1f)
    {
        var shape = new GameObject(name);
        shape.transform.SetParent(visualRoot);
        shape.transform.localPosition = position;
        shape.transform.localRotation = Quaternion.identity;
        shape.transform.localScale = new Vector3(scale.x, scale.y, 1f);

        var renderer = shape.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        if (useGlowMaterial)
        {
            var material = RuntimeMaterials.CreateAtmosphereGlow(color, glowIntensity, pulseSpeed > 0f ? pulseSpeed : 1.1f + GetPulseOffset(sortingOrder));
            if (material != null)
            {
                renderer.material = material;
            }
        }
    }

    private static float GetPulseOffset(int sortingOrder)
    {
        return Mathf.Repeat(sortingOrder * 0.17f, 1.2f);
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
