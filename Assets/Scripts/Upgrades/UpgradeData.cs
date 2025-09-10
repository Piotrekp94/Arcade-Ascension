using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Arcade Ascension/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Basic Info")]
    public UpgradeType upgradeType;
    public string upgradeName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    
    [Header("Upgrade Levels")]
    public UpgradeLevel[] levels;
    
    [Header("Display Settings")]
    public string effectUnit = "%";
    public bool showAsMultiplier = true;
    
    public int GetMaxLevel()
    {
        return levels != null ? levels.Length : 0;
    }
    
    public UpgradeLevel GetLevelData(int level)
    {
        if (levels == null || level < 0 || level >= levels.Length)
        {
            return new UpgradeLevel { level = -1, cost = 0, effectValue = 0, description = "Invalid level" };
        }
        
        return levels[level];
    }
    
    public int GetCostForLevel(int level)
    {
        var levelData = GetLevelData(level);
        return levelData.cost;
    }
    
    public float GetEffectValueForLevel(int level)
    {
        var levelData = GetLevelData(level);
        return levelData.effectValue;
    }
    
    public string GetFormattedEffect(int level)
    {
        float effect = GetEffectValueForLevel(level);
        
        if (showAsMultiplier)
        {
            return $"+{effect * 100:F0}{effectUnit}";
        }
        else
        {
            return $"{effect:F1}{effectUnit}";
        }
    }
    
    public bool IsValidLevel(int level)
    {
        return level >= 0 && level < GetMaxLevel();
    }
}