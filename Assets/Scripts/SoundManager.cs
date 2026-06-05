using UnityEngine;

public enum SoundEvent
{
    Drop,
    CollisionSoft,
    MergePop,
    HighMergeBoom,
    Chain,
    DangerWarning,
    GameOver,
    NewRecord,
    ButtonClick,
    BlackHoleWarning,
    BlackHoleAbsorb
}

public sealed class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public static SoundManager Build()
    {
        if (Instance != null)
        {
            return Instance;
        }

        var soundObject = new GameObject("Sound Manager");
        var soundManager = soundObject.AddComponent<SoundManager>();
        return soundManager;
    }

    public static void Play(SoundEvent soundEvent)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.PlayEvent(soundEvent);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void PlayEvent(SoundEvent soundEvent)
    {
        // Placeholder hook for the MVP sound pass. Final clips can be mapped here later.
    }
}
