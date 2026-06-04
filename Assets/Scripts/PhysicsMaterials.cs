using UnityEngine;

public static class PhysicsMaterials
{
    private static PhysicsMaterial2D ballMaterial;
    private static PhysicsMaterial2D wallMaterial;

    public static PhysicsMaterial2D BallMaterial
    {
        get
        {
            if (ballMaterial == null)
            {
                ballMaterial = new PhysicsMaterial2D("Generated Ball Material")
                {
                    friction = 0.25f,
                    bounciness = 0.08f
                };
            }

            return ballMaterial;
        }
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
