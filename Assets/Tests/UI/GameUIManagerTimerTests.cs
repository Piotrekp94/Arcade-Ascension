using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TMPro;

public class GameUIManagerTimerTests
{
    private GameUIManager uiManager;
    private GameManager gameManager;
    private GlobalGameConfig testGlobalConfig;
    private GameObject uiManagerGO;
    private GameObject gameManagerGO;
    private TextMeshProUGUI timerText;

    [SetUp]
    public void Setup()
    {
        // Create global config for testing
        testGlobalConfig = ScriptableObject.CreateInstance<GlobalGameConfig>();
        SetGlobalTimeLimit(120f); // Default for most tests
        GlobalGameConfig.SetInstanceForTesting(testGlobalConfig);

        // Create UI Manager GameObject
        uiManagerGO = new GameObject("GameUIManager");
        uiManager = uiManagerGO.AddComponent<GameUIManager>();

        // Create GameManager GameObject
        gameManagerGO = new GameObject("GameManager");
        gameManager = gameManagerGO.AddComponent<GameManager>();
        GameManager.SetInstanceForTesting(gameManager);

        // Create timer text component
        GameObject timerTextGO = new GameObject("TimerText");
        timerText = timerTextGO.AddComponent<TextMeshProUGUI>();

        // Link components
        uiManager.SetTimerText(timerText);
        uiManager.SetGameManager(gameManager);
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up GameObjects
        GameManager.SetInstanceForTesting(null);
        GlobalGameConfig.SetInstanceForTesting(null);
        
        if (Application.isPlaying)
        {
            if (uiManagerGO != null) Object.Destroy(uiManagerGO);
            if (gameManagerGO != null) Object.Destroy(gameManagerGO);
            if (timerText != null && timerText.gameObject != null) Object.Destroy(timerText.gameObject);
            if (testGlobalConfig != null) Object.Destroy(testGlobalConfig);
        }
        else
        {
            if (uiManagerGO != null) Object.DestroyImmediate(uiManagerGO);
            if (gameManagerGO != null) Object.DestroyImmediate(gameManagerGO);
            if (timerText != null && timerText.gameObject != null) Object.DestroyImmediate(timerText.gameObject);
            if (testGlobalConfig != null) Object.DestroyImmediate(testGlobalConfig);
        }
    }

    [Test]
    public void GameUIManager_HasTimerTextReference()
    {
        // Test that GameUIManager can hold a reference to timer text
        Assert.IsNotNull(uiManager.GetTimerText());
        Assert.AreSame(timerText, uiManager.GetTimerText());
    }

    [Test]
    public void GameUIManager_CanSetTimerText()
    {
        // Test that timer text can be set and retrieved
        GameObject newTimerTextGO = new GameObject("NewTimerText");
        TextMeshProUGUI newTimerText = newTimerTextGO.AddComponent<TextMeshProUGUI>();
        
        uiManager.SetTimerText(newTimerText);
        Assert.AreSame(newTimerText, uiManager.GetTimerText());
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(newTimerTextGO);
        else
            Object.DestroyImmediate(newTimerTextGO);
    }

    [Test]
    public void GameUIManager_UpdateTimerDisplay_UpdatesText()
    {
        // Test that timer display updates correctly
        uiManager.UpdateTimerDisplay(120f);
        Assert.AreEqual("02:00", timerText.text);
        
        uiManager.UpdateTimerDisplay(90f);
        Assert.AreEqual("01:30", timerText.text);
        
        uiManager.UpdateTimerDisplay(45f);
        Assert.AreEqual("00:45", timerText.text);
    }

    [Test]
    public void GameUIManager_UpdateTimerDisplay_HandlesNegativeTime()
    {
        // Test that negative time displays as 00:00
        uiManager.UpdateTimerDisplay(-10f);
        Assert.AreEqual("00:00", timerText.text);
    }

