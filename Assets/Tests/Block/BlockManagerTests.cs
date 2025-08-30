using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

public class BlockManagerTests
{
    private BlockManager blockManager;
    private GameObject blockManagerGO;
    private GameManager gameManager;
    private GameObject gameManagerGO;

    [SetUp]
    public void Setup()
    {
        // Create GameManager for testing integration
        gameManagerGO = new GameObject("GameManager");
        gameManager = gameManagerGO.AddComponent<GameManager>();
        GameManager.SetInstanceForTesting(gameManager);

        // Create BlockManager
        blockManagerGO = new GameObject("BlockManager");
        blockManager = blockManagerGO.AddComponent<BlockManager>();
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
        }
        else
        {
            if (blockManagerGO != null) Object.DestroyImmediate(blockManagerGO);
            if (gameManagerGO != null) Object.DestroyImmediate(gameManagerGO);
        }
    }

    [Test]
    public void BlockManager_HasConfigurableBlockRows()
    {
        // Test default block rows
        Assert.Greater(blockManager.GetBlockRows(), 0);
        
        // Test setting custom block rows
        blockManager.SetBlockRows(3);
        Assert.AreEqual(3, blockManager.GetBlockRows());
    }

    [Test]
    public void BlockManager_HasConfigurableBlockColumns()
    {
        // Test default block columns
        Assert.Greater(blockManager.GetBlockColumns(), 0);
        
        // Test setting custom block columns
        blockManager.SetBlockColumns(6);
        Assert.AreEqual(6, blockManager.GetBlockColumns());
    }

    [Test]
    public void BlockManager_HasConfigurableBlockSpacing()
    {
        // Test default block spacing
        Assert.Greater(blockManager.GetBlockSpacing(), 0f);
        
        // Test setting custom block spacing
        blockManager.SetBlockSpacing(0.2f);
        Assert.That(blockManager.GetBlockSpacing(), Is.EqualTo(0.2f).Within(0.01f));
    }

    [Test]
    public void BlockManager_HasConfigurableSpawnAreaOffset()
    {
        // Test default spawn area offset
        Vector2 defaultOffset = blockManager.GetSpawnAreaOffset();
        Assert.Less(defaultOffset.y, 0f); // Should spawn below top wall
        
        // Test setting custom spawn area offset
        Vector2 customOffset = new Vector2(0.5f, -1.5f);
        blockManager.SetSpawnAreaOffset(customOffset);
        Assert.That(Vector2.Distance(blockManager.GetSpawnAreaOffset(), customOffset), Is.LessThan(0.01f));
    }

    [Test]
    public void BlockManager_CanSetAndGetBlockPrefab()
    {
        // Initially might be null
        GameObject currentPrefab = blockManager.GetBlockPrefab();
        
        // Create a test prefab
        GameObject testPrefab = new GameObject("TestBlockPrefab");
        testPrefab.AddComponent<Block>();
        
        blockManager.SetBlockPrefab(testPrefab);
        Assert.AreSame(testPrefab, blockManager.GetBlockPrefab());
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(testPrefab);
        else
            Object.DestroyImmediate(testPrefab);
    }

    [Test]
    public void BlockManager_CalculatesCorrectBlockPositions()
    {
        // Set up test configuration
        blockManager.SetBlockRows(2);
        blockManager.SetBlockColumns(3);
        blockManager.SetBlockSpacing(0.1f);
        
        // Calculate expected positions based on wall boundaries
        // Left wall: -3.5, Right wall: 3.53, Top wall: 4.48
        Vector2 pos00 = blockManager.CalculateBlockPosition(0, 0); // Top-left
        Vector2 pos01 = blockManager.CalculateBlockPosition(0, 1); // Top-middle
        Vector2 pos10 = blockManager.CalculateBlockPosition(1, 0); // Bottom-left
        
        // Verify positions are within wall boundaries
        Assert.Greater(pos00.x, -3.5f, "Block position should be right of left wall");
        Assert.Less(pos01.x, 3.53f, "Block position should be left of right wall");
        Assert.Less(pos00.y, 4.48f, "Block position should be below top wall");
        
        // Verify spacing between blocks
        float horizontalSpacing = Mathf.Abs(pos01.x - pos00.x);
        Assert.Greater(horizontalSpacing, blockManager.GetBlockSpacing(), "Horizontal spacing should account for block spacing");
        
        float verticalSpacing = Mathf.Abs(pos10.y - pos00.y);
        Assert.Greater(verticalSpacing, blockManager.GetBlockSpacing(), "Vertical spacing should account for block spacing");
    }

    [Test]
    public void BlockManager_SpawnedBlocksAreWithinBoundaries()
    {
        // Create a simple block prefab for testing
        GameObject blockPrefab = CreateTestBlockPrefab();
        blockManager.SetBlockPrefab(blockPrefab);
        
        // Set test configuration
        blockManager.SetBlockRows(2);
        blockManager.SetBlockColumns(3);
        
        // Spawn blocks
        blockManager.SpawnBlocks();
        
        // Get spawned blocks
        List<GameObject> spawnedBlocks = blockManager.GetSpawnedBlocks();
        Assert.AreEqual(6, spawnedBlocks.Count); // 2 rows × 3 columns = 6 blocks
        
        // Verify all blocks are within boundaries
        foreach (GameObject block in spawnedBlocks)
        {
            Vector3 pos = block.transform.position;
            Assert.Greater(pos.x, -3.5f, "Block should be right of left wall");
            Assert.Less(pos.x, 3.53f, "Block should be left of right wall");
            Assert.Less(pos.y, 4.48f, "Block should be below top wall");
        }
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(blockPrefab);
        else
            Object.DestroyImmediate(blockPrefab);
    }

    [Test]
    public void BlockManager_ClearAllBlocksRemovesSpawnedBlocks()
    {
        // Create and spawn blocks
        GameObject blockPrefab = CreateTestBlockPrefab();
        blockManager.SetBlockPrefab(blockPrefab);
        blockManager.SetBlockRows(2);
        blockManager.SetBlockColumns(2);
        
        blockManager.SpawnBlocks();
        Assert.AreEqual(4, blockManager.GetSpawnedBlocks().Count);
        
        // Clear all blocks
        blockManager.ClearAllBlocks();
        Assert.AreEqual(0, blockManager.GetSpawnedBlocks().Count);
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(blockPrefab);
        else
            Object.DestroyImmediate(blockPrefab);
    }

    [Test]
    public void BlockManager_SpawnsBlocksOnGameStartEvent()
    {
        // Set up block manager with test configuration
        GameObject blockPrefab = CreateTestBlockPrefab();
        blockManager.SetBlockPrefab(blockPrefab);
        blockManager.SetBlockRows(1);
        blockManager.SetBlockColumns(2);
        
        // Initially no blocks should be spawned
        Assert.AreEqual(0, blockManager.GetSpawnedBlocks().Count);
        
        // Manually trigger game start event through GameManager
        gameManager.SetGameState(GameManager.GameState.Start); // Ensure we're in Start state
        gameManager.StartGame();
        
        // Blocks should now be spawned
        Assert.AreEqual(2, blockManager.GetSpawnedBlocks().Count);
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(blockPrefab);
        else
            Object.DestroyImmediate(blockPrefab);
    }

    [Test]
    public void BlockManager_ClearsOldBlocksBeforeSpawningNew()
    {
        // Create test setup
        GameObject blockPrefab = CreateTestBlockPrefab();
        blockManager.SetBlockPrefab(blockPrefab);
        blockManager.SetBlockRows(1);
        blockManager.SetBlockColumns(1);
        
        // Spawn first set of blocks
        blockManager.SpawnBlocks();
        Assert.AreEqual(1, blockManager.GetSpawnedBlocks().Count);
        GameObject firstBlock = blockManager.GetSpawnedBlocks()[0];
        
        // Spawn again - should clear old blocks and create new ones
        blockManager.SpawnBlocks();
        Assert.AreEqual(1, blockManager.GetSpawnedBlocks().Count);
        GameObject secondBlock = blockManager.GetSpawnedBlocks()[0];
        
        // Should be different instances
        Assert.AreNotSame(firstBlock, secondBlock);
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(blockPrefab);
        else
            Object.DestroyImmediate(blockPrefab);
    }

    [Test]
    public void BlockManager_RegistersWithGameManagerEvents()
    {
        // Test that BlockManager can register with GameManager events
        // This is more of an integration test
        Assert.DoesNotThrow(() => blockManager.RegisterWithGameManager());
        
        // Verify it doesn't crash when GameManager triggers events
        GameObject blockPrefab = CreateTestBlockPrefab();
        blockManager.SetBlockPrefab(blockPrefab);
        
        gameManager.SetGameState(GameManager.GameState.Start); // Ensure we're in Start state
        Assert.DoesNotThrow(() => gameManager.StartGame());
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(blockPrefab);
        else
            Object.DestroyImmediate(blockPrefab);
    }

    [Test]
    public void BlockManager_HandlesNullBlockPrefabGracefully()
    {
        // Test that BlockManager handles null block prefab without crashing
        blockManager.SetBlockPrefab(null);
        Assert.IsNull(blockManager.GetBlockPrefab());
        
        // Should not crash when trying to spawn with null prefab
        Assert.DoesNotThrow(() => blockManager.SpawnBlocks());
        Assert.AreEqual(0, blockManager.GetSpawnedBlocks().Count);
    }

    [Test]
    public void BlockManager_ValidatesConfigurationValues()
    {
        // Test that negative or zero values are handled appropriately
        blockManager.SetBlockRows(0);
        Assert.Greater(blockManager.GetBlockRows(), 0); // Should clamp to minimum value
        
        blockManager.SetBlockColumns(-1);
        Assert.Greater(blockManager.GetBlockColumns(), 0); // Should clamp to minimum value
        
        blockManager.SetBlockSpacing(-0.5f);
        Assert.GreaterOrEqual(blockManager.GetBlockSpacing(), 0f); // Should clamp to non-negative
    }

    [Test]
    public void BlockManager_CalculatesGridDimensionsProperly()
    {
        // Test that grid calculations work correctly for different configurations
        blockManager.SetBlockRows(3);
        blockManager.SetBlockColumns(4);
        blockManager.SetBlockSpacing(0.2f);
        
        // Test boundary calculations
        Vector2 topLeft = blockManager.CalculateBlockPosition(0, 0);
        Vector2 bottomRight = blockManager.CalculateBlockPosition(2, 3); // Last block
        
        // Verify grid spans properly within boundaries
        float gridWidth = Mathf.Abs(bottomRight.x - topLeft.x);
        float gridHeight = Mathf.Abs(bottomRight.y - topLeft.y);
        
        Assert.Greater(gridWidth, 0f, "Grid should have positive width");
        Assert.Greater(gridHeight, 0f, "Grid should have positive height");
        
        // Grid should fit within wall boundaries
        Assert.Greater(topLeft.x, -3.5f, "Grid should start right of left wall");
        Assert.Less(bottomRight.x, 3.53f, "Grid should end left of right wall");
    }

    // Helper method to create a test block prefab
    private GameObject CreateTestBlockPrefab()
    {
        GameObject prefab = new GameObject("TestBlock");
        prefab.AddComponent<Block>();
        prefab.AddComponent<BoxCollider2D>();
        prefab.AddComponent<SpriteRenderer>();
        return prefab;
    }
}