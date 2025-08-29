using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // For Action

public class UpgradeButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI upgradeNameText;
    [SerializeField]
    private TextMeshProUGUI descriptionText;
    [SerializeField]
    private TextMeshProUGUI costText;
    [SerializeField]
    private Button button;

    private UpgradeManager.Upgrade currentUpgrade;
    private Action<UpgradeManager.Upgrade> onUpgradeSelectedCallback;

    public void Setup(UpgradeManager.Upgrade upgrade, Action<UpgradeManager.Upgrade> callback)
    {
        currentUpgrade = upgrade;
        onUpgradeSelectedCallback = callback;

        if (upgradeNameText != null)
        {
            upgradeNameText.text = upgrade.upgradeName;
        }
        if (descriptionText != null)
        {
            descriptionText.text = upgrade.description;
        }
        if (costText != null)
        {
            costText.text = "Cost: " + upgrade.cost;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        onUpgradeSelectedCallback?.Invoke(currentUpgrade);
    }
}