using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField]
    private GameObject startPanel;
    [SerializeField]
    private GameObject gameplayPanel;
    [SerializeField]
    private GameObject gameOverPanel;

    [Header("Buttons")]
    [SerializeField]
    private Button startGameButton;
    [SerializeField]
    private Button restartGameButton;

    [Header("Game References")]
    [SerializeField]
    private GameManager gameManager;

    void Start()
    {
        // Find GameManager if not assigned
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        // Setup button listeners
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
        }

        if (restartGameButton != null)
        {
            restartGameButton.onClick.AddListener(OnRestartGameClicked);
        }

        // Subscribe to game manager events
        if (gameManager != null)
        {
            gameManager.OnGameStarted += OnGameStarted;
            gameManager.OnGameOver += OnGameOver;
        }

        // Initialize UI state
        UpdateUIForGameState(GameManager.GameState.Start);
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (gameManager != null)
        {
            gameManager.OnGameStarted -= OnGameStarted;
            gameManager.OnGameOver -= OnGameOver;
        }

        // Remove button listeners
        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveListener(OnStartGameClicked);
        }

        if (restartGameButton != null)
        {
            restartGameButton.onClick.RemoveListener(OnRestartGameClicked);
        }
    }

    private void OnStartGameClicked()
    {
        if (gameManager != null)
        {
            gameManager.StartGame();
        }
    }

    private void OnRestartGameClicked()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    private void OnGameStarted()
    {
        UpdateUIForGameState(GameManager.GameState.Playing);
    }

    private void OnGameOver()
    {
        UpdateUIForGameState(GameManager.GameState.GameOver);
    }

    private void UpdateUIForGameState(GameManager.GameState state)
    {
        // Hide all panels first
        if (startPanel != null) startPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Show appropriate panel based on state
        switch (state)
        {
            case GameManager.GameState.Start:
                if (startPanel != null) startPanel.SetActive(true);
                break;

            case GameManager.GameState.Playing:
                if (gameplayPanel != null) gameplayPanel.SetActive(true);
                break;

            case GameManager.GameState.GameOver:
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
                break;
        }
    }

    // Public method to manually update UI (useful for testing)
    [ContextMenu("Update UI State")]
    public void UpdateUIState()
    {
        if (gameManager != null)
        {
            UpdateUIForGameState(gameManager.CurrentGameState);
        }
    }

    // Public method to force show start panel
    [ContextMenu("Show Start Panel")]
    public void ShowStartPanel()
    {
        UpdateUIForGameState(GameManager.GameState.Start);
    }
}