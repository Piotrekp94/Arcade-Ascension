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
    private Vector2 spawnAreaOffset = new Vector2(0f, -1f);
    
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
    public Vector2 SpawnAreaOffset => spawnAreaOffset;
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
        copy.spawnAreaOffset = this.spawnAreaOffset;
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
        if (scoreMultiplier <= 0f) scoreMultiplier = 1.0f;
        if (defaultBlockScore < 0) defaultBlockScore = 0;
    }
}