using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerUpgradeProgress
{
    [SerializeField]
    private Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();
    
    [SerializeField]
    private int totalPointsEarned = 0;
    
    [SerializeField]
    private int pointsSpent = 0;
    
    public event Action<UpgradeType, int> OnUpgradePurchased;
    public event Action<int> OnPointsChanged;
    
    public PlayerUpgradeProgress()
    {
        InitializeUpgrades();
    }
    
    private void InitializeUpgrades()
    {
        foreach (UpgradeType upgradeType in Enum.GetValues(typeof(UpgradeType)))
        {
            if (!upgradeLevels.ContainsKey(upgradeType))
            {
                upgradeLevels[upgradeType] = 0;
            }
        }
    }
    
    public int GetUpgradeLevel(UpgradeType upgradeType)
    {
        if (!upgradeLevels.ContainsKey(upgradeType))
        {
            upgradeLevels[upgradeType] = 0;
            return 0;
        }
        
        return upgradeLevels[upgradeType];
    }
    
    public bool CanPurchaseUpgrade(UpgradeData upgradeData)
    {
        if (upgradeData == null) return false;
        
        int currentLevel = GetUpgradeLevel(upgradeData.upgradeType);
        
        if (currentLevel >= upgradeData.GetMaxLevel()) return false;
        
        int cost = upgradeData.GetCostForLevel(currentLevel);
        return GetAvailablePoints() >= cost;
    }
    
    public bool PurchaseUpgrade(UpgradeData upgradeData)
    {
        if (!CanPurchaseUpgrade(upgradeData)) return false;
        
        int currentLevel = GetUpgradeLevel(upgradeData.upgradeType);
        int cost = upgradeData.GetCostForLevel(currentLevel);
        
        pointsSpent += cost;
        upgradeLevels[upgradeData.upgradeType] = currentLevel + 1;
        
        OnUpgradePurchased?.Invoke(upgradeData.upgradeType, currentLevel + 1);
        OnPointsChanged?.Invoke(GetAvailablePoints());
        
        Debug.Log($"Purchased {upgradeData.upgradeName} level {currentLevel + 1} for {cost} points");
        
        return true;
    }
    
    public void AddPoints(int points)
    {
        totalPointsEarned += points;
        OnPointsChanged?.Invoke(GetAvailablePoints());
    }
    
    public int GetTotalPointsEarned()
    {
        return totalPointsEarned;
    }
    
    public int GetPointsSpent()
    {
        return pointsSpent;
    }
    
    public int GetAvailablePoints()
    {
        return totalPointsEarned - pointsSpent;
    }
    
    public float GetUpgradeEffectValue(UpgradeType upgradeType, UpgradeData[] allUpgrades)
    {
        int level = GetUpgradeLevel(upgradeType);
        
        if (level == 0) return 0f;
        
        foreach (var upgradeData in allUpgrades)
        {
            if (upgradeData.upgradeType == upgradeType)
            {
                return upgradeData.GetEffectValueForLevel(level - 1);
            }
        }
        
        return 0f;
    }
    
    public Dictionary<UpgradeType, int> GetAllUpgradeLevels()
    {
        return new Dictionary<UpgradeType, int>(upgradeLevels);
    }
    
    public void ResetProgress()
    {
        upgradeLevels.Clear();
        InitializeUpgrades();
        totalPointsEarned = 0;
        pointsSpent = 0;
        OnPointsChanged?.Invoke(0);
    }
    
    public string GetProgressSummary()
    {
        return $"Total Points: {totalPointsEarned}, Spent: {pointsSpent}, Available: {GetAvailablePoints()}";
    }
}