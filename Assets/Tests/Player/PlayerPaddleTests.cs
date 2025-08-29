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
        
        // Launch ball
        playerPaddle.LaunchAttachedBall();
        
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
}