using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Arcade Ascension/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    [Header("Level Identity")]
    [SerializeField]
    private int levelId;
    [SerializeField]
    private string levelName;
    [SerializeField, TextArea(2, 3)]
    private string levelDescription;
    
    [Header("Block Configuration")]
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
    private Vector2 spawnAreaOffset = new Vector2(0f, -1f);
    [SerializeField]
    private Sprite[] blockSprites; // Array of sprites for random block appearance
    
    [Header("Scoring Configuration")]
    [SerializeField]
    private float scoreMultiplier = 1.0f;
    [SerializeField]
    private int defaultBlockScore = 10;
    
    // Public properties for read-only access
    public int LevelId => levelId;
    public string LevelName => levelName;
    public string LevelDescription => levelDescription;
    public int BlockRows => blockRows;
    public int BlockColumns => blockColumns;
    public float BlockSpacing => blockSpacing;
    public float BlockSpacingX => blockSpacingX;
    public float BlockSpacingY => blockSpacingY;
    public Vector2 SpawnAreaOffset => spawnAreaOffset;
    public Sprite[] BlockSprites => blockSprites;
    public float ScoreMultiplier => scoreMultiplier;
    public int DefaultBlockScore => defaultBlockScore;
    
    // Validation method
    public bool IsValid()
    {
        return levelId > 0 &&
               !string.IsNullOrEmpty(levelName) &&
               blockRows > 0 &&
               blockColumns > 0 &&
               blockSpacing >= 0f &&
               blockSpacingX >= 0f &&
               blockSpacingY >= 0f &&
               scoreMultiplier > 0f &&
               defaultBlockScore >= 0;
    }
    
    // Method to create a copy of this level data with modifications
    public LevelData CreateCopy()
    {
        LevelData copy = CreateInstance<LevelData>();
        copy.levelId = this.levelId;
        copy.levelName = this.levelName;
        copy.levelDescription = this.levelDescription;
        copy.blockRows = this.blockRows;
        copy.blockColumns = this.blockColumns;
        copy.blockSpacing = this.blockSpacing;
        copy.blockSpacingX = this.blockSpacingX;
        copy.blockSpacingY = this.blockSpacingY;
        copy.spawnAreaOffset = this.spawnAreaOffset;
        copy.blockSprites = this.blockSprites; // Copy sprite array reference
        copy.scoreMultiplier = this.scoreMultiplier;
        copy.defaultBlockScore = this.defaultBlockScore;
        return copy;
    }
    
    // Editor-only validation
    void OnValidate()
    {
        if (levelId < 1) levelId = 1;
        if (blockRows < 1) blockRows = 1;
        if (blockColumns < 1) blockColumns = 1;
        if (blockSpacing < 0f) blockSpacing = 0f;
        if (blockSpacingX < 0f) blockSpacingX = 0f;
        if (blockSpacingY < 0f) blockSpacingY = 0f;
        if (scoreMultiplier <= 0f) scoreMultiplier = 1.0f;
        if (defaultBlockScore < 0) defaultBlockScore = 0;
    }
}