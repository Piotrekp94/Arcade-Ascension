using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class LevelSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private GameObject levelSelectionPanel;
    [SerializeField]
    private Transform buttonContainer;
    [SerializeField]
    private Button levelButtonPrefab;
    
    // Event for level selection
    public event Action<int> OnLevelSelected;
    
    // Internal state
    private List<Button> levelButtons = new List<Button>();
    private LevelManager levelManager;
    private bool isInitialized = false;
    private CanvasGroup canvasGroup;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (isInitialized)
            return;
            
        // Setup CanvasGroup for visibility control
        SetupCanvasGroup();
            
        levelManager = LevelManager.Instance;
        
        if (levelManager != null)
        {
            CreateLevelButtons();
            SubscribeToEvents();
            UpdateButtonStates();
            isInitialized = true;
        }
        else
        {
            Debug.LogWarning("LevelSelectionUI: LevelManager instance not found!");
        }
    }

    void SetupCanvasGroup()
    {
        // Try to get CanvasGroup from the panel first, then from this GameObject
        if (levelSelectionPanel != null)
        {
            canvasGroup = levelSelectionPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = levelSelectionPanel.AddComponent<CanvasGroup>();
            }
        }
        else
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    public void InitializeForTesting()
    {
        // Setup CanvasGroup for testing
        SetupCanvasGroup();
        
        // Try to initialize with LevelManager if available
        levelManager = LevelManager.Instance;
        if (levelManager != null)
        {
            if (levelButtons.Count == 0)
            {
                CreateLevelButtons();
            }
            SubscribeToEvents();
        }
        
        isInitialized = true;
    }

    public void SetupCanvasGroupForTesting()
    {
        // Simpler method just for CanvasGroup setup
        SetupCanvasGroup();
        isInitialized = true; // Mark as initialized so blocking logic works
    }

    void CreateLevelButtons()
    {
        if (levelManager == null)
            return;
            
        // Clear existing buttons
        ClearLevelButtons();
        
        int totalLevels = levelManager.GetTotalLevelCount();
        
        for (int levelId = 1; levelId <= totalLevels; levelId++)
        {
            Button levelButton = CreateLevelButton(levelId);
            if (levelButton != null)
            {
                levelButtons.Add(levelButton);
            }
        }
    }

    Button CreateLevelButton(int levelId)
    {
        GameObject buttonGO = null;
        Button button = null;
        
        if (levelButtonPrefab != null && buttonContainer != null)
        {
            // Create from prefab
            buttonGO = Instantiate(levelButtonPrefab.gameObject, buttonContainer);
            button = buttonGO.GetComponent<Button>();
        }
        else if (levelButtons.Count < levelId)
        {
            // Create basic button for testing
            buttonGO = new GameObject($"LevelButton{levelId}");
            if (buttonContainer != null)
                buttonGO.transform.SetParent(buttonContainer);
            else
                buttonGO.transform.SetParent(transform);
                
            button = buttonGO.AddComponent<Button>();
            
            // Add text component
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);
            Text text = textGO.AddComponent<Text>();
            text.text = levelId.ToString();
        }
        
        if (button != null)
        {
            // Set up button click handler
            int capturedLevelId = levelId; // Capture for closure
            button.onClick.AddListener(() => OnLevelButtonClicked(capturedLevelId));
            
            // Set button text
            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = levelId.ToString();
            }
        }
        
        return button;
    }

    void ClearLevelButtons()
    {
        foreach (Button button in levelButtons)
        {
            if (button != null)
            {
                if (Application.isPlaying)
                    Destroy(button.gameObject);
                else
                    DestroyImmediate(button.gameObject);
            }
        }
        levelButtons.Clear();
    }

    void SubscribeToEvents()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelUnlocked += OnLevelUnlocked;
        }
    }

    void UnsubscribeFromEvents()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelUnlocked -= OnLevelUnlocked;
        }
    }

    void OnLevelUnlocked(int levelId)
    {
        UpdateButtonStates();
    }

    public void UpdateButtonStates()
    {
        if (levelManager == null || levelButtons.Count == 0)
            return;
            
        List<int> unlockedLevels = levelManager.GetUnlockedLevels();
        
        for (int i = 0; i < levelButtons.Count; i++)
        {
            int levelId = i + 1; // Convert to 1-based level ID
            Button button = levelButtons[i];
            
            if (button != null)
            {
                bool isUnlocked = unlockedLevels.Contains(levelId);
                button.interactable = isUnlocked;
                
                // Visual feedback for locked/unlocked states
                UpdateButtonVisuals(button, isUnlocked);
            }
        }
    }

    void UpdateButtonVisuals(Button button, bool isUnlocked)
    {
        // Update button appearance based on locked/unlocked state
        ColorBlock colors = button.colors;
        if (isUnlocked)
        {
            colors.normalColor = Color.white;
            colors.disabledColor = Color.white;
        }
        else
        {
            colors.normalColor = Color.gray;
            colors.disabledColor = Color.gray;
        }
        button.colors = colors;
        
        // Update text color if available
        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.color = isUnlocked ? Color.black : Color.darkGray;
        }
    }

    public void OnLevelButtonClicked(int levelId)
    {
        Debug.Log($"LevelSelectionUI: Button clicked for level {levelId}");
        
        if (levelManager == null)
        {
            Debug.LogError("LevelSelectionUI: LevelManager is null!");
            return;
        }
            
        if (levelManager.IsLevelUnlocked(levelId))
        {
            Debug.Log($"LevelSelectionUI: Level {levelId} is unlocked, firing OnLevelSelected event");
            OnLevelSelected?.Invoke(levelId);
        }
        else
        {
            Debug.LogWarning($"LevelSelectionUI: Level {levelId} is locked!");
        }
    }

    public void ShowLevelSelection()
    {
        // Only show if we're not in playing state
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState == GameManager.GameState.Playing)
        {
            Debug.Log("LevelSelectionUI: Not showing level selection - game is currently playing");
            // Ensure UI is hidden when blocked by game state
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            return;
        }
        
        Debug.Log("LevelSelectionUI: Showing level selection");
        
        // Ensure GameObject is active (needed for first-time setup)
        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
        
        // Use CanvasGroup for visibility control
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        // Update button states when showing
        UpdateButtonStates();
    }

    public void HideLevelSelection()
    {
        Debug.Log("LevelSelectionUI: Hiding level selection");
        
        // Use CanvasGroup to hide visually but keep GameObject active
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            // Fallback to old behavior if CanvasGroup isn't available
            if (levelSelectionPanel != null)
            {
                levelSelectionPanel.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    public bool IsVisible()
    {
        // Check CanvasGroup visibility if available
        if (canvasGroup != null)
        {
            return canvasGroup.alpha > 0f && canvasGroup.interactable;
        }
        
        // Fallback to GameObject active state
        if (levelSelectionPanel != null)
        {
            return levelSelectionPanel.activeSelf;
        }
        return gameObject.activeSelf;
    }

    // Public methods for testing and external access
    public List<Button> GetLevelButtons()
    {
        return new List<Button>(levelButtons);
    }

    public Button GetLevelButton(int levelId)
    {
        if (levelId < 1 || levelId > levelButtons.Count)
            return null;
            
        return levelButtons[levelId - 1]; // Convert to zero-based index
    }

    public string GetLevelButtonText(int levelId)
    {
        Button button = GetLevelButton(levelId);
        if (button == null)
            return null;
            
        Text buttonText = button.GetComponentInChildren<Text>();
        return buttonText?.text;
    }

    // Testing methods
    public void SetMockUIElementsForTesting(GameObject panel, List<Button> buttons)
    {
        levelSelectionPanel = panel;
        levelButtons = new List<Button>(buttons);
        
        // Set up click handlers for mock buttons
        for (int i = 0; i < buttons.Count; i++)
        {
            int levelId = i + 1; // Convert to 1-based
            int capturedLevelId = levelId;
            buttons[i].onClick.AddListener(() => OnLevelButtonClicked(capturedLevelId));
        }
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
        ClearLevelButtons();
    }

    // Debug methods
    [ContextMenu("Show Level Selection")]
    public void DebugShowLevelSelection()
    {
        ShowLevelSelection();
    }

    [ContextMenu("Hide Level Selection")]
    public void DebugHideLevelSelection()
    {
        HideLevelSelection();
    }

    [ContextMenu("Update Button States")]
    public void DebugUpdateButtonStates()
    {
        UpdateButtonStates();
    }
}