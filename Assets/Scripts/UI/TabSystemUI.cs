using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabSystemUI : MonoBehaviour
{
    [Header("Tab System Components")]
    [SerializeField] private TabManager tabManager;
    
    [Header("Tab Panels")]
    [SerializeField] private CanvasGroup levelsPanel;
    [SerializeField] private CanvasGroup upgradesPanel;
    
    [Header("Tab Buttons")]
    [SerializeField] private Button levelsTabButton;
    [SerializeField] private Button upgradesTabButton;
    
    [Header("Level Selection Integration")]
    [SerializeField] private LevelSelectionUI levelSelectionUI;
    
    // Tab button visual states
    [Header("Tab Visual Settings")]
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = Color.gray;
    
    // Events
    public event System.Action<TabManager.TabType> OnTabSwitched;

    private bool isInitialized = false;

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (isInitialized)
            return;

        // Initialize TabManager if not already set up
        if (tabManager == null)
        {
            tabManager = GetComponent<TabManager>();
            if (tabManager == null)
            {
                tabManager = gameObject.AddComponent<TabManager>();
            }
        }

        // Set up TabManager references
        SetupTabManager();
        
        // Subscribe to tab change events
        SubscribeToEvents();
        
        // Set up initial UI state
        UpdateTabButtonVisuals(TabManager.TabType.Levels);
        
        isInitialized = true;
        
        Debug.Log("TabSystemUI: Initialized successfully");
    }

    void SetupTabManager()
    {
        if (tabManager != null)
        {
            // Connect panels and buttons to TabManager
            tabManager.SetTabPanels(levelsPanel, upgradesPanel);
            tabManager.SetTabButtons(levelsTabButton, upgradesTabButton);
            tabManager.Initialize();
        }
    }

    void SubscribeToEvents()
    {
        if (tabManager != null)
        {
            tabManager.OnTabChanged += OnTabChanged;
        }
    }

    void UnsubscribeFromEvents()
    {
        if (tabManager != null)
        {
            tabManager.OnTabChanged -= OnTabChanged;
        }
    }

    void OnTabChanged(TabManager.TabType newTab)
    {
        Debug.Log($"TabSystemUI: Tab changed to {newTab}");
        
        // Update tab button visuals
        UpdateTabButtonVisuals(newTab);
        
        // Handle tab-specific logic
        HandleTabSpecificLogic(newTab);
        
        // Fire external event
        OnTabSwitched?.Invoke(newTab);
    }

    void UpdateTabButtonVisuals(TabManager.TabType activeTab)
    {
        // Update Levels button
        if (levelsTabButton != null)
        {
            UpdateButtonVisual(levelsTabButton, activeTab == TabManager.TabType.Levels);
        }
        
        // Update Upgrades button  
        if (upgradesTabButton != null)
        {
            UpdateButtonVisual(upgradesTabButton, activeTab == TabManager.TabType.Upgrades);
        }
    }

    void UpdateButtonVisual(Button button, bool isActive)
    {
        if (button == null) return;
        
        // Update button colors
        ColorBlock colors = button.colors;
        colors.normalColor = isActive ? activeTabColor : inactiveTabColor;
        colors.highlightedColor = isActive ? activeTabColor : Color.Lerp(inactiveTabColor, Color.white, 0.2f);
        button.colors = colors;
        
        // Update text color if using TextMeshPro
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = isActive ? Color.black : Color.gray;
        }
        else
        {
            // Fallback for regular Text component
            Text regularText = button.GetComponentInChildren<Text>();
            if (regularText != null)
            {
                regularText.color = isActive ? Color.black : Color.gray;
            }
        }
    }

    void HandleTabSpecificLogic(TabManager.TabType tab)
    {
        switch (tab)
        {
            case TabManager.TabType.Levels:
                HandleLevelsTabActivated();
                break;
                
            case TabManager.TabType.Upgrades:
                HandleUpgradesTabActivated();
                break;
        }
    }

    void HandleLevelsTabActivated()
    {
        // Ensure LevelSelectionUI is properly shown
        if (levelSelectionUI != null)
        {
            if (!levelSelectionUI.IsVisible())
            {
                levelSelectionUI.ShowLevelSelection();
            }
            levelSelectionUI.UpdateButtonStates();
        }
        
        Debug.Log("TabSystemUI: Levels tab activated");
    }

    void HandleUpgradesTabActivated()
    {
        // Future: Initialize upgrade system UI
        Debug.Log("TabSystemUI: Upgrades tab activated");
    }

    // Public methods for external access
    public void SwitchToLevelsTab()
    {
        if (tabManager != null)
        {
            tabManager.SwitchToTab(TabManager.TabType.Levels);
        }
    }

    public void SwitchToUpgradesTab()
    {
        if (tabManager != null)
        {
            tabManager.SwitchToTab(TabManager.TabType.Upgrades);
        }
    }

    public TabManager.TabType GetCurrentTab()
    {
        return tabManager != null ? tabManager.GetCurrentTab() : TabManager.TabType.Levels;
    }

    public bool IsTransitioning()
    {
        return tabManager != null && tabManager.IsTransitioning();
    }

    // Show/Hide the entire tab system
    public void ShowTabSystem()
    {
        gameObject.SetActive(true);
        
        // Ensure current tab content is properly displayed
        if (GetCurrentTab() == TabManager.TabType.Levels && levelSelectionUI != null)
        {
            levelSelectionUI.ShowLevelSelection();
        }
        
        Debug.Log("TabSystemUI: Tab system shown");
    }

    public void HideTabSystem()
    {
        // Hide level selection UI if it's active
        if (levelSelectionUI != null)
        {
            levelSelectionUI.HideLevelSelection();
        }
        
        gameObject.SetActive(false);
        Debug.Log("TabSystemUI: Tab system hidden");
    }

    public bool IsVisible()
    {
        return gameObject.activeSelf;
    }

    // Testing and setup methods
    public void SetupForTesting(TabManager manager, CanvasGroup levels, CanvasGroup upgrades, Button levelsBtn, Button upgradesBtn)
    {
        tabManager = manager;
        levelsPanel = levels;
        upgradesPanel = upgrades;
        levelsTabButton = levelsBtn;
        upgradesTabButton = upgradesBtn;
        
        SetupTabManager();
        SubscribeToEvents(); // This was missing!
        UpdateTabButtonVisuals(TabManager.TabType.Levels);
        isInitialized = true;
    }

    public LevelSelectionUI GetLevelSelectionUI()
    {
        return levelSelectionUI;
    }

    public void SetLevelSelectionUI(LevelSelectionUI ui)
    {
        levelSelectionUI = ui;
    }

    // Context menu methods for debugging
    [ContextMenu("Switch to Levels")]
    void DebugSwitchToLevels()
    {
        SwitchToLevelsTab();
    }

    [ContextMenu("Switch to Upgrades")]
    void DebugSwitchToUpgrades()
    {
        SwitchToUpgradesTab();
    }

    [ContextMenu("Log Current State")]
    void DebugLogCurrentState()
    {
        Debug.Log($"TabSystemUI State:" +
                  $"\n- Current Tab: {GetCurrentTab()}" +
                  $"\n- Is Transitioning: {IsTransitioning()}" +
                  $"\n- Is Visible: {IsVisible()}" +
                  $"\n- Level Selection UI: {(levelSelectionUI != null ? "Found" : "NULL")}");
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
}