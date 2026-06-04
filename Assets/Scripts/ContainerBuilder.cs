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
        CreateWall(root.transform, "Left Wall", new Vector2(left - 0.12f, (bottom + top) * 0.5f), new Vector2(0.24f, top - bottom), new Color(0.16f, 0.18f, 0.21f));
        CreateWall(root.transform, "Right Wall", new Vector2(right + 0.12f, (bottom + top) * 0.5f), new Vector2(0.24f, top - bottom), new Color(0.16f, 0.18f, 0.21f));
        CreateWall(root.transform, "Bottom Wall", new Vector2(0f, bottom - 0.12f), new Vector2(containerWidth + 0.48f, 0.24f), new Color(0.16f, 0.18f, 0.21f));

        return new ContainerBounds(left, right, bottom, top, dangerY);
    }

    private static void CreateWall(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        var wall = new GameObject(name);
        wall.transform.SetParent(parent);
        wall.transform.position = position;
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);

        var renderer = wall.AddComponent<SpriteRenderer>();
        renderer.sprite = CircleSpriteCache.Square;
        renderer.color = color;
        renderer.sortingOrder = 2;

        var collider = wall.AddComponent<BoxCollider2D>();
        collider.sharedMaterial = PhysicsMaterials.WallMaterial;
    }
}
