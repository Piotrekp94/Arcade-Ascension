using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class BoundarySystemIntegrationTests
{
    private GameManager gameManager;
    private Ball ball;
    private GameObject ballGO;
    private Wall topWall;
    private DeathZone deathZone;
    private GameObject gameManagerGO;

    [SetUp]
    public void Setup()
    {
        // Set up GameManager
        gameManagerGO = new GameObject("GameManager");
        gameManager = gameManagerGO.AddComponent<GameManager>();
        GameManager.SetInstanceForTesting(gameManager);

        // Set up Ball
        ballGO = new GameObject("Ball");
        ballGO.tag = "Ball";
        ballGO.AddComponent<CircleCollider2D>();
        ballGO.AddComponent<Rigidbody2D>();
        ball = ballGO.AddComponent<Ball>();

        // Set up Top Wall
        GameObject topWallGO = new GameObject("TopWall");
        topWallGO.AddComponent<BoxCollider2D>();
        topWall = topWallGO.AddComponent<Wall>();
        topWall.SetWallType(Wall.WallType.Top);

        // Set up Death Zone
        GameObject deathZoneGO = new GameObject("DeathZone");
        BoxCollider2D deathZoneCollider = deathZoneGO.AddComponent<BoxCollider2D>();
        deathZoneCollider.isTrigger = true;
        deathZone = deathZoneGO.AddComponent<DeathZone>();
    }

    [TearDown]
    public void Teardown()
    {
        GameManager.SetInstanceForTesting(null);
        
        if (Application.isPlaying)
        {
            if (ballGO != null) Object.Destroy(ballGO);
            if (topWall != null && topWall.gameObject != null) Object.Destroy(topWall.gameObject);
            if (deathZone != null && deathZone.gameObject != null) Object.Destroy(deathZone.gameObject);
            if (gameManagerGO != null) Object.Destroy(gameManagerGO);
        }
        else
        {
            if (ballGO != null) Object.DestroyImmediate(ballGO);
            if (topWall != null && topWall.gameObject != null) Object.DestroyImmediate(topWall.gameObject);
            if (deathZone != null && deathZone.gameObject != null) Object.DestroyImmediate(deathZone.gameObject);
            if (gameManagerGO != null) Object.DestroyImmediate(gameManagerGO);
        }
    }

    [Test]
    public void BoundarySystem_BallHitsWallTriggersEvent()
    {
        // Integration test: Ball hitting wall triggers wall hit event
        bool wallHitDetected = false;
        ball.OnWallHit += (wallType) => {
            wallHitDetected = true;
            Assert.AreEqual(Wall.WallType.Top, wallType);
        };

        // Simulate ball hitting top wall
        ball.SimulateWallCollision(topWall);
        
        Assert.IsTrue(wallHitDetected);
    }

    [Test]
    public void BoundarySystem_BallEntersDeathZoneTriggersRespawn()
    {
        // Integration test: Ball entering death zone triggers respawn during Playing state
        gameManager.SetGameState(GameManager.GameState.Playing);
        
        // Ball enters death zone
        deathZone.SimulateBallEntry(ballGO);
        
        // Game should remain in Playing state with respawn timer active
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
        Assert.IsTrue(gameManager.IsRespawnTimerActive());
    }

    [Test]
    public void BoundarySystem_DeathZoneConnectsToGameManager()
    {
        // Integration test: DeathZone properly connects to GameManager
        gameManager.SetGameState(GameManager.GameState.Playing);
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);

        // Ball enters death zone during Playing state - should trigger respawn
        deathZone.SimulateBallEntry(ballGO);

        // Verify respawn logic is activated
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
        Assert.IsTrue(gameManager.IsRespawnTimerActive());
    }

    [Test]
    public void BoundarySystem_GameCanBeResetAfterBallLoss()
    {
        // Integration test: Complete game cycle - play, lose ball (respawn), manually trigger GameOver, reset
        
        // Start game
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.AddScore(100); // Player scored some points
        
        // Ball is lost - triggers respawn system
        deathZone.SimulateBallEntry(ballGO);
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
        Assert.IsTrue(gameManager.IsRespawnTimerActive());
        
        // Simulate game over (e.g., multiple lives lost or other condition)
        gameManager.SetGameState(GameManager.GameState.GameOver);
        Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentGameState);
        
        // Reset game
        gameManager.ResetGame();
        Assert.AreEqual(GameManager.GameState.Start, gameManager.CurrentGameState);
        Assert.AreEqual(0, gameManager.GetScore()); // Score should be reset
    }

    [Test]
    public void BoundarySystem_WallsAndDeathZoneWorkWithDifferentWallTypes()
    {
        // Integration test: Different wall types work correctly with ball collision system
        
        // Create left and right walls
        GameObject leftWallGO = new GameObject("LeftWall");
        leftWallGO.AddComponent<BoxCollider2D>();
        Wall leftWall = leftWallGO.AddComponent<Wall>();
        leftWall.SetWallType(Wall.WallType.Left);
        
        GameObject rightWallGO = new GameObject("RightWall");
        rightWallGO.AddComponent<BoxCollider2D>();
        Wall rightWall = rightWallGO.AddComponent<Wall>();
        rightWall.SetWallType(Wall.WallType.Right);

        Wall.WallType lastHitWallType = Wall.WallType.Top;
        ball.OnWallHit += (wallType) => lastHitWallType = wallType;

        // Test different wall collisions
        ball.SimulateWallCollision(topWall);
        Assert.AreEqual(Wall.WallType.Top, lastHitWallType);

        ball.SimulateWallCollision(leftWall);
        Assert.AreEqual(Wall.WallType.Left, lastHitWallType);

        ball.SimulateWallCollision(rightWall);
        Assert.AreEqual(Wall.WallType.Right, lastHitWallType);

        // Cleanup
        if (Application.isPlaying)
        {
            Object.Destroy(leftWallGO);
            Object.Destroy(rightWallGO);
        }
        else
        {
            Object.DestroyImmediate(leftWallGO);
            Object.DestroyImmediate(rightWallGO);
        }
    }

    [Test]
    public void BoundarySystem_CompleteBreakoutGameplayFlow()
    {
        // Integration test: Complete Breakout gameplay flow
        
        // Game starts
        Assert.AreEqual(GameManager.GameState.Start, gameManager.CurrentGameState);
        
        // Game begins
        gameManager.StartGame();
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
        
        // Ball bounces off walls (this should not end the game)
        ball.SimulateWallCollision(topWall);
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
        
        // Player scores points (simulated)
        gameManager.AddScore(50);
        Assert.AreEqual(50, gameManager.GetScore());
        
        // Ball is lost (enters death zone) - triggers respawn
        deathZone.SimulateBallEntry(ballGO);
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
        Assert.IsTrue(gameManager.IsRespawnTimerActive());
        
        // Simulate eventual game over condition
        gameManager.SetGameState(GameManager.GameState.GameOver);
        Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentGameState);
        
        // Game can be reset for another round
        gameManager.ResetGame();
        Assert.AreEqual(GameManager.GameState.Start, gameManager.CurrentGameState);
        Assert.AreEqual(0, gameManager.GetScore());
    }

    [UnityTest]
    public IEnumerator BoundarySystem_CompleteBallRespawnAndLaunchCycle()
    {
        // Integration test: Complete ball respawn and launch cycle
        
        // Set up paddle
        GameObject paddleGO = new GameObject("Paddle");
        PlayerPaddle paddle = paddleGO.AddComponent<PlayerPaddle>();
        gameManager.RegisterPaddleForSpawning(paddleGO.transform);
        
        // Start game
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.SetRespawnDelay(0.1f); // Short delay for testing
        
        // Ball is lost - create separate ball for this test
        GameObject testBallForRespawn = new GameObject("TestBallForRespawn");
        testBallForRespawn.tag = "Ball";
        deathZone.SimulateBallEntry(testBallForRespawn);
        Assert.IsTrue(gameManager.IsRespawnTimerActive());
        
        // Wait for respawn - manually trigger timer since Update() won't be called in tests
        yield return new WaitForSeconds(0.1f); // Small delay to let the timer start
        gameManager.UpdateRespawnTimer(0.2f); // Manually advance timer past respawn delay
        
        // Ball should be attached to paddle
        Assert.IsTrue(paddle.HasAttachedBall());
        Ball respawnedBall = paddle.GetAttachedBall();
        Assert.IsNotNull(respawnedBall);
        Assert.IsTrue(respawnedBall.IsAttached());
        
        // Launch ball from paddle using deterministic method for predictable testing
        paddle.SimulateLeftClickDeterministic();
        
        // Ball should no longer be attached and should be moving
        Assert.IsFalse(paddle.HasAttachedBall());
        Assert.IsFalse(respawnedBall.IsAttached());
        
        Rigidbody2D ballRb = respawnedBall.GetComponent<Rigidbody2D>();
        Assert.IsNotNull(ballRb);
        Assert.Greater(ballRb.linearVelocity.magnitude, 0f);
        Assert.Greater(ballRb.linearVelocity.y, 0f); // Should be moving upward (deterministic launch)
        
        // Cleanup
        if (Application.isPlaying)
        {
            if (respawnedBall != null && respawnedBall.gameObject != null) Object.Destroy(respawnedBall.gameObject);
            if (paddleGO != null) Object.Destroy(paddleGO);
        }
        else
        {
            if (respawnedBall != null && respawnedBall.gameObject != null) Object.DestroyImmediate(respawnedBall.gameObject);
            if (paddleGO != null) Object.DestroyImmediate(paddleGO);
        }
    }

    [UnityTest] 
    public IEnumerator BoundarySystem_BallRespawnOnlyDuringPlayingState()
    {
        // Integration test: Ball respawn only works during Playing state
        
        GameObject paddleGO = new GameObject("Paddle");
        PlayerPaddle paddle = paddleGO.AddComponent<PlayerPaddle>();
        gameManager.RegisterPaddleForSpawning(paddleGO.transform);
        gameManager.SetRespawnDelay(0.1f);
        
        // Test in Start state - no respawn
        gameManager.SetGameState(GameManager.GameState.Start);
        
        // Create separate ball for this test phase
        GameObject testBall1 = new GameObject("TestBall1");
        testBall1.tag = "Ball";
        deathZone.SimulateBallEntry(testBall1);
        
        // Manually trigger timer update to ensure no respawn happens
        gameManager.UpdateRespawnTimer(0.2f);
        yield return new WaitForSeconds(0.1f);
        Assert.IsFalse(gameManager.IsRespawnTimerActive());
        Assert.IsFalse(paddle.HasAttachedBall());
        
        // Test in GameOver state - no respawn
        gameManager.SetGameState(GameManager.GameState.GameOver);
        
        // Create separate ball for this test phase  
        GameObject testBall2 = new GameObject("TestBall2");
        testBall2.tag = "Ball";
        deathZone.SimulateBallEntry(testBall2);
        
        // Manually trigger timer update to ensure no respawn happens
        gameManager.UpdateRespawnTimer(0.2f);
        yield return new WaitForSeconds(0.1f);
        Assert.IsFalse(gameManager.IsRespawnTimerActive());
        Assert.IsFalse(paddle.HasAttachedBall());
        
        // Test in Playing state - should respawn
        gameManager.SetGameState(GameManager.GameState.Playing);
        
        // Create separate ball for this test phase
        GameObject testBall3 = new GameObject("TestBall3");
        testBall3.tag = "Ball";
        deathZone.SimulateBallEntry(testBall3);
        
        // Manually trigger the respawn timer since Update() won't be called in tests
        yield return new WaitForSeconds(0.1f); // Small delay to let the timer start
        gameManager.UpdateRespawnTimer(0.2f); // Manually advance timer past respawn delay
        
        Assert.IsTrue(paddle.HasAttachedBall());
        
        // Cleanup
        if (paddle.HasAttachedBall())
        {
            GameObject attachedBallGO = paddle.GetAttachedBall().gameObject;
            if (attachedBallGO != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(attachedBallGO);
                else
                    Object.DestroyImmediate(attachedBallGO);
            }
        }
        
        if (paddleGO != null)
        {
            if (Application.isPlaying)
                Object.Destroy(paddleGO);
            else
                Object.DestroyImmediate(paddleGO);
        }
    }

    [Test]
    public void BoundarySystem_AttachedBallFollowsPaddleMovement()
    {
        // Integration test: Attached ball follows paddle movement
        
        GameObject paddleGO = new GameObject("Paddle");
        paddleGO.transform.position = Vector2.zero;
        PlayerPaddle paddle = paddleGO.AddComponent<PlayerPaddle>();
        
        // Create and attach ball
        GameObject testBallGO = new GameObject("TestBall");
        Ball testBall = testBallGO.AddComponent<Ball>();
        paddle.AttachBall(testBall);
        
        Assert.IsTrue(paddle.HasAttachedBall());
        Assert.IsTrue(testBall.IsAttached());
        
        // Move paddle
        Vector2 newPaddlePos = new Vector2(3.0f, -2.0f);
        paddleGO.transform.position = newPaddlePos;
        
        // Update ball position (simulates paddle Update method)
        paddle.UpdateAttachedBallPosition();
        
        // Ball should follow paddle
        Vector2 expectedBallPos = newPaddlePos + Vector2.up * paddle.GetAttachmentOffset();
        Assert.That(Vector2.Distance(testBall.transform.position, expectedBallPos), Is.LessThan(0.1f));
        
        // Cleanup
        if (Application.isPlaying)
        {
            if (testBallGO != null) Object.Destroy(testBallGO);
            if (paddleGO != null) Object.Destroy(paddleGO);
        }
        else
        {
            if (testBallGO != null) Object.DestroyImmediate(testBallGO);
            if (paddleGO != null) Object.DestroyImmediate(paddleGO);
        }
    }

    [Test]
    public void BoundarySystem_PaddleWallCollisionDetection()
    {
        // Integration test: Paddle collision detection with left and right walls
        
        // Create paddle
        GameObject paddleGO = new GameObject("Paddle");
        paddleGO.AddComponent<BoxCollider2D>();
        PlayerPaddle paddle = paddleGO.AddComponent<PlayerPaddle>();
        paddleGO.transform.position = Vector2.zero;
        
        // Create left and right walls
        GameObject leftWallGO = new GameObject("LeftWall");
        leftWallGO.tag = "Wall";
        leftWallGO.layer = LayerMask.NameToLayer("Default");
        leftWallGO.AddComponent<BoxCollider2D>();
        leftWallGO.transform.position = new Vector2(-3.5f, 0f);
        
        GameObject rightWallGO = new GameObject("RightWall");
        rightWallGO.tag = "Wall";
        rightWallGO.layer = LayerMask.NameToLayer("Default");
        rightWallGO.AddComponent<BoxCollider2D>();
        rightWallGO.transform.position = new Vector2(3.5f, 0f);
        
        // Test configuration
        Assert.IsTrue(paddle.GetUseWallCollision());
        Assert.Greater(paddle.GetPaddleHalfWidth(), 0f);
        
        // Test wall collision fallback configuration
        paddle.SetUseWallCollision(false);
        Assert.IsFalse(paddle.GetUseWallCollision());
        
        paddle.SetUseWallCollision(true);
        Assert.IsTrue(paddle.GetUseWallCollision());
        
        // Cleanup
        if (Application.isPlaying)
        {
            Object.Destroy(paddleGO);
            Object.Destroy(leftWallGO);
            Object.Destroy(rightWallGO);
        }
        else
        {
            Object.DestroyImmediate(paddleGO);
            Object.DestroyImmediate(leftWallGO);
            Object.DestroyImmediate(rightWallGO);
        }
    }
}