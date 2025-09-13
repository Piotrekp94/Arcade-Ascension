using NUnit.Framework;
using UnityEngine;

public class UpgradeEffectManagerTests
{
    private GameObject upgradeManagerObject;
    private UpgradeEffectManager upgradeManager;
    private UpgradeData[] testUpgrades;
    
    [SetUp]
    public void SetUp()
    {
        // Create UpgradeEffectManager GameObject
        upgradeManagerObject = new GameObject("UpgradeEffectManager");
        upgradeManager = upgradeManagerObject.AddComponent<UpgradeEffectManager>();
        
        // Create test upgrade data
        UpgradeData paddleSpeedUpgrade = ScriptableObject.CreateInstance<UpgradeData>();
        paddleSpeedUpgrade.upgradeType = UpgradeType.PaddleSpeed;
        paddleSpeedUpgrade.upgradeName = "Test Paddle Speed";
        paddleSpeedUpgrade.levels = new UpgradeLevel[]
        {
            new UpgradeLevel { level = 0, cost = 25, effectValue = 0.2f, description = "+20% speed" }
        };
        
        UpgradeData scoreMultiplierUpgrade = ScriptableObject.CreateInstance<UpgradeData>();
        scoreMultiplierUpgrade.upgradeType = UpgradeType.ScoreMultiplier;
        scoreMultiplierUpgrade.upgradeName = "Test Score Multiplier";
        scoreMultiplierUpgrade.levels = new UpgradeLevel[]
        {
            new UpgradeLevel { level = 0, cost = 50, effectValue = 0.25f, description = "+25% score" }
        };
        
        testUpgrades = new UpgradeData[] { paddleSpeedUpgrade, scoreMultiplierUpgrade };
        
        // Set up the manager
        upgradeManager.SetAvailableUpgrades(testUpgrades);
        upgradeManager.Initialize();
    }
    
    [TearDown]
    public void TearDown()
    {
        foreach (var upgrade in testUpgrades)
        {
            if (upgrade != null)
                Object.DestroyImmediate(upgrade);
        }
        
        if (upgradeManagerObject != null)
        {
            if (Application.isPlaying)
                Object.Destroy(upgradeManagerObject);
            else
                Object.DestroyImmediate(upgradeManagerObject);
        }
    }
    
    [Test]
    public void UpgradeEffectManager_Initialize_SetsUpPlayerProgress()
    {
        Assert.IsNotNull(upgradeManager.GetPlayerProgress());
        Assert.AreEqual(0, upgradeManager.GetPlayerProgress().GetAvailablePoints());
    }
    
    [Test]
    public void UpgradeEffectManager_GetAvailableUpgrades_ReturnsCorrectUpgrades()
    {
        UpgradeData[] availableUpgrades = upgradeManager.GetAvailableUpgrades();
        
        Assert.IsNotNull(availableUpgrades);
        Assert.AreEqual(2, availableUpgrades.Length);
        Assert.AreEqual(UpgradeType.PaddleSpeed, availableUpgrades[0].upgradeType);
        Assert.AreEqual(UpgradeType.ScoreMultiplier, availableUpgrades[1].upgradeType);
    }
    
    [Test]
    public void UpgradeEffectManager_GetUpgradeData_ReturnsCorrectUpgrade()
    {
        UpgradeData paddleSpeedData = upgradeManager.GetUpgradeData(UpgradeType.PaddleSpeed);
        UpgradeData scoreData = upgradeManager.GetUpgradeData(UpgradeType.ScoreMultiplier);
        
        Assert.IsNotNull(paddleSpeedData);
        Assert.AreEqual("Test Paddle Speed", paddleSpeedData.upgradeName);
        
        Assert.IsNotNull(scoreData);
        Assert.AreEqual("Test Score Multiplier", scoreData.upgradeName);
    }
    
    [Test]
    public void UpgradeEffectManager_GetUpgradeData_ReturnsNullForInvalidType()
    {
        UpgradeData ballSpeedData = upgradeManager.GetUpgradeData(UpgradeType.BallSpeed);
        
        Assert.IsNull(ballSpeedData);
    }
    
    [Test]
    public void UpgradeEffectManager_PlayerProgress_HandlesPointsCorrectly()
    {
        PlayerUpgradeProgress progress = upgradeManager.GetPlayerProgress();
        
        // Add points
        progress.AddPoints(100);
        Assert.AreEqual(100, progress.GetAvailablePoints());
        
        // Purchase upgrade
        UpgradeData paddleUpgrade = upgradeManager.GetUpgradeData(UpgradeType.PaddleSpeed);
        bool success = progress.PurchaseUpgrade(paddleUpgrade);
        
        Assert.IsTrue(success);
        Assert.AreEqual(75, progress.GetAvailablePoints());
        Assert.AreEqual(1, progress.GetUpgradeLevel(UpgradeType.PaddleSpeed));
    }
    
    [Test]
    public void UpgradeEffectManager_ApplyAllUpgradeEffects_DoesNotThrow()
    {
        PlayerUpgradeProgress progress = upgradeManager.GetPlayerProgress();
        progress.AddPoints(100);
        
        // Purchase some upgrades
        progress.PurchaseUpgrade(upgradeManager.GetUpgradeData(UpgradeType.PaddleSpeed));
        progress.PurchaseUpgrade(upgradeManager.GetUpgradeData(UpgradeType.ScoreMultiplier));
        
        // This should not throw even without actual game objects
        Assert.DoesNotThrow(() => upgradeManager.ApplyAllUpgradeEffects());
    }
    
    [Test]
    public void UpgradeEffectManager_SetAvailableUpgrades_UpdatesUpgradeList()
    {
        UpgradeData newUpgrade = ScriptableObject.CreateInstance<UpgradeData>();
        newUpgrade.upgradeType = UpgradeType.PaddleSize;
        newUpgrade.upgradeName = "Test Paddle Size";
        
        UpgradeData[] newUpgrades = new UpgradeData[] { newUpgrade };
        upgradeManager.SetAvailableUpgrades(newUpgrades);
        
        UpgradeData[] availableUpgrades = upgradeManager.GetAvailableUpgrades();
        Assert.AreEqual(1, availableUpgrades.Length);
        Assert.AreEqual(UpgradeType.PaddleSize, availableUpgrades[0].upgradeType);
        
        Object.DestroyImmediate(newUpgrade);
    }
}