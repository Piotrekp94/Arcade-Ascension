using UnityEngine;
using System.Collections.Generic;

public class BlockManager : MonoBehaviour
{
    [SerializeField]
    private int blockRows = 5;
    [SerializeField]
    private int blockColumns = 8;
    [SerializeField]
    private float blockSpacing = 0.1f;
    [SerializeField]
    private float blockSpacingX = 0.1f;
    [SerializeField]
    private float blockSpacingY = 0.1f;
    [SerializeField]
    private GameObject blockPrefab;
    [SerializeField]
    private Vector2 spawnAreaOffset = new Vector2(0f, -1f);
    [SerializeField]
    private Sprite[] blockSpriteList; // List of sprites for random block appearance
    
    // Score configuration
    [SerializeField]
    private int defaultScoreOverride = -1; // -1 means use GameManager default
    
    // Wall boundaries (from PADDLE_COLLISION_SETUP.md)
    private const float LEFT_WALL_X = -3.5f;
    private const float RIGHT_WALL_X = 3.53f;
    private const float TOP_WALL_Y = 4.48f;
    
    // List to track spawned blocks
    private List<GameObject> spawnedBlocks = new List<GameObject>();
    
    // Default block size (will be calculated from prefab or use defaults)
    private Vector2 blockSize = new Vector2(0.8f, 0.3f);

    void Start()
    {
        RegisterWithGameManager();
    }

    // Configuration getters and setters
    public int GetBlockRows()
    {
        return blockRows;
    }

    public void SetBlockRows(int rows)
    {
        blockRows = Mathf.Max(1, rows); // Ensure minimum of 1 row
    }

    public int GetBlockColumns()
    {
        return blockColumns;
    }

    public void SetBlockColumns(int columns)
    {
        blockColumns = Mathf.Max(1, columns); // Ensure minimum of 1 column
    }

    public float GetBlockSpacing()
    {
        return blockSpacing;
    }

    public void SetBlockSpacing(float spacing)
    {
        blockSpacing = Mathf.Max(0f, spacing); // Ensure non-negative spacing
    }

    public float GetBlockSpacingX()
    {
        return blockSpacingX;
    }

    public void SetBlockSpacingX(float spacing)
    {
        blockSpacingX = Mathf.Max(0f, spacing); // Ensure non-negative spacing
    }

    public float GetBlockSpacingY()
    {
        return blockSpacingY;
    }

    public void SetBlockSpacingY(float spacing)
    {
        blockSpacingY = Mathf.Max(0f, spacing); // Ensure non-negative spacing
    }

    public Vector2 GetSpawnAreaOffset()
    {
        return spawnAreaOffset;
    }

    public void SetSpawnAreaOffset(Vector2 offset)
    {
        spawnAreaOffset = offset;
    }

    public GameObject GetBlockPrefab()
    {
        return blockPrefab;
    }

    public void SetBlockPrefab(GameObject prefab)
    {
        blockPrefab = prefab;
        
        // Update block size based on prefab if available
        if (prefab != null)
        {
            UpdateBlockSizeFromPrefab();
        }
    }

    public List<GameObject> GetSpawnedBlocks()
    {
        // Remove any null references (destroyed blocks)
        spawnedBlocks.RemoveAll(block => block == null);
        return new List<GameObject>(spawnedBlocks);
    }

    // Score configuration getters and setters
    public int GetDefaultScoreOverride()
    {
        return defaultScoreOverride;
    }

    public void SetDefaultScoreOverride(int score)
    {
        defaultScoreOverride = score;
    }

    public Sprite[] GetBlockSpriteList()
    {
        return blockSpriteList;
    }

    public void SetBlockSpriteList(Sprite[] sprites)
    {
        blockSpriteList = sprites;
    }


    // Core block spawning functionality
    public void SpawnBlocks()
    {
        // Clear any existing blocks first
        ClearAllBlocks();
        
        // Don't spawn if no prefab is assigned
        if (blockPrefab == null)
        {
            Debug.LogWarning("BlockManager: No block prefab assigned. Cannot spawn blocks.");
            return;
        }
        
        // Update block size from prefab
        UpdateBlockSizeFromPrefab();
        
        // Spawn blocks in grid pattern
        for (int row = 0; row < blockRows; row++)
        {
            for (int col = 0; col < blockColumns; col++)
            {
                Vector2 blockPosition = CalculateBlockPosition(row, col);
                GameObject newBlock = SpawnBlockAtPosition(blockPosition);
                if (newBlock != null)
                {
                    spawnedBlocks.Add(newBlock);
                }
            }
        }
        
        // Notify GameManager about total block count
        NotifyGameManagerOfBlockCount();
        
        Debug.Log($"BlockManager: Spawned {spawnedBlocks.Count} blocks ({blockRows}x{blockColumns} grid)");
    }

