using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TMPro;

public class TimerIntegrationTests
{
    private GameManager gameManager;
    private GameUIManager uiManager;
    private LevelData testLevelData;
    private GlobalGameConfig testGlobalConfig;
    private GameObject gameManagerGO;
    private GameObject uiManagerGO;
    private TextMeshProUGUI timerText;

    [SetUp]
    public void Setup()
    {
        // Create global config for testing
        testGlobalConfig = ScriptableObject.CreateInstance<GlobalGameConfig>();
        SetGlobalConfigDefaults(testGlobalConfig);
        GlobalGameConfig.SetInstanceForTesting(testGlobalConfig);

        // Create GameManager
        gameManagerGO = new GameObject("GameManager");
        gameManager = gameManagerGO.AddComponent<GameManager>();
        GameManager.SetInstanceForTesting(gameManager);

        // Create UI Manager
        uiManagerGO = new GameObject("GameUIManager");
        uiManager = uiManagerGO.AddComponent<GameUIManager>();

        // Create timer text component
        GameObject timerTextGO = new GameObject("TimerText");
        timerText = timerTextGO.AddComponent<TextMeshProUGUI>();

        // Create test level data
        testLevelData = ScriptableObject.CreateInstance<LevelData>();
        SetLevelDataFields(testLevelData);

        // Wire up components
        uiManager.SetTimerText(timerText);
        uiManager.SetGameManager(gameManager);
        uiManager.InitializeTimerSubscriptions();
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up GameObjects
        uiManager?.CleanupTimerSubscriptions();
        GameManager.SetInstanceForTesting(null);
        GlobalGameConfig.SetInstanceForTesting(null);

        if (Application.isPlaying)
        {
            if (gameManagerGO != null) Object.Destroy(gameManagerGO);
            if (uiManagerGO != null) Object.Destroy(uiManagerGO);
            if (timerText != null && timerText.gameObject != null) Object.Destroy(timerText.gameObject);
            if (testLevelData != null) Object.Destroy(testLevelData);
            if (testGlobalConfig != null) Object.Destroy(testGlobalConfig);
        }
        else
        {
            if (gameManagerGO != null) Object.DestroyImmediate(gameManagerGO);
            if (uiManagerGO != null) Object.DestroyImmediate(uiManagerGO);
            if (timerText != null && timerText.gameObject != null) Object.DestroyImmediate(timerText.gameObject);
            if (testLevelData != null) Object.DestroyImmediate(testLevelData);
            if (testGlobalConfig != null) Object.DestroyImmediate(testGlobalConfig);
        }
    }

    [Test]
    public void TimerIntegration_LevelDataToGameManagerToUI()
    {
        // Test complete integration: GlobalConfig -> GameManager -> UI
        SetGlobalTimeLimit(90f);
        
        // Start game with level data
        gameManager.StartGame(testLevelData);
        
        // GameManager should use global time limit
        Assert.AreEqual(90f, gameManager.GetTimeLimit());
        Assert.AreEqual(90f, gameManager.GetTimeRemaining());
        
        // UI should display the correct time
        Assert.AreEqual("01:30", timerText.text);
        Assert.AreEqual(Color.green, timerText.color); // 90s out of 90s (100%) should be green
    }

    [Test]
    public void TimerIntegration_CountdownUpdatesUI()
    {
        // Test that timer countdown updates UI correctly
        SetGlobalTimeLimit(60f);
        gameManager.StartGame(testLevelData);
        
        // Initial state
        Assert.AreEqual("01:00", timerText.text);
        Assert.AreEqual(Color.green, timerText.color); // 60s out of 60s (100%) should be green
        
        // Advance timer by 30 seconds
        gameManager.UpdateTimer(30f);
        
        Assert.AreEqual("00:30", timerText.text);
        Assert.AreEqual(Color.yellow, timerText.color);
        
        // Advance timer by another 15 seconds (at yellow/red boundary)
        gameManager.UpdateTimer(15f);
        
        Assert.AreEqual("00:15", timerText.text);
        Assert.AreEqual(Color.yellow, timerText.color); // 15s out of 60s (25%) should be yellow
        
        // Advance timer by another 10 seconds (into red zone)
        gameManager.UpdateTimer(10f);
        
        Assert.AreEqual("00:05", timerText.text);
        Assert.AreEqual(Color.red, timerText.color); // 5s out of 60s (8.3%) should be red
    }

    [Test]
    public void TimerIntegration_ExpirationHandling()
    {
        // Test complete timer expiration flow
        bool timerExpiredEventFired = false;
        gameManager.OnTimerExpired += () => timerExpiredEventFired = true;
        
        SetGlobalTimeLimit(5f);
        gameManager.StartGame(testLevelData);
        
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
        Assert.IsTrue(gameManager.IsTimerActive());
        
        // Expire the timer
        gameManager.UpdateTimer(6f);
        
        // Check integration results
        Assert.IsTrue(timerExpiredEventFired);
        Assert.IsFalse(gameManager.IsTimerActive());
        Assert.AreEqual(GameManager.GameState.Start, gameManager.CurrentGameState);
        Assert.AreEqual("00:00", timerText.text);
        Assert.AreEqual(Color.red, timerText.color);
    }

