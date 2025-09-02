using UnityEngine;

/// <summary>
/// Integrates LevelSelectionUI with game systems to start levels when selected
/// </summary>
public class LevelSelectionIntegrator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelSelectionUI levelSelectionUI;
    [SerializeField] private GameUIManager gameUIManager;
    
    void Start()
    {
        SetupLevelSelectionIntegration();
    }
    
    void SetupLevelSelectionIntegration()
    {
        // Find components if not assigned
        if (levelSelectionUI == null)
        {
            levelSelectionUI = FindFirstObjectByType<LevelSelectionUI>();
        }
        
        if (gameUIManager == null)
        {
            gameUIManager = FindFirstObjectByType<GameUIManager>();
        }
        
        // Connect level selection to game start
        if (levelSelectionUI != null)
        {
            levelSelectionUI.OnLevelSelected += OnLevelSelected;
            Debug.Log("LevelSelectionIntegrator: Connected to LevelSelectionUI");
        }
        else
        {
            Debug.LogError("LevelSelectionIntegrator: LevelSelectionUI not found!");
        }
        
        // Connect to GameManager level completion
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelCompleted += OnLevelCompleted;
            Debug.Log("LevelSelectionIntegrator: Connected to GameManager level completion");
        }
    }
    
    void OnLevelSelected(int levelId)
    {
        Debug.Log($"LevelSelectionIntegrator: Level {levelId} selected");
        
        // Validate level selection
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelSelectionIntegrator: LevelManager not found!");
            return;
        }
        
        if (!LevelManager.Instance.IsLevelUnlocked(levelId))
        {
            Debug.LogWarning($"LevelSelectionIntegrator: Level {levelId} is locked!");
            return;
        }
        
        // Set current level in LevelManager
        LevelManager.Instance.SetCurrentLevel(levelId);
        LevelData selectedLevelData = LevelManager.Instance.GetCurrentLevelData();
        
        if (selectedLevelData == null)
        {
            Debug.LogError($"LevelSelectionIntegrator: No level data found for level {levelId}!");
            return;
        }
        
        Debug.Log($"LevelSelectionIntegrator: Configuring game for {selectedLevelData.LevelName}");
        
        // Configure BlockManager with selected level
        BlockManager blockManager = FindFirstObjectByType<BlockManager>();
        if (blockManager != null)
        {
            blockManager.ConfigureForLevel(selectedLevelData);
            Debug.Log($"LevelSelectionIntegrator: BlockManager configured for level {levelId}");
        }
        else
        {
            Debug.LogError("LevelSelectionIntegrator: BlockManager not found!");
        }
        
        // Start the game FIRST to change game state
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
            Debug.Log($"LevelSelectionIntegrator: Started game for level {levelId}");
        }
        else
        {
            Debug.LogError("LevelSelectionIntegrator: GameManager not found!");
            return;
        }
        
        // Hide level selection UI immediately - the UI now checks game state before showing
        if (levelSelectionUI != null)
        {
            levelSelectionUI.HideLevelSelection();
        }
    }
    
    void OnLevelCompleted()
    {
        Debug.Log("LevelSelectionIntegrator: Level completed!");
        
        // Unlock next level
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnlockNextLevel();
            Debug.Log("LevelSelectionIntegrator: Next level unlocked");
        }
        
        // Show level selection after a brief delay
        StartCoroutine(ShowLevelSelectionAfterDelay(2.0f));
    }
    
    System.Collections.IEnumerator ShowLevelSelectionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (levelSelectionUI != null)
        {
            levelSelectionUI.ShowLevelSelection();
            Debug.Log("LevelSelectionIntegrator: Returned to level selection");
        }
    }
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        if (levelSelectionUI != null)
        {
            levelSelectionUI.OnLevelSelected -= OnLevelSelected;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelCompleted -= OnLevelCompleted;
        }
    }
    
    // Debug methods
    [ContextMenu("Test Level 1 Selection")]
    public void TestLevel1Selection()
    {
        OnLevelSelected(1);
    }
    
    [ContextMenu("Show Level Selection")]
    public void ShowLevelSelection()
    {
        if (levelSelectionUI != null)
        {
            levelSelectionUI.ShowLevelSelection();
        }
    }
}