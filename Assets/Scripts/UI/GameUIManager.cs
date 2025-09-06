using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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

    [Header("Timer Display")]
    [SerializeField]
    private TextMeshProUGUI timerText;
    
    // Timer display events
    public event System.Action OnTimerDisplayUpdated;

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
            gameManager.OnGameStateChanged += OnGameStateChanged;
            
            // Subscribe to timer events
            gameManager.OnTimerUpdated += UpdateTimerDisplay;
            gameManager.OnTimerExpired += OnTimerExpired;
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
            gameManager.OnGameStateChanged -= OnGameStateChanged;
            
            // Unsubscribe from timer events
            gameManager.OnTimerUpdated -= UpdateTimerDisplay;
            gameManager.OnTimerExpired -= OnTimerExpired;
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

    private void OnGameStateChanged(GameManager.GameState newState)
    {
        Debug.Log($"GameUIManager: State changed to {newState}");
        UpdateUIForGameState(newState);
    }

    private void UpdateUIForGameState(GameManager.GameState state)
    {
        Debug.Log($"GameUIManager: Updating UI for state {state} - NOTE: Panel management is now handled by UICoordinator");
        
        // GameUIManager now focuses only on timer display and events
        // Panel visibility is managed by UICoordinator
        // This method is kept for backward compatibility and timer updates
        
        // Timer visibility is handled by UICoordinator through the timer text reference
        // No need to manage panel visibility here anymore
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

    // Timer display methods
    public void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText != null)
        {
            // Format time as MM:SS
            string formattedTime = FormatTime(timeRemaining);
            timerText.text = formattedTime;
            
            // Update color based on time remaining
            UpdateTimerColor(timeRemaining);
            
            OnTimerDisplayUpdated?.Invoke();
        }
    }
    
    private void UpdateTimerColor(float timeRemaining)
    {
        if (timerText == null) return;
        
        // Get the total time limit from GameManager to calculate percentage
        float totalTime = 120f; // Default fallback
        if (gameManager != null)
        {
            totalTime = gameManager.GetTimeLimit();
        }
        
        // Calculate percentage of time remaining
        float percentage = (timeRemaining / totalTime) * 100f;
        
        // Color based on percentage: >50% green, 25-50% yellow, <25% red
        if (percentage > 50f)
        {
            timerText.color = Color.green;
        }
        else if (percentage >= 25f)
        {
            timerText.color = Color.yellow;
        }
        else
        {
            timerText.color = Color.red;
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds < 0f)
            timeInSeconds = 0f;
            
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    public void OnTimerExpired()
    {
        if (timerText != null)
        {
            timerText.text = "00:00";
            timerText.color = Color.red;
        }
    }
    
    // Testing and configuration methods
    public TextMeshProUGUI GetTimerText()
    {
        return timerText;
    }
    
    public void SetTimerText(TextMeshProUGUI newTimerText)
    {
        timerText = newTimerText;
    }
    
    public void SetGameManager(GameManager newGameManager)
    {
        gameManager = newGameManager;
    }
    
    public void InitializeTimerSubscriptions()
    {
        if (gameManager != null)
        {
            gameManager.OnTimerUpdated += UpdateTimerDisplay;
            gameManager.OnTimerExpired += OnTimerExpired;
        }
    }
    
    public void CleanupTimerSubscriptions()
    {
        if (gameManager != null)
        {
            gameManager.OnTimerUpdated -= UpdateTimerDisplay;
            gameManager.OnTimerExpired -= OnTimerExpired;
        }
    }
}