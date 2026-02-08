using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Hook these public methods to your DeathMenu UI Buttons (OnClick).
/// </summary>
public class DeathMenuButtons : MonoBehaviour
{
    [Tooltip("Gameplay scene to reload when pressing Restart. If empty, reloads the active scene.")]
    [SerializeField] private string gameplaySceneName = "";

    [Tooltip("Main menu scene name.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Tooltip("Reset timeScale to 1 when leaving death menu.")]
    [SerializeField] private bool resetTimeScaleOnAction = true;

    public void Restart()
    {
        if (resetTimeScaleOnAction)
            Time.timeScale = 1f;

        string scene = string.IsNullOrWhiteSpace(gameplaySceneName)
            ? SceneManager.GetActiveScene().name
            : gameplaySceneName;

        SceneManager.LoadScene(scene);
    }

    public void GoToMainMenu()
    {
        if (resetTimeScaleOnAction)
            Time.timeScale = 1f;

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("[DeathMenuButtons] mainMenuSceneName is empty.", this);
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        if (resetTimeScaleOnAction)
            Time.timeScale = 1f;

#if UNITY_EDITOR
        Debug.Log("[DeathMenuButtons] QuitGame() called (Editor won't close).", this);
#else
        Application.Quit();
#endif
    }
}
