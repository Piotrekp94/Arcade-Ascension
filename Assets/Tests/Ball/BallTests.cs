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

    [UnityTest]
    public IEnumerator Ball_LaunchBallSetsVelocity()
    {
        // Wait a frame for Start to be called
        yield return null; 

        // Check if velocity is non-zero after launch
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
}