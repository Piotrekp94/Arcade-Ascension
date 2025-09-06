using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICoordinatorTests
{
    private GameObject coordinatorObject;
    private UICoordinator coordinator;
    private GameObject gameManagerObject;
    private GameManager gameManager;
    private GameObject tabSystemObject;
    private TabSystemUI tabSystemUI;
    private GameObject gameUIManagerObject;
    private GameUIManager gameUIManager;
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

        // Create timer and score text elements
        timerTextObject = new GameObject("TimerText");
        timerText = timerTextObject.AddComponent<TextMeshProUGUI>();
        
        scoreTextObject = new GameObject("ScoreText");
        scoreText = scoreTextObject.AddComponent<TextMeshProUGUI>();

        // Create TabSystemUI
        tabSystemObject = new GameObject("TabSystemUI");
        tabSystemUI = tabSystemObject.AddComponent<TabSystemUI>();

        // Create GameUIManager
        gameUIManagerObject = new GameObject("GameUIManager");
        gameUIManager = gameUIManagerObject.AddComponent<GameUIManager>();

        // Create UICoordinator
        coordinatorObject = new GameObject("UICoordinator");
        coordinator = coordinatorObject.AddComponent<UICoordinator>();

        // Setup UICoordinator for testing
        coordinator.SetupForTesting(tabSystemUI, gameUIManager, timerText, scoreText, gameManager);
    }

    [TearDown]
    public void TearDown()
    {
        if (coordinatorObject != null)
            Object.DestroyImmediate(coordinatorObject);
        if (gameManagerObject != null)
            Object.DestroyImmediate(gameManagerObject);
        if (tabSystemObject != null)
            Object.DestroyImmediate(tabSystemObject);
        if (gameUIManagerObject != null)
            Object.DestroyImmediate(gameUIManagerObject);
        if (timerTextObject != null)
            Object.DestroyImmediate(timerTextObject);
        if (scoreTextObject != null)
            Object.DestroyImmediate(scoreTextObject);
    }

    [Test]
    public void UICoordinator_InitializesCorrectly()
    {
        // Assert
        Assert.IsNotNull(coordinator);
        Assert.AreEqual(coordinator, UICoordinator.Instance);
    }

    [Test]
    public void UICoordinator_StartState_ShowsMenuUI()
    {
        // Arrange
        gameManager.SetGameState(GameManager.GameState.Start);

        // Act
        // State change should trigger UI update automatically

        // Assert
        Assert.IsTrue(coordinator.IsMenuUIVisible(), "Menu UI should be visible in Start state");
        Assert.IsFalse(coordinator.IsInGameUIVisible(), "In-game UI should be hidden in Start state");
    }

    [Test]
    public void UICoordinator_PlayingState_ShowsInGameUI()
    {
        // Arrange
        gameManager.SetGameState(GameManager.GameState.Start);

        // Act
        gameManager.SetGameState(GameManager.GameState.Playing);

        // Assert
        Assert.IsFalse(coordinator.IsMenuUIVisible(), "Menu UI should be hidden in Playing state");
        Assert.IsTrue(coordinator.IsInGameUIVisible(), "In-game UI should be visible in Playing state");
    }

    [Test]
    public void UICoordinator_GameOverState_ShowsMenuUI()
    {
        // Arrange
        gameManager.SetGameState(GameManager.GameState.Playing);

        // Act
        gameManager.SetGameState(GameManager.GameState.GameOver);

        // Assert
        Assert.IsTrue(coordinator.IsMenuUIVisible(), "Menu UI should be visible in GameOver state");
        Assert.IsFalse(coordinator.IsInGameUIVisible(), "In-game UI should be hidden in GameOver state");
    }

    [Test]
    public void UICoordinator_ForceShowMenuUI_ShowsMenuRegardlessOfGameState()
    {
        // Arrange
        gameManager.SetGameState(GameManager.GameState.Playing);
        Assert.IsFalse(coordinator.IsMenuUIVisible(), "Menu UI should initially be hidden");

        // Act
        coordinator.ForceShowMenuUI();

        // Assert
        Assert.IsTrue(coordinator.IsMenuUIVisible(), "Menu UI should be visible after force show");
    }

    [Test]
    public void UICoordinator_ForceShowInGameUI_ShowsInGameRegardlessOfGameState()
    {
        // Arrange
        gameManager.SetGameState(GameManager.GameState.Start);
        Assert.IsFalse(coordinator.IsInGameUIVisible(), "In-game UI should initially be hidden");

        // Act
        coordinator.ForceShowInGameUI();

        // Assert
        Assert.IsTrue(coordinator.IsInGameUIVisible(), "In-game UI should be visible after force show");
    }

    [Test]
    public void UICoordinator_ForceHideMenuUI_HidesMenuRegardlessOfGameState()
    {
        // Arrange
        gameManager.SetGameState(GameManager.GameState.Start);
        Assert.IsTrue(coordinator.IsMenuUIVisible(), "Menu UI should initially be visible");

        // Act
        coordinator.ForceHideMenuUI();

        // Assert
        Assert.IsFalse(coordinator.IsMenuUIVisible(), "Menu UI should be hidden after force hide");
    }

    [Test]
    public void UICoordinator_ForceHideInGameUI_HidesInGameRegardlessOfGameState()
    {
        // Arrange
        gameManager.SetGameState(GameManager.GameState.Playing);
        Assert.IsTrue(coordinator.IsInGameUIVisible(), "In-game UI should initially be visible");

        // Act
        coordinator.ForceHideInGameUI();

        // Assert
        Assert.IsFalse(coordinator.IsInGameUIVisible(), "In-game UI should be hidden after force hide");
    }

    [Test]
    public void UICoordinator_TransitionFromStartToPlaying_SwitchesUICorrectly()
    {
        // Arrange
        gameManager.SetGameState(GameManager.GameState.Start);
        Assert.IsTrue(coordinator.IsMenuUIVisible(), "Menu UI should be visible initially");
        Assert.IsFalse(coordinator.IsInGameUIVisible(), "In-game UI should be hidden initially");

        // Act
        gameManager.SetGameState(GameManager.GameState.Playing);

        // Assert
        Assert.IsFalse(coordinator.IsMenuUIVisible(), "Menu UI should be hidden after transition");
        Assert.IsTrue(coordinator.IsInGameUIVisible(), "In-game UI should be visible after transition");
    }

    [Test]
    public void UICoordinator_TransitionFromPlayingToStart_SwitchesUICorrectly()
    {
        // Arrange
        gameManager.SetGameState(GameManager.GameState.Playing);
        Assert.IsFalse(coordinator.IsMenuUIVisible(), "Menu UI should be hidden initially");
        Assert.IsTrue(coordinator.IsInGameUIVisible(), "In-game UI should be visible initially");

        // Act
        gameManager.SetGameState(GameManager.GameState.Start);

        // Assert
        Assert.IsTrue(coordinator.IsMenuUIVisible(), "Menu UI should be visible after transition");
        Assert.IsFalse(coordinator.IsInGameUIVisible(), "In-game UI should be hidden after transition");
    }
}