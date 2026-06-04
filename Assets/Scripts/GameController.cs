using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameController : MonoBehaviour
{
    private const string BestScoreKey = "MergePrototypeBestScore";

    private readonly List<Ball> activeBalls = new List<Ball>();
    private GameUi gameUi;
    private GameEffects effects;
    private Transform ballParent;
    private int nextBallId;

    public int Score { get; private set; }
    public int BestScore { get; private set; }
    public bool IsGameOver { get; private set; }
    public IReadOnlyList<Ball> ActiveBalls => activeBalls;

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

        Score += GetScoreForLevel(nextLevel);
        if (Score > BestScore)
        {
            BestScore = Score;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
        }

        effects.PlayMerge(midpoint, nextLevel);
        Haptics.LightImpact();
        UpdateUi();
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
        return 10 * (1 << Mathf.Clamp(level - 1, 0, 20));
    }

    private void UpdateUi()
    {
        gameUi.SetScore(Score);
        gameUi.SetBestScore(BestScore);
    }
}
