using UnityEngine;

public static class RuntimeMaterials
{
    private static Material nebulaMaterial;

    public static Material NebulaBackdrop
    {
        get
        {
            if (nebulaMaterial == null)
            {
                var shader = Shader.Find("VerticalMerge/NebulaBackdrop");
                if (shader != null)
                {
                    nebulaMaterial = new Material(shader);
                    nebulaMaterial.SetColor("_Tint", new Color(0.18f, 0.34f, 0.78f, 0.34f));
                    nebulaMaterial.SetColor("_Accent", new Color(0.84f, 0.26f, 1f, 0.26f));
                    nebulaMaterial.SetFloat("_Intensity", 0.7f);
                    nebulaMaterial.SetFloat("_Vignette", 0.74f);
                }
            }

            return nebulaMaterial;
        }
    }

    public static Material CreateAtmosphereGlow(Color color, float intensity = 1f, float pulseSpeed = 1.4f)
    {
        var shader = Shader.Find("VerticalMerge/AtmosphereGlow");
        if (shader == null)
        {
            return null;
        }

        var material = new Material(shader);
        color.a = 1f;
        material.SetColor("_Tint", color);
        material.SetFloat("_Intensity", intensity);
        material.SetFloat("_PulseSpeed", pulseSpeed);
        return material;
    }

    public static Material CreateEnergyBeam(Color color, float intensity = 1f, float pulseSpeed = 2.2f)
    {
        var shader = Shader.Find("VerticalMerge/EnergyBeam");
        if (shader == null)
        {
            return null;
        }

        var material = new Material(shader);
        color.a = 1f;
        material.SetColor("_Tint", color);
        material.SetFloat("_Intensity", intensity);
        material.SetFloat("_PulseSpeed", pulseSpeed);
        return material;
    }
}
