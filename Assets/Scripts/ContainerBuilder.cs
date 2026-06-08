using UnityEngine;

public readonly struct ContainerBounds
{
    public ContainerBounds(float left, float right, float bottom, float top, float dangerY)
    {
        Left = left;
        Right = right;
        Bottom = bottom;
        Top = top;
        DangerY = dangerY;
    }

    public float Left { get; }
    public float Right { get; }
    public float Bottom { get; }
    public float Top { get; }
    public float DangerY { get; }
    public float Width => Right - Left;
}

public static class ContainerBuilder
{
    public static ContainerBounds Build(Camera cameraToUse)
    {
        var halfHeight = cameraToUse.orthographicSize;
        var visibleWidth = halfHeight * 2f * cameraToUse.aspect;
        var containerWidth = Mathf.Min(visibleWidth - 0.9f, 6.6f);
        var left = -containerWidth * 0.5f;
        var right = containerWidth * 0.5f;
        var bottom = -halfHeight + 0.85f;
        var top = halfHeight - 0.65f;
        var dangerY = top - 1.9f;

        var root = new GameObject("Container");
        var wallColor = new Color(0.014f, 0.024f, 0.038f, 0.56f);
        var safetyTop = halfHeight + 1.2f;
        var sideWallHeight = safetyTop - bottom;
        CreateWall(root.transform, "Left Wall", new Vector2(left - 0.12f, bottom + sideWallHeight * 0.5f), new Vector2(0.24f, sideWallHeight), wallColor);
        CreateWall(root.transform, "Right Wall", new Vector2(right + 0.12f, bottom + sideWallHeight * 0.5f), new Vector2(0.24f, sideWallHeight), wallColor);
        CreateWall(root.transform, "Bottom Wall", new Vector2(0f, bottom - 0.12f), new Vector2(containerWidth + 0.48f, 0.24f), wallColor);
        CreateWall(root.transform, "Top Safety Wall", new Vector2(0f, safetyTop + 0.12f), new Vector2(containerWidth + 0.48f, 0.24f), Color.clear, false);

        var bounds = new ContainerBounds(left, right, bottom, top, dangerY);
        CosmicArenaVisuals.Build(cameraToUse, bounds, root.transform);
        return bounds;
    }

    private static void CreateWall(Transform parent, string name, Vector2 position, Vector2 size, Color color, bool visible = true)
    {
        var wall = new GameObject(name);
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);

        if (visible)
        {
            var renderer = wall.AddComponent<SpriteRenderer>();
            renderer.sprite = CircleSpriteCache.Square;
            renderer.color = color;
            renderer.sortingOrder = 2;
        }

        var collider = wall.AddComponent<BoxCollider2D>();
        collider.sharedMaterial = PhysicsMaterials.WallMaterial;
    }
}
