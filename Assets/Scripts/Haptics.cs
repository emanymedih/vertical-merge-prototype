public static class Haptics
{
    public static void LightImpact()
    {
#if UNITY_IOS && !UNITY_EDITOR
        UnityEngine.Handheld.Vibrate();
#endif
    }
}
