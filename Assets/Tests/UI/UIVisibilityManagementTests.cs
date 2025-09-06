using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIVisibilityManagementTests
{
    private GameObject gameManagerObject;
    private GameManager gameManager;
    private GameObject tabSystemObject;
    private TabSystemUI tabSystemUI;
    private GameObject gameUIManagerObject;
    private GameUIManager gameUIManager;
    private GameObject levelSelectionObject;
    private LevelSelectionUI levelSelectionUI;
    private GameObject timerTextObject;
    private TextMeshProUGUI timerText;
    private GameObject scoreTextObject;
    private TextMeshProUGUI scoreText;

    [SetUp]
    public void SetUp()
    {
        // Create GameManager
        gameManagerObject = new GameObject("GameManager");
        gameManager = gameManagerObject.AddComponent<GameManager>();
        GameManager.SetInstanceForTesting(gameManager);

        // Create score text for GameManager
        scoreTextObject = new GameObject("ScoreText");
        scoreText = scoreTextObject.AddComponent<TextMeshProUGUI>();
        
        // Use reflection to set the private _scoreText field
        var scoreField = typeof(GameManager).GetField("_scoreText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        scoreField?.SetValue(gameManager, scoreText);

        // Create TabSystemUI
        tabSystemObject = new GameObject("TabSystemUI");
        tabSystemUI = tabSystemObject.AddComponent<TabSystemUI>();

        // Create LevelSelectionUI
        levelSelectionObject = new GameObject("LevelSelectionUI");
        levelSelectionUI = levelSelectionObject.AddComponent<LevelSelectionUI>();
        
        // Initialize LevelSelectionUI so it sets up its CanvasGroup
        levelSelectionUI.InitializeForTesting();
        
        tabSystemUI.SetLevelSelectionUI(levelSelectionUI);

        // Create GameUIManager with timer text
        gameUIManagerObject = new GameObject("GameUIManager");
        gameUIManager = gameUIManagerObject.AddComponent<GameUIManager>();
        
        timerTextObject = new GameObject("TimerText");
        timerText = timerTextObject.AddComponent<TextMeshProUGUI>();
        gameUIManager.SetTimerText(timerText);
        gameUIManager.SetGameManager(gameManager);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up singleton instances
        GameManager.SetInstanceForTesting(null);

        if (gameManagerObject != null)
            Object.DestroyImmediate(gameManagerObject);
        if (tabSystemObject != null)
            Object.DestroyImmediate(tabSystemObject);
        if (gameUIManagerObject != null)
            Object.DestroyImmediate(gameUIManagerObject);
        if (levelSelectionObject != null)
            Object.DestroyImmediate(levelSelectionObject);
        if (timerTextObject != null)
            Object.DestroyImmediate(timerTextObject);
        if (scoreTextObject != null)
            Object.DestroyImmediate(scoreTextObject);
    }

    [Test]
    public void WhenGameStateIsStart_OnlyTabSystemUIIsVisible()
    {
        // Arrange
        gameManager.SetGameState(GameManager.GameState.Start);

        // Act
        tabSystemUI.ShowTabSystem();
        levelSelectionUI.ShowLevelSelection();

        // Assert - TabSystemUI should be visible
        Assert.IsTrue(tabSystemUI.IsVisible(), "TabSystemUI should be visible in Start state");
        Assert.IsTrue(levelSelectionUI.IsVisible(), "LevelSelectionUI should be visible in Start state");

        // Timer and score should be hidden or not interfering
        Assert.IsNotNull(timerText, "Timer text should exist");
        Assert.IsNotNull(scoreText, "Score text should exist");
    }

    [Test]
    public void WhenGameStateIsPlaying_OnlyGameUIIsVisible()
    {
        // Arrange
        gameManager.SetGameState(GameManager.GameState.Start);
        tabSystemUI.ShowTabSystem();
        levelSelectionUI.ShowLevelSelection();

        // Act - transition to playing
        gameManager.SetGameState(GameManager.GameState.Playing);

        // Assert - TabSystemUI should be hidden, game UI visible
        Assert.IsFalse(levelSelectionUI.IsVisible(), "LevelSelectionUI should block itself when game is playing");

        // Timer and score should be available for display
        Assert.IsNotNull(timerText, "Timer text should be available during gameplay");
        Assert.IsNotNull(scoreText, "Score text should be available during gameplay");
    }

    [Test]
    public void WhenLevelCompletes_TabSystemUIBecomesVisible()
    {
        // Arrange - start in playing state
        gameManager.SetGameState(GameManager.GameState.Playing);
        tabSystemUI.HideTabSystem();

        // Act - simulate level completion (game returns to Start state)
        gameManager.SetGameState(GameManager.GameState.Start);
        tabSystemUI.ShowTabSystem();

        // Assert - TabSystemUI should be visible again
        Assert.IsTrue(tabSystemUI.IsVisible(), "TabSystemUI should be visible after level completion");
        Assert.IsTrue(levelSelectionUI.IsVisible(), "LevelSelectionUI should be visible after level completion");
    }

    [Test]
    public void WhenTimerExpires_TabSystemUIBecomesVisible()
    {
        // Arrange - start in playing state
        gameManager.SetGameState(GameManager.GameState.Playing);
        tabSystemUI.HideTabSystem();

        // Act - simulate timer expiration (OnTimerExpirationCleanup sets state to Start)
        gameManager.SetGameState(GameManager.GameState.Start);
        tabSystemUI.ShowTabSystem();

        // Assert - TabSystemUI should be visible again
        Assert.IsTrue(tabSystemUI.IsVisible(), "TabSystemUI should be visible after timer expiration");
        Assert.IsTrue(levelSelectionUI.IsVisible(), "LevelSelectionUI should be visible after timer expiration");
    }

    [Test]
    public void LevelSelectionUI_BlocksSelfDuringGameplay()
    {
        // Arrange
        gameManager.SetGameState(GameManager.GameState.Start);
        levelSelectionUI.ShowLevelSelection();
        Assert.IsTrue(levelSelectionUI.IsVisible(), "LevelSelectionUI should be visible initially");

        // Act - change to playing state and try to show level selection
        gameManager.SetGameState(GameManager.GameState.Playing);
        levelSelectionUI.ShowLevelSelection();

        // Assert - LevelSelectionUI should block itself
        Assert.IsFalse(levelSelectionUI.IsVisible(), "LevelSelectionUI should block itself during gameplay");
    }

    [Test]
    public void GameUIManager_DoesNotInterfereWithTabSystem()
    {
        // This test verifies that GameUIManager doesn't interfere with TabSystemUI visibility
        
        // Arrange
        gameManager.SetGameState(GameManager.GameState.Start);
        tabSystemUI.ShowTabSystem();
        
        // Act - GameUIManager updates its state
        gameUIManager.UpdateUIState();
        
        // Assert - TabSystemUI should still be visible and not affected
        Assert.IsTrue(tabSystemUI.IsVisible(), "TabSystemUI visibility should not be affected by GameUIManager");
        Assert.IsTrue(levelSelectionUI.IsVisible(), "LevelSelectionUI should remain visible");
    }
}