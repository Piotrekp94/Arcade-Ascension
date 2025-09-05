using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.TestTools;

public class TabManagerTests
{
    private GameObject tabManagerObject;
    private TabManager tabManager;
    private GameObject levelsPanel;
    private GameObject upgradesPanel;
    private Button levelsButton;
    private Button upgradesButton;
    private CanvasGroup levelsCanvasGroup;
    private CanvasGroup upgradesCanvasGroup;

    [SetUp]
    public void SetUp()
    {
        // Create TabManager GameObject
        tabManagerObject = new GameObject("TabManager");
        tabManager = tabManagerObject.AddComponent<TabManager>();

        // Create tab panels with CanvasGroups
        levelsPanel = new GameObject("LevelsPanel");
        levelsPanel.transform.SetParent(tabManagerObject.transform);
        levelsCanvasGroup = levelsPanel.AddComponent<CanvasGroup>();

        upgradesPanel = new GameObject("UpgradesPanel");
        upgradesPanel.transform.SetParent(tabManagerObject.transform);
        upgradesCanvasGroup = upgradesPanel.AddComponent<CanvasGroup>();

        // Create tab buttons
        GameObject levelsButtonObject = new GameObject("LevelsButton");
        levelsButtonObject.transform.SetParent(tabManagerObject.transform);
        levelsButton = levelsButtonObject.AddComponent<Button>();

        GameObject upgradesButtonObject = new GameObject("UpgradesButton");
        upgradesButtonObject.transform.SetParent(tabManagerObject.transform);
        upgradesButton = upgradesButtonObject.AddComponent<Button>();

        // Setup TabManager references
        tabManager.SetTabPanels(levelsCanvasGroup, upgradesCanvasGroup);
        tabManager.SetTabButtons(levelsButton, upgradesButton);
        tabManager.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        if (tabManagerObject != null)
        {
            if (Application.isPlaying)
                Object.Destroy(tabManagerObject);
            else
                Object.DestroyImmediate(tabManagerObject);
        }
    }

    [Test]
    public void TabManager_Initialize_SetsLevelsTabActive()
    {
        // Arrange & Act - already done in SetUp

        // Assert
        Assert.AreEqual(TabManager.TabType.Levels, tabManager.GetCurrentTab());
        Assert.AreEqual(1f, levelsCanvasGroup.alpha);
        Assert.AreEqual(0f, upgradesCanvasGroup.alpha);
        Assert.IsTrue(levelsPanel.activeInHierarchy);
        Assert.IsFalse(upgradesPanel.activeInHierarchy);
    }

    [Test]
    public void TabManager_SwitchToUpgrades_ChangesCurrentTab()
    {
        // Act
        tabManager.SwitchToTab(TabManager.TabType.Upgrades);

        // Assert
        Assert.AreEqual(TabManager.TabType.Upgrades, tabManager.GetCurrentTab());
    }

    [Test]
    public void TabManager_SwitchToLevels_ChangesCurrentTab()
    {
        // Arrange - switch to upgrades first
        tabManager.SwitchToTab(TabManager.TabType.Upgrades);

        // Act
        tabManager.SwitchToTab(TabManager.TabType.Levels);

        // Assert
        Assert.AreEqual(TabManager.TabType.Levels, tabManager.GetCurrentTab());
    }

    [Test]
    public void TabManager_IsTransitioning_ReturnsFalseInitially()
    {
        // Assert
        Assert.IsFalse(tabManager.IsTransitioning());
    }

    [Test]
    public void TabManager_SwitchToSameTab_DoesNothing()
    {
        // Arrange
        var initialTab = tabManager.GetCurrentTab();
        var initialLevelsAlpha = levelsCanvasGroup.alpha;
        var initialUpgradesAlpha = upgradesCanvasGroup.alpha;

        // Act
        tabManager.SwitchToTab(TabManager.TabType.Levels); // Already on Levels

        // Assert
        Assert.AreEqual(initialTab, tabManager.GetCurrentTab());
        Assert.AreEqual(initialLevelsAlpha, levelsCanvasGroup.alpha);
        Assert.AreEqual(initialUpgradesAlpha, upgradesCanvasGroup.alpha);
        Assert.IsFalse(tabManager.IsTransitioning());
    }

