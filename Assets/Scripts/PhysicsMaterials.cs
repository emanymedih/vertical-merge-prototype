using System.Collections.Generic;
using UnityEngine;

public static class PhysicsMaterials
{
    private static readonly Dictionary<int, PhysicsMaterial2D> ballMaterials = new Dictionary<int, PhysicsMaterial2D>();
    private static PhysicsMaterial2D wallMaterial;

    public static PhysicsMaterial2D GetBallMaterial(int level)
    {
        if (!ballMaterials.TryGetValue(level, out var material))
        {
            var physicsT = Mathf.Clamp01((level - 1) / 9f);
            material = new PhysicsMaterial2D($"Generated Ball Material L{level}")
            {
                friction = Mathf.Lerp(0.24f, 0.48f, physicsT),
                bounciness = Mathf.Lerp(0.16f, 0.035f, physicsT)
            };
            ballMaterials.Add(level, material);
        }

        return material;
    }

    public static PhysicsMaterial2D WallMaterial
    {
        get
        {
            if (wallMaterial == null)
            {
                wallMaterial = new PhysicsMaterial2D("Generated Wall Material")
                {
                    friction = 0.45f,
                    bounciness = 0.02f
                };
            }

            return wallMaterial;
        }
    }
}
