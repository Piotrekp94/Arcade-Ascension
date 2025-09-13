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

    // For testing - store GameObject reference to avoid Transform issues
    private GameObject upgradeCardsContainerObject;
    
    [Header("Upgrade System")]
    [SerializeField] private UpgradeEffectManager upgradeManager;
    
    private bool isInitialized = false;
    private bool isTestMode = false;
    private CanvasGroup canvasGroup;
    private List<UpgradeCardUI> upgradeCards = new List<UpgradeCardUI>();
    private PlayerUpgradeProgress playerProgress;
    
    public event System.Action OnUpgradePurchased;
    
    void Start()
    {
        if (!isTestMode)
        {
            Initialize();
        }
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
            Debug.LogWarning($"UpgradesUI: upgradeManager was null, trying to get Instance: {(upgradeManager == null ? "FAILED" : "SUCCESS")}");
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
        string containerName = "NULL";
        try
        {
            if (upgradeCardsContainer != null && upgradeCardsContainer.gameObject != null)
            {
                containerName = upgradeCardsContainer.name;
            }
        }
        catch (System.Exception)
        {
            containerName = "DESTROYED";
        }
        Debug.Log($"SetupUpgradeCards called: upgradeCardsContainer={upgradeCardsContainer}, upgradeCardsContainer.name={containerName}");

        // If Transform reference is null but GameObject reference exists, get the transform
        if (upgradeCardsContainer == null && upgradeCardsContainerObject != null)
        {
            upgradeCardsContainer = upgradeCardsContainerObject.transform;
            Debug.Log($"Restored upgradeCardsContainer from GameObject reference: {upgradeCardsContainer.name}");
        }

        if (upgradeManager == null || upgradeCardsContainer == null || upgradeCardPrefab == null)
        {
            Debug.LogWarning($"UpgradesUI: Missing references for upgrade card setup. upgradeManager: {(upgradeManager == null ? "NULL" : "OK")}, upgradeCardsContainer: {(upgradeCardsContainer == null ? "NULL" : "OK")}, upgradeCardPrefab: {(upgradeCardPrefab == null ? "NULL" : "OK")}");
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
                if (upgradeData != null)
                {
                    CreateUpgradeCard(upgradeData);
                }
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
        isTestMode = true;
        upgradesPanel = panel;
        pointsDisplayText = points;
        upgradeCardsContainer = container;
        // Store GameObject reference for stability - check if Transform is valid first
        if (container != null && container.gameObject != null)
        {
            upgradeCardsContainerObject = container.gameObject;
        }
        else
        {
            upgradeCardsContainerObject = null;
        }
        upgradeCardPrefab = cardPrefab;
        upgradeManager = manager;

        string containerInfo = "NULL";
        try
        {
            if (container != null && container.gameObject != null)
            {
                containerInfo = container.name;
            }
        }
        catch (System.Exception)
        {
            containerInfo = "DESTROYED";
        }
        Debug.Log($"SetupForTesting: container={container}, container.name={containerInfo}");

        if (upgradeManager != null)
        {
            playerProgress = upgradeManager.GetPlayerProgress();
        }

        SetupCanvasGroup();

        // Reset initialization flag to allow re-initialization
        isInitialized = false;

        // Verify the references were set correctly
        string finalContainerInfo = "NULL";
        try
        {
            if (upgradeCardsContainer != null && upgradeCardsContainer.gameObject != null)
            {
                finalContainerInfo = upgradeCardsContainer.name;
            }
        }
        catch (System.Exception)
        {
            finalContainerInfo = "DESTROYED";
        }
        Debug.Log($"SetupForTesting complete: upgradeCardsContainer={upgradeCardsContainer}, upgradeCardsContainer.name={finalContainerInfo}");
    }

    public void ClearTestReferences()
    {
        isTestMode = false;
        upgradesPanel = null;
        pointsDisplayText = null;
        upgradeCardsContainer = null;
        upgradeCardsContainerObject = null;
        upgradeCardPrefab = null;
        upgradeManager = null;
        playerProgress = null;
        isInitialized = false;
    }
    
    public List<UpgradeCardUI> GetUpgradeCards()
    {
        return upgradeCards;
    }
    
    public PlayerUpgradeProgress GetPlayerProgress()
    {
        return playerProgress;
    }
    
    // Testing helper to force re-initialization
    public void ForceReInitialize()
    {
        isInitialized = false;
        Initialize();
    }
    
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
        ClearUpgradeCards();
    }
}