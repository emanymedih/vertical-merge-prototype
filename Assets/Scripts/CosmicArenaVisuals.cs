using UnityEngine;

public static class CosmicArenaVisuals
{
    private const int StarCount = 58;

    public static void Build(Camera cameraToUse, ContainerBounds bounds, Transform parent)
    {
        var root = new GameObject("Cosmic Arena Visuals");
        root.transform.SetParent(parent);

        CreateBackground(root.transform, cameraToUse);
        CreateEnergyBoundaries(root.transform, bounds);
    }

    private static void CreateBackground(Transform parent, Camera cameraToUse)
    {
        var halfHeight = cameraToUse.orthographicSize;
        var visibleWidth = halfHeight * 2f * cameraToUse.aspect;

        CreatePanel(parent, "Deep Space Backdrop", Vector2.zero, new Vector2(visibleWidth + 1.8f, halfHeight * 2f + 1.4f), new Color(0.025f, 0.035f, 0.06f), -30);
        CreatePanel(parent, "Upper Space Tint", new Vector2(0f, halfHeight * 0.36f), new Vector2(visibleWidth + 1.8f, halfHeight * 0.9f), new Color(0.04f, 0.06f, 0.11f, 0.42f), -29);
        CreatePanel(parent, "Lower Space Shadow", new Vector2(0f, -halfHeight * 0.52f), new Vector2(visibleWidth + 1.8f, halfHeight * 0.75f), new Color(0.005f, 0.012f, 0.025f, 0.5f), -28);

        var previousRandomState = Random.state;
        Random.InitState(1837);
        for (var i = 0; i < StarCount; i++)
        {
            var x = Random.Range(-visibleWidth * 0.5f, visibleWidth * 0.5f);
            var y = Random.Range(-halfHeight, halfHeight);
            var size = Random.Range(0.012f, 0.032f);
            var alpha = Random.Range(0.12f, 0.34f);
            CreateCircle(parent, "Distant Star", new Vector2(x, y), size, new Color(0.7f, 0.88f, 1f, alpha), -24);
        }

        Random.state = previousRandomState;
    }

    private static void CreateEnergyBoundaries(Transform parent, ContainerBounds bounds)
    {
        var centerY = (bounds.Bottom + bounds.Top) * 0.5f;
        var height = bounds.Top - bounds.Bottom;
        var cyanGlow = new Color(0.2f, 0.82f, 1f, 0.34f);
        var wallCore = new Color(0.06f, 0.1f, 0.14f, 0.8f);

        CreatePanel(parent, "Left Energy Wall", new Vector2(bounds.Left - 0.09f, centerY), new Vector2(0.18f, height), wallCore, 1);
        CreatePanel(parent, "Right Energy Wall", new Vector2(bounds.Right + 0.09f, centerY), new Vector2(0.18f, height), wallCore, 1);
        CreatePanel(parent, "Left Inner Glow", new Vector2(bounds.Left + 0.015f, centerY), new Vector2(0.035f, height), cyanGlow, 4);
        CreatePanel(parent, "Right Inner Glow", new Vector2(bounds.Right - 0.015f, centerY), new Vector2(0.035f, height), cyanGlow, 4);

        CreatePanel(parent, "Gravity Base", new Vector2(0f, bounds.Bottom - 0.1f), new Vector2(bounds.Width + 0.48f, 0.22f), new Color(0.05f, 0.13f, 0.18f, 0.88f), 1);
        CreatePanel(parent, "Base Energy Glow", new Vector2(0f, bounds.Bottom + 0.015f), new Vector2(bounds.Width + 0.36f, 0.045f), new Color(0.24f, 0.84f, 1f, 0.46f), 4);
        CreatePanel(parent, "Chamber Inner Shadow", new Vector2(0f, centerY), new Vector2(bounds.Width, height), new Color(0f, 0f, 0f, 0.08f), -1);
    }

    private static void CreatePanel(Transform parent, string name, Vector2 position, Vector2 scale, Color color, int sortingOrder)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent);
        panel.transform.position = position;
        panel.transform.localScale = new Vector3(scale.x, scale.y, 1f);

        var renderer = panel.AddComponent<SpriteRenderer>();
        renderer.sprite = CircleSpriteCache.Square;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    private static void CreateCircle(Transform parent, string name, Vector2 position, float scale, Color color, int sortingOrder)
    {
        var circle = new GameObject(name);
        circle.transform.SetParent(parent);
        circle.transform.position = position;
        circle.transform.localScale = Vector3.one * scale;

        var renderer = circle.AddComponent<SpriteRenderer>();
        renderer.sprite = CircleSpriteCache.Circle;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }
}
