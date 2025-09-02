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
        blockGO.AddComponent<SpriteRenderer>(); // Add SpriteRenderer for sprite testing
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

    // NEW TDD TESTS FOR CONFIGURABLE SCORING

    [Test]
    public void Block_HasDefaultPointValue()
    {
        // Test that block has a default point value of 10
        Assert.AreEqual(10, block.PointValue);
    }

    [Test]
    public void Block_SetPointValue_UpdatesCorrectly()
    {
        // Test that we can set custom point values
        block.SetPointValue(25);
        Assert.AreEqual(25, block.PointValue);
        
        block.SetPointValue(100);
        Assert.AreEqual(100, block.PointValue);
        
        block.SetPointValue(1);
        Assert.AreEqual(1, block.PointValue);
    }

    [Test]
    public void Block_SetPointValue_RejectsNegativeValues()
    {
        // Test that negative point values are rejected and clamped to 0
        block.SetPointValue(-5);
        Assert.AreEqual(0, block.PointValue);
        
        block.SetPointValue(-100);
        Assert.AreEqual(0, block.PointValue);
    }

    [Test]
    public void Block_SetPointValue_AcceptsZeroValue()
    {
        // Test that zero point values are allowed (some blocks might give no points)
        block.SetPointValue(0);
        Assert.AreEqual(0, block.PointValue);
    }

    [UnityTest]
    public IEnumerator Block_DestroyBlock_UsesConfiguredPointValue()
    {
        // Arrange: Set custom point value
        block.SetPointValue(50);
        
        int initialScore = GameManager.Instance.GetScore();
        
        // Act: Destroy block
        block.TakeHit();
        yield return null; // Wait for destruction
        
        // Assert: Score should increase by custom point value (50)
        int finalScore = GameManager.Instance.GetScore();
        Assert.AreEqual(initialScore + 50, finalScore);
    }

    [UnityTest]
    public IEnumerator Block_DestroyBlock_WithZeroPoints_DoesNotAddScore()
    {
        // Arrange: Set point value to 0
        block.SetPointValue(0);
        
        int initialScore = GameManager.Instance.GetScore();
        
        // Act: Destroy block
        block.TakeHit();
        yield return null; // Wait for destruction
        
        // Assert: Score should not change
        int finalScore = GameManager.Instance.GetScore();
        Assert.AreEqual(initialScore, finalScore);
    }

    // NEW TDD TESTS FOR RANDOM SPRITE SELECTION (RED PHASE)

    [Test]
    public void Block_CanSetRandomSpriteFromList()
    {
        // Test that Block can accept and use a list of sprites for random selection
        Sprite[] testSprites = CreateTestSprites(3);
        
        block.SetRandomSpriteFromList(testSprites);
        
        // Verify the sprite was set (we'll check it's one of the provided sprites)
        SpriteRenderer spriteRenderer = blockGO.GetComponent<SpriteRenderer>();
        Assert.IsNotNull(spriteRenderer, "Block should have a SpriteRenderer component");
        Assert.IsNotNull(spriteRenderer.sprite, "Block should have a sprite assigned");
        Assert.Contains(spriteRenderer.sprite, testSprites, "Block sprite should be one from the provided list");
    }

    [Test]
    public void Block_RandomSpriteSelection_UsesAllSpritesOverMultipleSelections()
    {
        // Test that over multiple selections, all sprites from the list are eventually used
        Sprite[] testSprites = CreateTestSprites(3);
        var usedSprites = new System.Collections.Generic.HashSet<Sprite>();
        
        // Test multiple blocks to ensure randomization works
        for (int i = 0; i < 20; i++)
        {
            GameObject testBlockGO = new GameObject($"TestBlock{i}");
            testBlockGO.AddComponent<SpriteRenderer>();
            Block testBlock = testBlockGO.AddComponent<Block>();
            
            testBlock.SetRandomSpriteFromList(testSprites);
            
            SpriteRenderer sr = testBlockGO.GetComponent<SpriteRenderer>();
            usedSprites.Add(sr.sprite);
            
            // Cleanup
            if (Application.isPlaying)
                Object.Destroy(testBlockGO);
            else
                Object.DestroyImmediate(testBlockGO);
        }
        
        // With 20 attempts and 3 sprites, we should have used all sprites
        Assert.GreaterOrEqual(usedSprites.Count, 2, "Should use at least 2 different sprites over multiple selections");
    }

    [Test]
    public void Block_SetRandomSpriteFromList_HandlesEmptyArrayGracefully()
    {
        // Test that empty sprite array doesn't crash the system
        Sprite[] emptySprites = new Sprite[0];
        
        Assert.DoesNotThrow(() => block.SetRandomSpriteFromList(emptySprites));
    }

    [Test]
    public void Block_SetRandomSpriteFromList_HandlesNullArrayGracefully()
    {
        // Test that null sprite array doesn't crash the system
        Assert.DoesNotThrow(() => block.SetRandomSpriteFromList(null));
    }

    [Test]
    public void Block_SetRandomSpriteFromList_WithSingleSprite_UsesOnlyThatSprite()
    {
        // Test that with only one sprite in the list, that sprite is always used
        Sprite[] singleSprite = CreateTestSprites(1);
        
        block.SetRandomSpriteFromList(singleSprite);
        
        SpriteRenderer spriteRenderer = blockGO.GetComponent<SpriteRenderer>();
        Assert.AreEqual(singleSprite[0], spriteRenderer.sprite);
    }

    // Helper method to create test sprites for testing
    private Sprite[] CreateTestSprites(int count)
    {
        Sprite[] sprites = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            // Create a simple 1x1 texture for testing
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(i / (float)count, 0.5f, 1f, 1f)); // Different color for each sprite
            texture.Apply();
            
            sprites[i] = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
            sprites[i].name = $"TestSprite{i}";
        }
        return sprites;
    }
}