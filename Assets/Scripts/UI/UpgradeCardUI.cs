using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    
    [Header("Visual States")]
    [SerializeField] private Color affordableColor = Color.white;
    [SerializeField] private Color unaffordableColor = Color.gray;
    [SerializeField] private Color maxLevelColor = Color.green;
    
    private UpgradeData upgradeData;
    private PlayerUpgradeProgress playerProgress;
    private int currentLevel;
    private bool isMaxLevel;
    
    public event Action<UpgradeData> OnPurchaseRequested;
    
    void Start()
    {
        SetupButtonListener();
    }
    
    void SetupButtonListener()
    {
        if (purchaseButton != null)
        {
            // Clear any existing listeners first
            purchaseButton.onClick.RemoveAllListeners();
            
            // Add our listener
            purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
        }
        else
        {
            Debug.LogError("UpgradeCardUI: purchaseButton is null in SetupButtonListener!");
        }
    }
    
    public void Initialize(UpgradeData data, PlayerUpgradeProgress progress)
    {
        upgradeData = data;
        playerProgress = progress;
        
        // Ensure button listener is set up
        SetupButtonListener();
        
        if (upgradeData != null)
        {
            UpdateDisplay();
        }
    }
    
    public void UpdateDisplay()
    {
        if (upgradeData == null)
        {
            Debug.LogError("UpgradeCardUI: upgradeData is null!");
            return;
        }
        
        if (playerProgress == null)
        {
            Debug.LogError("UpgradeCardUI: playerProgress is null!");
            return;
        }
        
        currentLevel = playerProgress.GetUpgradeLevel(upgradeData.upgradeType);
        isMaxLevel = currentLevel >= upgradeData.GetMaxLevel();
        
        UpdateBasicInfo();
        UpdateLevelInfo();
        UpdateCostAndEffect();
        UpdatePurchaseButton();
        UpdateVisualState();
    }
    
    private void UpdateBasicInfo()
    {
        // Set icon
        if (iconImage != null && upgradeData.icon != null)
        {
            iconImage.sprite = upgradeData.icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }
        
        // Set name
        if (nameText != null)
        {
            nameText.text = upgradeData.upgradeName;
        }
        
        // Set description
        if (descriptionText != null)
        {
            string desc = string.IsNullOrEmpty(upgradeData.description) ? upgradeData.upgradeName + " upgrade" : upgradeData.description;
            descriptionText.text = desc;
        }
    }
    
    private void UpdateLevelInfo()
    {
        if (currentLevelText != null)
        {
            if (isMaxLevel)
            {
                currentLevelText.text = $"Level: {currentLevel}/{upgradeData.GetMaxLevel()} (MAX)";
            }
            else
            {
                currentLevelText.text = $"Level: {currentLevel}/{upgradeData.GetMaxLevel()}";
            }
        }
    }
    
    private void UpdateCostAndEffect()
    {
        if (isMaxLevel)
        {
            // Max level - show current effect
            if (costText != null)
            {
                costText.text = "MAXED OUT";
            }
            
            if (effectText != null)
            {
                string currentEffect = upgradeData.GetFormattedEffect(currentLevel - 1);
                effectText.text = $"Current: {currentEffect}";
            }
        }
        else
        {
            // Show next level cost and effect
            int nextLevelIndex = currentLevel;
            
            if (nextLevelIndex >= upgradeData.GetMaxLevel())
            {
                return;
            }
            
            int cost = upgradeData.GetCostForLevel(nextLevelIndex);
            
            if (costText != null)
            {
                costText.text = $"Cost: {cost} pts";
            }
            
            if (effectText != null)
            {
                string nextEffect = upgradeData.GetFormattedEffect(nextLevelIndex);
                string currentEffect = currentLevel > 0 ? upgradeData.GetFormattedEffect(currentLevel - 1) : "0%";
                effectText.text = $"Current: {currentEffect} → Next: {nextEffect}";
            }
        }
    }
    
    private void UpdatePurchaseButton()
    {
        if (purchaseButton == null) return;
        
        bool canAfford = playerProgress.CanPurchaseUpgrade(upgradeData);
        
        if (isMaxLevel)
        {
            purchaseButton.interactable = false;
            if (buttonText != null)
            {
                buttonText.text = "MAX LEVEL";
            }
        }
        else if (canAfford)
        {
            purchaseButton.interactable = true;
            if (buttonText != null)
            {
                buttonText.text = "UPGRADE";
            }
        }
        else
        {
            purchaseButton.interactable = false;
            if (buttonText != null)
            {
                buttonText.text = "NOT ENOUGH POINTS";
            }
        }
    }
    
    private void UpdateVisualState()
    {
        Color targetColor;
        
        if (isMaxLevel)
        {
            targetColor = maxLevelColor;
        }
        else if (playerProgress.CanPurchaseUpgrade(upgradeData))
        {
            targetColor = affordableColor;
        }
        else
        {
            targetColor = unaffordableColor;
        }
        
        // Apply color to main UI elements
        if (nameText != null)
        {
            nameText.color = targetColor;
        }
        
        // Apply subtle color tint to the card background if available
        Image backgroundImage = GetComponent<Image>();
        if (backgroundImage != null)
        {
            Color backgroundColor = targetColor;
            backgroundColor.a = 0.1f; // Subtle tint
            backgroundImage.color = backgroundColor;
        }
    }
    
    private void OnPurchaseButtonClicked()
    {
        if (upgradeData != null && playerProgress != null && playerProgress.CanPurchaseUpgrade(upgradeData))
        {
            OnPurchaseRequested?.Invoke(upgradeData);
        }
    }
    
    // Public methods for external updates
    public void RefreshDisplay()
    {
        UpdateDisplay();
    }
    
    public UpgradeData GetUpgradeData()
    {
        return upgradeData;
    }
    
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    
    public bool IsMaxLevel()
    {
        return isMaxLevel;
    }
    
    
    void OnDestroy()
    {
        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveListener(OnPurchaseButtonClicked);
        }
    }
}