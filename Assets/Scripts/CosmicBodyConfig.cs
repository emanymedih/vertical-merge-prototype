using UnityEngine;

public enum CosmicVisualType
{
    Asteroid,
    Moon,
    Planet,
    BluePlanet,
    GasGiant,
    Star,
    RedGiant,
    NeutronStar,
    BlackHole,
    GalaxyCore
}

public readonly struct CosmicBodyMetadata
{
    public CosmicBodyMetadata(
        int level,
        string displayName,
        string shortName,
        CosmicVisualType visualType,
        Color baseColor,
        Color detailColor,
        Color glowColor,
        string description)
    {
        Level = level;
        DisplayName = displayName;
        ShortName = shortName;
        VisualType = visualType;
        BaseColor = baseColor;
        DetailColor = detailColor;
        GlowColor = glowColor;
        Description = description;
    }

    public int Level { get; }
    public string DisplayName { get; }
    public string ShortName { get; }
    public CosmicVisualType VisualType { get; }
    public Color BaseColor { get; }
    public Color DetailColor { get; }
    public Color GlowColor { get; }
    public string Description { get; }
}

public static class CosmicBodyConfig
{
    private static readonly CosmicBodyMetadata[] Bodies =
    {
        new CosmicBodyMetadata(1, "Asteroid", "Asteroid", CosmicVisualType.Asteroid, new Color(0.55f, 0.45f, 0.34f), new Color(0.27f, 0.2f, 0.16f), new Color(0.82f, 0.68f, 0.48f), "Small rocky body"),
        new CosmicBodyMetadata(2, "Moon", "Moon", CosmicVisualType.Moon, new Color(0.78f, 0.82f, 0.88f), new Color(0.48f, 0.54f, 0.62f), new Color(0.9f, 0.94f, 1f), "Small satellite"),
        new CosmicBodyMetadata(3, "Small Planet", "Planet", CosmicVisualType.Planet, new Color(0.32f, 0.76f, 0.5f), new Color(0.17f, 0.47f, 0.84f), new Color(0.48f, 0.95f, 0.68f), "Young planet"),
        new CosmicBodyMetadata(4, "Blue Planet", "Blue Planet", CosmicVisualType.BluePlanet, new Color(0.18f, 0.58f, 1f), new Color(0.78f, 0.95f, 0.95f), new Color(0.4f, 0.82f, 1f), "Water world"),
        new CosmicBodyMetadata(5, "Gas Giant", "Gas Giant", CosmicVisualType.GasGiant, new Color(1f, 0.62f, 0.22f), new Color(0.55f, 0.28f, 0.12f), new Color(1f, 0.84f, 0.46f), "Massive gas planet"),
        new CosmicBodyMetadata(6, "Star", "Star", CosmicVisualType.Star, new Color(1f, 0.9f, 0.34f), new Color(1f, 0.98f, 0.74f), new Color(1f, 0.98f, 0.72f), "Burning star"),
        new CosmicBodyMetadata(7, "Red Giant", "Red Giant", CosmicVisualType.RedGiant, new Color(1f, 0.33f, 0.18f), new Color(1f, 0.73f, 0.32f), new Color(1f, 0.56f, 0.28f), "Expanding star"),
        new CosmicBodyMetadata(8, "Neutron Star", "Neutron Star", CosmicVisualType.NeutronStar, new Color(0.68f, 0.96f, 1f), new Color(1f, 1f, 1f), new Color(0.9f, 1f, 1f), "Dense stellar core"),
        new CosmicBodyMetadata(9, "Black Hole", "Black Hole", CosmicVisualType.BlackHole, new Color(0.12f, 0.08f, 0.18f), new Color(0.44f, 0.2f, 0.82f), new Color(0.74f, 0.42f, 1f), "Gravity well"),
        new CosmicBodyMetadata(10, "Galaxy Core", "Galaxy Core", CosmicVisualType.GalaxyCore, new Color(1f, 0.48f, 0.95f), new Color(1f, 0.86f, 0.35f), new Color(1f, 0.82f, 0.34f), "Cosmic center")
    };

    public static CosmicBodyMetadata Get(int level)
    {
        if (level >= 1 && level <= Bodies.Length)
        {
            return Bodies[level - 1];
        }

        return new CosmicBodyMetadata(
            level,
            $"Cosmic Body L{level}",
            $"Cosmic L{level}",
            CosmicVisualType.GalaxyCore,
            Color.Lerp(Bodies[Bodies.Length - 1].BaseColor, Color.white, 0.18f),
            Bodies[Bodies.Length - 1].DetailColor,
            Bodies[Bodies.Length - 1].GlowColor,
            "Unknown cosmic body");
    }

    public static string GetDisplayName(int level)
    {
        return Get(level).DisplayName;
    }

    public static string GetShortName(int level)
    {
        return Get(level).ShortName;
    }

    public static string GetLeveledName(int level)
    {
        return $"L{level} {GetShortName(level)}";
    }

    public static Color GetBaseColor(int level)
    {
        return Get(level).BaseColor;
    }

    public static Color GetGlowColor(int level)
    {
        return Get(level).GlowColor;
    }

    public static Color GetDetailColor(int level)
    {
        return Get(level).DetailColor;
    }
}
