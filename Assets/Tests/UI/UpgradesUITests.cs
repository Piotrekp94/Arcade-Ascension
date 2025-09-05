using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradesUITests
{
    private GameObject upgradesUIObject;
    private UpgradesUI upgradesUI;
    private GameObject upgradesPanel;
    private TextMeshProUGUI placeholderText;
    private TextMeshProUGUI pointsDisplayText;
    private Button comingSoonButton;
    private CanvasGroup canvasGroup;

    [SetUp]
    public void SetUp()
    {
        // Create main UpgradesUI object
        upgradesUIObject = new GameObject("UpgradesUI");
        upgradesUI = upgradesUIObject.AddComponent<UpgradesUI>();

        // Create upgrades panel
        upgradesPanel = new GameObject("UpgradesPanel");
        upgradesPanel.transform.SetParent(upgradesUIObject.transform);
        canvasGroup = upgradesPanel.AddComponent<CanvasGroup>();

        // Create placeholder text
        GameObject placeholderTextObject = new GameObject("PlaceholderText");
        placeholderTextObject.transform.SetParent(upgradesPanel.transform);
        placeholderText = placeholderTextObject.AddComponent<TextMeshProUGUI>();

        // Create points display text
        GameObject pointsTextObject = new GameObject("PointsText");
        pointsTextObject.transform.SetParent(upgradesPanel.transform);
        pointsDisplayText = pointsTextObject.AddComponent<TextMeshProUGUI>();

        // Create coming soon button
        GameObject buttonObject = new GameObject("ComingSoonButton");
        buttonObject.transform.SetParent(upgradesPanel.transform);
        comingSoonButton = buttonObject.AddComponent<Button>();

        // Setup UpgradesUI with references
        upgradesUI.SetupForTesting(upgradesPanel, placeholderText, pointsDisplayText);
        upgradesUI.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        if (upgradesUIObject != null)
        {
            if (Application.isPlaying)
                Object.Destroy(upgradesUIObject);
            else
                Object.DestroyImmediate(upgradesUIObject);
        }
    }

    [Test]
    public void UpgradesUI_Initialize_SetsUpCorrectly()
    {
        // Assert
        Assert.IsNotNull(upgradesUI);
        Assert.IsNotNull(upgradesUI.GetCanvasGroup());
    }

    [Test]
    public void UpgradesUI_ShowUpgrades_MakesVisible()
    {
        // Arrange - hide first
        upgradesUI.HideUpgrades();
        Assert.IsFalse(upgradesUI.IsVisible());

        // Act
        upgradesUI.ShowUpgrades();

        // Assert
        Assert.IsTrue(upgradesUI.IsVisible());
        Assert.AreEqual(1f, canvasGroup.alpha);
        Assert.IsTrue(canvasGroup.interactable);
        Assert.IsTrue(canvasGroup.blocksRaycasts);
    }

    [Test]
    public void UpgradesUI_HideUpgrades_MakesInvisible()
    {
        // Arrange - show first
        upgradesUI.ShowUpgrades();
        Assert.IsTrue(upgradesUI.IsVisible());

        // Act
        upgradesUI.HideUpgrades();

        // Assert
        Assert.IsFalse(upgradesUI.IsVisible());
        Assert.AreEqual(0f, canvasGroup.alpha);
        Assert.IsFalse(canvasGroup.interactable);
        Assert.IsFalse(canvasGroup.blocksRaycasts);
    }

    [Test]
    public void UpgradesUI_UpdatePlaceholderText_ChangesText()
    {
        // Arrange
        string newText = "Test placeholder text";

        // Act
        upgradesUI.UpdatePlaceholderText(newText);

        // Assert
        Assert.AreEqual(newText, placeholderText.text);
    }

    [Test]
    public void UpgradesUI_GetCanvasGroup_ReturnsCorrectComponent()
    {
        // Assert
        Assert.AreEqual(canvasGroup, upgradesUI.GetCanvasGroup());
    }

    [Test]
    public void UpgradesUI_IsVisible_ReflectsCanvasGroupState()
    {
        // Test visible state
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        Assert.IsTrue(upgradesUI.IsVisible());

        // Test invisible state
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        Assert.IsFalse(upgradesUI.IsVisible());
    }

    [Test]
    public void UpgradesUI_Initialize_SetsPlaceholderContent()
    {
        // Assert - should have set placeholder text
        Assert.IsTrue(placeholderText.text.Contains("UPGRADES"));
        Assert.IsTrue(placeholderText.text.Contains("Coming Soon"));
        Assert.AreEqual(TextAlignmentOptions.Center, placeholderText.alignment);
    }

    [Test]
    public void UpgradesUI_Initialize_SetsPointsDisplay()
    {
        // Assert - should have set points display text
        Assert.IsTrue(pointsDisplayText.text.Contains("Available Points:"));
    }

    [Test]
    public void UpgradesUI_SetupForTesting_UpdatesReferences()
    {
        // Arrange
        GameObject newPanel = new GameObject("NewPanel");
        TextMeshProUGUI newPlaceholder = new GameObject("NewPlaceholder").AddComponent<TextMeshProUGUI>();
        TextMeshProUGUI newPoints = new GameObject("NewPoints").AddComponent<TextMeshProUGUI>();

        // Act
        upgradesUI.SetupForTesting(newPanel, newPlaceholder, newPoints);

        // Assert - should not throw exceptions when using new references
        Assert.DoesNotThrow(() => upgradesUI.ShowUpgrades());
        Assert.DoesNotThrow(() => upgradesUI.HideUpgrades());
        Assert.DoesNotThrow(() => upgradesUI.UpdatePlaceholderText("Test"));

        // Cleanup
        Object.DestroyImmediate(newPanel);
        Object.DestroyImmediate(newPlaceholder.gameObject);
        Object.DestroyImmediate(newPoints.gameObject);
    }
}