using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TMPro; // Required for TextMeshProUGUI

public class GameManagerTests
{
    private GameManager gameManager;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI currencyText;

    [SetUp]
    public void Setup()
    {
        // Create a new GameObject for GameManager
        GameObject gameManagerGO = new GameObject();
        gameManager = gameManagerGO.AddComponent<GameManager>();

        // Create mock TextMeshProUGUI components
        GameObject scoreTextGO = new GameObject();
        scoreText = scoreTextGO.AddComponent<TextMeshProUGUI>();
        GameObject currencyTextGO = new GameObject();
        currencyText = currencyTextGO.AddComponent<TextMeshProUGUI>();

        // Use reflection or a public setter to assign the private fields
        // For simplicity, assuming public setters or direct assignment for testing purposes
        // In a real scenario, you might use a test-specific constructor or public properties
        // For now, we'll directly set the private fields using reflection if necessary,
        // or modify GameManager to have public setters for testing.
        // For this example, let's assume we'll make them public for testing.
        // (Note: In a real Unity project, you'd typically use [SerializeField] and assign in editor,
        // or use a more robust mocking framework for UI elements.)

        // Directly setting private fields for testing purposes (not ideal for production)
        // This would require making the fields public or using reflection.
        // For this example, we'll assume they are accessible for testing.
        // gameManager.scoreText = scoreText;
        // gameManager.currencyText = currencyText;

        // A more robust way would be to have a test-specific initialization method in GameManager
        // or use a mocking library. For now, we'll proceed with the assumption that
        // the GameManager's Awake and Start methods will handle initialization,
        // and we'll check the public methods.

        // Initialize GameManager (Awake and Start are called automatically by Unity Test Runner)
        // However, for isolated unit tests, we might need to manually call them or ensure they are called.
        // For now, we'll rely on Unity Test Runner's lifecycle.
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up GameObjects after each test
        Object.Destroy(gameManager.gameObject);
        Object.Destroy(scoreText.gameObject);
        Object.Destroy(currencyText.gameObject);
    }

    [Test]
    public void GameManager_InitialScoreIsZero()
    {
        // GameManager's Start method sets score to 0
        Assert.AreEqual(0, gameManager.GetScore());
    }

    [Test]
    public void GameManager_AddScoreIncreasesScore()
    {
        gameManager.AddScore(100);
        Assert.AreEqual(100, gameManager.GetScore());
    }

    [Test]
    public void GameManager_InitialCurrencyIsZero()
    {
        // GameManager's Start method sets currency to 0
        Assert.AreEqual(0, gameManager.GetCurrency());
    }

    [Test]
    public void GameManager_AddCurrencyIncreasesCurrency()
    {
        gameManager.AddCurrency(50);
        Assert.AreEqual(50, gameManager.GetCurrency());
    }

    [Test]
    public void GameManager_SpendCurrencyDecreasesCurrency()
    {
        gameManager.AddCurrency(100);
        gameManager.SpendCurrency(30);
        Assert.AreEqual(70, gameManager.GetCurrency());
    }

    [Test]
    public void GameManager_SpendCurrencyReturnsTrueWhenEnoughCurrency()
    {
        gameManager.AddCurrency(50);
        bool result = gameManager.SpendCurrency(20);
        Assert.IsTrue(result);
    }

    [Test]
    public void GameManager_SpendCurrencyReturnsFalseWhenNotEnoughCurrency()
    {
        gameManager.AddCurrency(10);
        bool result = gameManager.SpendCurrency(20);
        Assert.IsFalse(result);
        Assert.AreEqual(10, gameManager.GetCurrency()); // Currency should not change
    }

    [Test]
    public void GameManager_SetGameStateChangesState()
    {
        gameManager.SetGameState(GameManager.GameState.Playing);
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
    }
}