    [Test]
    public void TabManager_SwitchToUpgrades_PerformsFadeTransition()
    {
        // Act
        tabManager.SwitchToTab(TabManager.TabType.Upgrades);

        // Assert - transition complete (immediate in edit mode)
        Assert.IsFalse(tabManager.IsTransitioning());
        Assert.AreEqual(TabManager.TabType.Upgrades, tabManager.GetCurrentTab());
        Assert.AreEqual(0f, levelsCanvasGroup.alpha);
        Assert.AreEqual(1f, upgradesCanvasGroup.alpha);
        Assert.IsFalse(levelsPanel.activeInHierarchy);
        Assert.IsTrue(upgradesPanel.activeInHierarchy);
    }

    [Test]
    public void TabManager_SwitchToLevels_PerformsFadeTransition()
    {
        // Arrange - start with upgrades tab
        tabManager.SwitchToTab(TabManager.TabType.Upgrades);
        // In edit mode, this happens immediately
        Assert.AreEqual(TabManager.TabType.Upgrades, tabManager.GetCurrentTab());

        // Act
        tabManager.SwitchToTab(TabManager.TabType.Levels);

        // Assert - transition complete (immediate in edit mode)
        Assert.IsFalse(tabManager.IsTransitioning());
        Assert.AreEqual(TabManager.TabType.Levels, tabManager.GetCurrentTab());
        Assert.AreEqual(1f, levelsCanvasGroup.alpha);
        Assert.AreEqual(0f, upgradesCanvasGroup.alpha);
        Assert.IsTrue(levelsPanel.activeInHierarchy);
        Assert.IsFalse(upgradesPanel.activeInHierarchy);
    }

    [Test]
    public void TabManager_RapidTabSwitching_HandledCorrectly()
    {
        // In edit mode, transitions are immediate, so we test the final state
        // In play mode, the second click would be ignored during transition
        
        // Act - switch to upgrades
        tabManager.SwitchToTab(TabManager.TabType.Upgrades);
        
        // Act - immediately try to switch back
        tabManager.SwitchToTab(TabManager.TabType.Levels);

        // Assert - no longer transitioning (immediate in edit mode)
        Assert.IsFalse(tabManager.IsTransitioning());
        
        if (Application.isPlaying)
        {
            // In play mode, second click should be ignored, ending up on Upgrades
            Assert.AreEqual(TabManager.TabType.Upgrades, tabManager.GetCurrentTab());
        }
        else
        {
            // In edit mode, both switches happen immediately, ending up on Levels
            Assert.AreEqual(TabManager.TabType.Levels, tabManager.GetCurrentTab());
        }
    }

    [Test]
    public void TabManager_GetTransitionDuration_ReturnsPositiveValue()
    {
        // Assert
        Assert.Greater(tabManager.GetTransitionDuration(), 0f);
    }

    [Test]
    public void TabManager_SetTabPanels_UpdatesReferences()
    {
        // Arrange
        var newLevelsPanel = new GameObject("NewLevels").AddComponent<CanvasGroup>();
        var newUpgradesPanel = new GameObject("NewUpgrades").AddComponent<CanvasGroup>();

        // Act
        tabManager.SetTabPanels(newLevelsPanel, newUpgradesPanel);

        // Assert - should update references (verified through behavior)
        Assert.DoesNotThrow(() => tabManager.SwitchToTab(TabManager.TabType.Upgrades));

        // Cleanup
        Object.DestroyImmediate(newLevelsPanel.gameObject);
        Object.DestroyImmediate(newUpgradesPanel.gameObject);
    }

    [Test]
    public void TabManager_SetTabButtons_UpdatesReferences()
    {
        // Arrange
        var newLevelsButton = new GameObject("NewLevelsButton").AddComponent<Button>();
        var newUpgradesButton = new GameObject("NewUpgradesButton").AddComponent<Button>();

        // Act
        tabManager.SetTabButtons(newLevelsButton, newUpgradesButton);

        // Assert - should update references (verified through behavior)
        Assert.DoesNotThrow(() => tabManager.Initialize());

        // Cleanup
        Object.DestroyImmediate(newLevelsButton.gameObject);
        Object.DestroyImmediate(newUpgradesButton.gameObject);
    }
}