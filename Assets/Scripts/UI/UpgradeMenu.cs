using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject upgradeButtonPrefab; // Prefab for individual upgrade buttons
    [SerializeField]
    private Transform buttonContainer; // Parent transform for upgrade buttons
    [SerializeField]
    private TextMeshProUGUI currencyDisplay; // Text to show current currency

    void Start()
    {
        // Initially hide the upgrade menu
        gameObject.SetActive(false);
    }

    public void ShowMenu()
    {
        gameObject.SetActive(true);
        UpdateCurrencyDisplay();
        PopulateUpgradeOptions();
    }

    public void HideMenu()
    {
        gameObject.SetActive(false);
    }

    void UpdateCurrencyDisplay()
    {
        if (currencyDisplay != null && GameManager.Instance != null)
        {
            currencyDisplay.text = "Currency: " + GameManager.Instance.GetCurrency();
        }
    }

    void PopulateUpgradeOptions()
    {
        // Clear existing buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        if (UpgradeManager.Instance != null)
        {
            List<UpgradeManager.Upgrade> upgrades = UpgradeManager.Instance.GetAvailableUpgrades();
            foreach (UpgradeManager.Upgrade upgrade in upgrades)
            {
                GameObject buttonGO = Instantiate(upgradeButtonPrefab, buttonContainer);
                UpgradeButton upgradeButton = buttonGO.GetComponent<UpgradeButton>();
                if (upgradeButton != null)
                {
                    upgradeButton.Setup(upgrade, OnUpgradeSelected);
                }
            }
        }
    }

    void OnUpgradeSelected(UpgradeManager.Upgrade upgrade)
    {
        if (GameManager.Instance != null && UpgradeManager.Instance != null)
        {
            if (GameManager.Instance.SpendCurrency(upgrade.cost))
            {
                UpgradeManager.Instance.ApplyUpgrade(upgrade);
                UpdateCurrencyDisplay();
                // Optionally hide menu or refresh options after purchase
            }
            else
            {
                Debug.Log("Not enough currency for " + upgrade.upgradeName);
            }
        }
    }
}