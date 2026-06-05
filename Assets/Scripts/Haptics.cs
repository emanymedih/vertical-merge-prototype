using System.Runtime.InteropServices;

public static class Haptics
{
    public static void LightImpact()
    {
#if UNITY_IOS && !UNITY_EDITOR
        VM_HapticLightImpact();
#endif
    }

    public static void HeavyImpact()
    {
#if UNITY_IOS && !UNITY_EDITOR
        VM_HapticHeavyImpact();
#endif
    }

    public static void SuccessPattern()
    {
#if UNITY_IOS && !UNITY_EDITOR
        VM_HapticSuccessPattern();
#endif
    }

    public static void Play(HapticFeedbackType feedbackType)
    {
        switch (feedbackType)
        {
            case HapticFeedbackType.LightImpact:
                LightImpact();
                break;
            case HapticFeedbackType.HeavyImpact:
                HeavyImpact();
                break;
            case HapticFeedbackType.SuccessPattern:
                SuccessPattern();
                break;
        }
    }

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void VM_HapticLightImpact();

    [DllImport("__Internal")]
    private static extern void VM_HapticHeavyImpact();

    [DllImport("__Internal")]
    private static extern void VM_HapticSuccessPattern();
#endif
}
