using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private int hitPoints = 1; // Number of hits required to destroy the block
    [SerializeField]
    private GameObject destructionEffectPrefab; // Particle effect prefab for destruction
    [SerializeField]
    private int pointValue = 10; // Configurable point value for destroying this block

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