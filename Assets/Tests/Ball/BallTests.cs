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
        ballGO.tag = "Ball"; // Ensure ball is properly tagged
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

    [Test]
    public void Ball_HasBallTag()
    {
        // Test that ball is properly tagged for detection by DeathZone
        Assert.AreEqual("Ball", ballGO.tag);
    }

    [Test]
    public void Ball_CanDetectWallCollisions()
    {
        // Test that ball can differentiate between wall types
        GameObject wallGO = new GameObject();
        Wall wall = wallGO.AddComponent<Wall>();
        wall.SetWallType(Wall.WallType.Top);
        
        bool wallHitDetected = false;
        ball.OnWallHit += (wallType) => {
            wallHitDetected = true;
            Assert.AreEqual(Wall.WallType.Top, wallType);
        };

        // Simulate wall collision
        ball.SimulateWallCollision(wall);
        
        Assert.IsTrue(wallHitDetected);

        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(wallGO);
        else
            Object.DestroyImmediate(wallGO);
    }

    [Test]
    public void Ball_OnWallHitEventFiresForDifferentWallTypes()
    {
        // Test that the event fires correctly for different wall types
        GameObject topWallGO = new GameObject();
        Wall topWall = topWallGO.AddComponent<Wall>();
        topWall.SetWallType(Wall.WallType.Top);

        GameObject leftWallGO = new GameObject();
        Wall leftWall = leftWallGO.AddComponent<Wall>();
        leftWall.SetWallType(Wall.WallType.Left);

        Wall.WallType detectedWallType = Wall.WallType.Top;
        ball.OnWallHit += (wallType) => detectedWallType = wallType;

        // Test top wall
        ball.SimulateWallCollision(topWall);
        Assert.AreEqual(Wall.WallType.Top, detectedWallType);

        // Test left wall
        ball.SimulateWallCollision(leftWall);
        Assert.AreEqual(Wall.WallType.Left, detectedWallType);

        // Cleanup
        if (Application.isPlaying)
        {
            Object.Destroy(topWallGO);
            Object.Destroy(leftWallGO);
        }
        else
        {
            Object.DestroyImmediate(topWallGO);
            Object.DestroyImmediate(leftWallGO);
        }
    }

    [Test]
    public void Ball_OnlyReactsToWallComponents()
    {
        // Test that ball only fires wall hit events for objects with Wall component
        GameObject nonWallGO = new GameObject();
        nonWallGO.AddComponent<BoxCollider2D>(); // Has collider but no Wall component

        bool wallHitDetected = false;
        ball.OnWallHit += (wallType) => wallHitDetected = true;

        // Simulate collision with non-wall object
        ball.SimulateNonWallCollision(nonWallGO);
        
        Assert.IsFalse(wallHitDetected);

        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(nonWallGO);
        else
            Object.DestroyImmediate(nonWallGO);
    }

    [Test]
    public void Ball_MaintainsSpeedAfterWallCollision()
    {
        // Test that ball maintains its speed after bouncing off walls
        float initialSpeed = ball.InitialSpeed;
        ball.LaunchBall();
        
        GameObject wallGO = new GameObject();
        Wall wall = wallGO.AddComponent<Wall>();
        
        // Simulate wall collision
        ball.SimulateWallCollision(wall);
        
        // Speed should remain consistent (Unity physics will handle direction change)
        Assert.That(rb.linearVelocity.magnitude, Is.EqualTo(initialSpeed).Within(0.1f));

        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(wallGO);
        else
            Object.DestroyImmediate(wallGO);
    }
}