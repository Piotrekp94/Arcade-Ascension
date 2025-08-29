using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    // Define a simple structure for an upgrade
    [System.Serializable]
    public class Upgrade
    {
        public string upgradeName;
        public string description;
        public int cost;
        public UpgradeType type;
        public float value; // Generic value for the upgrade (e.g., speed increase, size multiplier)
    }

    public enum UpgradeType
    {
        PaddleSpeed,
        PaddleSize,
        BallSpeed,
        MultiBall,
        ExtraLife
    }

    [SerializeField]
    private List<Upgrade> availableUpgrades = new List<Upgrade>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void ApplyUpgrade(Upgrade upgrade)
    {
        // This method will be expanded to apply specific upgrade effects
        Debug.Log($"Applying upgrade: {upgrade.upgradeName} (Type: {upgrade.type}, Value: {upgrade.value})");

        switch (upgrade.type)
        {
            case UpgradeType.PaddleSpeed:
                PlayerPaddle playerPaddle = FindObjectOfType<PlayerPaddle>();
                if (playerPaddle != null)
                {
                    playerPaddle.SetSpeed(playerPaddle.speed + upgrade.value);
                }
                break;
            case UpgradeType.PaddleSize:
                PlayerPaddle playerPaddleSize = FindObjectOfType<PlayerPaddle>();
                if (playerPaddleSize != null)
                {
                    playerPaddleSize.SetSize(playerPaddleSize.transform.localScale.x + upgrade.value);
                }
                break;
            case UpgradeType.BallSpeed:
                Ball ball = FindObjectOfType<Ball>();
                if (ball != null)
                {
                    ball.SetSpeed(ball.initialSpeed + upgrade.value);
                }
                break;
            case UpgradeType.MultiBall:
                // Logic to spawn multiple balls
                break;
            case UpgradeType.ExtraLife:
                // Logic to grant an extra life
                break;
        }
    }

    // Method to get available upgrades (can be filtered later)
    public List<Upgrade> GetAvailableUpgrades()
    {
        return availableUpgrades;
    }
}