using UnityEngine;
using System;

public class Ball : MonoBehaviour
{
    [SerializeField]
    private float _initialSpeed = 5f;
    private Rigidbody2D rb;
    
    // Events for boundary interactions
    public event Action<Wall.WallType> OnWallHit;
    
    // Public getter for testing
    public float InitialSpeed { get { return _initialSpeed; } }

    void Awake()
    {
        // Ensure ball is properly tagged
        if (gameObject.tag != "Ball")
        {
            gameObject.tag = "Ball";
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        LaunchBall();
    }

    public void LaunchBall()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
            
        if (rb != null)
        {
            float x = Random.Range(0, 2) == 0 ? -1 : 1;
            float y = Random.Range(0, 2) == 0 ? -1 : 1;
            rb.linearVelocity = new Vector2(x, y).normalized * _initialSpeed;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject other)
    {
        // Check if collision is with a wall
        Wall wall = other.GetComponent<Wall>();
        if (wall != null)
        {
            // Fire wall hit event
            OnWallHit?.Invoke(wall.GetWallType());
        }
        
        // Handle other collision types (blocks, paddle) here
    }

    // Testing methods
    public void SimulateWallCollision(Wall wall)
    {
        HandleCollision(wall.gameObject);
    }

    public void SimulateNonWallCollision(GameObject other)
    {
        HandleCollision(other);
    }
}