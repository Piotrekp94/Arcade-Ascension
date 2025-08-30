using UnityEngine;
using System;

public class Ball : MonoBehaviour
{
    [SerializeField]
    private float _initialSpeed = 5f;
    private Rigidbody2D rb;
    
    // Events for boundary interactions
    public event Action<Wall.WallType> OnWallHit;
    public event Action OnPaddleHit;
    
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
            return;
        }
        
        // Check if collision is with a paddle
        PlayerPaddle paddle = other.GetComponent<PlayerPaddle>();
        if (paddle != null)
        {
            HandlePaddleCollision(paddle);
            return;
        }
        
        // Handle other collision types (blocks) here
    }

    private void HandlePaddleCollision(PlayerPaddle paddle)
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
            
        if (rb != null)
        {
            // Calculate bounce direction - ball should bounce upward from paddle
            Vector2 currentVelocity = rb.linearVelocity;
            
            // Simple paddle bounce - reverse Y direction and maintain speed
            Vector2 bounceDirection = new Vector2(currentVelocity.x, Mathf.Abs(currentVelocity.y));
            rb.linearVelocity = bounceDirection.normalized * _initialSpeed;
        }
        
        // Fire paddle hit event
        OnPaddleHit?.Invoke();
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

    public void SimulatePaddleCollision(GameObject paddle)
    {
        HandleCollision(paddle);
    }

    // Debug method to test paddle collision behavior
    [ContextMenu("Test Paddle Collision")]
    public void TestPaddleCollision()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
            
        Debug.Log("Ball Paddle Collision Test:");
        Debug.Log($"- Current Velocity: ({rb.linearVelocity.x:F2}, {rb.linearVelocity.y:F2})");
        Debug.Log($"- Is Attached: {isAttached}");
        
        // Simulate a downward velocity hitting paddle
        Vector2 testVelocity = new Vector2(2.0f, -3.0f);
        rb.linearVelocity = testVelocity;
        
        Debug.Log($"- Test Velocity (downward): ({testVelocity.x:F2}, {testVelocity.y:F2})");
        
        // Create a test paddle for collision
        GameObject testPaddleGO = new GameObject("TestPaddle");
        PlayerPaddle testPaddle = testPaddleGO.AddComponent<PlayerPaddle>();
        
        HandlePaddleCollision(testPaddle);
        
        Debug.Log($"- After Paddle Hit: ({rb.linearVelocity.x:F2}, {rb.linearVelocity.y:F2})");
        Debug.Log($"- Y Now Positive: {rb.linearVelocity.y > 0}");
        
        // Cleanup
        if (Application.isPlaying)
            UnityEngine.Object.Destroy(testPaddleGO);
        else
            UnityEngine.Object.DestroyImmediate(testPaddleGO);
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
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            else
            {
                // When detached, restore normal physics
                rb.bodyType = RigidbodyType2D.Dynamic;
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