using NUnit.Framework;
using UnityEngine;

public class UpgradeSystemTests
{
    private PlayerUpgradeProgress playerProgress;
    private UpgradeData testUpgradeData;
    
    [SetUp]
    public void SetUp()
    {
        playerProgress = new PlayerUpgradeProgress();
        
        // Create test upgrade data
        testUpgradeData = ScriptableObject.CreateInstance<UpgradeData>();
        testUpgradeData.upgradeType = UpgradeType.PaddleSpeed;
        testUpgradeData.upgradeName = "Test Paddle Speed";
        testUpgradeData.description = "Test upgrade";
        testUpgradeData.levels = new UpgradeLevel[]
        {
            new UpgradeLevel { level = 0, cost = 25, effectValue = 0.2f, description = "+20% speed" },
            new UpgradeLevel { level = 1, cost = 50, effectValue = 0.4f, description = "+40% speed" }
        };
    }
    
    [TearDown]
    public void TearDown()
    {
        if (testUpgradeData != null)
        {
            Object.DestroyImmediate(testUpgradeData);
        }
    }
    
    [Test]
    public void PlayerUpgradeProgress_InitializesWithZeroLevels()
    {
        Assert.AreEqual(0, playerProgress.GetUpgradeLevel(UpgradeType.PaddleSpeed));
        Assert.AreEqual(0, playerProgress.GetUpgradeLevel(UpgradeType.PaddleSize));
        Assert.AreEqual(0, playerProgress.GetUpgradeLevel(UpgradeType.BallSpeed));
        Assert.AreEqual(0, playerProgress.GetUpgradeLevel(UpgradeType.ScoreMultiplier));
    }
    
    [Test]
    public void PlayerUpgradeProgress_InitializesWithZeroPoints()
    {
        Assert.AreEqual(0, playerProgress.GetTotalPointsEarned());
        Assert.AreEqual(0, playerProgress.GetPointsSpent());
        Assert.AreEqual(0, playerProgress.GetAvailablePoints());
    }
    
    [Test]
    public void AddPoints_IncreasesTotalAndAvailablePoints()
    {
        playerProgress.AddPoints(100);
        
        Assert.AreEqual(100, playerProgress.GetTotalPointsEarned());
        Assert.AreEqual(100, playerProgress.GetAvailablePoints());
        Assert.AreEqual(0, playerProgress.GetPointsSpent());
    }
    
    [Test]
    public void CanPurchaseUpgrade_ReturnsFalseWhenInsufficientPoints()
    {
        playerProgress.AddPoints(10); // Less than required 25
        
        Assert.IsFalse(playerProgress.CanPurchaseUpgrade(testUpgradeData));
    }
    
    [Test]
    public void CanPurchaseUpgrade_ReturnsTrueWhenSufficientPoints()
    {
        playerProgress.AddPoints(50); // More than required 25
        
        Assert.IsTrue(playerProgress.CanPurchaseUpgrade(testUpgradeData));
    }
    
    [Test]
    public void PurchaseUpgrade_SucceedsWithSufficientPoints()
    {
        playerProgress.AddPoints(50);
        
        bool success = playerProgress.PurchaseUpgrade(testUpgradeData);
        
        Assert.IsTrue(success);
        Assert.AreEqual(1, playerProgress.GetUpgradeLevel(UpgradeType.PaddleSpeed));
        Assert.AreEqual(25, playerProgress.GetPointsSpent());
        Assert.AreEqual(25, playerProgress.GetAvailablePoints());
    }
    
    [Test]
    public void PurchaseUpgrade_FailsWithInsufficientPoints()
    {
        playerProgress.AddPoints(10); // Less than required 25
        
        bool success = playerProgress.PurchaseUpgrade(testUpgradeData);
        
        Assert.IsFalse(success);
        Assert.AreEqual(0, playerProgress.GetUpgradeLevel(UpgradeType.PaddleSpeed));
        Assert.AreEqual(0, playerProgress.GetPointsSpent());
    }
    
    [Test]
    public void PurchaseUpgrade_IncreasesLevelCorrectly()
    {
        playerProgress.AddPoints(100);
        
        // First purchase (level 0 -> 1)
        playerProgress.PurchaseUpgrade(testUpgradeData);
        Assert.AreEqual(1, playerProgress.GetUpgradeLevel(UpgradeType.PaddleSpeed));
        
        // Second purchase (level 1 -> 2)
        playerProgress.PurchaseUpgrade(testUpgradeData);
        Assert.AreEqual(2, playerProgress.GetUpgradeLevel(UpgradeType.PaddleSpeed));
    }
    
    [Test]
    public void PurchaseUpgrade_FailsAtMaxLevel()
    {
        playerProgress.AddPoints(1000);
        
        // Purchase both available levels
        playerProgress.PurchaseUpgrade(testUpgradeData);
        playerProgress.PurchaseUpgrade(testUpgradeData);
        
        // Try to purchase beyond max level
        bool success = playerProgress.PurchaseUpgrade(testUpgradeData);
        
        Assert.IsFalse(success);
        Assert.AreEqual(2, playerProgress.GetUpgradeLevel(UpgradeType.PaddleSpeed));
    }
    
    [Test]
    public void UpgradeData_GetMaxLevel_ReturnsCorrectValue()
    {
        Assert.AreEqual(2, testUpgradeData.GetMaxLevel());
    }
    
    [Test]
    public void UpgradeData_GetCostForLevel_ReturnsCorrectCost()
    {
        Assert.AreEqual(25, testUpgradeData.GetCostForLevel(0));
        Assert.AreEqual(50, testUpgradeData.GetCostForLevel(1));
    }
    
    [Test]
    public void UpgradeData_GetEffectValueForLevel_ReturnsCorrectEffect()
    {
        Assert.AreEqual(0.2f, testUpgradeData.GetEffectValueForLevel(0), 0.001f);
        Assert.AreEqual(0.4f, testUpgradeData.GetEffectValueForLevel(1), 0.001f);
    }
    
    [Test]
    public void UpgradeData_GetFormattedEffect_ReturnsCorrectFormat()
    {
        string effect0 = testUpgradeData.GetFormattedEffect(0);
        string effect1 = testUpgradeData.GetFormattedEffect(1);
        
        Assert.AreEqual("+20%", effect0);
        Assert.AreEqual("+40%", effect1);
    }
    
    [Test]
    public void ResetProgress_ClearsAllUpgradesAndPoints()
    {
        // Set up some progress
        playerProgress.AddPoints(100);
        playerProgress.PurchaseUpgrade(testUpgradeData);
        
        // Reset
        playerProgress.ResetProgress();
        
        // Verify reset
        Assert.AreEqual(0, playerProgress.GetUpgradeLevel(UpgradeType.PaddleSpeed));
        Assert.AreEqual(0, playerProgress.GetTotalPointsEarned());
        Assert.AreEqual(0, playerProgress.GetPointsSpent());
        Assert.AreEqual(0, playerProgress.GetAvailablePoints());
    }
    
    [Test]
    public void GetUpgradeEffectValue_ReturnsCorrectValueForLevel()
    {
        UpgradeData[] testUpgrades = { testUpgradeData };
        
        playerProgress.AddPoints(50);
        playerProgress.PurchaseUpgrade(testUpgradeData);
        
        float effectValue = playerProgress.GetUpgradeEffectValue(UpgradeType.PaddleSpeed, testUpgrades);
        
        Assert.AreEqual(0.2f, effectValue, 0.001f);
    }
}