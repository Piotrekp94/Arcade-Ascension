using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the spawning and destruction of all game objects for each level
/// </summary>
public class LevelLifecycleManager : MonoBehaviour
{
    public static LevelLifecycleManager Instance { get; private set; }

    [Header("Prefab References")]
    [SerializeField] private GameObject paddlePrefab;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject blockPrefab;

    [Header("Wall Settings")]
    [SerializeField] private Vector3 wallsPosition = new Vector3(0f, 0f, 0f);
    
    [Header("Paddle Settings")]
    [SerializeField] private Vector3 paddleStartPosition = new Vector3(0f, -3f, 0f);
    
    [Header("Ball Settings")]
    [SerializeField] private Vector3 ballStartOffset = new Vector3(0f, 0.5f, 0f); // Offset above paddle

    // Tracking spawned objects
    private List<GameObject> spawnedWalls = new List<GameObject>(); // Contains combined walls prefab
    private GameObject spawnedPaddle = null;
    private GameObject spawnedBall = null;
    private List<GameObject> spawnedBlocks = new List<GameObject>();

    // Component references for spawned objects
    private PlayerPaddle paddleComponent = null;
    private Ball ballComponent = null;
    private BlockManager blockManager = null;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple LevelLifecycleManager instances found. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Spawns all objects needed for a level
    /// </summary>
    public void SpawnLevelObjects(LevelData levelData)
    {
        Debug.Log($"LevelLifecycleManager: Spawning objects for {levelData?.LevelName ?? "Unknown Level"}");

        // Clear any existing objects first
        DestroyLevelObjects();

        // Spawn in order: Combined Walls -> Paddle -> Blocks -> Ball
        SpawnWalls();
        SpawnPaddle();
        SpawnBlocks(levelData);
        SpawnBall();

        Debug.Log("LevelLifecycleManager: All level objects spawned successfully");
    }

    /// <summary>
    /// Destroys all spawned level objects
    /// </summary>
    public void DestroyLevelObjects()
    {
        Debug.Log("LevelLifecycleManager: Destroying all level objects");

        DestroyWalls();
        DestroyPaddle();
        DestroyBlocks();
        DestroyBall();

        Debug.Log("LevelLifecycleManager: All level objects destroyed");
    }

    #region Wall Management

    void SpawnWalls()
    {
        if (wallPrefab == null)
        {
            Debug.LogError("LevelLifecycleManager: No wall prefab assigned!");
            return;
        }

        // Spawn the combined walls prefab (contains top, left, right walls)
        GameObject walls = Instantiate(wallPrefab, wallsPosition, Quaternion.identity);
        walls.name = "Walls";
        spawnedWalls.Add(walls);

        Debug.Log("LevelLifecycleManager: Combined walls prefab spawned");
    }

    void DestroyWalls()
    {
        foreach (GameObject wall in spawnedWalls)
        {
            if (wall != null)
            {
                if (Application.isPlaying)
                    Destroy(wall);
                else
                    DestroyImmediate(wall);
            }
        }
        spawnedWalls.Clear();
    }

    #endregion

    #region Paddle Management

    void SpawnPaddle()
    {
        if (paddlePrefab == null)
        {
            Debug.LogError("LevelLifecycleManager: No paddle prefab assigned!");
            return;
        }

        spawnedPaddle = Instantiate(paddlePrefab, paddleStartPosition, Quaternion.identity);
        spawnedPaddle.name = "PlayerPaddle";
        
        paddleComponent = spawnedPaddle.GetComponent<PlayerPaddle>();
        if (paddleComponent == null)
        {
            Debug.LogError("LevelLifecycleManager: Paddle prefab missing PlayerPaddle component!");
        }

        Debug.Log("LevelLifecycleManager: Paddle spawned");
    }

    void DestroyPaddle()
    {
        if (spawnedPaddle != null)
        {
            if (Application.isPlaying)
                Destroy(spawnedPaddle);
            else
                DestroyImmediate(spawnedPaddle);
                
            spawnedPaddle = null;
            paddleComponent = null;
        }
    }

    #endregion

    #region Ball Management

