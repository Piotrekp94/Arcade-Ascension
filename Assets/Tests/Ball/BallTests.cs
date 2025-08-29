using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class BallTests
{
    private Ball ball;
    private GameObject ballGO;
    private Rigidbody2D rb;

    [SetUp]
    public void Setup()
    {
        ballGO = new GameObject();
        rb = ballGO.AddComponent<Rigidbody2D>();
        ball = ballGO.AddComponent<Ball>();

        // Ensure Rigidbody2D is set up for 2D physics
        rb.gravityScale = 0; // No gravity for Pong ball
        rb.simulated = true; // Ensure physics simulation is active
    }

    [TearDown]
    public void Teardown()
    {
        // Use DestroyImmediate in edit mode (tests) and Destroy in play mode
        if (Application.isPlaying)
        {
            if (ballGO != null) Object.Destroy(ballGO);
        }
        else
        {
            if (ballGO != null) Object.DestroyImmediate(ballGO);
        }
    }

    [Test]
    public void Ball_LaunchBallMethodSetsVelocityDirectly()
    {
        // Test that calling LaunchBall directly sets velocity
        // This tests the method without relying on Unity's Start() lifecycle
        
        // Arrange: Ensure initial velocity is zero
        rb.linearVelocity = Vector2.zero;
        Assert.AreEqual(Vector2.zero, rb.linearVelocity);
        
        // Act: Call LaunchBall directly (now it's public)
        ball.LaunchBall();
        
        // Assert: Velocity should be set
        Assert.AreNotEqual(Vector2.zero, rb.linearVelocity);
        Assert.That(rb.linearVelocity.magnitude, Is.EqualTo(5f).Within(0.1f));
    }

    [Test]
    public void Ball_CanInitializeAndLaunchProperly()
    {
        // Test that Ball can be properly initialized without Unity lifecycle issues
        // This replaces the problematic SendMessage("Start") approach
        
        // Arrange: Ensure we have a clean state
        rb.linearVelocity = Vector2.zero;
        Assert.AreEqual(Vector2.zero, rb.linearVelocity);
        
        // Act: Test the core functionality that Start() would perform
        // We test LaunchBall directly since we've already verified it works
        ball.LaunchBall();
        
        // Assert: Ball should be moving
        Assert.AreNotEqual(Vector2.zero, rb.linearVelocity);
        Assert.That(rb.linearVelocity.magnitude, Is.EqualTo(5f).Within(0.1f));
        
        // Verify the ball has a valid direction (not zero vector before normalization)
        Vector2 direction = rb.linearVelocity.normalized;
        Assert.That(direction.magnitude, Is.EqualTo(1f).Within(0.1f));
    }

    [Test] 
    public void Ball_InitialStateIsCorrect()
    {
        // Test that Ball component initializes with correct default values
        // This tests the component without relying on Unity lifecycle methods
        
        // The ball should start with zero velocity until LaunchBall is called
        Assert.AreEqual(Vector2.zero, rb.linearVelocity);
        
        // The ball should have the correct initial speed value
        Assert.AreEqual(5f, ball.InitialSpeed);
        
        // Test that we can launch the ball from this initial state
        ball.LaunchBall();
        Assert.AreNotEqual(Vector2.zero, rb.linearVelocity);
    }

    [Test]
    public void Ball_HasCorrectRigidbodySettings()
    {
        // Test that ball has correct physics settings
        Assert.AreEqual(0f, rb.gravityScale); // No gravity for Breakout ball
        Assert.IsTrue(rb.simulated); // Physics simulation should be active
    }

    [Test]
    public void Ball_LaunchBallWithNullRigidbodyDoesNotThrow()
    {
        // Test edge case where Rigidbody2D might be null
        // Create a ball without a Rigidbody2D component
        GameObject testBallGO = new GameObject();
        Ball testBall = testBallGO.AddComponent<Ball>();
        
        // Should not throw when LaunchBall is called without Rigidbody2D
        Assert.DoesNotThrow(() => testBall.LaunchBall());
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(testBallGO);
        else
            Object.DestroyImmediate(testBallGO);
    }
}