    [Test]
    public void GameUIManager_TimerColorChanges_GreenYellowRed()
    {
        // Test color changes based on time remaining
        uiManager.UpdateTimerDisplay(120f); // Green zone
        Assert.AreEqual(Color.green, timerText.color);
        
        uiManager.UpdateTimerDisplay(45f); // Yellow zone  
        Assert.AreEqual(Color.yellow, timerText.color);
        
        uiManager.UpdateTimerDisplay(15f); // Red zone
        Assert.AreEqual(Color.red, timerText.color);
    }

    [Test]
    public void GameUIManager_TimerColorThresholds()
    {
        // Test specific color threshold values
        uiManager.UpdateTimerDisplay(61f); // Should be green (> 60)
        Assert.AreEqual(Color.green, timerText.color);
        
        uiManager.UpdateTimerDisplay(60f); // Should be yellow (60 exactly)
        Assert.AreEqual(Color.yellow, timerText.color);
        
        uiManager.UpdateTimerDisplay(30f); // Should be yellow (= 30)
        Assert.AreEqual(Color.yellow, timerText.color);
        
        uiManager.UpdateTimerDisplay(29f); // Should be red (< 30)
        Assert.AreEqual(Color.red, timerText.color);
    }

    [Test]
    public void GameUIManager_SubscribesToTimerEvents()
    {
        // Test that UI manager subscribes to GameManager timer events
        bool eventHandled = false;
        
        // Initialize timer subscriptions first
        uiManager.InitializeTimerSubscriptions();
        
        // Subscribe to a test event to verify subscription works
        uiManager.OnTimerDisplayUpdated += () => eventHandled = true;
        
        // Trigger timer update from GameManager
        SetGlobalTimeLimit(60f);
        gameManager.SetGameState(GameManager.GameState.Playing); // Required for UpdateTimer to work
        gameManager.StartTimer();
        gameManager.UpdateTimer(1f);
        
        // Should trigger display update
        Assert.IsTrue(eventHandled);
    }

    [Test]
    public void GameUIManager_TimerExpiredHandling()
    {
        // Test that UI handles timer expiration correctly
        string initialText = timerText.text;
        
        uiManager.OnTimerExpired();
        
        // Timer text should show 00:00 when expired
        Assert.AreEqual("00:00", timerText.text);
        // Color should be red for expired timer
        Assert.AreEqual(Color.red, timerText.color);
    }

    [Test]
    public void GameUIManager_NullTimerTextHandling()
    {
        // Test that UI gracefully handles null timer text
        uiManager.SetTimerText(null);
        
        Assert.DoesNotThrow(() => {
            uiManager.UpdateTimerDisplay(60f);
        });
        
        Assert.DoesNotThrow(() => {
            uiManager.OnTimerExpired();
        });
    }

    [Test]
    public void GameUIManager_TimerDisplayFormat()
    {
        // Test various time formatting scenarios
        uiManager.UpdateTimerDisplay(3661f); // 1 hour, 1 minute, 1 second
        Assert.AreEqual("61:01", timerText.text); // Should show minutes:seconds only
        
        uiManager.UpdateTimerDisplay(659f); // 10 minutes, 59 seconds
        Assert.AreEqual("10:59", timerText.text);
        
        uiManager.UpdateTimerDisplay(0f);
        Assert.AreEqual("00:00", timerText.text);
        
        uiManager.UpdateTimerDisplay(9f);
        Assert.AreEqual("00:09", timerText.text);
    }

    [Test]
    public void GameUIManager_HasTimerDisplayUpdatedEvent()
    {
        // Test that UI manager has event for display updates
        bool canSubscribeToEvent = false;
        
        try
        {
            uiManager.OnTimerDisplayUpdated += () => { };
            canSubscribeToEvent = true;
            uiManager.OnTimerDisplayUpdated -= () => { };
        }
        catch
        {
            canSubscribeToEvent = false;
        }
        
        Assert.IsTrue(canSubscribeToEvent, "Should be able to subscribe to OnTimerDisplayUpdated event");
    }

