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
    public void BoundarySystem_BallEntersDeathZoneTriggersGameOver()
    {
        // Integration test: Ball entering death zone triggers game over
        gameManager.SetGameState(GameManager.GameState.Playing);
        
        // Ball enters death zone
        deathZone.SimulateBallEntry(ballGO);
        
        // Game should be over
        Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentGameState);
    }

    [Test]
    public void BoundarySystem_DeathZoneConnectsToGameManager()
    {
        // Integration test: DeathZone properly connects to GameManager
        gameManager.SetGameState(GameManager.GameState.Playing);
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);

        bool gameOverEventTriggered = false;
        gameManager.OnGameOver += () => gameOverEventTriggered = true;

        // Ball enters death zone
        deathZone.SimulateBallEntry(ballGO);

        // Verify complete chain reaction
        Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentGameState);
        Assert.IsTrue(gameOverEventTriggered);
    }

    [Test]
    public void BoundarySystem_GameCanBeResetAfterBallLoss()
    {
        // Integration test: Complete game cycle - play, lose ball, reset
        
        // Start game
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.AddScore(100); // Player scored some points
        
        // Ball is lost
        deathZone.SimulateBallEntry(ballGO);
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
        
        // Ball is lost (enters death zone)
        deathZone.SimulateBallEntry(ballGO);
        Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentGameState);
        
        // Game can be reset for another round
        gameManager.ResetGame();
        Assert.AreEqual(GameManager.GameState.Start, gameManager.CurrentGameState);
        Assert.AreEqual(0, gameManager.GetScore());
    }
}