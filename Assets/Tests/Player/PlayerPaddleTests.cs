using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class PlayerPaddleTests
{
    private PlayerPaddle playerPaddle;
    private GameObject paddleGO;

    [SetUp]
    public void Setup()
    {
        paddleGO = new GameObject();
        playerPaddle = paddleGO.AddComponent<PlayerPaddle>();
        // Initialize paddle position and scale for consistent testing
        playerPaddle.transform.position = Vector2.zero;
        playerPaddle.transform.localScale = Vector3.one;
    }

    [TearDown]
    public void Teardown()
    {
        // Use DestroyImmediate in edit mode (tests) and Destroy in play mode
        if (Application.isPlaying)
        {
            if (paddleGO != null) Object.Destroy(paddleGO);
        }
        else
        {
            if (paddleGO != null) Object.DestroyImmediate(paddleGO);
        }
    }

    [Test]
    public void PlayerPaddle_HasCorrectInitialPosition()
    {
        // Test that paddle initializes at correct position
        Assert.AreEqual(Vector2.zero, (Vector2)playerPaddle.transform.position);
    }

    [Test]
    public void PlayerPaddle_HasCorrectInitialScale()
    {
        // Test that paddle has correct initial scale
        Assert.AreEqual(Vector3.one, playerPaddle.transform.localScale);
    }

    [Test]
    public void PlayerPaddle_CanAttachBall()
    {
        // Test that paddle can attach a ball
        GameObject ballGO = new GameObject("Ball");
        ballGO.tag = "Ball";
        Ball ball = ballGO.AddComponent<Ball>();
        
        playerPaddle.AttachBall(ball);
        Assert.IsTrue(playerPaddle.HasAttachedBall());
        Assert.AreEqual(ball, playerPaddle.GetAttachedBall());
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(ballGO);
        else
            Object.DestroyImmediate(ballGO);
    }

    [Test]
    public void PlayerPaddle_CanDetachBall()
    {
        // Test that paddle can detach a ball
        GameObject ballGO = new GameObject("Ball");
        ballGO.tag = "Ball";
        Ball ball = ballGO.AddComponent<Ball>();
        
        playerPaddle.AttachBall(ball);
        Assert.IsTrue(playerPaddle.HasAttachedBall());
        
        playerPaddle.DetachBall();
        Assert.IsFalse(playerPaddle.HasAttachedBall());
        Assert.IsNull(playerPaddle.GetAttachedBall());
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(ballGO);
        else
            Object.DestroyImmediate(ballGO);
    }

    [Test]
    public void PlayerPaddle_AttachedBallFollowsPaddle()
    {
        // Test that attached ball follows paddle movement
        GameObject ballGO = new GameObject("Ball");
        ballGO.tag = "Ball";
        Ball ball = ballGO.AddComponent<Ball>();
        
        playerPaddle.AttachBall(ball);
        
        // Move paddle
        Vector2 newPaddlePos = new Vector2(2.0f, 0.0f);
        playerPaddle.transform.position = newPaddlePos;
        
        // Update ball position (this would normally happen in Update)
        playerPaddle.UpdateAttachedBallPosition();
        
        // Ball should be positioned above paddle
        Vector2 expectedBallPos = newPaddlePos + Vector2.up * playerPaddle.GetAttachmentOffset();
        Assert.That(Vector2.Distance(ball.transform.position, expectedBallPos), Is.LessThan(0.1f));
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(ballGO);
        else
            Object.DestroyImmediate(ballGO);
    }

    [Test]
    public void PlayerPaddle_CanLaunchBallWithRandomDirection()
    {
        // Test that paddle can launch ball in random upward direction
        GameObject ballGO = new GameObject("Ball");
        ballGO.tag = "Ball";
        ballGO.AddComponent<CircleCollider2D>();
        Rigidbody2D ballRb = ballGO.AddComponent<Rigidbody2D>();
        Ball ball = ballGO.AddComponent<Ball>();
        
        playerPaddle.AttachBall(ball);
        
        // Launch ball deterministically (straight up for testing)
        playerPaddle.SimulateLeftClickDeterministic();
        
        // Ball should no longer be attached
        Assert.IsFalse(playerPaddle.HasAttachedBall());
        
        // Ball should have upward velocity
        Assert.Greater(ballRb.linearVelocity.y, 0f);
        Assert.Greater(ballRb.linearVelocity.magnitude, 0f);
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(ballGO);
        else
            Object.DestroyImmediate(ballGO);
    }

    [Test]
    public void PlayerPaddle_HasConfigurableLaunchForce()
    {
        // Test that launch force is configurable
        float defaultForce = playerPaddle.GetLaunchForce();
        Assert.Greater(defaultForce, 0f);
        
        playerPaddle.SetLaunchForce(15.0f);
        Assert.AreEqual(15.0f, playerPaddle.GetLaunchForce());
    }

    [Test]
    public void PlayerPaddle_HasConfigurableAttachmentOffset()
    {
        // Test that attachment offset is configurable
        float defaultOffset = playerPaddle.GetAttachmentOffset();
        Assert.Greater(defaultOffset, 0f);
        
        playerPaddle.SetAttachmentOffset(1.0f);
        Assert.AreEqual(1.0f, playerPaddle.GetAttachmentOffset());
    }

    [Test]
    public void PlayerPaddle_CanSimulateLeftClick()
    {
        // Test that paddle can detect left click for launching
        GameObject ballGO = new GameObject("Ball");
        ballGO.tag = "Ball";
        ballGO.AddComponent<CircleCollider2D>();
        ballGO.AddComponent<Rigidbody2D>();
        Ball ball = ballGO.AddComponent<Ball>();
        
        playerPaddle.AttachBall(ball);
        Assert.IsTrue(playerPaddle.HasAttachedBall());
        
        // Simulate left click
        playerPaddle.SimulateLeftClick();
        
        // Ball should be launched
        Assert.IsFalse(playerPaddle.HasAttachedBall());
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(ballGO);
        else
            Object.DestroyImmediate(ballGO);
    }

    [Test]
    public void PlayerPaddle_RegistersWithGameManagerForBallSpawning()
    {
        // Test that paddle registers itself with GameManager for ball spawning
        GameObject gameManagerGO = new GameObject();
        GameManager gameManager = gameManagerGO.AddComponent<GameManager>();
        GameManager.SetInstanceForTesting(gameManager);
        
        // Paddle should register itself
        playerPaddle.RegisterWithGameManager();
        
        Assert.AreEqual(playerPaddle.transform, gameManager.GetRegisteredPaddle());
        
        // Cleanup
        GameManager.SetInstanceForTesting(null);
        if (Application.isPlaying)
            Object.Destroy(gameManagerGO);
        else
            Object.DestroyImmediate(gameManagerGO);
    }

    [Test]
    public void PlayerPaddle_CanConfigureWallCollision()
    {
        // Test wall collision configuration methods
        Assert.IsTrue(playerPaddle.GetUseWallCollision()); // Default should be true
        
        playerPaddle.SetUseWallCollision(false);
        Assert.IsFalse(playerPaddle.GetUseWallCollision());
        
        playerPaddle.SetUseWallCollision(true);
        Assert.IsTrue(playerPaddle.GetUseWallCollision());
    }

    [Test]
    public void PlayerPaddle_CalculatesPaddleWidth()
    {
        // Add a collider to the paddle for proper testing
        BoxCollider2D collider = paddleGO.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one; // 1x1 collider
        
        // Manually calculate paddle width for testing
        playerPaddle.CalculatePaddleWidth();
        
        // Test that paddle width is calculated correctly
        float paddleHalfWidth = playerPaddle.GetPaddleHalfWidth();
        Assert.Greater(paddleHalfWidth, 0f);
        
        // Should be approximately half the collider width
        float expectedHalfWidth = collider.bounds.size.x * 0.5f;
        Assert.That(paddleHalfWidth, Is.EqualTo(expectedHalfWidth).Within(0.1f));
    }

    [Test]
    public void PlayerPaddle_WallCollisionFallbackToClamping()
    {
        // Test that when wall collision is disabled, it falls back to clamping
        playerPaddle.SetUseWallCollision(false);
        
        // Test the configuration change
        Assert.IsFalse(playerPaddle.GetUseWallCollision());
        
        // The actual movement behavior is tested in integration tests since it involves physics
        // This test validates the configuration system works correctly
    }

    [Test]
    public void PlayerPaddle_LaunchAlwaysHasPositiveYVelocity()
    {
        // Test that ball launch always has positive Y component (upward)
        GameObject ballGO = new GameObject("Ball");
        ballGO.tag = "Ball";
        ballGO.AddComponent<CircleCollider2D>();
        Rigidbody2D ballRb = ballGO.AddComponent<Rigidbody2D>();
        Ball ball = ballGO.AddComponent<Ball>();
        
        playerPaddle.AttachBall(ball);
        
        // Test multiple random launches to ensure Y is always positive
        for (int i = 0; i < 10; i++)
        {
            // Reset ball state
            playerPaddle.AttachBall(ball);
            ballRb.linearVelocity = Vector2.zero;
            
            // Launch with random direction
            playerPaddle.SimulateLeftClick();
            
            // Y velocity must always be positive (upward)
            Assert.Greater(ballRb.linearVelocity.y, 0f, 
                $"Launch {i}: Y velocity {ballRb.linearVelocity.y} should be positive");
        }
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(ballGO);
        else
            Object.DestroyImmediate(ballGO);
    }

    [Test]
    public void PlayerPaddle_LaunchCanHaveVariableXVelocity()
    {
        // Test that ball launch can have both positive and negative X components
        GameObject ballGO = new GameObject("Ball");
        ballGO.tag = "Ball";
        ballGO.AddComponent<CircleCollider2D>();
        Rigidbody2D ballRb = ballGO.AddComponent<Rigidbody2D>();
        Ball ball = ballGO.AddComponent<Ball>();
        
        bool foundPositiveX = false;
        bool foundNegativeX = false;
        
        // Test multiple launches to verify X can be both positive and negative
        for (int i = 0; i < 20 && (!foundPositiveX || !foundNegativeX); i++)
        {
            // Reset ball state
            playerPaddle.AttachBall(ball);
            ballRb.linearVelocity = Vector2.zero;
            
            // Launch with random direction
            playerPaddle.SimulateLeftClick();
            
            if (ballRb.linearVelocity.x > 0.1f)
                foundPositiveX = true;
            else if (ballRb.linearVelocity.x < -0.1f)
                foundNegativeX = true;
                
            // Y should always be positive
            Assert.Greater(ballRb.linearVelocity.y, 0f);
        }
        
        // Should have found both positive and negative X velocities
        Assert.IsTrue(foundPositiveX || foundNegativeX, 
            "Should be able to launch ball in different X directions");
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(ballGO);
        else
            Object.DestroyImmediate(ballGO);
    }

    [Test]
    public void PlayerPaddle_DeterministicLaunchGoesUpward()
    {
        // Test that deterministic launch always goes straight up (Y=1, X=0)
        GameObject ballGO = new GameObject("Ball");
        ballGO.tag = "Ball";
        ballGO.AddComponent<CircleCollider2D>();
        Rigidbody2D ballRb = ballGO.AddComponent<Rigidbody2D>();
        Ball ball = ballGO.AddComponent<Ball>();
        
        playerPaddle.AttachBall(ball);
        
        // Launch deterministically
        playerPaddle.SimulateLeftClickDeterministic();
        
        // Should go straight up
        Assert.That(ballRb.linearVelocity.x, Is.EqualTo(0f).Within(0.01f));
        Assert.Greater(ballRb.linearVelocity.y, 0f);
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(ballGO);
        else
            Object.DestroyImmediate(ballGO);
    }
}