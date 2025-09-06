using UnityEngine;
using TMPro;

/// <summary>
/// Coordinates visibility between different UI systems:
/// - TabSystemUI (level selection, upgrades) - shown during menu states
/// - In-game UI elements (timer, score) - shown during gameplay
/// </summary>
public class UICoordinator : MonoBehaviour
{
    public static UICoordinator Instance { get; private set; }

    [Header("UI System References")]
    [SerializeField] private TabSystemUI tabSystemUI;
    [SerializeField] private GameUIManager gameUIManager;

    [Header("In-Game UI Elements")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private CanvasGroup inGameUIGroup;

    private GameManager gameManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple UICoordinator instances found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        // Find GameManager
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("UICoordinator: GameManager not found!");
            return;
        }

        // Subscribe to game state changes
        gameManager.OnGameStateChanged += OnGameStateChanged;

        // Find UI systems if not assigned
        if (tabSystemUI == null)
            tabSystemUI = FindFirstObjectByType<TabSystemUI>();
        if (gameUIManager == null)
            gameUIManager = FindFirstObjectByType<GameUIManager>();

        // Setup in-game UI group if needed
        SetupInGameUIGroup();

        // Initialize UI state based on current game state
        UpdateUIVisibility(gameManager.CurrentGameState);
    }

    void SetupInGameUIGroup()
    {
        if (inGameUIGroup == null && (timerText != null || scoreText != null))
        {
            // Create a CanvasGroup to manage in-game UI elements
            GameObject inGameUIContainer = new GameObject("InGameUI");
            inGameUIContainer.transform.SetParent(transform);
            inGameUIGroup = inGameUIContainer.AddComponent<CanvasGroup>();

            // Move timer and score under the in-game UI group
            if (timerText != null)
                timerText.transform.SetParent(inGameUIContainer.transform);
            if (scoreText != null)
                scoreText.transform.SetParent(inGameUIContainer.transform);
        }
    }

    void OnGameStateChanged(GameManager.GameState newState)
    {
        Debug.Log($"UICoordinator: Game state changed to {newState}");
        UpdateUIVisibility(newState);
    }

    void UpdateUIVisibility(GameManager.GameState gameState)
    {
        switch (gameState)
        {
            case GameManager.GameState.Start:
                ShowMenuUI();
                HideInGameUI();
                break;

            case GameManager.GameState.Playing:
                HideMenuUI();
                ShowInGameUI();
                break;

            case GameManager.GameState.GameOver:
                HideInGameUI();
                ShowMenuUI();
                break;
        }
    }

    void ShowMenuUI()
    {
        Debug.Log("UICoordinator: Showing menu UI");
        
        if (tabSystemUI != null)
        {
            tabSystemUI.ShowTabSystem();
        }
    }

    void HideMenuUI()
    {
        Debug.Log("UICoordinator: Hiding menu UI");
        
        if (tabSystemUI != null)
        {
            tabSystemUI.HideTabSystem();
        }
    }

    void ShowInGameUI()
    {
        Debug.Log("UICoordinator: Showing in-game UI");
        
        if (inGameUIGroup != null)
        {
            inGameUIGroup.alpha = 1f;
            inGameUIGroup.interactable = false; // In-game UI is display-only
            inGameUIGroup.blocksRaycasts = false;
        }
        else
        {
            // Fallback: ensure individual elements are visible
            if (timerText != null)
                timerText.gameObject.SetActive(true);
            if (scoreText != null)
                scoreText.gameObject.SetActive(true);
        }
    }

    void HideInGameUI()
    {
        Debug.Log("UICoordinator: Hiding in-game UI");
        
        if (inGameUIGroup != null)
        {
            inGameUIGroup.alpha = 0f;
            inGameUIGroup.interactable = false;
            inGameUIGroup.blocksRaycasts = false;
        }
        else
        {
            // Fallback: hide individual elements
            if (timerText != null)
                timerText.gameObject.SetActive(false);
            if (scoreText != null)
                scoreText.gameObject.SetActive(false);
        }
    }

    // Public methods for manual control
    public void ForceShowMenuUI()
    {
        ShowMenuUI();
    }

    public void ForceHideMenuUI()
    {
        HideMenuUI();
    }

    public void ForceShowInGameUI()
    {
        ShowInGameUI();
    }

    public void ForceHideInGameUI()
    {
        HideInGameUI();
    }

    // Testing and debugging methods
    public bool IsMenuUIVisible()
    {
        return tabSystemUI != null && tabSystemUI.IsVisible();
    }

    public bool IsInGameUIVisible()
    {
        if (inGameUIGroup != null)
        {
            return inGameUIGroup.alpha > 0f;
        }
        
        // Fallback check
        bool timerVisible = timerText != null && timerText.gameObject.activeSelf;
        bool scoreVisible = scoreText != null && scoreText.gameObject.activeSelf;
        return timerVisible || scoreVisible;
    }

    // Setup methods for testing
    public void SetupForTesting(TabSystemUI tabSystem, GameUIManager gameUI, 
                               TextMeshProUGUI timer, TextMeshProUGUI score, 
                               GameManager testGameManager)
    {
        tabSystemUI = tabSystem;
        gameUIManager = gameUI;
        timerText = timer;
        scoreText = score;
        gameManager = testGameManager;
        
        SetupInGameUIGroup();
        
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += OnGameStateChanged;
            UpdateUIVisibility(gameManager.CurrentGameState);
        }
    }

    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }

    // Debug methods
    [ContextMenu("Show Menu UI")]
    void DebugShowMenuUI()
    {
        ForceShowMenuUI();
    }

    [ContextMenu("Show In-Game UI")]
    void DebugShowInGameUI()
    {
        ForceShowInGameUI();
    }

    [ContextMenu("Log UI State")]
    void DebugLogUIState()
    {
        Debug.Log($"UICoordinator State:" +
                  $"\n- Game State: {(gameManager != null ? gameManager.CurrentGameState.ToString() : "No GameManager")}" +
                  $"\n- Menu UI Visible: {IsMenuUIVisible()}" +
                  $"\n- In-Game UI Visible: {IsInGameUIVisible()}" +
                  $"\n- TabSystemUI: {(tabSystemUI != null ? "Found" : "NULL")}" +
                  $"\n- GameUIManager: {(gameUIManager != null ? "Found" : "NULL")}");
    }
}