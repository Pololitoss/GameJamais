using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene to load when clicking Play")]
    [Tooltip("Name of the gameplay scene (must be added in File > Build Settings)")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Optional")]
    [Tooltip("If true, sets Time.timeScale back to 1 before loading scenes (useful if coming from pause/death screens).")]
    [SerializeField] private bool resetTimeScaleOnAction = true;

    public void Play()
    {
        if (resetTimeScaleOnAction)
            Time.timeScale = 1f;

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("[MainMenuController] gameSceneName is empty. Set it in the Inspector.", this);
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void Quit()
    {
        if (resetTimeScaleOnAction)
            Time.timeScale = 1f;

#if UNITY_EDITOR
        Debug.Log("[MainMenuController] Quit() called (Editor won't close).", this);
#else
        Application.Quit();
#endif
    }
}
