using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField]
    private GameObject mainMenuPanel;
    [SerializeField]
    private GameObject pauseMenuPanel;
    [SerializeField]
    private GameObject upgradeMenuPanel; // Reference to the UpgradeMenu GameObject

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        upgradeMenuPanel.SetActive(false);
        Time.timeScale = 0f; // Pause game
        if (GameManager.Instance != null) GameManager.Instance.SetGameState(GameManager.GameState.Start);
    }

    public void HideMainMenu()
    {
        mainMenuPanel.SetActive(false);
    }

    public void TogglePauseMenu()
    {
        bool isPaused = pauseMenuPanel.activeSelf;
        pauseMenuPanel.SetActive(!isPaused);
        Time.timeScale = isPaused ? 1f : 0f; // Toggle pause
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameState(isPaused ? GameManager.GameState.Playing : GameManager.GameState.Start); // Adjust state as needed
        }
    }

    public void ShowUpgradeMenu()
    {
        upgradeMenuPanel.SetActive(true);
        if (upgradeMenuPanel.GetComponent<UpgradeMenu>() != null)
        {
            upgradeMenuPanel.GetComponent<UpgradeMenu>().ShowMenu();
        }
        Time.timeScale = 0f; // Pause game
    }

    public void HideUpgradeMenu()
    {
        upgradeMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game
    }

    public void StartGame()
    {
        HideMainMenu();
        Time.timeScale = 1f; // Resume game
        if (GameManager.Instance != null) GameManager.Instance.SetGameState(GameManager.GameState.Playing);
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}