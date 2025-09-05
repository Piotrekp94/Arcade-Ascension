using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradesUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject upgradesPanel;
    [SerializeField] private TextMeshProUGUI placeholderText;
    [SerializeField] private TextMeshProUGUI pointsDisplayText;
    
    [Header("Placeholder Content")]
    [SerializeField] private Button comingSoonButton;
    
    // Events for future implementation
    public event System.Action OnUpgradePurchased;
    
    private bool isInitialized = false;
    private CanvasGroup canvasGroup;

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
        
        // Setup placeholder content
        SetupPlaceholderContent();
        
        // Subscribe to game events for points display
        SubscribeToEvents();
        
        isInitialized = true;
        Debug.Log("UpgradesUI: Initialized with placeholder content");
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

    void SetupPlaceholderContent()
    {
        // Setup placeholder text
        if (placeholderText != null)
        {
            placeholderText.text = "UPGRADES\n\nComing Soon!\n\nThis section will allow you to\npurchase upgrades using points\nearned from completing levels.";
            placeholderText.alignment = TextAlignmentOptions.Center;
        }
        
        // Setup points display
        UpdatePointsDisplay();
        
        // Setup coming soon button
        if (comingSoonButton != null)
        {
            comingSoonButton.onClick.AddListener(OnComingSoonButtonClicked);
            
            // Set button text if it has a text component
            TextMeshProUGUI buttonText = comingSoonButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Coming Soon";
            }
            
            // Make button non-interactive for now
            comingSoonButton.interactable = false;
        }
    }

    void SubscribeToEvents()
    {
        // Subscribe to GameManager score changes for points display
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += OnScoreChanged;
        }
    }

    void UnsubscribeFromEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= OnScoreChanged;
        }
    }

    void OnScoreChanged(int newScore)
    {
        UpdatePointsDisplay();
    }

    void UpdatePointsDisplay()
    {
        if (pointsDisplayText != null)
        {
            int currentScore = GameManager.Instance?.GetScore() ?? 0;
            pointsDisplayText.text = $"Available Points: {currentScore}";
        }
    }

    void OnComingSoonButtonClicked()
    {
        Debug.Log("UpgradesUI: Coming Soon button clicked - upgrade system not yet implemented");
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
        
        // Update points display when showing
        UpdatePointsDisplay();
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

    // Methods for testing
    public void SetupForTesting(GameObject panel, TextMeshProUGUI placeholder, TextMeshProUGUI points)
    {
        upgradesPanel = panel;
        placeholderText = placeholder;
        pointsDisplayText = points;
        SetupCanvasGroup();
    }

    public void UpdatePlaceholderText(string newText)
    {
        if (placeholderText != null)
        {
            placeholderText.text = newText;
        }
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

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
}