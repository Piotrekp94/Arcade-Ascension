using UnityEngine;
using System;

public class UpgradeEffectManager : MonoBehaviour
{
    public static UpgradeEffectManager Instance { get; private set; }
    
    [Header("Upgrade Data")]
    [SerializeField] private UpgradeData[] availableUpgrades;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    
    private PlayerUpgradeProgress playerProgress;
    
    public event Action<UpgradeType, int> OnUpgradeEffectApplied;
    
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
            DontDestroyOnLoad(gameObject);
        }
    }
    
    void Start()
    {
        Initialize();
    }
    
    public void Initialize()
    {
        if (playerProgress == null)
        {
            playerProgress = new PlayerUpgradeProgress();
        }
        
        playerProgress.OnUpgradePurchased += OnUpgradePurchased;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreAdded += OnScoreAdded;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log("UpgradeEffectManager: Initialized");
        }
    }
    
    private void OnScoreAdded(int amountAdded, int newTotal)
    {
        playerProgress.AddPoints(amountAdded);
        
        if (enableDebugLogging)
        {
            Debug.Log($"UpgradeEffectManager: Added {amountAdded} points, total available: {playerProgress.GetAvailablePoints()}");
        }
    }
    
    private void OnUpgradePurchased(UpgradeType upgradeType, int newLevel)
    {
        ApplyUpgradeEffect(upgradeType, newLevel);
        OnUpgradeEffectApplied?.Invoke(upgradeType, newLevel);
    }
    
    private void ApplyUpgradeEffect(UpgradeType upgradeType, int level)
    {
        switch (upgradeType)
        {
            case UpgradeType.PaddleSpeed:
                ApplyPaddleSpeedUpgrade(level);
                break;
                
            case UpgradeType.PaddleSize:
                ApplyPaddleSizeUpgrade(level);
                break;
                
            case UpgradeType.BallSpeed:
                ApplyBallSpeedUpgrade(level);
                break;
                
            case UpgradeType.ScoreMultiplier:
                ApplyScoreMultiplierUpgrade(level);
                break;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"UpgradeEffectManager: Applied {upgradeType} upgrade to level {level}");
        }
    }
    
    private void ApplyPaddleSpeedUpgrade(int level)
    {
        float effectValue = playerProgress.GetUpgradeEffectValue(UpgradeType.PaddleSpeed, availableUpgrades);
        
        PlayerPaddle paddle = FindFirstObjectByType<PlayerPaddle>();
        if (paddle != null)
        {
            float baseSpeed = 5f;
            float newSpeed = baseSpeed * (1f + effectValue);
            paddle.SetSpeed(newSpeed);
            
            if (enableDebugLogging)
            {
                Debug.Log($"UpgradeEffectManager: Paddle speed set to {newSpeed} (base: {baseSpeed}, multiplier: {effectValue})");
            }
        }
    }
    
    private void ApplyPaddleSizeUpgrade(int level)
    {
        float effectValue = playerProgress.GetUpgradeEffectValue(UpgradeType.PaddleSize, availableUpgrades);
        
        PlayerPaddle paddle = FindFirstObjectByType<PlayerPaddle>();
        if (paddle != null)
        {
            Vector3 baseScale = Vector3.one;
            Vector3 newScale = new Vector3(baseScale.x * (1f + effectValue), baseScale.y, baseScale.z);
            paddle.transform.localScale = newScale;
            paddle.CalculatePaddleWidth();
            
            if (enableDebugLogging)
            {
                Debug.Log($"UpgradeEffectManager: Paddle size set to {newScale.x} (base: {baseScale.x}, multiplier: {effectValue})");
            }
        }
    }
    
    private void ApplyBallSpeedUpgrade(int level)
    {
        float effectValue = playerProgress.GetUpgradeEffectValue(UpgradeType.BallSpeed, availableUpgrades);
        
        Ball[] balls = FindObjectsByType<Ball>(FindObjectsSortMode.None);
        foreach (Ball ball in balls)
        {
            ball.ApplySpeedMultiplier(1f + effectValue);
        }
        
        if (enableDebugLogging)
        {
            Debug.Log($"UpgradeEffectManager: Ball speed multiplier set to {1f + effectValue}");
        }
    }
    
    private void ApplyScoreMultiplierUpgrade(int level)
    {
        float effectValue = playerProgress.GetUpgradeEffectValue(UpgradeType.ScoreMultiplier, availableUpgrades);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetScoreMultiplier(1f + effectValue);
            
            if (enableDebugLogging)
            {
                Debug.Log($"UpgradeEffectManager: Score multiplier set to {1f + effectValue}");
            }
        }
    }
    
    public void ApplyAllUpgradeEffects()
    {
        if (playerProgress == null) return;
        
        foreach (UpgradeType upgradeType in Enum.GetValues(typeof(UpgradeType)))
        {
            int level = playerProgress.GetUpgradeLevel(upgradeType);
            if (level > 0)
            {
                ApplyUpgradeEffect(upgradeType, level);
            }
        }
        
        if (enableDebugLogging)
        {
            Debug.Log("UpgradeEffectManager: Applied all upgrade effects");
        }
    }
    
    public PlayerUpgradeProgress GetPlayerProgress()
    {
        return playerProgress;
    }
    
    public UpgradeData[] GetAvailableUpgrades()
    {
        return availableUpgrades;
    }
    
    public UpgradeData GetUpgradeData(UpgradeType upgradeType)
    {
        foreach (var upgrade in availableUpgrades)
        {
            if (upgrade.upgradeType == upgradeType)
                return upgrade;
        }
        
        return null;
    }
    
    public void SetAvailableUpgrades(UpgradeData[] upgrades)
    {
        availableUpgrades = upgrades;
    }
    
    [ContextMenu("Apply All Upgrades")]
    public void DebugApplyAllUpgrades()
    {
        ApplyAllUpgradeEffects();
    }
    
    [ContextMenu("Reset All Upgrades")]
    public void DebugResetUpgrades()
    {
        if (playerProgress != null)
        {
            playerProgress.ResetProgress();
            Debug.Log("UpgradeEffectManager: Reset all upgrades");
        }
    }
    
    [ContextMenu("Log Progress")]
    public void DebugLogProgress()
    {
        if (playerProgress != null)
        {
            Debug.Log($"UpgradeEffectManager Progress: {playerProgress.GetProgressSummary()}");
        }
    }
    
    void OnDestroy()
    {
        if (playerProgress != null)
        {
            playerProgress.OnUpgradePurchased -= OnUpgradePurchased;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreAdded -= OnScoreAdded;
        }
    }
}