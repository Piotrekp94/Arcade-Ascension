using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private int hitPoints = 1; // Number of hits required to destroy the block
    [SerializeField]
    private GameObject destructionEffectPrefab; // Particle effect prefab for destruction

    public void TakeHit()
    {
        hitPoints--;
        if (hitPoints <= 0)
        {
            DestroyBlock();
        }
    }

    void DestroyBlock()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(10); // Add 10 points for destroying a block
            GameManager.Instance.AddCurrency(5); // Add 5 currency for destroying a block
        }

        if (destructionEffectPrefab != null)
        {
            Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
        }
        // Play sound here later
        Destroy(gameObject);
    }
}