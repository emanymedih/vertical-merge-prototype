using UnityEngine;

public enum SpawnPayloadType
{
    Normal,
    Comet
}

public readonly struct SpawnPayload
{
    private const float CometRadius = 0.42f;

    private SpawnPayload(SpawnPayloadType type, int level)
    {
        Type = type;
        Level = level;
    }

    public SpawnPayloadType Type { get; }
    public int Level { get; }
    public bool IsComet => Type == SpawnPayloadType.Comet;
    public float Radius => IsComet ? CometRadius : BallConfig.GetRadius(Level);
    public float Diameter => Radius * 2f;

    public static SpawnPayload Normal(int level)
    {
        return new SpawnPayload(SpawnPayloadType.Normal, level);
    }

    public static SpawnPayload Comet()
    {
        return new SpawnPayload(SpawnPayloadType.Comet, 0);
    }

    public Color GetPreviewColor()
    {
        return IsComet ? new Color(0.36f, 0.92f, 1f) : CircleSpriteCache.GetBallColor(Level);
    }

    public Color GetGlowColor()
    {
        return IsComet ? new Color(0.86f, 0.98f, 1f) : CosmicBodyConfig.GetGlowColor(Level);
    }
}
