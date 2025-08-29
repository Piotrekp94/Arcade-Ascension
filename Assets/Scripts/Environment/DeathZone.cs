using UnityEngine;
using System;

public class DeathZone : MonoBehaviour
{
    public event Action OnBallLost;
    private bool ballDestroyed = false;
    private BoxCollider2D deathZoneCollider;

    void Awake()
    {
        // Ensure DeathZone has a BoxCollider2D component set as trigger
        deathZoneCollider = GetComponent<BoxCollider2D>();
        if (deathZoneCollider == null)
        {
            deathZoneCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        deathZoneCollider.isTrigger = true;
    }

    void Start()
    {
        // Additional validation in Start to catch any issues
        if (deathZoneCollider == null)
        {
            deathZoneCollider = GetComponent<BoxCollider2D>();
        }
        
        if (deathZoneCollider != null && !deathZoneCollider.isTrigger)
        {
            deathZoneCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleBallEntry(other.gameObject);
    }

    public void SimulateBallEntry(GameObject obj)
    {
        HandleBallEntry(obj);
    }

    private void HandleBallEntry(GameObject obj)
    {
        // Check if the object is a ball
        if (obj.CompareTag("Ball"))
        {
            // Trigger ball lost event
            OnBallLost?.Invoke();
            
            // Notify GameManager of ball loss (triggers respawn or game over based on game state)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBallLost();
            }
            
            // Destroy the ball
            ballDestroyed = true;
            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }
    }

    public bool WasBallDestroyed()
    {
        return ballDestroyed;
    }

    public void ResetDeathZone()
    {
        ballDestroyed = false;
    }

    // Public method to ensure proper initialization for testing
    public void EnsureProperInitialization()
    {
        if (deathZoneCollider == null)
        {
            deathZoneCollider = GetComponent<BoxCollider2D>();
        }
        if (deathZoneCollider == null)
        {
            deathZoneCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        deathZoneCollider.isTrigger = true;
    }
}