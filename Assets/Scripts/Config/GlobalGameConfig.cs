using UnityEngine;

[CreateAssetMenu(fileName = "GlobalGameConfig", menuName = "Arcade Ascension/Global Game Config", order = 0)]
public class GlobalGameConfig : ScriptableObject
{
    [Header("Timer Configuration")]
    [SerializeField]
    [Tooltip("Global time limit applied to all levels (in seconds)")]
    private float globalTimeLimit = 120f; // Default 2 minutes for all levels
    
    // Public properties for read-only access
    public float GlobalTimeLimit => globalTimeLimit;
    
    // Validation method
    public bool IsValid()
    {
        return globalTimeLimit > 0f;
    }
    
    // Editor-only validation
    void OnValidate()
    {
        if (globalTimeLimit <= 0f) globalTimeLimit = 120f;
    }
    
    // Static instance for easy access
    private static GlobalGameConfig _instance;
    public static GlobalGameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GlobalGameConfig>("GlobalGameConfig");
                if (_instance == null)
                {
                    Debug.LogWarning("GlobalGameConfig not found in Resources folder. Creating default instance.");
                    _instance = CreateInstance<GlobalGameConfig>();
                }
            }
            return _instance;
        }
    }
    
    // Method to set instance for testing
    public static void SetInstanceForTesting(GlobalGameConfig testInstance)
    {
        _instance = testInstance;
    }
}