using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI root to show when paused (optional)")]
    [SerializeField] private GameObject pauseMenuRoot;

    [Header("Input")]
    [Tooltip("Keyboard key used to toggle pause.")]
    [SerializeField] private Key pauseKey = Key.Escape;

    [Tooltip("Enable logs to help debug why the menu doesn’t appear.")]
    [SerializeField] private bool debugLogs = false;

    [Header("Optional: prevent pause while death menu shown")]
    [SerializeField] private DeathMenuController deathMenu;

    [Header("Exit")]
    [Tooltip("Scene name of the main menu to load when clicking Exit.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    private InputAction pauseAction;

    private void Awake()
    {
        if (pauseMenuRoot == null)
            pauseMenuRoot = gameObject;

        if (debugLogs) Debug.Log("[PauseMenuController] Awake", this);

        pauseMenuRoot.SetActive(false);

        // Create a dedicated action so pause works reliably with PlayerInput / InputActions.
        // This does not require editing the project's .inputactions asset.
        pauseAction = new InputAction("Pause", InputActionType.Button);
        // Input System binding paths are lowercase (e.g. "escape").
        pauseAction.AddBinding($"<Keyboard>/{pauseKey.ToString().ToLowerInvariant()}");
    }

    private void OnEnable()
    {
        if (pauseAction == null)
            return;

        pauseAction.performed += OnPausePerformed;
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        if (pauseAction == null)
            return;

        pauseAction.performed -= OnPausePerformed;
        pauseAction.Disable();
    }

    private void OnDestroy()
    {
        pauseAction?.Dispose();
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        // Avoid toggling if Unity is not in play mode.
        if (!Application.isPlaying)
            return;

        if (debugLogs) Debug.Log($"[PauseMenuController] Pause action performed ({pauseKey})", this);
        Toggle();
    }

    public void Toggle()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (isPaused) return;

        // If death menu is up, don’t allow pausing over it.
        // IMPORTANT: don’t use activeInHierarchy on the controller itself, because if the pause menu
        // lives under an inactive parent, some unrelated parent state could block pausing.
        // Keep it simple: only block if the DeathMenuController component is enabled AND its GameObject is active.
        if (deathMenu != null && deathMenu.enabled && deathMenu.gameObject.activeSelf)
        {
            if (debugLogs) Debug.Log("[PauseMenuController] Death menu appears active; pause blocked", this);
            return;
        }

        isPaused = true;
        pauseMenuRoot.SetActive(true);
        Time.timeScale = 0f;
        if (debugLogs) Debug.Log("[PauseMenuController] Paused (menu shown)", this);
    }

    public void Resume()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuRoot.SetActive(false);
        if (debugLogs) Debug.Log("[PauseMenuController] Resumed (menu hidden)", this);
    }

    public void ExitToMainMenu()
    {
        // Ensure game is unpaused before switching scenes.
        Time.timeScale = 1f;
        isPaused = false;

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("[PauseMenuController] mainMenuSceneName is empty. Set it in the Inspector.", this);
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
