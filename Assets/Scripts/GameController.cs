using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameController : MonoBehaviour
{
    private const string BestScoreKey = "MergePrototypeBestScore";
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

    public int Score { get; private set; }
    public int BestScore { get; private set; }
    public bool IsGameOver { get; private set; }
    public IReadOnlyList<Ball> ActiveBalls => activeBalls;
    public int HighestMergedLevel => highestMergedLevel;

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

        first.MarkMerging();
        second.MarkMerging();

        var nextLevel = first.Level + 1;
        var midpoint = ((Vector2)first.transform.position + (Vector2)second.transform.position) * 0.5f;

        Destroy(first.gameObject);
        Destroy(second.gameObject);

        var mergedBall = SpawnBall(nextLevel, midpoint);
        mergedBall.PlayPop();

        if (nextLevel > highestMergedLevel)
        {
            highestMergedLevel = nextLevel;
        }

        var scoreToAdd = GetScoreForLevel(nextLevel);
        Score += scoreToAdd;
        if (Score > BestScore)
        {
            BestScore = Score;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
        }

        effects.PlayMerge(midpoint, nextLevel, scoreToAdd);
        pressureFloor?.ApplyMergeRelief(nextLevel);
        Haptics.LightImpact();
        UpdateUi();
    }

    public void SetPressureFloor(PressureFloor floor)
    {
        pressureFloor = floor;
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
        gameUi.ShowGameOver(Score, BestScore);
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
    }
}
