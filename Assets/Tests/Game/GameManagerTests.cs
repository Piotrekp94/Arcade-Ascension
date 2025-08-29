using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TMPro; // Required for TextMeshProUGUI

public class GameManagerTests
{
    private GameManager gameManager;
    private TextMeshProUGUI scoreText;

    [SetUp]
    public void Setup()
    {
        // Create a new GameObject for GameManager
        GameObject gameManagerGO = new GameObject();
        gameManager = gameManagerGO.AddComponent<GameManager>();
        
        // Set up singleton for testing
        GameManager.SetInstanceForTesting(gameManager);

        // Create mock TextMeshProUGUI components
        GameObject scoreTextGO = new GameObject();
        scoreText = scoreTextGO.AddComponent<TextMeshProUGUI>();
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up singleton
        GameManager.SetInstanceForTesting(null);
        
        // Clean up GameObjects after each test
        if (Application.isPlaying)
        {
            if (gameManager != null && gameManager.gameObject != null) Object.Destroy(gameManager.gameObject);
            if (scoreText != null && scoreText.gameObject != null) Object.Destroy(scoreText.gameObject);
        }
        else
        {
            if (gameManager != null && gameManager.gameObject != null) Object.DestroyImmediate(gameManager.gameObject);
            if (scoreText != null && scoreText.gameObject != null) Object.DestroyImmediate(scoreText.gameObject);
        }
    }

    [Test]
    public void GameManager_InitialScoreIsZero()
    {
        // GameManager's Start method sets score to 0
        Assert.AreEqual(0, gameManager.GetScore());
    }

    [Test]
    public void GameManager_AddScoreIncreasesScore()
    {
        gameManager.AddScore(100);
        Assert.AreEqual(100, gameManager.GetScore());
    }


    [Test]
    public void GameManager_SetGameStateChangesState()
    {
        gameManager.SetGameState(GameManager.GameState.Playing);
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
    }

    [Test]
    public void GameManager_CanHandleBallLoss()
    {
        // Test that GameManager can handle ball loss events
        gameManager.SetGameState(GameManager.GameState.Playing);
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);

        // Simulate ball loss
        gameManager.OnBallLost();

        // Game state should remain Playing (respawn system active)
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
        
