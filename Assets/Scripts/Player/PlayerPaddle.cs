using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPaddle : MonoBehaviour
{
    [SerializeField]
    private float _speed = 5f;
    [SerializeField]
    private float minX = -7.5f; // Example boundary, adjust as needed
    [SerializeField]
    private float maxX = 7.5f;  // Example boundary, adjust as needed
    
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
        // Register with GameManager for ball spawning
        RegisterWithGameManager();
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

        Vector2 position = transform.position;
        position.x += horizontalInput * _speed * Time.deltaTime;
        position.x = Mathf.Clamp(position.x, minX, maxX); // Clamp position within boundaries
        transform.position = position;
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

    // Testing methods
    public void SimulateLeftClick()
    {
        if (hasBallAttached)
        {
            LaunchAttachedBall();
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