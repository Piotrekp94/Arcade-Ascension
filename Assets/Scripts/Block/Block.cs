using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private int hitPoints = 1; // Number of hits required to destroy the block
    [SerializeField]
    private GameObject destructionEffectPrefab; // Particle effect prefab for destruction

    // Public getter for testing
    public int HitPoints { get { return hitPoints; } }
    
    // Public setter for testing (in production, this could be removed or made internal)
    public void SetHitPoints(int newHitPoints)
    {
        hitPoints = Mathf.Max(0, newHitPoints); // Ensure hit points never go below 0
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
            GameManager.Instance.AddScore(10); // Add 10 points for destroying a block
        }

        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        }
        // Play sound here later
        Destroy(gameObject);
    }
}