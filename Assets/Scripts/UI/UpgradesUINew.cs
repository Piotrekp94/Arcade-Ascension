using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradesUINew : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject upgradesPanel;
    [SerializeField] private TextMeshProUGUI pointsDisplayText;
    [SerializeField] private Transform upgradeCardsContainer;
    [SerializeField] private GameObject upgradeCardPrefab;
    
    [Header("Upgrade System")]
    [SerializeField] private UpgradeEffectManager upgradeManager;
    
    private bool isInitialized = false;
    private CanvasGroup canvasGroup;
    private List<UpgradeCardUI> upgradeCards = new List<UpgradeCardUI>();
    private PlayerUpgradeProgress playerProgress;
    
    public event System.Action OnUpgradePurchased;
    
    void Start()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        if (isInitialized)
            return;
            
        // Setup CanvasGroup for visibility control
        SetupCanvasGroup();
        
        // Find or set upgrade manager
        if (upgradeManager == null)
        {
            upgradeManager = UpgradeEffectManager.Instance;
        }
        
        // Get player progress from manager
        if (upgradeManager != null)
        {
            playerProgress = upgradeManager.GetPlayerProgress();
        }
        
        // Setup upgrade cards
        SetupUpgradeCards();
        
        // Subscribe to events
        SubscribeToEvents();
        
        // Update initial display
        UpdatePointsDisplay();
        UpdateAllCards();
        
        isInitialized = true;
        Debug.Log("UpgradesUI: Initialized with upgrade system");
    }
    
    void SetupCanvasGroup()
    {
        // Try to get CanvasGroup from the panel first, then from this GameObject
        if (upgradesPanel != null)
        {
            canvasGroup = upgradesPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = upgradesPanel.AddComponent<CanvasGroup>();
            }
        }
        else
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
    
    void SetupUpgradeCards()
    {
        if (upgradeManager == null || upgradeCardsContainer == null || upgradeCardPrefab == null)
        {
            Debug.LogWarning("UpgradesUI: Missing references for upgrade card setup");
            return;
        }
        
        // Clear existing cards
        ClearUpgradeCards();
        
        // Create card for each available upgrade
        UpgradeData[] availableUpgrades = upgradeManager.GetAvailableUpgrades();
        if (availableUpgrades != null)
        {
            foreach (UpgradeData upgradeData in availableUpgrades)
            {
                CreateUpgradeCard(upgradeData);
            }
        }
    }
    
    void CreateUpgradeCard(UpgradeData upgradeData)
    {
        GameObject cardObject = Instantiate(upgradeCardPrefab, upgradeCardsContainer);
        UpgradeCardUI cardUI = cardObject.GetComponent<UpgradeCardUI>();
        
        if (cardUI != null)
        {
            cardUI.Initialize(upgradeData, playerProgress);
            cardUI.OnPurchaseRequested += OnUpgradeCardPurchaseRequested;
            upgradeCards.Add(cardUI);
        }
        else
        {
            Debug.LogError("UpgradesUI: Upgrade card prefab missing UpgradeCardUI component");
            Destroy(cardObject);
        }
    }
    
    void ClearUpgradeCards()
    {
        foreach (UpgradeCardUI card in upgradeCards)
        {
            if (card != null)
            {
                card.OnPurchaseRequested -= OnUpgradeCardPurchaseRequested;
                if (Application.isPlaying)
                    Destroy(card.gameObject);
                else
                    DestroyImmediate(card.gameObject);
            }
        }
        upgradeCards.Clear();
    }
    
    void SubscribeToEvents()
    {
        if (playerProgress != null)
        {
            playerProgress.OnPointsChanged += OnPointsChanged;
            playerProgress.OnUpgradePurchased += OnUpgradePurchasedInternal;
        }
    }
    
    void UnsubscribeFromEvents()
    {
        if (playerProgress != null)
        {
            playerProgress.OnPointsChanged -= OnPointsChanged;
            playerProgress.OnUpgradePurchased -= OnUpgradePurchasedInternal;
        }
    }
    
    void OnPointsChanged(int newPoints)
    {
        UpdatePointsDisplay();
        UpdateAllCards();
    }
    
    void OnUpgradePurchasedInternal(UpgradeType upgradeType, int newLevel)
    {
        UpdatePointsDisplay();
        UpdateAllCards();
        OnUpgradePurchased?.Invoke();
        
        Debug.Log($"UpgradesUI: Upgrade purchased - {upgradeType} to level {newLevel}");
    }
    
    void OnUpgradeCardPurchaseRequested(UpgradeData upgradeData)
    {
        if (playerProgress != null && playerProgress.PurchaseUpgrade(upgradeData))
        {
            Debug.Log($"UpgradesUI: Successfully purchased {upgradeData.upgradeName}");
        }
        else
        {
            Debug.LogWarning($"UpgradesUI: Failed to purchase {upgradeData.upgradeName}");
        }
    }
    
    void UpdatePointsDisplay()
    {
        if (pointsDisplayText != null && playerProgress != null)
        {
            int availablePoints = playerProgress.GetAvailablePoints();
            pointsDisplayText.text = $"Available Points: {availablePoints}";
        }
    }
    
    void UpdateAllCards()
    {
        foreach (UpgradeCardUI card in upgradeCards)
        {
            if (card != null)
            {
                card.UpdateDisplay();
            }
        }
    }
    
    // Public methods for showing/hiding
    public void ShowUpgrades()
    {
        Debug.Log("UpgradesUI: Showing upgrades panel");
        
        // Ensure GameObject is active
        if (upgradesPanel != null)
        {
            upgradesPanel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
        
        // Use CanvasGroup for visibility control
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        // Update displays when showing
        UpdatePointsDisplay();
        UpdateAllCards();
    }
    
    public void HideUpgrades()
    {
        Debug.Log("UpgradesUI: Hiding upgrades panel");
        
        // Use CanvasGroup to hide visually but keep GameObject active
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            // Fallback to old behavior if CanvasGroup isn't available
            if (upgradesPanel != null)
            {
                upgradesPanel.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
    
    public bool IsVisible()
    {
        // Check CanvasGroup visibility if available
        if (canvasGroup != null)
        {
            return canvasGroup.alpha > 0f && canvasGroup.interactable;
        }
        
        // Fallback to GameObject active state
        if (upgradesPanel != null)
        {
            return upgradesPanel.activeSelf;
        }
        return gameObject.activeSelf;
    }
    
    public CanvasGroup GetCanvasGroup()
    {
        return canvasGroup;
    }
    
    // Testing and setup methods
    public void SetupForTesting(GameObject panel, TextMeshProUGUI points, Transform container, GameObject cardPrefab, UpgradeEffectManager manager)
    {
        upgradesPanel = panel;
        pointsDisplayText = points;
        upgradeCardsContainer = container;
        upgradeCardPrefab = cardPrefab;
        upgradeManager = manager;
        
        if (upgradeManager != null)
        {
            playerProgress = upgradeManager.GetPlayerProgress();
        }
        
        SetupCanvasGroup();
    }
    
    public List<UpgradeCardUI> GetUpgradeCards()
    {
        return upgradeCards;
    }
    
    public PlayerUpgradeProgress GetPlayerProgress()
    {
        return playerProgress;
    }
    
    // Context menu methods for debugging
    [ContextMenu("Show Upgrades")]
    void DebugShowUpgrades()
    {
        ShowUpgrades();
    }
    
    [ContextMenu("Hide Upgrades")]
    void DebugHideUpgrades()
    {
        HideUpgrades();
    }
    
    [ContextMenu("Update Points Display")]
    void DebugUpdatePointsDisplay()
    {
        UpdatePointsDisplay();
    }
    
    [ContextMenu("Refresh All Cards")]
    void DebugRefreshAllCards()
    {
        UpdateAllCards();
    }
    
    [ContextMenu("Log Upgrade State")]
    void DebugLogUpgradeState()
    {
        if (playerProgress != null)
        {
            Debug.Log($"UpgradesUI State: {playerProgress.GetProgressSummary()}");
            Debug.Log($"Card Count: {upgradeCards.Count}");
            Debug.Log($"Is Visible: {IsVisible()}");
        }
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
        ClearUpgradeCards();
    }
}