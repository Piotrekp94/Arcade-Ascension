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

    // NEW TDD TESTS FOR SCORE MULTIPLIERS (Phase 2 - Red)

    [Test]
    public void GameManager_HasDefaultScoreMultiplier()
    {
        // Test that GameManager has a default score multiplier of 1.0
        Assert.AreEqual(1.0f, gameManager.GetScoreMultiplier());
    }

    [Test]
    public void GameManager_SetScoreMultiplier_UpdatesCorrectly()
    {
        // Test that score multiplier can be set to different values
        gameManager.SetScoreMultiplier(2.0f);
        Assert.AreEqual(2.0f, gameManager.GetScoreMultiplier());
        
        gameManager.SetScoreMultiplier(0.5f);
        Assert.AreEqual(0.5f, gameManager.GetScoreMultiplier());
        
        gameManager.SetScoreMultiplier(1.5f);
        Assert.AreEqual(1.5f, gameManager.GetScoreMultiplier());
    }

    [Test]
    public void GameManager_SetScoreMultiplier_RejectsNegativeValues()
    {
        // Test that negative multipliers are rejected and clamped to 0
        gameManager.SetScoreMultiplier(-1.0f);
        Assert.AreEqual(0.0f, gameManager.GetScoreMultiplier());
        
        gameManager.SetScoreMultiplier(-2.5f);
        Assert.AreEqual(0.0f, gameManager.GetScoreMultiplier());
    }

    [Test]
    public void GameManager_AddScoreWithMultiplier_AppliesCorrectly()
    {
        // Test that score multiplier is applied when adding scores
        gameManager.SetScoreMultiplier(2.0f);
        
        gameManager.AddScore(10);
        Assert.AreEqual(20, gameManager.GetScore()); // 10 * 2.0 = 20
        
        gameManager.AddScore(5);
        Assert.AreEqual(30, gameManager.GetScore()); // 20 + (5 * 2.0) = 30
    }

    [Test]
    public void GameManager_AddScoreWithFractionalMultiplier_RoundsCorrectly()
    {
        // Test that fractional multipliers are handled correctly (rounded to nearest int)
        gameManager.SetScoreMultiplier(1.5f);
        
        gameManager.AddScore(10);
        Assert.AreEqual(15, gameManager.GetScore()); // 10 * 1.5 = 15
        
        gameManager.AddScore(3);
        Assert.AreEqual(19, gameManager.GetScore()); // 15 + round(3 * 1.5) = 15 + 4 = 19
    }

    [Test] 
    public void GameManager_AddScoreWithZeroMultiplier_AddsNothing()
    {
        // Test that zero multiplier results in no score addition
        gameManager.SetScoreMultiplier(0.0f);
        
        gameManager.AddScore(100);
        Assert.AreEqual(0, gameManager.GetScore()); // 100 * 0.0 = 0
    }

    [UnityTest]
    public IEnumerator GameManager_ScoreMultiplier_AffectsBlockDestruction()
    {
        // Test that score multiplier affects points gained from block destruction
        gameManager.SetScoreMultiplier(3.0f);
        
        // Create and destroy a block to test integration
        GameObject blockGO = new GameObject("TestBlock");
        Block block = blockGO.AddComponent<Block>();
        block.SetPointValue(10); // Block worth 10 points
        
        int initialScore = gameManager.GetScore();
        
        // Destroy block (should add 10 * 3.0 = 30 points)
        block.TakeHit();
        yield return null; // Wait for destruction
        
        int finalScore = gameManager.GetScore();
        Assert.AreEqual(initialScore + 30, finalScore);
    }

    [Test]
    public void GameManager_HasDefaultBlockScore()
    {
        // Test that GameManager has a default block score for new blocks
        Assert.AreEqual(10, gameManager.GetDefaultBlockScore());
    }

    [Test]
    public void GameManager_SetDefaultBlockScore_UpdatesCorrectly()
    {
        // Test that default block score can be configured
        gameManager.SetDefaultBlockScore(25);
        Assert.AreEqual(25, gameManager.GetDefaultBlockScore());
        
        gameManager.SetDefaultBlockScore(5);
        Assert.AreEqual(5, gameManager.GetDefaultBlockScore());
    }

    [Test]
    public void GameManager_SetDefaultBlockScore_RejectsNegativeValues()
    {
        // Test that negative default scores are rejected
        gameManager.SetDefaultBlockScore(-10);
        Assert.AreEqual(0, gameManager.GetDefaultBlockScore());
    }

    // ENHANCED SCORING FEATURES TESTS (Phase 4)

    [Test]
    public void GameManager_ScoreEvents_TriggeredOnScoreChange()
    {
        // Test that score events are triggered when score changes
        int scoreChangedCallCount = 0;
        int scoreAddedCallCount = 0;
        int lastScore = 0;
        int lastAmountAdded = 0;
        int lastNewTotal = 0;
        
        gameManager.OnScoreChanged += (newScore) => {
            scoreChangedCallCount++;
            lastScore = newScore;
        };
        
        gameManager.OnScoreAdded += (amount, newTotal) => {
            scoreAddedCallCount++;
            lastAmountAdded = amount;
            lastNewTotal = newTotal;
        };
        
        gameManager.AddScore(50);
        
        Assert.AreEqual(1, scoreChangedCallCount);
        Assert.AreEqual(1, scoreAddedCallCount);
        Assert.AreEqual(50, lastScore);
        Assert.AreEqual(50, lastAmountAdded);
        Assert.AreEqual(50, lastNewTotal);
    }

    [Test]
    public void GameManager_ScoreMultiplierWithDifficulty_AppliesCorrectly()
    {
        // Test that difficulty affects score multiplier
        gameManager.SetDifficultyMultiplier(2.0f);
        gameManager.SetScoreMultiplierWithDifficulty(1.5f);
        
        // Should be 1.5 * 2.0 = 3.0
        Assert.AreEqual(3.0f, gameManager.GetScoreMultiplier());
        
        gameManager.AddScore(10);
        Assert.AreEqual(30, gameManager.GetScore()); // 10 * 3.0 = 30
    }

    [Test]
    public void GameManager_MultipleScoreAdditions_TriggerMultipleEvents()
    {
        // Test that multiple score additions trigger events correctly
        int eventCount = 0;
        int totalScoreFromEvents = 0;
        
        gameManager.OnScoreAdded += (amount, newTotal) => {
            eventCount++;
            totalScoreFromEvents = newTotal;
        };
        
        gameManager.AddScore(10);
        gameManager.AddScore(20);
        gameManager.AddScore(30);
        
        Assert.AreEqual(3, eventCount);
        Assert.AreEqual(60, totalScoreFromEvents);
        Assert.AreEqual(60, gameManager.GetScore());
    }

    // LEVEL COMPLETION INTEGRATION TESTS

    [Test]
    public void GameManager_HasOnLevelCompletedEvent()
    {
        // Test that GameManager has level completion event by subscribing to it
        bool canSubscribeToEvent = false;
        
        try
        {
            gameManager.OnLevelCompleted += () => { };
            canSubscribeToEvent = true;
            gameManager.OnLevelCompleted -= () => { };
        }
        catch
        {
            canSubscribeToEvent = false;
        }
        
        Assert.IsTrue(canSubscribeToEvent, "Should be able to subscribe to OnLevelCompleted event");
    }

    [Test] 
    public void GameManager_CheckLevelCompletion_ReturnsFalseWithRemainingBlocks()
    {
        // Test that level completion check returns false when blocks remain
        Assert.IsFalse(gameManager.CheckLevelCompletion());
    }

    [Test]
    public void GameManager_CheckLevelCompletion_ReturnsTrueWithNoBlocks()
    {
        // Test level completion when no blocks remain
        // This would be integration tested with BlockManager
        gameManager.SetBlocksRemainingForTesting(0);
        Assert.IsTrue(gameManager.CheckLevelCompletion());
    }

    [Test]
    public void GameManager_OnAllBlocksDestroyed_TriggersLevelCompleted()
    {
        // Test that destroying all blocks triggers level completion
        bool levelCompletedTriggered = false;
        gameManager.OnLevelCompleted += () => levelCompletedTriggered = true;
        
        // Set game state to Playing (required for level completion)
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.OnAllBlocksDestroyed();
        
        Assert.IsTrue(levelCompletedTriggered);
        // Verify game state transitions to Start to allow level selection UI
        Assert.AreEqual(GameManager.GameState.Start, gameManager.CurrentGameState);
    }

    [Test]
    public void GameManager_OnAllBlocksDestroyed_DoesNotTriggerInWrongState()
    {
        // Test that level completion only triggers during Playing state
        bool levelCompletedTriggered = false;
        gameManager.OnLevelCompleted += () => levelCompletedTriggered = true;
        
        gameManager.SetGameState(GameManager.GameState.Start);
        gameManager.OnAllBlocksDestroyed();
        
        Assert.IsFalse(levelCompletedTriggered);
    }

    [Test]
    public void GameManager_HasLevelIntegrationMethods()
    {
        // Test that GameManager has methods for level system integration
        // These methods should be available for BlockManager integration
        
        // Should be able to set blocks remaining for testing
        Assert.DoesNotThrow(() => gameManager.SetBlocksRemainingForTesting(5));
        
        // Should be able to notify about block destruction
        Assert.DoesNotThrow(() => gameManager.OnBlockDestroyed());
        
        // Should be able to check completion status
        Assert.DoesNotThrow(() => gameManager.CheckLevelCompletion());
    }

    [Test]
    public void GameManager_OnBlockDestroyed_DecreasesBlockCount()
    {
        // Test that block destruction decreases remaining block count
        gameManager.SetBlocksRemainingForTesting(3);
        
        gameManager.OnBlockDestroyed();
        Assert.IsFalse(gameManager.CheckLevelCompletion()); // 2 blocks remain
        
        gameManager.OnBlockDestroyed();
        Assert.IsFalse(gameManager.CheckLevelCompletion()); // 1 block remains
        
        gameManager.OnBlockDestroyed();
        Assert.IsTrue(gameManager.CheckLevelCompletion()); // 0 blocks remain
    }

    [Test]
    public void GameManager_OnBlockDestroyed_TriggersLevelCompletionWhenLastBlock()
    {
        // Test that destroying the last block automatically triggers level completion
        bool levelCompletedTriggered = false;
        gameManager.OnLevelCompleted += () => levelCompletedTriggered = true;
        
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.SetBlocksRemainingForTesting(1);
        
        gameManager.OnBlockDestroyed();
        
        Assert.IsTrue(levelCompletedTriggered);
    }

    // TIMER SYSTEM TESTS (TDD Phase 1 - Red)

    [Test]
    public void GameManager_HasDefaultTimeLimitConfiguration()
    {
        // Test that GameManager has a default time limit setting
        float defaultTimeLimit = gameManager.GetTimeLimit();
        Assert.Greater(defaultTimeLimit, 0f, "Time limit should be positive");
        Assert.AreEqual(120f, defaultTimeLimit, "Default time limit should be 120 seconds");
    }

    [Test]
    public void GameManager_SetTimeLimit_UpdatesCorrectly()
    {
        // Test that time limit can be configured
        gameManager.SetTimeLimit(180f);
        Assert.AreEqual(180f, gameManager.GetTimeLimit());
        
        gameManager.SetTimeLimit(60f);
        Assert.AreEqual(60f, gameManager.GetTimeLimit());
        
        gameManager.SetTimeLimit(300f);
        Assert.AreEqual(300f, gameManager.GetTimeLimit());
    }

    [Test]
    public void GameManager_SetTimeLimit_RejectsNegativeValues()
    {
        // Test that negative time limits are rejected and clamped to 0
        gameManager.SetTimeLimit(-30f);
        Assert.AreEqual(0f, gameManager.GetTimeLimit());
        
        gameManager.SetTimeLimit(-120f);
        Assert.AreEqual(0f, gameManager.GetTimeLimit());
    }

    [Test]
    public void GameManager_TimerInitiallyInactive()
    {
        // Test that timer starts inactive
        Assert.IsFalse(gameManager.IsTimerActive());
    }

    [Test]
    public void GameManager_GetTimeRemaining_ReturnsTimeLimitWhenNotStarted()
    {
        // Test that time remaining equals time limit when timer not started
        gameManager.SetTimeLimit(120f);
        Assert.AreEqual(120f, gameManager.GetTimeRemaining());
        
        gameManager.SetTimeLimit(180f);
        Assert.AreEqual(180f, gameManager.GetTimeRemaining());
    }

    [Test]
    public void GameManager_StartTimer_ActivatesTimer()
    {
        // Test that starting timer activates it
        gameManager.SetTimeLimit(120f);
        gameManager.StartTimer();
        Assert.IsTrue(gameManager.IsTimerActive());
    }

    [Test]
    public void GameManager_StopTimer_DeactivatesTimer()
    {
        // Test that stopping timer deactivates it
        gameManager.SetTimeLimit(120f);
        gameManager.StartTimer();
        Assert.IsTrue(gameManager.IsTimerActive());
        
        gameManager.StopTimer();
        Assert.IsFalse(gameManager.IsTimerActive());
    }

    [Test]
    public void GameManager_TimerCountdown_ReducesTimeRemaining()
    {
        // Test that timer counts down over time
        gameManager.SetTimeLimit(10f);
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.StartTimer();
        
        float initialTime = gameManager.GetTimeRemaining();
        
        // Manually advance timer by 2 seconds
        gameManager.UpdateTimer(2f);
        
        float currentTime = gameManager.GetTimeRemaining();
        Assert.AreEqual(initialTime - 2f, currentTime, 0.01f);
    }

    [Test]
    public void GameManager_TimerExpiration_TriggersEvent()
    {
        // Test that timer expiration triggers event
        bool timerExpiredTriggered = false;
        gameManager.OnTimerExpired += () => timerExpiredTriggered = true;
        
        gameManager.SetTimeLimit(1f);
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.StartTimer();
        
        // Advance timer past expiration
        gameManager.UpdateTimer(1.1f);
        
        Assert.IsTrue(timerExpiredTriggered);
    }

    [Test]
    public void GameManager_TimerExpiration_SetsGameStateToStart()
    {
        // Test that timer expiration returns to level selection
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.SetTimeLimit(1f);
        gameManager.StartTimer();
        
        // Advance timer past expiration
        gameManager.UpdateTimer(1.1f);
        
        Assert.AreEqual(GameManager.GameState.Start, gameManager.CurrentGameState);
    }

    [Test]
    public void GameManager_TimerExpiration_LogsLevelCleanup()
    {
        // Test that timer expiration attempts to clean up level objects
        // We can't easily mock LevelLifecycleManager singleton, but we can verify
        // the method completes without errors and the game state changes correctly
        
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.SetTimeLimit(1f);
        gameManager.StartTimer();
        
        // This should complete without throwing exceptions
        Assert.DoesNotThrow(() => {
            gameManager.UpdateTimer(1.1f);
        });
        
        // Verify proper state after timer expiration
        Assert.AreEqual(GameManager.GameState.Start, gameManager.CurrentGameState);
        Assert.IsFalse(gameManager.IsTimerActive());
        Assert.AreEqual(0f, gameManager.GetTimeRemaining());
    }

    [Test]
    public void GameManager_TimerExpiration_DeactivatesTimer()
    {
        // Test that timer deactivates when expired
        gameManager.SetTimeLimit(1f);
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.StartTimer();
        Assert.IsTrue(gameManager.IsTimerActive());
        
        // Advance timer past expiration
        gameManager.UpdateTimer(1.1f);
        
        Assert.IsFalse(gameManager.IsTimerActive());
    }

    [Test]
    public void GameManager_TimerUpdate_TriggersUpdateEvent()
    {
        // Test that timer updates trigger events
        bool updateEventTriggered = false;
        float lastTimeReported = -1f;
        
        gameManager.OnTimerUpdated += (timeRemaining) => {
            updateEventTriggered = true;
            lastTimeReported = timeRemaining;
        };
        
        gameManager.SetTimeLimit(10f);
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.StartTimer();
        gameManager.UpdateTimer(1f);
        
        Assert.IsTrue(updateEventTriggered);
        Assert.AreEqual(9f, lastTimeReported, 0.01f);
    }

    [Test]
    public void GameManager_TimerUpdate_DoesNotCountDownWhenInactive()
    {
        // Test that timer doesn't count down when not active
        gameManager.SetTimeLimit(10f);
        float initialTime = gameManager.GetTimeRemaining();
        
        // Don't start timer, just try to update
        gameManager.UpdateTimer(2f);
        
        Assert.AreEqual(initialTime, gameManager.GetTimeRemaining());
        Assert.IsFalse(gameManager.IsTimerActive());
    }

    [Test]
    public void GameManager_TimerUpdate_OnlyCountsDownDuringPlayingState()
    {
        // Test that timer only counts down during Playing state
        gameManager.SetTimeLimit(10f);
        gameManager.StartTimer();
        
        // Set to Start state
        gameManager.SetGameState(GameManager.GameState.Start);
        float timeBeforeUpdate = gameManager.GetTimeRemaining();
        gameManager.UpdateTimer(2f);
        
        // Time should not have changed in Start state
        Assert.AreEqual(timeBeforeUpdate, gameManager.GetTimeRemaining());
        
        // Switch to Playing state
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.UpdateTimer(2f);
        
        // Now time should have decreased
        Assert.AreEqual(timeBeforeUpdate - 2f, gameManager.GetTimeRemaining(), 0.01f);
    }

    [Test]
    public void GameManager_FormatTime_HandlesMinutesAndSeconds()
    {
        // Test time formatting method
        Assert.AreEqual("02:00", gameManager.FormatTime(120f));
        Assert.AreEqual("01:30", gameManager.FormatTime(90f));
        Assert.AreEqual("00:45", gameManager.FormatTime(45f));
        Assert.AreEqual("00:00", gameManager.FormatTime(0f));
        Assert.AreEqual("10:05", gameManager.FormatTime(605f));
    }

    [Test]
    public void GameManager_FormatTime_HandlesNegativeTime()
    {
        // Test that negative time formats as 00:00
        Assert.AreEqual("00:00", gameManager.FormatTime(-10f));
        Assert.AreEqual("00:00", gameManager.FormatTime(-1f));
    }

    [Test]
    public void GameManager_StartGame_StartsTimer()
    {
        // Test that starting game starts the timer
        gameManager.SetTimeLimit(120f);
        Assert.IsFalse(gameManager.IsTimerActive());
        
        gameManager.StartGame();
        
        Assert.IsTrue(gameManager.IsTimerActive());
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
    }

    [Test]
    public void GameManager_OnAllBlocksDestroyed_StopsTimer()
    {
        // Test that completing level stops timer
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.SetTimeLimit(120f);
        gameManager.StartTimer();
        Assert.IsTrue(gameManager.IsTimerActive());
        
        gameManager.OnAllBlocksDestroyed();
        
        Assert.IsFalse(gameManager.IsTimerActive());
    }

    [Test]
    public void GameManager_ResetGame_ResetsTimer()
    {
        // Test that resetting game resets timer
        gameManager.SetTimeLimit(120f);
        gameManager.SetGameState(GameManager.GameState.Playing); // Required for UpdateTimer to work
        gameManager.StartTimer();
        gameManager.UpdateTimer(30f); // Use 30 seconds
        
        Assert.AreEqual(90f, gameManager.GetTimeRemaining(), 0.01f);
        
        gameManager.ResetGame();
        
        Assert.AreEqual(120f, gameManager.GetTimeRemaining());
        Assert.IsFalse(gameManager.IsTimerActive());
    }

    [UnityTest]
    public IEnumerator GameManager_TimerIntegrationWithRespawnSystem()
    {
        // Test that timer continues running during ball respawn
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.SetTimeLimit(10f);
        gameManager.StartTimer();
        
        float timeBeforeBallLoss = gameManager.GetTimeRemaining();
        
        // Trigger ball loss (should not stop timer)
        gameManager.OnBallLost();
        yield return null;
        
        // Advance timer
        gameManager.UpdateTimer(2f);
        
        // Timer should still be active and counting down
        Assert.IsTrue(gameManager.IsTimerActive());
        Assert.AreEqual(timeBeforeBallLoss - 2f, gameManager.GetTimeRemaining(), 0.01f);
    }

    [Test]
    public void GameManager_HasTimerEvents()
    {
        // Test that GameManager has timer events available for subscription
        bool canSubscribeToTimerUpdated = false;
        bool canSubscribeToTimerExpired = false;
        
        try
        {
            gameManager.OnTimerUpdated += (timeRemaining) => { };
            canSubscribeToTimerUpdated = true;
            gameManager.OnTimerUpdated -= (timeRemaining) => { };
        }
        catch
        {
            canSubscribeToTimerUpdated = false;
        }
        
        try
        {
            gameManager.OnTimerExpired += () => { };
            canSubscribeToTimerExpired = true;
            gameManager.OnTimerExpired -= () => { };
        }
        catch
        {
            canSubscribeToTimerExpired = false;
        }
        
        Assert.IsTrue(canSubscribeToTimerUpdated, "Should be able to subscribe to OnTimerUpdated event");
        Assert.IsTrue(canSubscribeToTimerExpired, "Should be able to subscribe to OnTimerExpired event");
    }

    [Test]
    public void GameManager_GameStateChanged_EventTriggered()
    {
        // Test that game state changes trigger the OnGameStateChanged event
        bool eventTriggered = false;
        GameManager.GameState receivedState = GameManager.GameState.Start;
        
        gameManager.OnGameStateChanged += (newState) => {
            eventTriggered = true;
            receivedState = newState;
        };
        
        gameManager.SetGameState(GameManager.GameState.Playing);
        
        Assert.IsTrue(eventTriggered, "OnGameStateChanged event should be triggered");
        Assert.AreEqual(GameManager.GameState.Playing, receivedState, "Event should pass the correct state");
    }

    [Test] 
    public void GameManager_TimerExpiration_TriggersStateChangeEvent()
    {
        // Test that timer expiration triggers state change event to Start
        bool stateChangeEventTriggered = false;
        GameManager.GameState finalState = GameManager.GameState.Playing;
        
        gameManager.OnGameStateChanged += (newState) => {
            if (newState == GameManager.GameState.Start)
            {
                stateChangeEventTriggered = true;
                finalState = newState;
            }
        };
        
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.SetTimeLimit(1f);
        gameManager.StartTimer();
        
        // Advance timer past expiration
        gameManager.UpdateTimer(1.1f);
        
        Assert.IsTrue(stateChangeEventTriggered, "Timer expiration should trigger state change to Start");
        Assert.AreEqual(GameManager.GameState.Start, finalState, "Final state should be Start");
    }
}