        // Respawn timer should now be active
        Assert.IsTrue(gameManager.IsRespawnTimerActive());
    }

    [Test]
    public void GameManager_BallLossFromPlayingStateTriggersRespawn()
    {
        // Test specific respawn behavior when ball is lost during Playing state
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.OnBallLost();
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
        Assert.IsTrue(gameManager.IsRespawnTimerActive());
    }

    [Test]
    public void GameManager_BallLossFromStartStateDoesNothing()
    {
        // Test that ball loss during Start state doesn't trigger game over
        gameManager.SetGameState(GameManager.GameState.Start);
        gameManager.OnBallLost();
        
        // Should remain in Start state (ball loss only matters during gameplay)
        Assert.AreEqual(GameManager.GameState.Start, gameManager.CurrentGameState);
    }

    [Test]
    public void GameManager_CanResetAfterGameOver()
    {
        // Test that game can be reset after game over
        gameManager.SetGameState(GameManager.GameState.GameOver);
        
        // Reset game
        gameManager.ResetGame();
        
        // Should return to Start state with score reset
        Assert.AreEqual(GameManager.GameState.Start, gameManager.CurrentGameState);
        Assert.AreEqual(0, gameManager.GetScore());
    }

    [Test]
    public void GameManager_GameOverTriggersEvent()
    {
        // Test that game over event is triggered when explicitly setting GameOver state
        bool gameOverTriggered = false;
        gameManager.OnGameOver += () => gameOverTriggered = true;
        
        // Manually set to GameOver state (ball loss no longer triggers GameOver)
        gameManager.SetGameState(GameManager.GameState.GameOver);
        
        Assert.IsTrue(gameOverTriggered);
    }

    [Test] 
    public void GameManager_SingletonInstanceIsProperlySet()
    {
        // Test that singleton instance is properly maintained
        Assert.IsNotNull(GameManager.Instance);
        Assert.AreSame(gameManager, GameManager.Instance);
    }

    [Test]
    public void GameManager_HasConfigurableRespawnDelay()
    {
        // Test that GameManager has configurable respawn delay
        float defaultRespawnDelay = gameManager.GetRespawnDelay();
        Assert.Greater(defaultRespawnDelay, 0f); // Should have a positive default value

        // Test setting custom respawn delay
        gameManager.SetRespawnDelay(5.0f);
        Assert.AreEqual(5.0f, gameManager.GetRespawnDelay());
    }

    [Test]
    public void GameManager_CanSpawnBallAtPaddlePosition()
    {
        // Test that GameManager can spawn ball at specified position
        Vector2 spawnPosition = new Vector2(1.0f, -3.0f);
        
        // Mock paddle for position reference
        GameObject mockPaddle = new GameObject("MockPaddle");
        mockPaddle.transform.position = spawnPosition;
        
        GameObject spawnedBall = gameManager.SpawnBallAtPosition(spawnPosition);
        
        Assert.IsNotNull(spawnedBall);
        Assert.AreEqual("Ball", spawnedBall.tag);
        Assert.That(Vector2.Distance(spawnedBall.transform.position, spawnPosition), Is.LessThan(0.1f));
        
        // Cleanup
        if (Application.isPlaying)
        {
            Object.Destroy(spawnedBall);
            Object.Destroy(mockPaddle);
        }
        else
        {
            Object.DestroyImmediate(spawnedBall);
            Object.DestroyImmediate(mockPaddle);
        }
    }

    [UnityTest]
    public IEnumerator GameManager_StartsRespawnTimerOnBallLoss()
    {
        // Test that respawn timer starts when ball is lost
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.SetRespawnDelay(1.0f); // Short delay for testing
        
        Assert.IsFalse(gameManager.IsRespawnTimerActive());
        
        // Trigger ball loss
        gameManager.OnBallLost();
        
        // Timer should now be active
        Assert.IsTrue(gameManager.IsRespawnTimerActive());
        
        // Manually advance the timer since Update() won't be called in tests
        gameManager.UpdateRespawnTimer(1.1f);
        
        // Timer should no longer be active
        Assert.IsFalse(gameManager.IsRespawnTimerActive());
        
        yield return null; // Required for IEnumerator method
    }

    [Test]
    public void GameManager_CanRegisterPaddleForBallSpawning()
    {
        // Test that GameManager can register paddle for ball spawning
        GameObject mockPaddle = new GameObject("MockPaddle");
        
        gameManager.RegisterPaddleForSpawning(mockPaddle.transform);
        Assert.AreEqual(mockPaddle.transform, gameManager.GetRegisteredPaddle());
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(mockPaddle);
        else
            Object.DestroyImmediate(mockPaddle);
    }

    [Test]
    public void GameManager_HasBallPrefabReference()
    {
        // Test that GameManager has a ball prefab reference for spawning
        // Initially might be null, but should have a setter
        gameManager.SetBallPrefab(new GameObject("BallPrefab"));
        Assert.IsNotNull(gameManager.GetBallPrefab());
        
        // Cleanup
        GameObject ballPrefab = gameManager.GetBallPrefab();
        if (Application.isPlaying)
            Object.Destroy(ballPrefab);
        else
            Object.DestroyImmediate(ballPrefab);
    }

    [Test]
    public void GameManager_RespawnOnlyDuringPlayingState()
    {
        // Test that ball respawn only happens during Playing state
        gameManager.SetGameState(GameManager.GameState.Start);
        gameManager.OnBallLost();
        Assert.IsFalse(gameManager.IsRespawnTimerActive());
        
        gameManager.SetGameState(GameManager.GameState.GameOver);
        gameManager.OnBallLost();
        Assert.IsFalse(gameManager.IsRespawnTimerActive());
        
        // Should work in Playing state
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.OnBallLost();
        Assert.IsTrue(gameManager.IsRespawnTimerActive());
    }

    [UnityTest]
    public IEnumerator GameManager_SpawnedBallIsAttachedToPaddle()
    {
        // Test that spawned ball is automatically attached to paddle
        GameObject paddleGO = new GameObject("Paddle");
        PlayerPaddle paddle = paddleGO.AddComponent<PlayerPaddle>();
        gameManager.RegisterPaddleForSpawning(paddleGO.transform);
        
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.SetRespawnDelay(0.1f); // Very short delay for testing
        
        // Trigger ball loss and respawn
        gameManager.OnBallLost();
        yield return new WaitForSeconds(0.1f); // Small delay
        gameManager.UpdateRespawnTimer(0.2f); // Manually advance timer
        
        // Paddle should now have an attached ball
        Assert.IsTrue(paddle.HasAttachedBall());
        Assert.IsNotNull(paddle.GetAttachedBall());
        
        // Cleanup
        if (paddle.HasAttachedBall())
        {
            GameObject attachedBallGO = paddle.GetAttachedBall().gameObject;
            if (Application.isPlaying)
                Object.Destroy(attachedBallGO);
            else
                Object.DestroyImmediate(attachedBallGO);
        }
        
        if (Application.isPlaying)
            Object.Destroy(paddleGO);
        else
            Object.DestroyImmediate(paddleGO);
    }

    [Test]
    public void GameManager_StartGameSpawnsInitialBallAttachedToPaddle()
    {
        // Test that StartGame() spawns an initial ball attached to paddle
        GameObject paddleGO = new GameObject("Paddle");
        PlayerPaddle paddle = paddleGO.AddComponent<PlayerPaddle>();
        gameManager.RegisterPaddleForSpawning(paddleGO.transform);
        
        // Initially no ball should be attached
        Assert.IsFalse(paddle.HasAttachedBall());
        
        // Start game should spawn initial ball
        gameManager.StartGame();
        
        // Ball should now be attached to paddle
        Assert.IsTrue(paddle.HasAttachedBall());
        Assert.IsNotNull(paddle.GetAttachedBall());
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
        
        // Ball should be in attached state
        Ball attachedBall = paddle.GetAttachedBall();
        Assert.IsTrue(attachedBall.IsAttached());
        
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
    public void GameManager_ForceSpawnInitialBallWorksManually()
    {
        // Test manual ball spawning for debugging
        GameObject paddleGO = new GameObject("Paddle");
        PlayerPaddle paddle = paddleGO.AddComponent<PlayerPaddle>();
        gameManager.RegisterPaddleForSpawning(paddleGO.transform);
        
        Assert.IsFalse(paddle.HasAttachedBall());
        
        // Force spawn ball manually
        gameManager.ForceSpawnInitialBall();
        
        Assert.IsTrue(paddle.HasAttachedBall());
        Assert.IsNotNull(paddle.GetAttachedBall());
        
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
}