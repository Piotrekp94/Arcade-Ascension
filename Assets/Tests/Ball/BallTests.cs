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

    [UnityTest]
    public IEnumerator Ball_StartMethodLaunchesBall()
    {
        // Test the full Unity lifecycle by calling Start manually
        // In real gameplay, Start is called automatically by Unity
        ball.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
        
        // Wait a frame for any physics updates
        yield return null; 

        // Check if velocity is non-zero after Start calls LaunchBall
        Assert.AreNotEqual(Vector2.zero, rb.linearVelocity);
        // Check if speed is approximately 5f (the fixed initial speed)
        Assert.That(rb.linearVelocity.magnitude, Is.EqualTo(5f).Within(0.1f));
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