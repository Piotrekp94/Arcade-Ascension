using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPaddle : MonoBehaviour
{
    [SerializeField]
    private float _speed = 5f;
    [SerializeField]
    private float minX = -7.5f; // Fallback boundary if walls not detected
    [SerializeField]
    private float maxX = 7.5f;  // Fallback boundary if walls not detected
    
    // Wall collision detection
    private bool useWallCollision = true;
    private float paddleHalfWidth;
    
    // Ball attachment and launching
    [SerializeField]
    private float launchForce = 10.0f; // Force applied when launching ball
    [SerializeField]
    private float attachmentOffset = 0.5f; // Distance above paddle for attached ball
    [SerializeField]
    private float launchAngleVariance = 30.0f; // Random variance in launch angle (degrees)
    
    private Ball attachedBall;
    private bool hasBallAttached = false;

    void Start()
    {
        // Calculate paddle half-width for collision detection
        CalculatePaddleWidth();
        
        // Register with GameManager for ball spawning
        RegisterWithGameManager();
    }
    
    private void CalculatePaddleWidth()
    {
        // Get paddle's collider to determine its width
        Collider2D paddleCollider = GetComponent<Collider2D>();
        if (paddleCollider != null)
        {
            paddleHalfWidth = paddleCollider.bounds.size.x * 0.5f;
        }
        else
        {
            // Fallback if no collider
            paddleHalfWidth = transform.localScale.x * 0.5f;
        }
    }

    void Update()
    {
        HandleMovement();
        HandleBallAttachment();
        HandleInput();
    }

    private void HandleMovement()
    {
        // Read input from the new Input System
        float horizontalInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                horizontalInput = -1f;
            }
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                horizontalInput = 1f;
            }
        }

        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = currentPosition;
        targetPosition.x += horizontalInput * _speed * Time.deltaTime;
        
        // Use wall collision detection if enabled, otherwise use clamping
        if (useWallCollision)
        {
            targetPosition = HandleWallCollisions(currentPosition, targetPosition, horizontalInput);
        }
        else
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        }
        
        transform.position = targetPosition;
    }
    
    private Vector2 HandleWallCollisions(Vector2 currentPos, Vector2 targetPos, float inputDirection)
    {
        // Only check for walls if paddle is trying to move
        if (Mathf.Abs(inputDirection) < 0.01f)
            return targetPos;
            
        // Calculate paddle edges
        float leftEdge = targetPos.x - paddleHalfWidth;
        float rightEdge = targetPos.x + paddleHalfWidth;
        
        // Check for wall collisions using raycasting
        if (inputDirection < 0) // Moving left
        {
            if (CheckWallCollision(currentPos, Vector2.left, paddleHalfWidth + 0.1f))
            {
                // Find exact wall position and stop at it
                float wallX = FindWallPosition(currentPos, Vector2.left);
                if (wallX != float.MaxValue)
                {
                    targetPos.x = Mathf.Max(targetPos.x, wallX + paddleHalfWidth + 0.05f);
                }
            }
        }
        else if (inputDirection > 0) // Moving right
        {
            if (CheckWallCollision(currentPos, Vector2.right, paddleHalfWidth + 0.1f))
            {
                // Find exact wall position and stop at it
                float wallX = FindWallPosition(currentPos, Vector2.right);
                if (wallX != float.MaxValue)
                {
                    targetPos.x = Mathf.Min(targetPos.x, wallX - paddleHalfWidth - 0.05f);
                }
            }
        }
        
        // Fallback to clamping if wall detection fails
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        
        return targetPos;
    }
    
    private bool CheckWallCollision(Vector2 position, Vector2 direction, float distance)
    {
        // Cast a ray from paddle center in the movement direction
        // Use layer 6 (Walls) and Default layer for wall detection
        int layerMask = (1 << 0) | (1 << 6); // Default (0) + Walls (6)
        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, layerMask);
        return hit.collider != null && hit.collider.CompareTag("Wall");
    }
    
    private float FindWallPosition(Vector2 position, Vector2 direction)
    {
        // Use layer 6 (Walls) and Default layer for wall detection
        int layerMask = (1 << 0) | (1 << 6); // Default (0) + Walls (6)
        RaycastHit2D hit = Physics2D.Raycast(position, direction, 10f, layerMask);
        if (hit.collider != null && hit.collider.CompareTag("Wall"))
        {
            return hit.point.x;
        }
        return float.MaxValue; // No wall found
    }

    private void HandleBallAttachment()
    {
        if (hasBallAttached && attachedBall != null)
        {
            UpdateAttachedBallPosition();
        }
    }

    private void HandleInput()
    {
        // Check for left click to launch ball
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (hasBallAttached)
            {
                LaunchAttachedBall();
            }
        }
    }

    // Ball attachment methods
    public void AttachBall(Ball ball)
    {
        attachedBall = ball;
        hasBallAttached = true;
        
        if (ball != null)
        {
            // Set ball to attached state
            ball.SetAttachedState(true);
            UpdateAttachedBallPosition();
        }
    }

    public void DetachBall()
    {
        if (attachedBall != null)
        {
            attachedBall.SetAttachedState(false);
        }
        
        attachedBall = null;
        hasBallAttached = false;
    }

    public bool HasAttachedBall()
    {
        return hasBallAttached && attachedBall != null;
    }

    public Ball GetAttachedBall()
    {
        return attachedBall;
    }

    public void UpdateAttachedBallPosition()
    {
        if (hasBallAttached && attachedBall != null)
        {
            Vector2 ballPosition = (Vector2)transform.position + Vector2.up * attachmentOffset;
            attachedBall.transform.position = ballPosition;
        }
    }

    public void LaunchAttachedBall()
    {
        if (hasBallAttached && attachedBall != null)
        {
            // Calculate launch direction with random variance
            float baseAngle = 90f; // Straight up
            float randomVariance = UnityEngine.Random.Range(-launchAngleVariance, launchAngleVariance);
            float launchAngle = baseAngle + randomVariance;
            
            Vector2 launchDirection = new Vector2(
                Mathf.Sin(launchAngle * Mathf.Deg2Rad),
                Mathf.Cos(launchAngle * Mathf.Deg2Rad)
            ).normalized;
            
            // Launch the ball
            attachedBall.LaunchFromPaddle(launchDirection, launchForce);
            
            // Detach ball
            DetachBall();
        }
    }

    // Configuration methods
    public float GetLaunchForce()
    {
        return launchForce;
    }

    public void SetLaunchForce(float force)
    {
        launchForce = force;
    }

    public float GetAttachmentOffset()
    {
        return attachmentOffset;
    }

    public void SetAttachmentOffset(float offset)
    {
        attachmentOffset = offset;
    }
    
    // Wall collision configuration methods
    public void SetUseWallCollision(bool useCollision)
    {
        useWallCollision = useCollision;
    }
    
    public bool GetUseWallCollision()
    {
        return useWallCollision;
    }
    
    public float GetPaddleHalfWidth()
    {
        return paddleHalfWidth;
    }
    
    // Debug method to test wall collision detection
    [ContextMenu("Test Wall Collision Detection")]
    public void TestWallCollisionDetection()
    {
        Debug.Log($"Paddle Wall Collision Test:");
        Debug.Log($"- Use Wall Collision: {useWallCollision}");
        Debug.Log($"- Paddle Half Width: {paddleHalfWidth}");
        Debug.Log($"- Current Position: {transform.position}");
        
        // Test left wall detection
        bool leftWallFound = CheckWallCollision(transform.position, Vector2.left, 5f);
        Debug.Log($"- Left Wall Detected: {leftWallFound}");
        
        // Test right wall detection  
        bool rightWallFound = CheckWallCollision(transform.position, Vector2.right, 5f);
        Debug.Log($"- Right Wall Detected: {rightWallFound}");
        
        if (leftWallFound)
        {
            float leftWallX = FindWallPosition(transform.position, Vector2.left);
            Debug.Log($"- Left Wall Position: {leftWallX}");
        }
        
        if (rightWallFound)
        {
            float rightWallX = FindWallPosition(transform.position, Vector2.right);
            Debug.Log($"- Right Wall Position: {rightWallX}");
        }
    }

    // Testing methods
    public void SimulateLeftClick()
    {
        if (hasBallAttached)
        {
            LaunchAttachedBall();
        }
    }
    
    public void SimulateLeftClickDeterministic()
    {
        if (hasBallAttached && attachedBall != null)
        {
            // Launch straight up for predictable testing
            Vector2 launchDirection = Vector2.up;
            attachedBall.LaunchFromPaddle(launchDirection, launchForce);
            DetachBall();
        }
    }

    public void RegisterWithGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPaddleForSpawning(transform);
        }
    }
}