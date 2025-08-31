using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class LevelManagerTests
{
    private LevelManager levelManager;
    private GameObject levelManagerGO;
    private List<LevelData> testLevelData;

    [SetUp]
    public void Setup()
    {
        // Create LevelManager GameObject
        levelManagerGO = new GameObject("LevelManager");
        levelManager = levelManagerGO.AddComponent<LevelManager>();
        
        // Set up singleton for testing
        LevelManager.SetInstanceForTesting(levelManager);

        // Create test level data
        testLevelData = new List<LevelData>();
        for (int i = 1; i <= 5; i++)
        {
            LevelData levelData = ScriptableObject.CreateInstance<LevelData>();
            SetPrivateField(levelData, "levelId", i);
            SetPrivateField(levelData, "levelName", $"Test Level {i}");
            SetPrivateField(levelData, "levelDescription", $"Test level {i} description");
            SetPrivateField(levelData, "blockRows", 3 + i);
            SetPrivateField(levelData, "blockColumns", 5 + i);
            SetPrivateField(levelData, "blockSpacing", 0.1f);
            SetPrivateField(levelData, "spawnAreaOffset", new Vector2(0f, -1f));
            SetPrivateField(levelData, "scoreMultiplier", 1.0f + (i * 0.2f));
            SetPrivateField(levelData, "defaultBlockScore", 10 * i);
            
            testLevelData.Add(levelData);
        }
        
        // Initialize level manager with test data
        levelManager.InitializeForTesting(testLevelData);
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up singleton
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
        
        // Clean up GameObject
        if (Application.isPlaying)
        {
            if (levelManagerGO != null) Object.Destroy(levelManagerGO);
        }
        else
        {
            if (levelManagerGO != null) Object.DestroyImmediate(levelManagerGO);
        }
    }

    [Test]
    public void LevelManager_SingletonInstanceIsProperlySet()
    {
        Assert.IsNotNull(LevelManager.Instance);
        Assert.AreSame(levelManager, LevelManager.Instance);
    }

    [Test]
    public void LevelManager_HasCorrectTotalLevelCount()
    {
        Assert.AreEqual(5, levelManager.GetTotalLevelCount());
    }

    [Test]
    public void LevelManager_Level1IsUnlockedByDefault()
    {
        Assert.IsTrue(levelManager.IsLevelUnlocked(1));
    }

    [Test]
    public void LevelManager_HigherLevelsAreLockedByDefault()
    {
        Assert.IsFalse(levelManager.IsLevelUnlocked(2));
        Assert.IsFalse(levelManager.IsLevelUnlocked(3));
        Assert.IsFalse(levelManager.IsLevelUnlocked(4));
        Assert.IsFalse(levelManager.IsLevelUnlocked(5));
    }

    [Test]
    public void LevelManager_GetUnlockedLevels_ReturnsOnlyLevel1Initially()
    {
        List<int> unlockedLevels = levelManager.GetUnlockedLevels();
        Assert.AreEqual(1, unlockedLevels.Count);
        Assert.Contains(1, unlockedLevels);
    }

    [Test]
    public void LevelManager_UnlockNextLevel_UnlocksLevel2()
    {
        levelManager.SetCurrentLevel(1);
        levelManager.UnlockNextLevel();
        
        Assert.IsTrue(levelManager.IsLevelUnlocked(2));
        Assert.IsFalse(levelManager.IsLevelUnlocked(3));
    }

    [Test]
    public void LevelManager_UnlockNextLevel_TriggersEvent()
    {
        bool eventTriggered = false;
        int unlockedLevelId = -1;
        
        levelManager.OnLevelUnlocked += (levelId) => {
            eventTriggered = true;
            unlockedLevelId = levelId;
        };
        
        levelManager.SetCurrentLevel(1);
        levelManager.UnlockNextLevel();
        
        Assert.IsTrue(eventTriggered);
        Assert.AreEqual(2, unlockedLevelId);
    }

    [Test]
    public void LevelManager_UnlockNextLevel_DoesNothingIfAlreadyUnlocked()
    {
        levelManager.SetCurrentLevel(1);
        levelManager.UnlockNextLevel(); // Unlock level 2
        
        int eventTriggerCount = 0;
        levelManager.OnLevelUnlocked += (levelId) => eventTriggerCount++;
        
        levelManager.UnlockNextLevel(); // Try to unlock level 2 again
        
        Assert.AreEqual(0, eventTriggerCount); // No event should be triggered
        Assert.IsTrue(levelManager.IsLevelUnlocked(2));
    }

    [Test]
    public void LevelManager_UnlockNextLevel_DoesNothingOnLastLevel()
    {
        // Unlock all levels up to the last one
        for (int i = 1; i < 5; i++)
        {
            levelManager.SetCurrentLevel(i);
            levelManager.UnlockNextLevel();
        }
        
        bool eventTriggered = false;
        levelManager.OnLevelUnlocked += (levelId) => eventTriggered = true;
        
        levelManager.SetCurrentLevel(5); // Set to last level
        levelManager.UnlockNextLevel(); // Try to unlock beyond last level
        
        Assert.IsFalse(eventTriggered);
    }

    [Test]
    public void LevelManager_SetCurrentLevel_UpdatesCurrentLevel()
    {
        levelManager.SetCurrentLevel(1);
        Assert.AreEqual(1, levelManager.GetCurrentLevel());
    }

    [Test]
    public void LevelManager_SetCurrentLevel_TriggersEvent()
    {
        bool eventTriggered = false;
        int newLevelId = -1;
        
        levelManager.OnCurrentLevelChanged += (levelId) => {
            eventTriggered = true;
            newLevelId = levelId;
        };
        
        levelManager.SetCurrentLevel(1);
        
        Assert.IsTrue(eventTriggered);
        Assert.AreEqual(1, newLevelId);
    }

    [Test]
    public void LevelManager_SetCurrentLevel_RejectsInvalidLevelId()
    {
        levelManager.SetCurrentLevel(0);
        Assert.AreEqual(-1, levelManager.GetCurrentLevel()); // Should remain unset
        
        levelManager.SetCurrentLevel(999);
        Assert.AreEqual(-1, levelManager.GetCurrentLevel()); // Should remain unset
    }

    [Test]
    public void LevelManager_SetCurrentLevel_RejectsLockedLevel()
    {
        levelManager.SetCurrentLevel(3); // Level 3 is locked by default
        Assert.AreEqual(-1, levelManager.GetCurrentLevel()); // Should remain unset
    }

    [Test]
    public void LevelManager_GetLevelData_ReturnsCorrectData()
    {
        LevelData levelData = levelManager.GetLevelData(1);
        
        Assert.IsNotNull(levelData);
        Assert.AreEqual(1, levelData.LevelId);
        Assert.AreEqual("Test Level 1", levelData.LevelName);
    }

    [Test]
    public void LevelManager_GetLevelData_ReturnsNullForInvalidId()
    {
        LevelData levelData = levelManager.GetLevelData(0);
        Assert.IsNull(levelData);
        
        levelData = levelManager.GetLevelData(999);
        Assert.IsNull(levelData);
    }

    [Test]
    public void LevelManager_GetCurrentLevelData_ReturnsCorrectData()
    {
        levelManager.SetCurrentLevel(1);
        LevelData currentLevelData = levelManager.GetCurrentLevelData();
        
        Assert.IsNotNull(currentLevelData);
        Assert.AreEqual(1, currentLevelData.LevelId);
    }

    [Test]
    public void LevelManager_GetCurrentLevelData_ReturnsNullIfNoLevelSet()
    {
        LevelData currentLevelData = levelManager.GetCurrentLevelData();
        Assert.IsNull(currentLevelData);
    }

    [Test]
    public void LevelManager_HasValidLevelCount()
    {
        Assert.Greater(levelManager.GetTotalLevelCount(), 0);
        Assert.LessOrEqual(levelManager.GetTotalLevelCount(), 1000); // Reasonable upper limit
    }

    [Test]
    public void LevelManager_HandlesSequentialUnlocking()
    {
        // Test sequential unlocking from level 1 to level 5
        for (int i = 1; i < 5; i++)
        {
            Assert.IsTrue(levelManager.IsLevelUnlocked(i));
            Assert.IsFalse(levelManager.IsLevelUnlocked(i + 1));
            
            levelManager.SetCurrentLevel(i);
            levelManager.UnlockNextLevel();
            
            Assert.IsTrue(levelManager.IsLevelUnlocked(i + 1));
        }
    }

    [Test]
    public void LevelManager_GetUnlockedLevels_ReturnsAllUnlockedLevels()
    {
        // Unlock first 3 levels
        levelManager.SetCurrentLevel(1);
        levelManager.UnlockNextLevel(); // Unlock level 2
        levelManager.SetCurrentLevel(2);
        levelManager.UnlockNextLevel(); // Unlock level 3
        
        List<int> unlockedLevels = levelManager.GetUnlockedLevels();
        Assert.AreEqual(3, unlockedLevels.Count);
        Assert.Contains(1, unlockedLevels);
        Assert.Contains(2, unlockedLevels);
        Assert.Contains(3, unlockedLevels);
        Assert.IsFalse(unlockedLevels.Contains(4));
        Assert.IsFalse(unlockedLevels.Contains(5));
    }

    [Test]
    public void LevelManager_ResetProgress_ResetsToLevel1Only()
    {
        // Unlock multiple levels first
        levelManager.SetCurrentLevel(1);
        levelManager.UnlockNextLevel();
        levelManager.SetCurrentLevel(2);
        levelManager.UnlockNextLevel();
        
        // Reset progress
        levelManager.ResetProgress();
        
        // Verify only level 1 is unlocked
        Assert.IsTrue(levelManager.IsLevelUnlocked(1));
        Assert.IsFalse(levelManager.IsLevelUnlocked(2));
        Assert.IsFalse(levelManager.IsLevelUnlocked(3));
        Assert.AreEqual(-1, levelManager.GetCurrentLevel());
    }

    [Test]
    public void LevelManager_GetMaxUnlockedLevel_ReturnsCorrectLevel()
    {
        Assert.AreEqual(1, levelManager.GetMaxUnlockedLevel());
        
        levelManager.SetCurrentLevel(1);
        levelManager.UnlockNextLevel();
        Assert.AreEqual(2, levelManager.GetMaxUnlockedLevel());
        
        levelManager.SetCurrentLevel(2);
        levelManager.UnlockNextLevel();
        Assert.AreEqual(3, levelManager.GetMaxUnlockedLevel());
    }

    // Helper method to set private fields via reflection
    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field {fieldName} not found");
        field.SetValue(target, value);
    }
}