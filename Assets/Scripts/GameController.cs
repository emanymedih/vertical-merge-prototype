using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameController : MonoBehaviour
{
    private const string BestScoreKey = "MergePrototypeBestScore";
    private const string BestLargestBallKey = "MergePrototypeBestLargestBall";
    private const string DiscoveredLevelKeyPrefix = "MergePrototypeDiscoveredLevel_";

    [SerializeField] private float chainWindowSeconds = 1.2f;
    [SerializeField] private float savedMessageCooldownSeconds = 5f;
    [SerializeField] private float savedCheckDelaySeconds = 0.25f;
    [SerializeField] private float savedDangerThreshold = 0.35f;
    private static readonly int[] MergeScoreByLevel =
    {
        0,
        0,
        10,
        25,
        60,
        140,
        320,
        720,
        1600,
        3500,
        7500
    };

    private readonly List<Ball> activeBalls = new List<Ball>();
    private GameUi gameUi;
    private GameEffects effects;
    private PressureFloor pressureFloor;
    private Transform ballParent;
    private int nextBallId;
    private int highestMergedLevel = 1;
    private int startingBestScore;
    private bool newBestScoreThisRun;
    private bool newBestLargestThisRun;
    private float chainWindowEndsAt;
    private int chainMergeCount;
    private int lastAnnouncedChainCount;
    private float dangerPressure;
    private float lastSavedMessageTime = -999f;

    public int Score { get; private set; }
    public int BestScore { get; private set; }
    public int BestLargestLevel { get; private set; }
    public bool IsGameOver { get; private set; }
    public IReadOnlyList<Ball> ActiveBalls => activeBalls;
    public int HighestMergedLevel => highestMergedLevel;
    public int CurrentGoalLevel => Mathf.Max(3, highestMergedLevel + 1);

    public int GetNextBallId()
    {
        nextBallId++;
        return nextBallId;
    }

    public void Initialize(GameUi ui, GameEffects gameEffects)
    {
        gameUi = ui;
        effects = gameEffects;
        ballParent = new GameObject("Balls").transform;

        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        BestLargestLevel = PlayerPrefs.GetInt(BestLargestBallKey, 1);
        startingBestScore = BestScore;
        UpdateUi();
    }

    public Ball SpawnBall(int level, Vector2 position)
    {
        var ballObject = new GameObject($"Ball L{level}");
        ballObject.transform.SetParent(ballParent);
        ballObject.transform.position = position;

        var ball = ballObject.AddComponent<Ball>();
        ball.Initialize(level, this);
        return ball;
    }

    public void MergeBalls(Ball first, Ball second)
    {
        if (IsGameOver || first == null || second == null || first.IsMerging || second.IsMerging)
        {
            return;
        }

        var hadDangerPressure = dangerPressure >= savedDangerThreshold;

        first.MarkMerging();
        second.MarkMerging();

        var nextLevel = first.Level + 1;
        var midpoint = ((Vector2)first.transform.position + (Vector2)second.transform.position) * 0.5f;

        Destroy(first.gameObject);
        Destroy(second.gameObject);

        var mergedBall = SpawnBall(nextLevel, midpoint);
        mergedBall.PlayPop(GetMergePopIntensity(nextLevel));

        if (nextLevel > highestMergedLevel)
        {
            highestMergedLevel = nextLevel;
        }

        if (nextLevel > BestLargestLevel)
        {
            BestLargestLevel = nextLevel;
            newBestLargestThisRun = true;
            PlayerPrefs.SetInt(BestLargestBallKey, BestLargestLevel);
            PlayerPrefs.Save();
        }

        TryShowDiscovery(nextLevel);

        var scoreToAdd = GetScoreForLevel(nextLevel);
        Score += scoreToAdd;
        if (Score > BestScore)
        {
            BestScore = Score;
            newBestScoreThisRun = true;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
        }

        effects.PlayMerge(midpoint, nextLevel, scoreToAdd);
        pressureFloor?.ApplyMergeRelief(nextLevel);
        Haptics.LightImpact();
        RegisterMergeForChain();
        if (hadDangerPressure)
        {
            StartCoroutine(SavedCheckRoutine());
        }

        UpdateUi();
    }

    public void BeginDropWindow()
    {
        chainWindowEndsAt = Time.time + chainWindowSeconds;
        chainMergeCount = 0;
        lastAnnouncedChainCount = 0;
    }

    public void SetPressureFloor(PressureFloor floor)
    {
        pressureFloor = floor;
    }

    public void SetDangerPressure(float pressure)
    {
        dangerPressure = Mathf.Clamp01(pressure);
    }

    public int GetNextSpawnLevel()
    {
        return BallConfig.PickSpawnLevel(highestMergedLevel);
    }

    public void TriggerGameOver()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        gameUi.ShowGameOver(Score, BestScore, highestMergedLevel, BestLargestLevel, GetMotivationLine());
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RegisterBall(Ball ball)
    {
        if (!activeBalls.Contains(ball))
        {
            activeBalls.Add(ball);
        }
    }

    public void UnregisterBall(Ball ball)
    {
        activeBalls.Remove(ball);
    }

    private static int GetScoreForLevel(int level)
    {
        if (level < MergeScoreByLevel.Length)
        {
            return MergeScoreByLevel[level];
        }

        var score = MergeScoreByLevel[MergeScoreByLevel.Length - 1];
        for (var currentLevel = MergeScoreByLevel.Length; currentLevel <= level; currentLevel++)
        {
            score = Mathf.RoundToInt(score * 2.15f);
        }

        return score;
    }

    private void UpdateUi()
    {
        gameUi.SetScore(Score);
        gameUi.SetBestScore(BestScore);
        gameUi.SetLargestLevel(highestMergedLevel);
        gameUi.SetGoalLevel(CurrentGoalLevel, IsLevelDiscovered(CurrentGoalLevel));
    }

    private void TryShowDiscovery(int level)
    {
        if (level < 2)
        {
            return;
        }

        var key = GetDiscoveredLevelKey(level);
        if (PlayerPrefs.HasKey(key))
        {
            return;
        }

        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        gameUi.ShowDiscoveryToast(level);
    }

    private string GetMotivationLine()
    {
        if (newBestScoreThisRun)
        {
            return "New Best Score!";
        }

        if (newBestLargestThisRun)
        {
            return $"New Largest Record: {CosmicBodyConfig.GetShortName(highestMergedLevel)}!";
        }

        if (startingBestScore > 0 && Score >= Mathf.RoundToInt(startingBestScore * 0.8f))
        {
            return "Almost! You were close.";
        }

        if (BestLargestLevel > 1 && highestMergedLevel == BestLargestLevel - 1)
        {
            return $"Almost reached {CosmicBodyConfig.GetShortName(BestLargestLevel)}.";
        }

        return "Try one more run.";
    }

    private static string GetDiscoveredLevelKey(int level)
    {
        return $"{DiscoveredLevelKeyPrefix}{level}";
    }

    private static bool IsLevelDiscovered(int level)
    {
        return level <= 1 || PlayerPrefs.HasKey(GetDiscoveredLevelKey(level));
    }

    private static float GetMergePopIntensity(int level)
    {
        if (level >= 8)
        {
            return 1.25f;
        }

        return level >= 6 ? 1.18f : 1f;
    }

    private void RegisterMergeForChain()
    {
        if (Time.time > chainWindowEndsAt)
        {
            return;
        }

        chainMergeCount++;
        if (chainMergeCount >= 3 && lastAnnouncedChainCount < 3)
        {
            lastAnnouncedChainCount = 3;
            gameUi.ShowMomentMessage("Great Chain!");
            return;
        }

        if (chainMergeCount >= 2 && lastAnnouncedChainCount < 2)
        {
            lastAnnouncedChainCount = 2;
            gameUi.ShowMomentMessage("Chain!");
        }
    }

    private IEnumerator SavedCheckRoutine()
    {
        yield return new WaitForSeconds(savedCheckDelaySeconds);

        if (IsGameOver || Time.time - lastSavedMessageTime < savedMessageCooldownSeconds)
        {
            yield break;
        }

        if (dangerPressure > 0.05f)
        {
            yield break;
        }

        lastSavedMessageTime = Time.time;
        gameUi.ShowMomentMessage("Saved!");
    }
}
