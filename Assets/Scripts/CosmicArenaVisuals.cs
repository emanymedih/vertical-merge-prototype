using UnityEngine;

public static class CosmicArenaVisuals
{
    private const int StarCount = 86;
    private const string WallpaperResourceName = "GravitationalChamberWallpaper";

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

        if (CreateWallpaper(parent, cameraToUse, visibleWidth, halfHeight * 2f))
        {
            CreatePanel(parent, "Wallpaper Depth Fade", Vector2.zero, new Vector2(visibleWidth + 0.8f, halfHeight * 2f + 0.8f), new Color(0f, 0.01f, 0.025f, 0.12f), -34);
            return;
        }

        CreatePanel(parent, "Deep Space Backdrop", Vector2.zero, new Vector2(visibleWidth + 1.8f, halfHeight * 2f + 1.4f), new Color(0.01f, 0.014f, 0.032f), -40);
        CreatePanel(parent, "Upper Nebula Haze", new Vector2(0f, halfHeight * 0.38f), new Vector2(visibleWidth + 1.8f, halfHeight * 0.96f), new Color(0.055f, 0.07f, 0.14f, 0.46f), -39);
        CreatePanel(parent, "Lower Gravity Shadow", new Vector2(0f, -halfHeight * 0.5f), new Vector2(visibleWidth + 1.8f, halfHeight * 0.84f), new Color(0.002f, 0.006f, 0.018f, 0.64f), -38);
        CreatePanel(parent, "Center Chamber Vignette", Vector2.zero, new Vector2(visibleWidth * 0.86f, halfHeight * 1.58f), new Color(0.08f, 0.13f, 0.19f, 0.12f), -37);

        var previousRandomState = Random.state;
        Random.InitState(1837);
        for (var i = 0; i < StarCount; i++)
        {
            var x = Random.Range(-visibleWidth * 0.5f, visibleWidth * 0.5f);
            var y = Random.Range(-halfHeight, halfHeight);
            var size = Random.Range(0.012f, 0.032f);
            var alpha = Random.Range(0.09f, 0.36f);
            CreateCircle(parent, "Distant Star", new Vector2(x, y), size, new Color(0.7f, 0.88f, 1f, alpha), -24);
        }

        Random.state = previousRandomState;
    }

    private static bool CreateWallpaper(Transform parent, Camera cameraToUse, float visibleWidth, float visibleHeight)
    {
        var sprite = Resources.Load<Sprite>(WallpaperResourceName);
        if (sprite == null)
        {
            var sprites = Resources.LoadAll<Sprite>(WallpaperResourceName);
            if (sprites == null || sprites.Length == 0)
            {
                return false;
            }

            sprite = sprites[0];
        }

        var wallpaper = new GameObject("Gravitational Chamber Wallpaper");
        wallpaper.transform.SetParent(parent);
        wallpaper.transform.position = new Vector3(cameraToUse.transform.position.x, cameraToUse.transform.position.y, 0f);

        var renderer = wallpaper.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.sortingOrder = -45;

        var spriteWidth = sprite.bounds.size.x;
        var spriteHeight = sprite.bounds.size.y;
        var coverScale = Mathf.Max(visibleWidth / spriteWidth, visibleHeight / spriteHeight);
        wallpaper.transform.localScale = Vector3.one * coverScale;
        return true;
    }

    private static void CreateEnergyBoundaries(Transform parent, ContainerBounds bounds)
    {
        var centerY = (bounds.Bottom + bounds.Top) * 0.5f;
        var height = bounds.Top - bounds.Bottom;
        var cyanGlow = new Color(0.24f, 0.86f, 1f, 0.38f);
        var wallCore = new Color(0.025f, 0.045f, 0.07f, 0.9f);

        CreatePanel(parent, "Left Chamber Wall Core", new Vector2(bounds.Left - 0.12f, centerY), new Vector2(0.24f, height + 0.1f), wallCore, 1);
        CreatePanel(parent, "Right Chamber Wall Core", new Vector2(bounds.Right + 0.12f, centerY), new Vector2(0.24f, height + 0.1f), wallCore, 1);
        CreatePanel(parent, "Left Outer Energy Bloom", new Vector2(bounds.Left - 0.06f, centerY), new Vector2(0.32f, height), new Color(0.08f, 0.58f, 1f, 0.11f), 2);
        CreatePanel(parent, "Right Outer Energy Bloom", new Vector2(bounds.Right + 0.06f, centerY), new Vector2(0.32f, height), new Color(0.08f, 0.58f, 1f, 0.11f), 2);
        CreatePanel(parent, "Left Inner Energy Boundary", new Vector2(bounds.Left + 0.018f, centerY), new Vector2(0.036f, height), cyanGlow, 5);
        CreatePanel(parent, "Right Inner Energy Boundary", new Vector2(bounds.Right - 0.018f, centerY), new Vector2(0.036f, height), cyanGlow, 5);
        CreatePanel(parent, "Left Hot Boundary Core", new Vector2(bounds.Left + 0.034f, centerY), new Vector2(0.012f, height), new Color(0.78f, 0.98f, 1f, 0.58f), 6);
        CreatePanel(parent, "Right Hot Boundary Core", new Vector2(bounds.Right - 0.034f, centerY), new Vector2(0.012f, height), new Color(0.78f, 0.98f, 1f, 0.58f), 6);

        CreatePanel(parent, "Gravity Platform Body", new Vector2(0f, bounds.Bottom - 0.12f), new Vector2(bounds.Width + 0.5f, 0.24f), new Color(0.025f, 0.075f, 0.105f, 0.94f), 1);
        CreatePanel(parent, "Gravity Platform Bloom", new Vector2(0f, bounds.Bottom + 0.02f), new Vector2(bounds.Width + 0.5f, 0.16f), new Color(0.08f, 0.66f, 1f, 0.16f), 3);
        CreatePanel(parent, "Gravity Platform Hot Edge", new Vector2(0f, bounds.Bottom + 0.055f), new Vector2(bounds.Width + 0.32f, 0.042f), new Color(0.48f, 0.94f, 1f, 0.62f), 6);
        CreatePanel(parent, "Chamber Interior Depth", new Vector2(0f, centerY), new Vector2(bounds.Width, height), new Color(0f, 0f, 0f, 0.13f), -1);
        CreatePanel(parent, "Top Field Fade", new Vector2(0f, bounds.Top - 0.28f), new Vector2(bounds.Width + 0.4f, 0.56f), new Color(0.04f, 0.09f, 0.14f, 0.22f), 0);
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
