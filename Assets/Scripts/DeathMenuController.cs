using UnityEngine;

public class DeathMenuController : MonoBehaviour
{
    [Header("UI root to show when dead (optional)")]
    [SerializeField] private GameObject deathMenuRoot;

    [Header("Pause behavior")]
    [SerializeField] private bool pauseTimeOnShow = true;

    private bool isShown;

    private void Awake()
    {
        if (deathMenuRoot == null)
            deathMenuRoot = gameObject;

        // Death menu should start hidden.
        deathMenuRoot.SetActive(false);
    }

    public void Show()
    {
        if (isShown) return;
        isShown = true;

        deathMenuRoot.SetActive(true);

        if (pauseTimeOnShow)
            Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (!isShown) return;
        isShown = false;

        if (pauseTimeOnShow)
            Time.timeScale = 1f;

        deathMenuRoot.SetActive(false);
    }
}
