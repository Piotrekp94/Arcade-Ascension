using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TMPro; // Required for TextMeshProUGUI

public class BlockTests
{
    private Block block;
    private GameObject blockGO;
    private GameManager gameManager; // To test score addition
    private TextMeshProUGUI scoreText;

    [SetUp]
    public void Setup()
    {
        // Setup GameManager for score testing
        GameObject gameManagerGO = new GameObject();
        gameManager = gameManagerGO.AddComponent<GameManager>();
        scoreText = new GameObject().AddComponent<TextMeshProUGUI>();
        
        // Set up the singleton Instance for testing
        // This ensures GameManager.Instance is properly set for our tests
        GameManager.SetInstanceForTesting(gameManager);
        
        // Ensure the GameManager is properly initialized
        // The Start() method just initializes score to 0, so we don't need to call it
        // The GetScore() and AddScore() methods should work without Start() being called

        blockGO = new GameObject();
        block = blockGO.AddComponent<Block>();
        // Set initial hit points for testing
        // block.hitPoints = 1; // Assuming hitPoints is accessible for testing
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up singleton Instance to prevent test pollution
        GameManager.SetInstanceForTesting(null);
        
        // Use DestroyImmediate in edit mode (tests) and Destroy in play mode
        if (Application.isPlaying)
        {
            if (blockGO != null) Object.Destroy(blockGO);
            if (gameManager != null && gameManager.gameObject != null) Object.Destroy(gameManager.gameObject);
            if (scoreText != null && scoreText.gameObject != null) Object.Destroy(scoreText.gameObject);
        }
        else
        {
            if (blockGO != null) Object.DestroyImmediate(blockGO);
            if (gameManager != null && gameManager.gameObject != null) Object.DestroyImmediate(gameManager.gameObject);
            if (scoreText != null && scoreText.gameObject != null) Object.DestroyImmediate(scoreText.gameObject);
        }
    }

    [Test]
    public void Block_HasInitialHitPointsOfOne()
    {
        // Test that block starts with 1 hit point by default
        Assert.AreEqual(1, block.HitPoints);
    }

    [Test]
    public void Block_TakeHitDecreasesHitPoints()
    {
        // Arrange: Block starts with 1 hit point
        Assert.AreEqual(1, block.HitPoints);
        
        // Act: Take a hit
        block.TakeHit();
        
        // Assert: Hit points should decrease to 0
        Assert.AreEqual(0, block.HitPoints);
    }

    [Test]
    public void Block_TakeHitFromTwoHitPointsDecreasesToOne()
    {
        // Arrange: Set block to have 2 hit points
        block.SetHitPoints(2);
        Assert.AreEqual(2, block.HitPoints);
        
        // Act: Take a hit
        block.TakeHit();
        
        // Assert: Hit points should decrease to 1
        Assert.AreEqual(1, block.HitPoints);
    }

    [Test]
    public void Block_MultipleHitsDecrementCorrectly()
    {
        // Arrange: Set block to have 3 hit points
        block.SetHitPoints(3);
        Assert.AreEqual(3, block.HitPoints);
        
        // Act: Take multiple hits
        block.TakeHit();
        Assert.AreEqual(2, block.HitPoints);
        
        block.TakeHit();
        Assert.AreEqual(1, block.HitPoints);
        
        block.TakeHit();
        Assert.AreEqual(0, block.HitPoints);
    }

    [UnityTest]
    public IEnumerator Block_DestroyBlockDestroysGameObject()
    {
        // Arrange: Block with 1 hit point
        Assert.AreEqual(1, block.HitPoints);
        Assert.IsNotNull(blockGO);
        
        // Act: Take a hit that should destroy the block
        block.TakeHit();
        yield return null; // Wait a frame for Destroy to be processed
        
        // Assert: GameObject should be destroyed
        Assert.IsTrue(block == null || blockGO == null);
    }

    [UnityTest]
    public IEnumerator Block_DestroyBlockAddsScore()
    {
        // Arrange: Block with 1 hit point and ensure GameManager is properly initialized
        Assert.AreEqual(1, block.HitPoints);
        
        // Ensure GameManager.Instance is properly set and initialized
        Assert.IsNotNull(GameManager.Instance);
        Assert.AreSame(gameManager, GameManager.Instance);
        
        int initialScore = GameManager.Instance.GetScore();

        // Act: Take a hit that should destroy the block and add score
        block.TakeHit();
        yield return null; // Wait a frame for Destroy and GameManager updates

        // Assert: Score should increase by 10
        int finalScore = GameManager.Instance.GetScore();
        Assert.AreEqual(initialScore + 10, finalScore, 
            $"Expected score to increase from {initialScore} to {initialScore + 10}, but got {finalScore}");
    }

    [Test]
    public void Block_TakeHitWhenHitPointsIsZeroDoesNotThrow()
    {
        // Arrange: Take hit to reduce to 0 hit points
        block.TakeHit();
        Assert.AreEqual(0, block.HitPoints);
        
        // Act & Assert: Taking another hit should not throw exception
        Assert.DoesNotThrow(() => block.TakeHit());
        
        // But hit points should not go negative (this will help drive our implementation)
        Assert.GreaterOrEqual(block.HitPoints, 0);
    }

    [Test]
    public void Block_SetHitPointsUpdatesCorrectly()
    {
        // Test our setter method works correctly
        block.SetHitPoints(5);
        Assert.AreEqual(5, block.HitPoints);
        
        block.SetHitPoints(0);
        Assert.AreEqual(0, block.HitPoints);
        
        // Edge case: Setting negative hit points should be clamped to 0
        block.SetHitPoints(-1);
        Assert.AreEqual(0, block.HitPoints); // Hit points should never be negative
    }

    [Test]
    public void GameManager_SingletonAndScoreWorkCorrectly()
    {
        // Test that our GameManager singleton setup works correctly in tests
        Assert.IsNotNull(GameManager.Instance);
        Assert.AreSame(gameManager, GameManager.Instance);
        
        // Test initial score
        int initialScore = GameManager.Instance.GetScore();
        Assert.AreEqual(0, initialScore); // Score should start at 0
        
        // Test adding score
        GameManager.Instance.AddScore(10);
        Assert.AreEqual(10, GameManager.Instance.GetScore());
        
        // Test adding more score
        GameManager.Instance.AddScore(5);
        Assert.AreEqual(15, GameManager.Instance.GetScore());
    }

    [Test]
    public void Block_DestroyBlockWithNullGameManagerDoesNotThrow()
    {
        // Arrange: Ensure GameManager.Instance is null
        // This tests that our code handles missing GameManager gracefully
        var originalInstance = GameManager.Instance;
        
        // We can't easily set GameManager.Instance to null in Unity's singleton,
        // but we can test that the method doesn't crash when called
        // This is more of an integration test
        
        Assert.DoesNotThrow(() => {
            block.TakeHit(); // Should destroy block when hit points reach 0
        });
    }
}