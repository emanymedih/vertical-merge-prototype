using UnityEngine;

public static class BallConfig
{
    private static readonly float[] Radii =
    {
        0f,
        0.32f,
        0.38f,
        0.45f,
        0.53f,
        0.62f,
        0.72f,
        0.83f,
        0.95f,
        1.08f,
        1.22f
    };

    private static readonly float[] Density =
    {
        0f,
        0.58f,
        0.68f,
        0.80f,
        0.95f,
        1.12f,
        1.30f,
        1.52f,
        1.78f,
        2.05f,
        2.35f
    };

    private static readonly float[] LinearDamping =
    {
        0f,
        0.02f,
        0.025f,
        0.035f,
        0.05f,
        0.07f,
        0.09f,
        0.115f,
        0.135f,
        0.15f,
        0.165f
    };

    private static readonly float[] AngularDamping =
    {
        0f,
        0.07f,
        0.085f,
        0.11f,
        0.15f,
        0.20f,
        0.25f,
        0.31f,
        0.36f,
        0.40f,
        0.44f
    };

    private static readonly float[] Friction =
    {
        0f,
        0.18f,
        0.22f,
        0.27f,
        0.32f,
        0.37f,
        0.42f,
        0.47f,
        0.51f,
        0.55f,
        0.58f
    };

    private static readonly float[] Bounciness =
    {
        0f,
        0.18f,
        0.15f,
        0.12f,
        0.095f,
        0.075f,
        0.06f,
        0.048f,
        0.04f,
        0.034f,
        0.03f
    };

    public static int MaxConfiguredLevel => Radii.Length - 1;
    public static int MaxSpawnLevel => 4;

    public static float GetRadius(int level)
    {
        return GetValue(Radii, level);
    }

    public static float GetDiameter(int level)
    {
        return GetRadius(level) * 2f;
    }

    public static float GetMass(int level)
    {
        var radius = GetRadius(level);
        var area = Mathf.PI * radius * radius;
        return Mathf.Clamp(area * GetDensity(level), 0.18f, 12f);
    }

    public static float GetDensity(int level)
    {
        return GetValue(Density, level);
    }

    public static float GetLinearDamping(int level)
    {
        return GetValue(LinearDamping, level);
    }

    public static float GetAngularDamping(int level)
    {
        return GetValue(AngularDamping, level);
    }

    public static float GetFriction(int level)
    {
        return GetValue(Friction, level);
    }

    public static float GetBounciness(int level)
    {
        return GetValue(Bounciness, level);
    }

    public static float GetGravityScale(int level)
    {
        var t = Mathf.Clamp01((level - 1) / 9f);
        return Mathf.Lerp(1f, 1.12f, t);
    }

    public static int PickSpawnLevel(int highestMergedLevel)
    {
        if (highestMergedLevel >= 6)
        {
            return PickWeightedLevel(new[] { 1, 2, 3, 4 }, new[] { 50, 30, 15, 5 });
        }

        if (highestMergedLevel >= 4)
        {
            return PickWeightedLevel(new[] { 1, 2, 3 }, new[] { 60, 30, 10 });
        }

        return PickWeightedLevel(new[] { 1, 2 }, new[] { 75, 25 });
    }

    private static float GetValue(float[] values, int level)
    {
        var clampedLevel = Mathf.Clamp(level, 1, values.Length - 1);
        return values[clampedLevel];
    }

    private static int PickWeightedLevel(int[] levels, int[] weights)
    {
        var totalWeight = 0;
        for (var i = 0; i < weights.Length; i++)
        {
            totalWeight += weights[i];
        }

        var roll = Random.Range(0, totalWeight);
        for (var i = 0; i < levels.Length; i++)
        {
            if (roll < weights[i])
            {
                return Mathf.Min(levels[i], MaxSpawnLevel);
            }

            roll -= weights[i];
        }

        return Mathf.Min(levels[levels.Length - 1], MaxSpawnLevel);
    }
}
