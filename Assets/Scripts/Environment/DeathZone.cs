using UnityEngine;
using System;

public class DeathZone : MonoBehaviour
{
    public event Action OnBallLost;
    private bool ballDestroyed = false;

    void Awake()
    {
        // Ensure DeathZone has a BoxCollider2D component set as trigger
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;
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
            
            // Set game state to game over if GameManager exists
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameState(GameManager.GameState.GameOver);
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
}