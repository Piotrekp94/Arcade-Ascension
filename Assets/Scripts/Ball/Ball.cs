using UnityEngine;
using System;

public class Ball : MonoBehaviour
{
    [SerializeField]
    private float _initialSpeed = 5f;
    private Rigidbody2D rb;
    
    // Events for boundary interactions
    public event Action<Wall.WallType> OnWallHit;
    
    // Attachment state management
    private bool isAttached = false;
    
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
        // Don't launch if ball is attached to paddle
        if (isAttached)
            return;
            
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
            
        if (rb != null)
        {
            float x = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
            float y = UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
            rb.linearVelocity = new Vector2(x, y).normalized * _initialSpeed;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject other)
    {
        // Don't handle collisions when attached to paddle
        if (isAttached)
            return;
            
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

    // Attachment state management methods
    public bool IsAttached()
    {
        return isAttached;
    }

    public void SetAttachedState(bool attached)
    {
        isAttached = attached;
        
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
            
        if (rb != null)
        {
            if (attached)
            {
                // When attached, stop movement and make kinematic
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.isKinematic = true;
            }
            else
            {
                // When detached, restore normal physics
                rb.isKinematic = false;
            }
        }
    }

    public void LaunchFromPaddle(Vector2 direction, float force)
    {
        // Only launch if ball is currently attached
        if (!isAttached)
            return;
            
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
            
        if (rb != null)
        {
            // Detach ball first
            SetAttachedState(false);
            
            // Apply launch velocity
            rb.linearVelocity = direction.normalized * force;
        }
    }
}