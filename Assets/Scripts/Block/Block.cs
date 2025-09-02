using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private int hitPoints = 1; // Number of hits required to destroy the block
    [SerializeField]
    private GameObject destructionEffectPrefab; // Particle effect prefab for destruction
    [SerializeField]
    private int pointValue = 10; // Configurable point value for destroying this block
    [SerializeField]
    private Sprite[] availableSprites; // List of sprites for random selection

    // Public getter for testing
    public int HitPoints { get { return hitPoints; } }
    
    // Public getter for point value
    public int PointValue { get { return pointValue; } }
    
    // Public setter for testing (in production, this could be removed or made internal)
    public void SetHitPoints(int newHitPoints)
    {
        hitPoints = Mathf.Max(0, newHitPoints); // Ensure hit points never go below 0
    }
    
    // Public setter for point value with validation
    public void SetPointValue(int newPointValue)
    {
        pointValue = Mathf.Max(0, newPointValue); // Ensure point value never goes below 0
    }

    // Set a random sprite from a provided list
    public void SetRandomSpriteFromList(Sprite[] spriteList)
    {
        if (spriteList == null || spriteList.Length == 0)
        {
            // Handle gracefully - don't change sprite if list is invalid
            return;
        }

        // Get or add SpriteRenderer component
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Select random sprite from the list
        int randomIndex = Random.Range(0, spriteList.Length);
        spriteRenderer.sprite = spriteList[randomIndex];
        
        // Store the available sprites for potential future use
        availableSprites = spriteList;
    }

    // Get the currently available sprite list
    public Sprite[] GetAvailableSprites()
    {
        return availableSprites;
    }

    public void TakeHit()
    {
        if (hitPoints > 0)
        {
            hitPoints--;
            if (hitPoints <= 0)
            {
                DestroyBlock();
            }
        }
    }

    void DestroyBlock()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(pointValue); // Add configurable points for destroying a block
            GameManager.Instance.OnBlockDestroyed(); // Notify GameManager for level completion tracking
        }

        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        }
        // Play sound here later
        if (Application.isPlaying)
        {
            Destroy(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }
}