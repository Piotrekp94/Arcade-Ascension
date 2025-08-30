using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

public class BlockManagerIntegrationTests
{
    private BlockManager blockManager;
    private GameObject blockManagerGO;
    private GameManager gameManager;
    private GameObject gameManagerGO;
    private Ball ball;
    private GameObject ballGO;

    [SetUp]
    public void Setup()
    {
        // Create GameManager
        gameManagerGO = new GameObject("GameManager");
        gameManager = gameManagerGO.AddComponent<GameManager>();
        GameManager.SetInstanceForTesting(gameManager);

        // Create BlockManager
        blockManagerGO = new GameObject("BlockManager");
        blockManager = blockManagerGO.AddComponent<BlockManager>();

        // Create Ball for collision testing
        ballGO = new GameObject("Ball");
        ballGO.tag = "Ball";
        ballGO.AddComponent<CircleCollider2D>();
        ballGO.AddComponent<Rigidbody2D>();
        ball = ballGO.AddComponent<Ball>();
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up all spawned blocks
        blockManager?.ClearAllBlocks();
        
        // Clean up singleton
        GameManager.SetInstanceForTesting(null);
        
        // Clean up GameObjects
        if (Application.isPlaying)
        {
            if (blockManagerGO != null) Object.Destroy(blockManagerGO);
            if (gameManagerGO != null) Object.Destroy(gameManagerGO);
            if (ballGO != null) Object.Destroy(ballGO);
        }
        else
        {
            if (blockManagerGO != null) Object.DestroyImmediate(blockManagerGO);
            if (gameManagerGO != null) Object.DestroyImmediate(gameManagerGO);
            if (ballGO != null) Object.DestroyImmediate(ballGO);
        }
    }

    [Test]
    public void BlockManagerIntegration_SpawnsBlocksOnGameStart()
    {
        // Create block prefab
        GameObject blockPrefab = CreateTestBlockPrefab();
        blockManager.SetBlockPrefab(blockPrefab);
        blockManager.SetBlockRows(2);
        blockManager.SetBlockColumns(3);
        
        // Initially no blocks
        Assert.AreEqual(0, blockManager.GetSpawnedBlocks().Count);
        
        // Start game (this should trigger block spawning)
        gameManager.SetGameState(GameManager.GameState.Start); // Ensure we're in Start state
        gameManager.StartGame();
        
        // Blocks should be spawned
        Assert.AreEqual(6, blockManager.GetSpawnedBlocks().Count); // 2x3 = 6 blocks
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(blockPrefab);
        else
            Object.DestroyImmediate(blockPrefab);
    }

    [Test]
    public void BlockManagerIntegration_BallCanHitSpawnedBlocks()
    {
        // Set up blocks
        GameObject blockPrefab = CreateTestBlockPrefab();
        blockManager.SetBlockPrefab(blockPrefab);
        blockManager.SetBlockRows(1);
        blockManager.SetBlockColumns(1);
        
        // Spawn blocks
        blockManager.SpawnBlocks();
        List<GameObject> spawnedBlocks = blockManager.GetSpawnedBlocks();
        Assert.AreEqual(1, spawnedBlocks.Count);
        
        GameObject spawnedBlock = spawnedBlocks[0];
        Block blockComponent = spawnedBlock.GetComponent<Block>();
        Assert.IsNotNull(blockComponent);
        Assert.AreEqual(1, blockComponent.HitPoints); // Default hit points
        
        // Test ball collision with block
        bool blockHitDetected = false;
        ball.OnBlockHit += (block) => {
            blockHitDetected = true;
            Assert.AreSame(blockComponent, block);
        };
        
        // Simulate ball hitting the block
        ball.SimulateBlockCollision(blockComponent);
        
        // Verify collision was detected
        Assert.IsTrue(blockHitDetected);
        Assert.AreEqual(0, blockComponent.HitPoints); // Block should have taken damage
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(blockPrefab);
        else
            Object.DestroyImmediate(blockPrefab);
    }

    [Test]
    public void BlockManagerIntegration_BlockDestructionAddsScore()
    {
        // Set up blocks and GameManager score tracking
        GameObject blockPrefab = CreateTestBlockPrefab();
        blockManager.SetBlockPrefab(blockPrefab);
        blockManager.SetBlockRows(1);
        blockManager.SetBlockColumns(1);
        
        // Initial score should be 0
        int initialScore = gameManager.GetScore();
        Assert.AreEqual(0, initialScore);
        
        // Spawn blocks
        blockManager.SpawnBlocks();
        Block blockComponent = blockManager.GetSpawnedBlocks()[0].GetComponent<Block>();
        
        // Simulate ball hitting block (this should destroy it and add score)
        ball.SimulateBlockCollision(blockComponent);
        
        // Score should have increased (Block adds 10 points when destroyed)
        Assert.AreEqual(initialScore + 10, gameManager.GetScore());
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(blockPrefab);
        else
            Object.DestroyImmediate(blockPrefab);
    }