    void SpawnBall()
    {
        if (ballPrefab == null)
        {
            Debug.LogError("LevelLifecycleManager: No ball prefab assigned!");
            return;
        }

        if (paddleComponent == null)
        {
            Debug.LogError("LevelLifecycleManager: Cannot spawn ball - no paddle available!");
            return;
        }

        // Spawn ball at paddle position + offset
        Vector3 ballPosition = paddleStartPosition + ballStartOffset;
        spawnedBall = Instantiate(ballPrefab, ballPosition, Quaternion.identity);
        spawnedBall.name = "Ball";
        
        ballComponent = spawnedBall.GetComponent<Ball>();
        if (ballComponent == null)
        {
            Debug.LogError("LevelLifecycleManager: Ball prefab missing Ball component!");
        }
        else
        {
            // Attach ball to paddle
            paddleComponent.AttachBall(ballComponent);
        }

        Debug.Log("LevelLifecycleManager: Ball spawned and attached to paddle");
    }

    void DestroyBall()
    {
        if (spawnedBall != null)
        {
            if (Application.isPlaying)
                Destroy(spawnedBall);
            else
                DestroyImmediate(spawnedBall);
                
            spawnedBall = null;
            ballComponent = null;
        }
    }

    #endregion

    #region Block Management

    void SpawnBlocks(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("LevelLifecycleManager: No level data provided for block spawning!");
            return;
        }

        if (blockPrefab == null)
        {
            Debug.LogError("LevelLifecycleManager: No block prefab assigned!");
            return;
        }

        // Create temporary BlockManager for this level
        GameObject blockManagerGO = new GameObject("BlockManager_Temp");
        blockManager = blockManagerGO.AddComponent<BlockManager>();
        
        // Configure BlockManager with level data
        ConfigureBlockManager(levelData);
        
        // Spawn blocks using BlockManager
        blockManager.SpawnBlocks();
        
        // Get references to spawned blocks for tracking
        var spawned = blockManager.GetSpawnedBlocks();
        spawnedBlocks.AddRange(spawned);

        Debug.Log($"LevelLifecycleManager: {spawnedBlocks.Count} blocks spawned for {levelData.LevelName}");
    }

    void ConfigureBlockManager(LevelData levelData)
    {
        if (blockManager == null) return;

        // Use reflection to set private fields or add public setters to BlockManager
        blockManager.SetBlockRows(levelData.BlockRows);
        blockManager.SetBlockColumns(levelData.BlockColumns);
        blockManager.SetBlockSpacingX(levelData.BlockSpacingX);
        blockManager.SetBlockSpacingY(levelData.BlockSpacingY);
        blockManager.SetSpawnAreaOffset(levelData.SpawnAreaOffset);
        blockManager.SetBlockSpriteList(levelData.BlockSprites);
        
        // Set block prefab
        var field = typeof(BlockManager).GetField("blockPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(blockManager, blockPrefab);
    }

    void DestroyBlocks()
    {
        // Clear blocks through BlockManager if available
        if (blockManager != null)
        {
            blockManager.ClearAllBlocks();
            
            // Destroy the temporary BlockManager GameObject
            if (Application.isPlaying)
                Destroy(blockManager.gameObject);
            else
                DestroyImmediate(blockManager.gameObject);
                
            blockManager = null;
        }
        
        // Fallback: destroy any remaining blocks
        foreach (GameObject block in spawnedBlocks)
        {
            if (block != null)
            {
                if (Application.isPlaying)
                    Destroy(block);
                else
                    DestroyImmediate(block);
            }
        }
        
        spawnedBlocks.Clear();
    }

    #endregion

    #region Public Accessors

    /// <summary>
    /// Get reference to spawned paddle component
    /// </summary>
    public PlayerPaddle GetPaddleComponent()
    {
        return paddleComponent;
    }

    /// <summary>
    /// Get reference to spawned ball component
    /// </summary>
    public Ball GetBallComponent()
    {
        return ballComponent;
    }

    /// <summary>
    /// Get reference to spawned ball GameObject
    /// </summary>
    public GameObject GetBallGameObject()
    {
        return spawnedBall;
    }

    /// <summary>
    /// Get count of spawned blocks for level completion tracking
    /// </summary>
    public int GetSpawnedBlockCount()
    {
        return spawnedBlocks.Count;
    }

    /// <summary>
    /// Check if level objects are currently spawned
    /// </summary>
    public bool HasSpawnedObjects()
    {
        return spawnedPaddle != null || spawnedBall != null || spawnedBlocks.Count > 0 || spawnedWalls.Count > 0;
    }

    #endregion

    #region Testing Support

    /// <summary>
    /// For testing - set instance
    /// </summary>
    public static void SetInstanceForTesting(LevelLifecycleManager testInstance)
    {
        Instance = testInstance;
    }

    #endregion
}