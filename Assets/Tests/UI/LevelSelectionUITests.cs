using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelSelectionUITests
{
    private LevelSelectionUI levelSelectionUI;
    private GameObject levelSelectionUIGO;
    private LevelManager levelManager;
    private GameObject levelManagerGO;
    private GameManager gameManager;
    private GameObject gameManagerGO;
    private List<LevelData> testLevelData;

    [SetUp]
    public void Setup()
    {
        // Create GameManager for testing
        gameManagerGO = new GameObject("GameManager");
        gameManager = gameManagerGO.AddComponent<GameManager>();
        GameManager.SetInstanceForTesting(gameManager);

        // Create test level data
        testLevelData = new List<LevelData>();
        for (int i = 1; i <= 3; i++)
        {
            LevelData levelData = ScriptableObject.CreateInstance<LevelData>();
            SetPrivateField(levelData, "levelId", i);
            SetPrivateField(levelData, "levelName", $"Level {i}");
            SetPrivateField(levelData, "levelDescription", $"Test level {i}");
            SetPrivateField(levelData, "blockRows", 3 + i);
            SetPrivateField(levelData, "blockColumns", 5);
            SetPrivateField(levelData, "blockSpacing", 0.1f);
            SetPrivateField(levelData, "spawnAreaOffset", new Vector2(0f, -1f));
            SetPrivateField(levelData, "scoreMultiplier", 1.0f);
            SetPrivateField(levelData, "defaultBlockScore", 10);
            
            testLevelData.Add(levelData);
        }

        // Create LevelManager
        levelManagerGO = new GameObject("LevelManager");
        levelManager = levelManagerGO.AddComponent<LevelManager>();
        LevelManager.SetInstanceForTesting(levelManager);
        levelManager.InitializeForTesting(testLevelData);

        // Create LevelSelectionUI
        levelSelectionUIGO = new GameObject("LevelSelectionUI");
        levelSelectionUI = levelSelectionUIGO.AddComponent<LevelSelectionUI>();
        
        // Create mock UI elements
        CreateMockUIElements();
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up singletons
        GameManager.SetInstanceForTesting(null);
        LevelManager.SetInstanceForTesting(null);
        
        // Clean up test level data
        foreach (var levelData in testLevelData)
        {
            if (levelData != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(levelData);
                else
                    Object.DestroyImmediate(levelData);
            }
        }
        testLevelData.Clear();
        
        // Clean up GameObjects
        if (Application.isPlaying)
        {
            if (levelSelectionUIGO != null) Object.Destroy(levelSelectionUIGO);
            if (levelManagerGO != null) Object.Destroy(levelManagerGO);
            if (gameManagerGO != null) Object.Destroy(gameManagerGO);
        }
        else
        {
            if (levelSelectionUIGO != null) Object.DestroyImmediate(levelSelectionUIGO);
            if (levelManagerGO != null) Object.DestroyImmediate(levelManagerGO);
            if (gameManagerGO != null) Object.DestroyImmediate(gameManagerGO);
        }
    }

    [Test]
    public void LevelSelectionUI_InitializesWithCorrectButtonCount()
    {
        levelSelectionUI.InitializeForTesting();
        
        List<Button> levelButtons = levelSelectionUI.GetLevelButtons();
        Assert.AreEqual(3, levelButtons.Count); // Should have 3 buttons for 3 levels
    }

    [Test]
    public void LevelSelectionUI_Level1ButtonIsUnlocked()
    {
        levelSelectionUI.InitializeForTesting();
        levelSelectionUI.UpdateButtonStates();
        
        Button level1Button = levelSelectionUI.GetLevelButton(1);
        Assert.IsTrue(level1Button.interactable);
    }

    [Test]
    public void LevelSelectionUI_HigherLevelButtonsAreLockedInitially()
    {
        levelSelectionUI.InitializeForTesting();
        levelSelectionUI.UpdateButtonStates();
        
        Button level2Button = levelSelectionUI.GetLevelButton(2);
        Button level3Button = levelSelectionUI.GetLevelButton(3);
        
        Assert.IsFalse(level2Button.interactable);
        Assert.IsFalse(level3Button.interactable);
    }

    [Test]
    public void LevelSelectionUI_UpdatesButtonStatesOnLevelUnlock()
    {
        levelSelectionUI.InitializeForTesting();
        levelSelectionUI.UpdateButtonStates();
        
        // Initially level 2 should be locked
        Button level2Button = levelSelectionUI.GetLevelButton(2);
        Assert.IsFalse(level2Button.interactable);
        
        // Unlock level 2
        levelManager.SetCurrentLevel(1);
        levelManager.UnlockNextLevel();
        levelSelectionUI.UpdateButtonStates();
        
        // Now level 2 should be unlocked
        Assert.IsTrue(level2Button.interactable);
    }

    [Test]
    public void LevelSelectionUI_LevelButtonClick_SelectsLevel()
    {
        levelSelectionUI.InitializeForTesting();
        
        bool levelSelected = false;
        int selectedLevelId = -1;
        
        levelSelectionUI.OnLevelSelected += (levelId) => {
            levelSelected = true;
            selectedLevelId = levelId;
        };
        
        levelSelectionUI.OnLevelButtonClicked(1);
        
        Assert.IsTrue(levelSelected);
        Assert.AreEqual(1, selectedLevelId);
    }

    [Test]
    public void LevelSelectionUI_CannotSelectLockedLevel()
    {
        levelSelectionUI.InitializeForTesting();
        
        bool levelSelected = false;
        levelSelectionUI.OnLevelSelected += (levelId) => levelSelected = true;
        
        levelSelectionUI.OnLevelButtonClicked(2); // Level 2 is locked
        
        Assert.IsFalse(levelSelected);
    }

    [Test]
    public void LevelSelectionUI_DisplaysCorrectLevelInformation()
    {
        levelSelectionUI.InitializeForTesting();
        
        string level1Text = levelSelectionUI.GetLevelButtonText(1);
        Assert.AreEqual("1", level1Text);
        
        string level2Text = levelSelectionUI.GetLevelButtonText(2);
        Assert.AreEqual("2", level2Text);
    }

    [Test]
    public void LevelSelectionUI_RespondsToLevelUnlockEvents()
    {
        levelSelectionUI.InitializeForTesting();
        
        // Mock that UI automatically updates when level manager unlocks levels
        levelManager.SetCurrentLevel(1);
        levelManager.UnlockNextLevel(); // This should trigger UI update
        
        Button level2Button = levelSelectionUI.GetLevelButton(2);
        Assert.IsTrue(level2Button.interactable);
    }

    [Test]
    public void LevelSelectionUI_HandlesInvalidLevelId()
    {
        levelSelectionUI.InitializeForTesting();
        
        Button invalidButton = levelSelectionUI.GetLevelButton(999);
        Assert.IsNull(invalidButton);
        
        string invalidText = levelSelectionUI.GetLevelButtonText(999);
        Assert.IsNull(invalidText);
    }

    [Test]
    public void LevelSelectionUI_ShowsAndHidesPanelCorrectly()
    {
        levelSelectionUI.InitializeForTesting();
        
        levelSelectionUI.ShowLevelSelection();
        Assert.IsTrue(levelSelectionUI.IsVisible());
        
        levelSelectionUI.HideLevelSelection();
        Assert.IsFalse(levelSelectionUI.IsVisible());
    }

    [Test]
    public void LevelSelectionUI_HasRequiredEventSystem()
    {
        // Test that LevelSelectionUI has proper event handling by subscribing to it
        bool canSubscribeToEvent = false;
        
        try
        {
            levelSelectionUI.OnLevelSelected += (levelId) => { };
            canSubscribeToEvent = true;
            levelSelectionUI.OnLevelSelected -= (levelId) => { };
        }
        catch
        {
            canSubscribeToEvent = false;
        }
        
        Assert.IsTrue(canSubscribeToEvent, "Should be able to subscribe to OnLevelSelected event");
    }

    // Helper methods
    private void CreateMockUIElements()
    {
        // Create a mock canvas and panel structure
        GameObject canvas = new GameObject("Canvas");
        canvas.AddComponent<Canvas>();
        canvas.transform.SetParent(levelSelectionUIGO.transform);
        
        GameObject panel = new GameObject("LevelSelectionPanel");
        panel.transform.SetParent(canvas.transform);
        
        // Create mock level buttons
        List<Button> mockButtons = new List<Button>();
        for (int i = 1; i <= 3; i++)
        {
            GameObject buttonGO = new GameObject($"LevelButton{i}");
            buttonGO.transform.SetParent(panel.transform);
            Button button = buttonGO.AddComponent<Button>();
            
            // Add text component
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform);
            Text text = textGO.AddComponent<Text>();
            text.text = i.ToString();
            
            mockButtons.Add(button);
        }
        
        levelSelectionUI.SetMockUIElementsForTesting(panel, mockButtons);
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field {fieldName} not found");
        field.SetValue(target, value);
    }
}