    [Test]
    public void TimerIntegration_LevelCompletionStopsTimer()
    {
        // Test that completing level stops timer
        SetGlobalTimeLimit(120f);
        gameManager.StartGame(testLevelData);
        
        Assert.IsTrue(gameManager.IsTimerActive());
        
        // Use some time
        gameManager.UpdateTimer(30f);
        Assert.AreEqual(90f, gameManager.GetTimeRemaining(), 0.1f);
        Assert.IsTrue(gameManager.IsTimerActive());
        
        // Complete level (simulate all blocks destroyed)
        gameManager.SetBlocksRemainingForTesting(0);
        gameManager.OnAllBlocksDestroyed();
        
        // Timer should be stopped
        Assert.IsFalse(gameManager.IsTimerActive());
        Assert.AreEqual(GameManager.GameState.Start, gameManager.CurrentGameState);
    }

    [Test]
    public void TimerIntegration_MultipleGameCycles()
    {
        // Test timer through multiple game start/reset cycles
        SetGlobalTimeLimit(60f);
        
        // First game
        gameManager.StartGame(testLevelData);
        gameManager.UpdateTimer(20f);
        Assert.AreEqual(40f, gameManager.GetTimeRemaining(), 0.1f);
        
        // Reset game
        gameManager.ResetGame();
        Assert.IsFalse(gameManager.IsTimerActive());
        Assert.AreEqual(60f, gameManager.GetTimeRemaining(), 0.1f);
        
        // Second game with different level
        SetGlobalTimeLimit(45f);
        gameManager.StartGame(testLevelData);
        Assert.AreEqual(45f, gameManager.GetTimeRemaining(), 0.1f);
        Assert.IsTrue(gameManager.IsTimerActive());
    }

    [Test]
    public void TimerIntegration_BallRespawnDoesNotAffectTimer()
    {
        // Test that ball respawn system doesn't interfere with timer
        SetGlobalTimeLimit(30f);
        gameManager.StartGame(testLevelData);
        
        gameManager.UpdateTimer(10f);
        Assert.AreEqual(20f, gameManager.GetTimeRemaining(), 0.1f);
        Assert.IsTrue(gameManager.IsTimerActive());
        
        // Trigger ball loss and respawn
        gameManager.OnBallLost();
        Assert.IsTrue(gameManager.IsRespawnTimerActive());
        
        // Timer should continue counting down
        gameManager.UpdateTimer(5f);
        Assert.AreEqual(15f, gameManager.GetTimeRemaining(), 0.1f);
        Assert.IsTrue(gameManager.IsTimerActive());
        
        // Respawn timer should also work
        Assert.IsTrue(gameManager.IsRespawnTimerActive());
    }

    [Test]
    public void TimerIntegration_GlobalTimeLimitForAllLevels()
    {
        // Test that all levels use the same global time limit
        LevelData level1 = ScriptableObject.CreateInstance<LevelData>();
        LevelData level2 = ScriptableObject.CreateInstance<LevelData>();
        
        SetLevelDataFields(level1);
        SetLevelDataFields(level2);
        
        // Set global time limit to 120 seconds
        SetGlobalTimeLimit(120f);
        
        // Start with level 1
        gameManager.StartGame(level1);
        Assert.AreEqual(120f, gameManager.GetTimeLimit());
        Assert.AreEqual("02:00", timerText.text);
        Assert.AreEqual(Color.green, timerText.color);
        
        // Reset and start with level 2 - should use same global time limit
        gameManager.ResetGame();
        gameManager.StartGame(level2);
        Assert.AreEqual(120f, gameManager.GetTimeLimit());
        Assert.AreEqual("02:00", timerText.text);
        Assert.AreEqual(Color.green, timerText.color); // 120s out of 120s (100%) should be green
        
        // Test percentage-based coloring with global limit (120s total)
        gameManager.UpdateTimer(60f); // 60s remaining out of 120s = 50%
        Assert.AreEqual("01:00", timerText.text);
        Assert.AreEqual(Color.yellow, timerText.color); // 50% should be yellow (at boundary)
        
        gameManager.UpdateTimer(30f); // 30s remaining out of 120s = 25% 
        Assert.AreEqual("00:30", timerText.text);
        Assert.AreEqual(Color.yellow, timerText.color); // 25% should be yellow (at boundary)
        
        gameManager.UpdateTimer(20f); // 10s remaining out of 120s = 8.3%
        Assert.AreEqual("00:10", timerText.text);
        Assert.AreEqual(Color.red, timerText.color); // <25% should be red
        
        // Cleanup
        if (Application.isPlaying)
        {
            Object.Destroy(level1);
            Object.Destroy(level2);
        }
        else
        {
            Object.DestroyImmediate(level1);
            Object.DestroyImmediate(level2);
        }
    }

