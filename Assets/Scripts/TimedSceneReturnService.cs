using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Lives across scene loads and returns the player to a previous scene after a real-time delay.
/// Uses unscaled time so it still works if Time.timeScale changes.
/// </summary>
public class TimedSceneReturnService : MonoBehaviour
{
    private static TimedSceneReturnService instance;

    private string returnSceneName;
    private float returnAtRealtime;
    private bool active;

    public static void StartTimedReturn(string sceneNameToReturnTo, float durationSeconds)
    {
        if (string.IsNullOrWhiteSpace(sceneNameToReturnTo))
            return;

        if (instance == null)
        {
            var go = new GameObject("TimedSceneReturnService");
            instance = go.AddComponent<TimedSceneReturnService>();
            DontDestroyOnLoad(go);
        }

        instance.returnSceneName = sceneNameToReturnTo;
        instance.returnAtRealtime = Time.realtimeSinceStartup + Mathf.Max(0.01f, durationSeconds);
        instance.active = true;
    }

    private void Update()
    {
        if (!active) return;

        if (Time.realtimeSinceStartup >= returnAtRealtime)
        {
            active = false;

            if (!string.IsNullOrWhiteSpace(returnSceneName))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(returnSceneName);
            }
        }
    }

    // If you want to cancel the return from code.
    public static void Cancel()
    {
        if (instance == null) return;
        instance.active = false;
    }
}
