#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class OpenGameSceneOnLoad
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string SessionKey = "MergePrototypeOpenedGameScene";

    static OpenGameSceneOnLoad()
    {
        EditorApplication.delayCall += OpenSceneIfNeeded;
    }

    private static void OpenSceneIfNeeded()
    {
        if (SessionState.GetBool(SessionKey, false))
        {
            return;
        }

        var activeScene = SceneManager.GetActiveScene();
        if (!EditorApplication.isPlayingOrWillChangePlaymode && string.IsNullOrEmpty(activeScene.path))
        {
            EditorSceneManager.OpenScene(ScenePath);
            SessionState.SetBool(SessionKey, true);
        }
    }
}
#endif