    [UnityTest]
    public IEnumerator TimerIntegration_UIUpdatesInRealTime()
    {
        // Test UI updates during real-time timer countdown
        SetGlobalTimeLimit(5f);
        gameManager.StartGame(testLevelData);
        
        Assert.AreEqual("00:05", timerText.text);
        
        yield return null; // Wait one frame
        
        // Advance timer
        gameManager.UpdateTimer(2f);
        
        Assert.AreEqual("00:03", timerText.text);
        Assert.AreEqual(Color.green, timerText.color); // 3s out of 5s (60%) should be green
        
        // Advance to yellow zone
        gameManager.UpdateTimer(1.5f); // 3 - 1.5 = 1.5s remaining
        
        Assert.AreEqual("00:01", timerText.text); // Should show 1 second (rounded down)
        Assert.AreEqual(Color.yellow, timerText.color); // 1.5s out of 5s (30%) should be yellow
        
        // Advance to expiration
        gameManager.UpdateTimer(1.6f);
        
        Assert.AreEqual("00:00", timerText.text);
        Assert.AreEqual(Color.red, timerText.color);
        Assert.IsFalse(gameManager.IsTimerActive());
    }

    [Test]
    public void TimerIntegration_EventChainVerification()
    {
        // Test complete event chain from GameManager to UI
        int timerUpdateEventCount = 0;
        int displayUpdateEventCount = 0;
        
        gameManager.OnTimerUpdated += (time) => timerUpdateEventCount++;
        uiManager.OnTimerDisplayUpdated += () => displayUpdateEventCount++;
        
        SetGlobalTimeLimit(10f);
        gameManager.StartGame(testLevelData);
        
        // Update timer multiple times
        gameManager.UpdateTimer(1f);
        gameManager.UpdateTimer(1f);
        gameManager.UpdateTimer(1f);
        
        // Both event chains should have fired: 1 from StartTimer + 3 from UpdateTimer calls
        Assert.AreEqual(4, timerUpdateEventCount);
        Assert.AreEqual(4, displayUpdateEventCount);
    }

    // Helper method to set up level data fields via reflection
    private void SetLevelDataFields(LevelData levelData)
    {
        SetPrivateField(levelData, "levelId", 1);
        SetPrivateField(levelData, "levelName", "Test Level");
        SetPrivateField(levelData, "levelDescription", "Integration test level");
        SetPrivateField(levelData, "blockRows", 3);
        SetPrivateField(levelData, "blockColumns", 5);
        SetPrivateField(levelData, "blockSpacing", 0.1f);
        SetPrivateField(levelData, "blockSpacingX", 0.1f);
        SetPrivateField(levelData, "blockSpacingY", 0.1f);
        SetPrivateField(levelData, "spawnAreaOffset", Vector2.zero);
        SetPrivateField(levelData, "scoreMultiplier", 1.0f);
        SetPrivateField(levelData, "defaultBlockScore", 10);
    }
    
    // Helper method to set up global config defaults
    private void SetGlobalConfigDefaults(GlobalGameConfig config)
    {
        SetPrivateField(config, "globalTimeLimit", 120f);
    }
    
    // Helper method to set global time limit for testing
    private void SetGlobalTimeLimit(float timeLimit)
    {
        SetPrivateField(testGlobalConfig, "globalTimeLimit", timeLimit);
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field {fieldName} not found");
        field.SetValue(obj, value);
    }

    [Test]
    public void TimerIntegration_ExpirationCleansUpAttachedBalls()
    {
        // Test that attached balls are cleaned up when timer expires
        // This reproduces the bug where balls remain stacked after timer expiration
        
        // Set up a short timer
        SetGlobalTimeLimit(2f);
        gameManager.StartGame(testLevelData);
        
        // Create and register a paddle
        GameObject paddleGO = new GameObject("Paddle");
        PlayerPaddle paddle = paddleGO.AddComponent<PlayerPaddle>();
        gameManager.RegisterPaddleForSpawning(paddleGO.transform);
        
        // Spawn a ball and attach it to the paddle (simulating initial game state)
        GameObject ballGO = gameManager.SpawnBallAtPosition(paddleGO.transform.position);
        Ball ball = ballGO.GetComponent<Ball>();
        if (ball == null) ball = ballGO.AddComponent<Ball>();
        
        paddle.AttachBall(ball);
        
        // Verify ball is attached
        Assert.IsTrue(paddle.HasAttachedBall(), "Ball should be attached to paddle");
        Assert.IsNotNull(GameObject.FindWithTag("Ball"), "Ball should exist in scene");
        
        // Expire the timer
        gameManager.UpdateTimer(3f);
        
        // After timer expiration, ball should be cleaned up
        Assert.AreEqual(GameManager.GameState.Start, gameManager.CurrentGameState);
        Assert.IsFalse(gameManager.IsTimerActive());
        Assert.IsNull(GameObject.FindWithTag("Ball"), "Ball should be cleaned up after timer expiration");
        Assert.IsFalse(paddle.HasAttachedBall(), "Paddle should no longer have attached ball");
        
        // Clean up test objects
        if (Application.isPlaying)
        {
            Object.Destroy(paddleGO);
            if (ballGO != null) Object.Destroy(ballGO);
        }
        else
        {
            Object.DestroyImmediate(paddleGO);
            if (ballGO != null) Object.DestroyImmediate(ballGO);
        }
    }
}