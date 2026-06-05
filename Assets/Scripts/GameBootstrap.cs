using UnityEngine;

public sealed class GameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Application.targetFrameRate = 60;
        Physics2D.gravity = new Vector2(0f, -9.81f);
    }

    private void Start()
    {
        var cameraToUse = CreateCamera();
        var bounds = ContainerBuilder.Build(cameraToUse);

        SoundManager.Build();

        var effectsObject = new GameObject("Game Effects");
        var effects = effectsObject.AddComponent<GameEffects>();
        effects.Initialize(cameraToUse);

        var controllerObject = new GameObject("Game Controller");
        var controller = controllerObject.AddComponent<GameController>();
        var ui = GameUi.Build(controller);
        controller.Initialize(ui, effects);
        OnboardingController.Build();

        var dangerLineObject = new GameObject("Danger Line");
        var dangerLine = dangerLineObject.AddComponent<DangerLine>();
        dangerLine.Initialize(controller, bounds.DangerY, bounds.Width);

        var pressureFloorObject = new GameObject("Pressure Floor");
        var pressureFloor = pressureFloorObject.AddComponent<PressureFloor>();
        pressureFloor.Initialize(controller, bounds, controller.IsFirstSessionPacingActive);
        controller.SetPressureFloor(pressureFloor);

        var anomalyObject = new GameObject("Cosmic Anomaly Event Controller");
        var anomalyController = anomalyObject.AddComponent<CosmicAnomalyEventController>();
        anomalyController.Initialize(controller, bounds);

        var magneticTensionObject = new GameObject("Magnetic Tension Controller");
        var magneticTension = magneticTensionObject.AddComponent<MagneticTensionController>();
        magneticTension.Initialize(controller);

        OpeningDemoController.Build(controller, bounds);

        var spawnerObject = new GameObject("Ball Spawner");
        var spawner = spawnerObject.AddComponent<BallSpawner>();
        spawner.Initialize(cameraToUse, controller, bounds.Left, bounds.Right, bounds.Bottom, bounds.Top - 0.25f);
    }

    private static Camera CreateCamera()
    {
        var existingCamera = Camera.main;
        if (existingCamera != null)
        {
            return existingCamera;
        }

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 7.5f;
        camera.backgroundColor = new Color(0.08f, 0.1f, 0.12f);
        return camera;
    }
}