    public void ClearAllBlocks()
    {
        // Destroy all spawned blocks
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

    public Vector2 CalculateBlockPosition(int row, int col)
    {
        // Calculate the total grid dimensions using separate X and Y spacing
        float totalGridWidth = (blockColumns - 1) * (blockSize.x + blockSpacingX);
        float totalGridHeight = (blockRows - 1) * (blockSize.y + blockSpacingY);
        
        // Calculate starting position (top-left of grid)
        float startX = (LEFT_WALL_X + RIGHT_WALL_X) * 0.5f - totalGridWidth * 0.5f + spawnAreaOffset.x;
        float startY = TOP_WALL_Y + spawnAreaOffset.y - blockSize.y * 0.5f;
        
        // Calculate individual block position using separate spacing
        float blockX = startX + col * (blockSize.x + blockSpacingX);
        float blockY = startY - row * (blockSize.y + blockSpacingY);
        
        return new Vector2(blockX, blockY);
    }

    // Event handling
    public void RegisterWithGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted += OnGameStarted;
        }
    }

    public void OnGameStarted()
    {
        SpawnBlocks();
    }

    // Helper methods
    private GameObject SpawnBlockAtPosition(Vector2 position)
    {
        if (blockPrefab == null)
            return null;
            
        GameObject newBlock = Instantiate(blockPrefab, position, Quaternion.identity);
        
        // Ensure the block has the correct tag and components
        if (newBlock.CompareTag("Untagged"))
        {
            newBlock.tag = "Block";
        }
        
        // Configure block score based on settings
        Block blockComponent = newBlock.GetComponent<Block>();
        if (blockComponent != null)
        {
            int blockScore = GetBaseScore();
            blockComponent.SetPointValue(blockScore);
            
            // Apply random sprite from the available list
            if (blockSpriteList != null && blockSpriteList.Length > 0)
            {
                blockComponent.SetRandomSpriteFromList(blockSpriteList);
            }
        }
        
        return newBlock;
    }

    private int GetBaseScore()
    {
        // Use override if set, otherwise use GameManager default
        if (defaultScoreOverride > 0)
        {
            return defaultScoreOverride;
        }
        
        // Try to get default from GameManager
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.GetDefaultBlockScore();
        }
        
        // Fallback default
        return 10;
    }

    private void UpdateBlockSizeFromPrefab()
    {
        if (blockPrefab == null)
            return;
            
        // Try to get size from collider
        Collider2D collider = blockPrefab.GetComponent<Collider2D>();
        if (collider != null)
        {
            blockSize = collider.bounds.size;
        }
        else
        {
            // Try to get size from renderer
            Renderer renderer = blockPrefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                blockSize = renderer.bounds.size;
            }
        }
        
        // Ensure minimum size
        blockSize.x = Mathf.Max(0.1f, blockSize.x);
        blockSize.y = Mathf.Max(0.1f, blockSize.y);
    }

    // Cleanup when destroyed
    void OnDestroy()
    {
        // Unregister from GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= OnGameStarted;
        }
        
        // Clear blocks
        ClearAllBlocks();
    }

    // Debug methods for testing and development
    [ContextMenu("Spawn Blocks")]
    public void DebugSpawnBlocks()
    {
        SpawnBlocks();
    }

    [ContextMenu("Clear All Blocks")]
    public void DebugClearAllBlocks()
    {
        ClearAllBlocks();
    }

    [ContextMenu("Log Block Manager Status")]
    public void DebugLogStatus()
    {
        Debug.Log($"BlockManager Status:" +
                  $"\n- Block Rows: {blockRows}" +
                  $"\n- Block Columns: {blockColumns}" +
                  $"\n- Block Spacing: {blockSpacing}" +
                  $"\n- Block Spacing X: {blockSpacingX}" +
                  $"\n- Block Spacing Y: {blockSpacingY}" +
                  $"\n- Spawn Area Offset: {spawnAreaOffset}" +
                  $"\n- Block Prefab: {(blockPrefab != null ? blockPrefab.name : "NULL")}" +
                  $"\n- Block Size: {blockSize}" +
                  $"\n- Spawned Blocks: {GetSpawnedBlocks().Count}" +
                  $"\n- Registered with GameManager: {(GameManager.Instance != null)}");
                  
        if (GetSpawnedBlocks().Count > 0)
        {
            Debug.Log($"First Block Position: {GetSpawnedBlocks()[0].transform.position}");
            Debug.Log($"Last Block Position: {GetSpawnedBlocks()[GetSpawnedBlocks().Count - 1].transform.position}");
        }
    }

    // Level system integration methods
    public void ConfigureForLevel(LevelData levelData)
    {
        if (levelData == null)
            return; // Keep existing configuration if null
            
        // Apply level configuration
        SetBlockRows(levelData.BlockRows);
        SetBlockColumns(levelData.BlockColumns);
        SetBlockSpacing(levelData.BlockSpacing);
        SetBlockSpacingX(levelData.BlockSpacingX);
        SetBlockSpacingY(levelData.BlockSpacingY);
        SetSpawnAreaOffset(levelData.SpawnAreaOffset);
        
        // Set score override from level data
        SetDefaultScoreOverride(levelData.DefaultBlockScore);
        
        // Set sprite list from level data
        SetBlockSpriteList(levelData.BlockSprites);
        
        // Apply score multiplier to GameManager if available
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetScoreMultiplier(levelData.ScoreMultiplier);
        }
    }

    private void NotifyGameManagerOfBlockCount()
    {
        if (GameManager.Instance != null)
        {
            int totalBlocks = spawnedBlocks.Count;
            GameManager.Instance.SetBlocksRemaining(totalBlocks);
        }
    }

    // Integration method for block destruction
    private void OnBlockDestroyed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBlockDestroyed();
        }
    }

}