using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    
    // Method to set Instance for testing purposes
    public static void SetInstanceForTesting(LevelManager testInstance)
    {
        Instance = testInstance;
    }

    [Header("Level Configuration")]
    [SerializeField]
    private List<LevelData> levelDataList = new List<LevelData>();
    
    // Current game state
    private int currentLevel = -1; // -1 means no level selected
    private bool[] unlockedLevels; // Array to track which levels are unlocked
    
    // Events for level system changes
    public event Action<int> OnLevelUnlocked;
    public event Action<int> OnCurrentLevelChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        InitializeLevelSystem();
    }

    private void InitializeLevelSystem()
    {
        if (levelDataList.Count > 0)
        {
            // Initialize unlocked levels array
            unlockedLevels = new bool[levelDataList.Count];
            
            // Only level 1 (index 0) is unlocked by default
            unlockedLevels[0] = true;
            
            // All other levels are locked
            for (int i = 1; i < unlockedLevels.Length; i++)
            {
                unlockedLevels[i] = false;
            }
        }
    }

    // Testing method to initialize with custom level data
    public void InitializeForTesting(List<LevelData> testLevelData)
    {
        levelDataList = new List<LevelData>(testLevelData);
        InitializeLevelSystem();
    }

    // Core level management methods
    public int GetTotalLevelCount()
    {
        return levelDataList.Count;
    }

    public bool IsLevelUnlocked(int levelId)
    {
        if (!IsValidLevelId(levelId))
            return false;
            
        int index = levelId - 1; // Convert to zero-based index
        return unlockedLevels[index];
    }

    public List<int> GetUnlockedLevels()
    {
        List<int> unlockedLevelIds = new List<int>();
        
        for (int i = 0; i < unlockedLevels.Length; i++)
        {
            if (unlockedLevels[i])
            {
                unlockedLevelIds.Add(i + 1); // Convert back to 1-based level ID
            }
        }
        
        return unlockedLevelIds;
    }

    public void UnlockNextLevel()
    {
        if (currentLevel == -1)
            return; // No current level set
            
        int nextLevelId = currentLevel + 1;
        
        if (!IsValidLevelId(nextLevelId))
            return; // Next level doesn't exist
            
        int nextIndex = nextLevelId - 1; // Convert to zero-based index
        
        if (unlockedLevels[nextIndex])
            return; // Already unlocked
            
        // Unlock the next level
        unlockedLevels[nextIndex] = true;
        
        // Trigger event
        OnLevelUnlocked?.Invoke(nextLevelId);
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void SetCurrentLevel(int levelId)
    {
        if (!IsValidLevelId(levelId))
            return; // Invalid level ID
            
        if (!IsLevelUnlocked(levelId))
            return; // Level is locked
            
        currentLevel = levelId;
        OnCurrentLevelChanged?.Invoke(currentLevel);
    }

    public LevelData GetLevelData(int levelId)
    {
        if (!IsValidLevelId(levelId))
            return null;
            
        int index = levelId - 1; // Convert to zero-based index
        return levelDataList[index];
    }

    public LevelData GetCurrentLevelData()
    {
        if (currentLevel == -1)
            return null;
            
        return GetLevelData(currentLevel);
    }

    public int GetMaxUnlockedLevel()
    {
        for (int i = unlockedLevels.Length - 1; i >= 0; i--)
        {
            if (unlockedLevels[i])
            {
                return i + 1; // Convert back to 1-based level ID
            }
        }
        
        return 1; // At minimum, level 1 should always be unlocked
    }

    public void ResetProgress()
    {
        // Reset current level
        currentLevel = -1;
        
        // Reset unlocked levels - only level 1 remains unlocked
        if (unlockedLevels != null)
        {
            for (int i = 0; i < unlockedLevels.Length; i++)
            {
                unlockedLevels[i] = (i == 0); // Only first level (index 0) is unlocked
            }
        }
    }

    // Helper methods
    private bool IsValidLevelId(int levelId)
    {
        return levelId >= 1 && levelId <= levelDataList.Count;
    }

    // Debug and testing methods
    [ContextMenu("Log Level Manager Status")]
    public void LogLevelManagerStatus()
    {
        Debug.Log($"LevelManager Status:" +
                  $"\n- Total Levels: {GetTotalLevelCount()}" +
                  $"\n- Current Level: {currentLevel}" +
                  $"\n- Unlocked Levels: [{string.Join(", ", GetUnlockedLevels())}]" +
                  $"\n- Max Unlocked Level: {GetMaxUnlockedLevel()}");
                  
        for (int i = 1; i <= GetTotalLevelCount(); i++)
        {
            LevelData levelData = GetLevelData(i);
            if (levelData != null)
            {
                Debug.Log($"Level {i}: {levelData.LevelName} - " +
                         $"{levelData.BlockRows}x{levelData.BlockColumns} blocks, " +
                         $"Score Multiplier: {levelData.ScoreMultiplier}");
            }
        }
    }

    [ContextMenu("Unlock All Levels")]
    public void DebugUnlockAllLevels()
    {
        if (unlockedLevels != null)
        {
            for (int i = 0; i < unlockedLevels.Length; i++)
            {
                unlockedLevels[i] = true;
            }
            Debug.Log("All levels unlocked (DEBUG MODE)");
        }
    }

    [ContextMenu("Reset All Progress")]
    public void DebugResetProgress()
    {
        ResetProgress();
        Debug.Log("Level progress reset");
    }
}