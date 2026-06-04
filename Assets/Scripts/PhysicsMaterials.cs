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
            material = new PhysicsMaterial2D($"Generated Ball Material L{level}")
            {
                friction = BallConfig.GetFriction(level),
                bounciness = BallConfig.GetBounciness(level)
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
