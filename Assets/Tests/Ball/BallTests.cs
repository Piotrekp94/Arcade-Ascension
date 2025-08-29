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
        Object.Destroy(ballGO);
    }

    [UnityTest]
    public IEnumerator Ball_LaunchBallSetsVelocity()
    {
        // Wait a frame for Start to be called
        yield return null; 

        // Check if velocity is non-zero after launch
        Assert.AreNotEqual(Vector2.zero, rb.linearVelocity);
        // Check if speed is approximately initialSpeed
        Assert.That(rb.linearVelocity.magnitude, Is.EqualTo(ball.initialSpeed).Within(0.1f));
    }

    [Test]
    public void Ball_SetSpeedChangesSpeed()
    {
        // Manually call Start for isolated test
        ball.SendMessage("Start"); 
        float newSpeed = 10f;
        ball.SetSpeed(newSpeed);
        Assert.That(rb.linearVelocity.magnitude, Is.EqualTo(newSpeed).Within(0.1f));
    }
}