    [UnityTest]
    public IEnumerator GameUIManager_TimerIntegrationWithGameManager()
    {
        // Test integration between GameManager and UI timer display
        SetGlobalTimeLimit(5f); // Set global time limit for this test
        uiManager.InitializeTimerSubscriptions(); // Ensure UI is subscribed to timer events
        gameManager.SetGameState(GameManager.GameState.Playing);
        gameManager.StartTimer();
        
        // Initial timer should show full time
        yield return null; // Wait one frame
        Assert.AreEqual("00:05", timerText.text);
        Assert.AreEqual(Color.green, timerText.color); // 5 seconds out of 5 total (100%) should be green
        
        // Advance timer
        gameManager.UpdateTimer(2f);
        yield return null;
        
        Assert.AreEqual("00:03", timerText.text);
        Assert.AreEqual(Color.green, timerText.color); // 3 seconds out of 5 total (60%) should be green
    }

    [Test]
    public void GameUIManager_InitializesTimerSubscriptions()
    {
        // Test that Start() method properly subscribes to timer events
        // This would be called by Unity lifecycle, simulate it
        uiManager.InitializeTimerSubscriptions();
        
        // Verify subscription by checking if timer updates affect display
        SetGlobalTimeLimit(30f);
        gameManager.SetGameState(GameManager.GameState.Playing); // Required for UpdateTimer to work
        gameManager.StartTimer();
        gameManager.UpdateTimer(1f);
        
        // Timer text should be updated
        Assert.AreEqual("00:29", timerText.text);
    }

    [Test]
    public void GameUIManager_CleanupsTimerSubscriptions()
    {
        // Test that OnDestroy properly unsubscribes from timer events
        uiManager.InitializeTimerSubscriptions();
        
        // Simulate OnDestroy cleanup
        uiManager.CleanupTimerSubscriptions();
        
        // After cleanup, timer updates should not affect display
        string textBeforeUpdate = timerText.text;
        SetGlobalTimeLimit(60f);
        gameManager.StartTimer();
        gameManager.UpdateTimer(1f);
        
        // Text should remain unchanged after cleanup
        Assert.AreEqual(textBeforeUpdate, timerText.text);
    }

    [Test]
    public void GameUIManager_TimerColor_PercentageBased_Green()
    {
        // Test green color for > 50% time remaining
        SetGlobalTimeLimit(120f);
        
        // 70% remaining (84 seconds out of 120)
        uiManager.UpdateTimerDisplay(84f);
        
        Assert.AreEqual(Color.green, timerText.color, "Timer should be green when > 50% time remaining");
    }

    [Test]
    public void GameUIManager_TimerColor_PercentageBased_Yellow()
    {
        // Test yellow color for 25%-50% time remaining
        SetGlobalTimeLimit(120f);
        
        // 35% remaining (42 seconds out of 120)
        uiManager.UpdateTimerDisplay(42f);
        
        Assert.AreEqual(Color.yellow, timerText.color, "Timer should be yellow when 25%-50% time remaining");
    }

    [Test]
    public void GameUIManager_TimerColor_PercentageBased_Red()
    {
        // Test red color for < 25% time remaining
        SetGlobalTimeLimit(120f);
        
        // 15% remaining (18 seconds out of 120)
        uiManager.UpdateTimerDisplay(18f);
        
        Assert.AreEqual(Color.red, timerText.color, "Timer should be red when < 25% time remaining");
    }

    [Test]
    public void GameUIManager_TimerColor_PercentageBased_DifferentLevels()
    {
        // Test that percentage works correctly with different level time limits
        
        // Level 1: 180 seconds, 60% remaining = 108 seconds (should be green)
        SetGlobalTimeLimit(180f);
        uiManager.UpdateTimerDisplay(108f);
        Assert.AreEqual(Color.green, timerText.color);
        
        // Level 10: 90 seconds, 30% remaining = 27 seconds (should be yellow)
        SetGlobalTimeLimit(90f);
        uiManager.UpdateTimerDisplay(27f);
        Assert.AreEqual(Color.yellow, timerText.color);
        
        // Short level: 10 seconds, 10% remaining = 1 second (should be red)
        SetGlobalTimeLimit(10f);
        uiManager.UpdateTimerDisplay(1f);
        Assert.AreEqual(Color.red, timerText.color);
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
}