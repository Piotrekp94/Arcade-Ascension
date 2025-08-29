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
        
        Debug.Log($"DeathZone Awake: Collider isTrigger = {deathZoneCollider.isTrigger}, GameObject active = {gameObject.activeInHierarchy}");
    }

    void Start()
    {
        // Additional validation in Start to catch any issues
        if (deathZoneCollider == null)
        {
            Debug.LogError("DeathZone: Collider is null in Start!");
            deathZoneCollider = GetComponent<BoxCollider2D>();
        }
        
        if (deathZoneCollider != null && !deathZoneCollider.isTrigger)
        {
            Debug.LogWarning("DeathZone: Collider is not set as trigger in Start! Fixing...");
            deathZoneCollider.isTrigger = true;
        }
        
        Debug.Log($"DeathZone Start: Collider isTrigger = {deathZoneCollider?.isTrigger}, GameObject active = {gameObject.activeInHierarchy}");
    }

    void OnEnable()
    {
        Debug.Log("DeathZone OnEnable called");
    }

    void OnDisable()
    {
        Debug.Log("DeathZone OnDisable called - this might explain why it stops working!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"DeathZone OnTriggerEnter2D: {other.gameObject.name} with tag '{other.gameObject.tag}'");
        HandleBallEntry(other.gameObject);
    }

    public void SimulateBallEntry(GameObject obj)
    {
        HandleBallEntry(obj);
    }

    private void HandleBallEntry(GameObject obj)
    {
        Debug.Log($"DeathZone HandleBallEntry: {obj.name} with tag '{obj.tag}'");
        
        // Check if the object is a ball
        if (obj.CompareTag("Ball"))
        {
            Debug.Log("DeathZone: Ball detected! Triggering game over...");
            
            // Trigger ball lost event
            OnBallLost?.Invoke();
            
            // Set game state to game over if GameManager exists
            if (GameManager.Instance != null)
            {
                Debug.Log($"DeathZone: Setting game state to GameOver. Current state: {GameManager.Instance.CurrentGameState}");
                GameManager.Instance.SetGameState(GameManager.GameState.GameOver);
            }
            else
            {
                Debug.LogError("DeathZone: GameManager.Instance is null!");
            }
            
            // Destroy the ball
            ballDestroyed = true;
            Debug.Log("DeathZone: Destroying ball...");
            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }
        else
        {
            Debug.Log($"DeathZone: Object '{obj.name}' with tag '{obj.tag}' is not a ball, ignoring.");
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
        
        Debug.Log($"DeathZone EnsureProperInitialization: Collider isTrigger = {deathZoneCollider.isTrigger}");
    }

    // Diagnostic method to check DeathZone status
    public void LogStatus()
    {
        Debug.Log($"DeathZone Status: GameObject active = {gameObject.activeInHierarchy}, " +
                  $"Component enabled = {enabled}, " +
                  $"Collider exists = {deathZoneCollider != null}, " +
                  $"Collider isTrigger = {deathZoneCollider?.isTrigger}, " +
                  $"Collider enabled = {deathZoneCollider?.enabled}");
    }
}