    [Test]
    public void BlockManagerIntegration_MultipleBlocksCanBeHit()
    {
        // Set up multiple blocks
        GameObject blockPrefab = CreateTestBlockPrefab();
        blockManager.SetBlockPrefab(blockPrefab);
        blockManager.SetBlockRows(2);
        blockManager.SetBlockColumns(2);
        
        blockManager.SpawnBlocks();
        List<GameObject> spawnedBlocks = blockManager.GetSpawnedBlocks();
        Assert.AreEqual(4, spawnedBlocks.Count);
        
        int blocksHit = 0;
        ball.OnBlockHit += (block) => blocksHit++;
        
        // Hit each block
        foreach (GameObject blockGO in spawnedBlocks)
        {
            Block blockComponent = blockGO.GetComponent<Block>();
            ball.SimulateBlockCollision(blockComponent);
        }
        
        // All blocks should have been hit
        Assert.AreEqual(4, blocksHit);
        
        // Score should reflect all destroyed blocks
        Assert.AreEqual(40, gameManager.GetScore()); // 4 blocks × 10 points each
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(blockPrefab);
        else
            Object.DestroyImmediate(blockPrefab);
    }

    [Test]
    public void BlockManagerIntegration_BlocksSpawnWithinGameBoundaries()
    {
        // Test with a larger grid to ensure boundary compliance
        GameObject blockPrefab = CreateTestBlockPrefab();
        blockManager.SetBlockPrefab(blockPrefab);
        blockManager.SetBlockRows(5);
        blockManager.SetBlockColumns(8); // Default maximum columns
        
        blockManager.SpawnBlocks();
        List<GameObject> spawnedBlocks = blockManager.GetSpawnedBlocks();
        Assert.AreEqual(40, spawnedBlocks.Count); // 5×8 = 40 blocks
        
        // Verify all blocks are within game boundaries
        foreach (GameObject block in spawnedBlocks)
        {
            Vector3 pos = block.transform.position;
            
            // Should be within wall boundaries
            Assert.Greater(pos.x, -3.5f, "Block should be right of left wall");
            Assert.Less(pos.x, 3.53f, "Block should be left of right wall");
            Assert.Less(pos.y, 4.48f, "Block should be below top wall");
            
            // Should be in upper area (not overlapping with paddle area)
            Assert.Greater(pos.y, 0f, "Block should be in upper game area");
        }
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(blockPrefab);
        else
            Object.DestroyImmediate(blockPrefab);
    }

    [Test]
    public void BlockManagerIntegration_ClearsBlocksOnNewGame()
    {
        // Spawn initial blocks
        GameObject blockPrefab = CreateTestBlockPrefab();
        blockManager.SetBlockPrefab(blockPrefab);
        blockManager.SetBlockRows(2);
        blockManager.SetBlockColumns(2);
        
        blockManager.SpawnBlocks();
        Assert.AreEqual(4, blockManager.GetSpawnedBlocks().Count);
        
        // Start new game (should clear existing blocks and spawn new ones)
        gameManager.SetGameState(GameManager.GameState.Start); // Ensure we're in Start state
        gameManager.StartGame();
        
        // Should still have the same number of blocks (cleared and respawned)
        Assert.AreEqual(4, blockManager.GetSpawnedBlocks().Count);
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(blockPrefab);
        else
            Object.DestroyImmediate(blockPrefab);
    }

    [Test]
    public void BlockManagerIntegration_HandlesMultiHitBlocks()
    {
        // Create block prefab with multi-hit capability
        GameObject blockPrefab = CreateTestBlockPrefab();
        blockManager.SetBlockPrefab(blockPrefab);
        blockManager.SpawnBlocks();
        
        Block blockComponent = blockManager.GetSpawnedBlocks()[0].GetComponent<Block>();
        
        // Set block to require 3 hits
        blockComponent.SetHitPoints(3);
        Assert.AreEqual(3, blockComponent.HitPoints);
        
        int initialScore = gameManager.GetScore();
        
        // Hit block twice (shouldn't destroy it)
        ball.SimulateBlockCollision(blockComponent);
        Assert.AreEqual(2, blockComponent.HitPoints);
        Assert.AreEqual(initialScore, gameManager.GetScore()); // No score yet
        
        ball.SimulateBlockCollision(blockComponent);
        Assert.AreEqual(1, blockComponent.HitPoints);
        Assert.AreEqual(initialScore, gameManager.GetScore()); // Still no score
        
        // Third hit should destroy it and add score
        ball.SimulateBlockCollision(blockComponent);
        Assert.AreEqual(0, blockComponent.HitPoints);
        Assert.AreEqual(initialScore + 10, gameManager.GetScore()); // Now score is added
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(blockPrefab);
        else
            Object.DestroyImmediate(blockPrefab);
    }

    // Helper method to create a test block prefab
    private GameObject CreateTestBlockPrefab()
    {
        GameObject prefab = new GameObject("TestBlock");
        prefab.AddComponent<Block>();
        
        // Add required components for proper collision and physics
        BoxCollider2D collider = prefab.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.8f, 0.3f); // Reasonable block size
        
        prefab.AddComponent<SpriteRenderer>();
        prefab.AddComponent<Rigidbody2D>();
        
        // Ensure proper tag
        prefab.tag = "Block";
        
        return prefab;
    }
}