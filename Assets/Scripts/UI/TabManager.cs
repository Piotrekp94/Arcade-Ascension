using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class TabManager : MonoBehaviour
{
    public enum TabType
    {
        Levels,
        Upgrades
    }

    [Header("Tab Panels")]
    [SerializeField] private CanvasGroup levelsPanel;
    [SerializeField] private CanvasGroup upgradesPanel;

    [Header("Tab Buttons")]
    [SerializeField] private Button levelsButton;
    [SerializeField] private Button upgradesButton;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.3f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Events
    public event System.Action<TabType> OnTabChanged;

    // Private state
    private TabType currentTab = TabType.Levels;
    private bool isTransitioning = false;
    private Coroutine transitionCoroutine;

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        // Set up button listeners
        if (levelsButton != null)
        {
            levelsButton.onClick.RemoveAllListeners();
            levelsButton.onClick.AddListener(() => SwitchToTab(TabType.Levels));
        }

        if (upgradesButton != null)
        {
            upgradesButton.onClick.RemoveAllListeners();
            upgradesButton.onClick.AddListener(() => SwitchToTab(TabType.Upgrades));
        }

        // Initialize with Levels tab active
        SetTabImmediate(TabType.Levels);
    }

    public void SwitchToTab(TabType targetTab)
    {
        // Ignore if already transitioning or already on target tab
        if (isTransitioning || currentTab == targetTab)
            return;

        // Stop any existing transition
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        // In edit mode tests, coroutines don't run automatically, so switch immediately
        if (!Application.isPlaying)
        {
            SwitchToTabImmediate(targetTab);
            return;
        }

        // Start new transition
        transitionCoroutine = StartCoroutine(TransitionToTab(targetTab));
    }

    // Method for immediate tab switching (used in edit mode tests)
    public void SwitchToTabImmediate(TabType targetTab)
    {
        if (currentTab == targetTab)
            return;

        CanvasGroup currentPanel = GetPanelForTab(currentTab);
        CanvasGroup targetPanel = GetPanelForTab(targetTab);

        if (currentPanel == null || targetPanel == null)
        {
            Debug.LogError($"TabManager: Missing panel reference for tab switch {currentTab} -> {targetTab}");
            return;
        }

        // Update panel states immediately
        currentPanel.alpha = 0f;
        currentPanel.gameObject.SetActive(false);
        currentPanel.interactable = false;
        
        targetPanel.alpha = 1f;
        targetPanel.gameObject.SetActive(true);
        targetPanel.interactable = true;

        // Update current tab
        currentTab = targetTab;

        // Fire event
        OnTabChanged?.Invoke(currentTab);

        Debug.Log($"TabManager: Immediately switched to {currentTab} tab");
    }

    private IEnumerator TransitionToTab(TabType targetTab)
    {
        isTransitioning = true;
        
        CanvasGroup currentPanel = GetPanelForTab(currentTab);
        CanvasGroup targetPanel = GetPanelForTab(targetTab);

        if (currentPanel == null || targetPanel == null)
        {
            Debug.LogError($"TabManager: Missing panel reference for tab transition {currentTab} -> {targetTab}");
            isTransitioning = false;
            yield break;
        }

        // Ensure target panel is active but transparent
        targetPanel.gameObject.SetActive(true);
        targetPanel.alpha = 0f;
        targetPanel.interactable = false;

        // Fade out current panel and fade in target panel
        float elapsed = 0f;
        float startAlpha = currentPanel.alpha;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / transitionDuration;
            float easedProgress = transitionCurve.Evaluate(progress);

            // Fade out current
            currentPanel.alpha = Mathf.Lerp(startAlpha, 0f, easedProgress);
            // Fade in target
            targetPanel.alpha = Mathf.Lerp(0f, 1f, easedProgress);

            yield return null;
        }

        // Ensure final values
        currentPanel.alpha = 0f;
        targetPanel.alpha = 1f;

        // Update panel states
        currentPanel.gameObject.SetActive(false);
        currentPanel.interactable = false;
        
        targetPanel.interactable = true;

        // Update current tab
        currentTab = targetTab;
        isTransitioning = false;

        // Fire event
        OnTabChanged?.Invoke(currentTab);

        Debug.Log($"TabManager: Switched to {currentTab} tab");
    }

    private void SetTabImmediate(TabType targetTab)
    {
        // Stop any transitions
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        isTransitioning = false;
        currentTab = targetTab;

        // Set panel states immediately
        if (levelsPanel != null)
        {
            bool isLevelsActive = (targetTab == TabType.Levels);
            levelsPanel.gameObject.SetActive(isLevelsActive);
            levelsPanel.alpha = isLevelsActive ? 1f : 0f;
            levelsPanel.interactable = isLevelsActive;
        }

        if (upgradesPanel != null)
        {
            bool isUpgradesActive = (targetTab == TabType.Upgrades);
            upgradesPanel.gameObject.SetActive(isUpgradesActive);
            upgradesPanel.alpha = isUpgradesActive ? 1f : 0f;
            upgradesPanel.interactable = isUpgradesActive;
        }

        // Fire event
        OnTabChanged?.Invoke(currentTab);
    }

    private CanvasGroup GetPanelForTab(TabType tab)
    {
        switch (tab)
        {
            case TabType.Levels: return levelsPanel;
            case TabType.Upgrades: return upgradesPanel;
            default: return null;
        }
    }

    // Public getters for testing and external access
    public TabType GetCurrentTab()
    {
        return currentTab;
    }

    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    public float GetTransitionDuration()
    {
        return transitionDuration;
    }

    // Methods for testing setup
    public void SetTabPanels(CanvasGroup levels, CanvasGroup upgrades)
    {
        levelsPanel = levels;
        upgradesPanel = upgrades;
    }

    public void SetTabButtons(Button levels, Button upgrades)
    {
        levelsButton = levels;
        upgradesButton = upgrades;
    }

    // Context menu for debugging
    [ContextMenu("Switch to Levels")]
    private void DebugSwitchToLevels()
    {
        SwitchToTab(TabType.Levels);
    }

    [ContextMenu("Switch to Upgrades")]
    private void DebugSwitchToUpgrades()
    {
        SwitchToTab(TabType.Upgrades);
    }

    [ContextMenu("Log Current State")]
    private void DebugLogCurrentState()
    {
        Debug.Log($"TabManager State:" +
                  $"\n- Current Tab: {currentTab}" +
                  $"\n- Is Transitioning: {isTransitioning}" +
                  $"\n- Levels Panel: {(levelsPanel != null ? $"Alpha={levelsPanel.alpha}, Active={levelsPanel.gameObject.activeInHierarchy}" : "NULL")}" +
                  $"\n- Upgrades Panel: {(upgradesPanel != null ? $"Alpha={upgradesPanel.alpha}, Active={upgradesPanel.gameObject.activeInHierarchy}" : "NULL")}");
    }
}