using UnityEngine;

public enum HapticFeedbackType
{
    None,
    LightImpact,
    HeavyImpact,
    SuccessPattern
}

[CreateAssetMenu(fileName = "CosmicBodyFeelProfile", menuName = "Vertical Merge/Cosmic Body Feel Profile")]
public sealed class CosmicBodyFeelProfile : ScriptableObject
{
    public int level = 1;
    public float mergeOverscale = 1.16f;
    public float mergeBirthDuration = 0.24f;
    public float preMergeSquash = 0.08f;
    public int particlesCount = 18;
    public float flashScale = 1.6f;
    public float ringScale = 1.9f;
    public float shakeStrength = 0f;
    public string audioClipId;
    public float pitch = 1f;
    public HapticFeedbackType hapticType = HapticFeedbackType.LightImpact;
}

public readonly struct CosmicBodyFeel
{
    public CosmicBodyFeel(
        int level,
        float mergeOverscale,
        float mergeBirthDuration,
        float preMergeSquash,
        int particlesCount,
        float flashScale,
        float ringScale,
        float shakeStrength,
        string audioClipId,
        float pitch,
        HapticFeedbackType hapticType)
    {
        Level = level;
        MergeOverscale = mergeOverscale;
        MergeBirthDuration = mergeBirthDuration;
        PreMergeSquash = preMergeSquash;
        ParticlesCount = particlesCount;
        FlashScale = flashScale;
        RingScale = ringScale;
        ShakeStrength = shakeStrength;
        AudioClipId = audioClipId;
        Pitch = pitch;
        HapticType = hapticType;
    }

    public int Level { get; }
    public float MergeOverscale { get; }
    public float MergeBirthDuration { get; }
    public float PreMergeSquash { get; }
    public int ParticlesCount { get; }
    public float FlashScale { get; }
    public float RingScale { get; }
    public float ShakeStrength { get; }
    public string AudioClipId { get; }
    public float Pitch { get; }
    public HapticFeedbackType HapticType { get; }
}

public static class CosmicBodyFeelDatabase
{
    private const string FeelProfileResourceFolder = "FeelProfiles";
    private static CosmicBodyFeelProfile[] loadedProfiles;

    public static CosmicBodyFeel Get(int level)
    {
        var profile = TryFindProfile(level);
        if (profile != null)
        {
            return new CosmicBodyFeel(
                level,
                profile.mergeOverscale,
                profile.mergeBirthDuration,
                profile.preMergeSquash,
                profile.particlesCount,
                profile.flashScale,
                profile.ringScale,
                profile.shakeStrength,
                profile.audioClipId,
                Mathf.Max(0.05f, profile.pitch),
                profile.hapticType);
        }

        return BuildFallback(level);
    }

    private static CosmicBodyFeelProfile TryFindProfile(int level)
    {
        if (loadedProfiles == null)
        {
            loadedProfiles = Resources.LoadAll<CosmicBodyFeelProfile>(FeelProfileResourceFolder);
        }

        if (loadedProfiles == null)
        {
            return null;
        }

        for (var i = 0; i < loadedProfiles.Length; i++)
        {
            if (loadedProfiles[i] != null && loadedProfiles[i].level == level)
            {
                return loadedProfiles[i];
            }
        }

        return null;
    }

    private static CosmicBodyFeel BuildFallback(int level)
    {
        var t = Mathf.Clamp01((level - 2f) / 8f);
        var haptic = level >= 10
            ? HapticFeedbackType.SuccessPattern
            : level >= 6 ? HapticFeedbackType.HeavyImpact : HapticFeedbackType.LightImpact;

        return new CosmicBodyFeel(
            level,
            Mathf.Lerp(1.2f, 1.4f, t),
            Mathf.Lerp(0.22f, 0.31f, t),
            Mathf.Lerp(0.06f, 0.13f, t),
            Mathf.RoundToInt(Mathf.Lerp(16f, 48f, t)),
            Mathf.Lerp(1.5f, 2.35f, t),
            Mathf.Lerp(1.85f, 3.05f, t),
            level >= 6 ? Mathf.Lerp(0.045f, 0.13f, Mathf.Clamp01((level - 6f) / 4f)) : 0f,
            null,
            Mathf.Lerp(1.18f, 0.72f, t),
            haptic);
    }
}
