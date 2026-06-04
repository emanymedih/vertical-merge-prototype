using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class OpeningDemoController : MonoBehaviour
{
    private const string OpeningDemoCompletedKey = "MergePrototypeOpeningDemoCompleted";
    private const float FirstDropDelaySeconds = 0.35f;
    private const float BetweenDropDelaySeconds = 0.65f;
    private const float MergeDelaySeconds = 0.65f;
    private const float FinalDropDelaySeconds = 0.6f;
    private const float FinishDelaySeconds = 1.1f;

    private GameController controller;
    private ContainerBounds bounds;
    private bool active;
    private bool completed;

    public static bool ShouldPlayOpeningDemo()
    {
        return !PlayerPrefs.HasKey(OpeningDemoCompletedKey);
    }

    public static OpeningDemoController Build(GameController gameController, ContainerBounds containerBounds)
    {
        if (!ShouldPlayOpeningDemo())
        {
            return null;
        }

        var demoObject = new GameObject("Opening Demo Controller");
        var demo = demoObject.AddComponent<OpeningDemoController>();
        demo.Initialize(gameController, containerBounds);
        return demo;
    }

    private void Initialize(GameController gameController, ContainerBounds containerBounds)
    {
        controller = gameController;
        bounds = containerBounds;
        controller.BeginOpeningDemo();
        StartCoroutine(DemoRoutine());
    }

    private void Update()
    {
        if (!active || completed)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || HasTouchBegan())
        {
            CompleteDemo(true);
        }
    }

    private IEnumerator DemoRoutine()
    {
        active = true;

        yield return new WaitForSeconds(FirstDropDelaySeconds);
        if (!active)
        {
            yield break;
        }

        var first = SpawnDemoBall(1, -0.42f);

        yield return new WaitForSeconds(BetweenDropDelaySeconds);
        if (!active)
        {
            yield break;
        }

        var second = SpawnDemoBall(1, -0.36f);

        yield return new WaitForSeconds(MergeDelaySeconds);
        if (!active)
        {
            yield break;
        }

        if (first != null && second != null && !first.IsMerging && !second.IsMerging)
        {
            controller.MergeBalls(first, second);
        }

        yield return new WaitForSeconds(FinalDropDelaySeconds);
        if (!active)
        {
            yield break;
        }

        SpawnDemoBall(1, 0.92f);

        yield return new WaitForSeconds(FinishDelaySeconds);
        if (active)
        {
            CompleteDemo(false);
        }
    }

    private Ball SpawnDemoBall(int level, float x)
    {
        var radius = BallConfig.GetRadius(level);
        var clampedX = Mathf.Clamp(x, bounds.Left + radius, bounds.Right - radius);
        var ball = controller.SpawnBall(level, new Vector2(clampedX, bounds.Top - 0.25f));
        ball.PlayPop(0.9f);
        SoundManager.Play(SoundEvent.Drop);
        return ball;
    }

    private void CompleteDemo(bool interrupted)
    {
        if (completed)
        {
            return;
        }

        completed = true;
        active = false;

        if (interrupted)
        {
            ClearDemoBalls();
        }

        PlayerPrefs.SetInt(OpeningDemoCompletedKey, 1);
        PlayerPrefs.Save();

        controller.EndOpeningDemo();
        OnboardingController.Instance?.ResumeAfterOpeningDemo();
        Destroy(gameObject);
    }

    private void ClearDemoBalls()
    {
        var balls = new List<Ball>(controller.ActiveBalls);
        foreach (var ball in balls)
        {
            if (ball != null)
            {
                Destroy(ball.gameObject);
            }
        }
    }

    private static bool HasTouchBegan()
    {
        if (Input.touchCount <= 0)
        {
            return false;
        }

        return Input.GetTouch(0).phase == TouchPhase.Began;
    }
}
