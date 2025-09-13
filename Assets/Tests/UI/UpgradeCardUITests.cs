using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCardUITests
{
    private GameObject cardObject;
    private UpgradeCardUI cardUI;
    private UpgradeData testUpgradeData;
    private PlayerUpgradeProgress playerProgress;
    private Button testButton;
    private TextMeshProUGUI nameText, descriptionText, costText, effectText, currentLevelText, buttonText;
    
    [SetUp]
    public void SetUp()
    {
        // Create card GameObject with UI components
        cardObject = new GameObject("UpgradeCard");
        cardUI = cardObject.AddComponent<UpgradeCardUI>();
        
        // Create UI components
        testButton = cardObject.AddComponent<Button>();
        cardObject.AddComponent<Image>(); // Buttons need Image component
        
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(cardObject.transform);
        buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        
        GameObject nameTextObj = new GameObject("NameText");
        nameTextObj.transform.SetParent(cardObject.transform);
        nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
        
        GameObject descriptionTextObj = new GameObject("DescriptionText");
        descriptionTextObj.transform.SetParent(cardObject.transform);
        descriptionText = descriptionTextObj.AddComponent<TextMeshProUGUI>();
        
        GameObject costTextObj = new GameObject("CostText");
        costTextObj.transform.SetParent(cardObject.transform);
        costText = costTextObj.AddComponent<TextMeshProUGUI>();
        
        GameObject effectTextObj = new GameObject("EffectText");
        effectTextObj.transform.SetParent(cardObject.transform);
        effectText = effectTextObj.AddComponent<TextMeshProUGUI>();
        
        GameObject levelTextObj = new GameObject("LevelText");
        levelTextObj.transform.SetParent(cardObject.transform);
        currentLevelText = levelTextObj.AddComponent<TextMeshProUGUI>();
        
        // Assign references using reflection (simulating Inspector assignment)
        var fields = typeof(UpgradeCardUI).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        foreach (var field in fields)
        {
            switch (field.Name)
            {
                case "purchaseButton":
                    field.SetValue(cardUI, testButton);
                    break;
                case "nameText":
                    field.SetValue(cardUI, nameText);
                    break;
                case "descriptionText":
                    field.SetValue(cardUI, descriptionText);
                    break;
                case "costText":
                    field.SetValue(cardUI, costText);
                    break;
                case "effectText":
                    field.SetValue(cardUI, effectText);
                    break;
                case "currentLevelText":
                    field.SetValue(cardUI, currentLevelText);
                    break;
                case "buttonText":
                    field.SetValue(cardUI, buttonText);
                    break;
            }
        }
        
        // Create test upgrade data
        testUpgradeData = ScriptableObject.CreateInstance<UpgradeData>();
        testUpgradeData.upgradeType = UpgradeType.PaddleSpeed;
        testUpgradeData.upgradeName = "Test Paddle Speed";
        testUpgradeData.description = "Test upgrade description";
        testUpgradeData.levels = new UpgradeLevel[]
        {
            new UpgradeLevel { level = 0, cost = 25, effectValue = 0.2f, description = "+20% speed" },
            new UpgradeLevel { level = 1, cost = 50, effectValue = 0.4f, description = "+40% speed" }
        };
        
        // Create player progress
        playerProgress = new PlayerUpgradeProgress();
    }
    
    [TearDown]
    public void TearDown()
    {
        if (testUpgradeData != null)
            Object.DestroyImmediate(testUpgradeData);
            
        if (cardObject != null)
        {
            if (Application.isPlaying)
                Object.Destroy(cardObject);
            else
                Object.DestroyImmediate(cardObject);
        }
    }
    
    [Test]
    public void UpgradeCardUI_Initialize_SetsUpDataAndProgress()
    {
        cardUI.Initialize(testUpgradeData, playerProgress);
        
        Assert.AreEqual(testUpgradeData, cardUI.GetUpgradeData());
        Assert.AreEqual(0, cardUI.GetCurrentLevel());
        Assert.IsFalse(cardUI.IsMaxLevel());
    }
    
    [Test]
    public void UpgradeCardUI_UpdateDisplay_SetsNameAndDescription()
    {
        cardUI.Initialize(testUpgradeData, playerProgress);
        
        Assert.AreEqual("Test Paddle Speed", nameText.text);
        Assert.AreEqual("Test upgrade description", descriptionText.text);
    }
    
    [Test]
    public void UpgradeCardUI_UpdateDisplay_ShowsCorrectLevel()
    {
        cardUI.Initialize(testUpgradeData, playerProgress);
        
        Assert.IsTrue(currentLevelText.text.Contains("Level: 0/2"));
    }
    
    [Test]
    public void UpgradeCardUI_UpdateDisplay_ShowsCorrectCostWhenCannotAfford()
    {
        cardUI.Initialize(testUpgradeData, playerProgress);
        
        Assert.IsTrue(costText.text.Contains("Cost: 25 pts"));
        Assert.IsFalse(testButton.interactable);
        Assert.AreEqual("NOT ENOUGH POINTS", buttonText.text);
    }
    
    [Test]
    public void UpgradeCardUI_UpdateDisplay_ShowsCorrectCostWhenCanAfford()
    {
        playerProgress.AddPoints(50);
        cardUI.Initialize(testUpgradeData, playerProgress);
        
        Assert.IsTrue(costText.text.Contains("Cost: 25 pts"));
        Assert.IsTrue(testButton.interactable);
        Assert.AreEqual("UPGRADE", buttonText.text);
    }
    
    [Test]
    public void UpgradeCardUI_UpdateDisplay_ShowsMaxLevelCorrectly()
    {
        playerProgress.AddPoints(1000);
        
        // Purchase all levels
        playerProgress.PurchaseUpgrade(testUpgradeData);
        playerProgress.PurchaseUpgrade(testUpgradeData);
        
        cardUI.Initialize(testUpgradeData, playerProgress);
        
        Assert.IsTrue(cardUI.IsMaxLevel());
        Assert.IsFalse(testButton.interactable);
        Assert.AreEqual("MAX LEVEL", buttonText.text);
        Assert.AreEqual("MAXED OUT", costText.text);
    }
    
    [Test]
    public void UpgradeCardUI_RefreshDisplay_UpdatesUI()
    {
        cardUI.Initialize(testUpgradeData, playerProgress);
        
        // Initially no points
        Assert.IsFalse(testButton.interactable);
        
        // Add points and refresh
        playerProgress.AddPoints(50);
        cardUI.RefreshDisplay();
        
        Assert.IsTrue(testButton.interactable);
        Assert.AreEqual("UPGRADE", buttonText.text);
    }
    
    [Test]
    public void UpgradeCardUI_GetUpgradeData_ReturnsCorrectData()
    {
        cardUI.Initialize(testUpgradeData, playerProgress);
        
        Assert.AreEqual(testUpgradeData, cardUI.GetUpgradeData());
        Assert.AreEqual("Test Paddle Speed", cardUI.GetUpgradeData().upgradeName);
    }
    
    [Test]
    public void UpgradeCardUI_GetCurrentLevel_ReturnsCorrectLevel()
    {
        cardUI.Initialize(testUpgradeData, playerProgress);
        Assert.AreEqual(0, cardUI.GetCurrentLevel());
        
        playerProgress.AddPoints(50);
        playerProgress.PurchaseUpgrade(testUpgradeData);
        cardUI.RefreshDisplay();
        
        Assert.AreEqual(1, cardUI.GetCurrentLevel());
    }
    
    [Test]
    public void UpgradeCardUI_IsMaxLevel_ReturnsCorrectStatus()
    {
        cardUI.Initialize(testUpgradeData, playerProgress);
        Assert.IsFalse(cardUI.IsMaxLevel());
        
        playerProgress.AddPoints(1000);
        playerProgress.PurchaseUpgrade(testUpgradeData);
        playerProgress.PurchaseUpgrade(testUpgradeData);
        cardUI.RefreshDisplay();
        
        Assert.IsTrue(cardUI.IsMaxLevel());
    }
}