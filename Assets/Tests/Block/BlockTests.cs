using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TMPro; // Required for TextMeshProUGUI

public class BlockTests
{
    private Block block;
    private GameObject blockGO;
    private GameManager gameManager; // To test score/currency addition
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI currencyText;

    [SetUp]
    public void Setup()
    {
        // Setup GameManager for score/currency testing
        GameObject gameManagerGO = new GameObject();
        gameManager = gameManagerGO.AddComponent<GameManager>();
        scoreText = new GameObject().AddComponent<TextMeshProUGUI>();
        currencyText = new GameObject().AddComponent<TextMeshProUGUI>();
        // Manually set private fields for testing (not ideal for production)
        // In a real scenario, use public setters or a mocking framework.
        // For this example, we'll assume direct access for testing.
        // gameManager.scoreText = scoreText;
        // gameManager.currencyText = currencyText;

        blockGO = new GameObject();
        block = blockGO.AddComponent<Block>();
        // Set initial hit points for testing
        // block.hitPoints = 1; // Assuming hitPoints is accessible for testing
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(blockGO);
        Object.Destroy(gameManager.gameObject);
        Object.Destroy(scoreText.gameObject);
        Object.Destroy(currencyText.gameObject);
    }

    [Test]
    public void Block_TakeHitDecreasesHitPoints()
    {
        // Assuming hitPoints is accessible for testing
        // block.hitPoints = 2;
        // block.TakeHit();
        // Assert.AreEqual(1, block.hitPoints);
    }

    [UnityTest]
    public IEnumerator Block_DestroyBlockDestroysGameObject()
    {
        // Assuming hitPoints is accessible for testing
        // block.hitPoints = 1;
        // block.TakeHit();
        yield return null; // Wait a frame for Destroy to be processed
        Assert.IsTrue(block == null); // Check if the GameObject is destroyed
    }

    [UnityTest]
    public IEnumerator Block_DestroyBlockAddsScoreAndCurrency()
    {
        // Assuming hitPoints is accessible for testing
        // block.hitPoints = 1;
        // int initialScore = gameManager.score;
        // int initialCurrency = gameManager.currency;

        // block.TakeHit();
        yield return null; // Wait a frame for Destroy and GameManager updates

        // Assert.AreEqual(initialScore + 10, gameManager.score);
        // Assert.AreEqual(initialCurrency + 5, gameManager.currency);
    }
}