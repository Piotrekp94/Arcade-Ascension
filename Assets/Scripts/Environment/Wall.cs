using UnityEngine;

public class Wall : MonoBehaviour
{
    public enum WallType
    {
        Top,
        Left,
        Right
    }

    [SerializeField]
    private WallType wallType = WallType.Top;

    void Awake()
    {
        // Ensure Wall has a BoxCollider2D component
        if (GetComponent<BoxCollider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    public void SetWallType(WallType type)
    {
        wallType = type;
    }

    public WallType GetWallType()
    {
        return wallType;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Wall collision handling - Unity physics handles the actual bouncing
        // We could add sound effects or particle effects here later
        
        // For now, just ensure the collision is handled without errors
        // The bouncing behavior is achieved through Physics2D materials and proper collider setup
    }
}