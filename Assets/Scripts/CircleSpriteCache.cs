using UnityEngine;

public static class CircleSpriteCache
{
    private static Sprite circleSprite;
    private static Sprite squareSprite;

    public static Sprite Circle
    {
        get
        {
            if (circleSprite == null)
            {
                circleSprite = CreateCircleSprite();
            }

            return circleSprite;
        }
    }

    public static Sprite Square
    {
        get
        {
            if (squareSprite == null)
            {
                var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;

                var pixels = new Color32[16];
                for (var i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = Color.white;
                }

                texture.SetPixels32(pixels);
                texture.Apply();
                squareSprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }

            return squareSprite;
        }
    }

    public static Color GetBallColor(int level)
    {
        return CosmicBodyConfig.GetBaseColor(level);
    }

    private static Sprite CreateCircleSprite()
    {
        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color32[size * size];
        var center = (size - 1) * 0.5f;
        var radius = size * 0.48f;

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                var alpha = Mathf.Clamp01(radius - distance);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
