using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using System.Collections;

public class GameUIManagerTests
{
    private GameUIManager uiManager;
    private GameManager gameManager;
    private GameObject uiManagerGO;
    private GameObject gameManagerGO;
    private Button startButton;
    private Button restartButton;

    [SetUp]
    public void Setup()
    {
        // Create GameManager
        gameManagerGO = new GameObject("GameManager");
        gameManager = gameManagerGO.AddComponent<GameManager>();
        GameManager.SetInstanceForTesting(gameManager);

        // Create UI Manager
        uiManagerGO = new GameObject("GameUIManager");
        uiManager = uiManagerGO.AddComponent<GameUIManager>();

        // Create test buttons
        GameObject startButtonGO = new GameObject("StartButton");
        startButton = startButtonGO.AddComponent<Button>();
        
        GameObject restartButtonGO = new GameObject("RestartButton");
        restartButton = restartButtonGO.AddComponent<Button>();

        // Manually assign references for testing (normally done in inspector)
        var gameManagerField = typeof(GameUIManager).GetField("gameManager", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        gameManagerField?.SetValue(uiManager, gameManager);

        var startButtonField = typeof(GameUIManager).GetField("startGameButton",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        startButtonField?.SetValue(uiManager, startButton);

        var restartButtonField = typeof(GameUIManager).GetField("restartGameButton",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        restartButtonField?.SetValue(uiManager, restartButton);
    }

    [TearDown]
    public void Teardown()
    {
        GameManager.SetInstanceForTesting(null);

        if (Application.isPlaying)
        {
            if (uiManagerGO != null) Object.Destroy(uiManagerGO);
            if (gameManagerGO != null) Object.Destroy(gameManagerGO);
            if (startButton != null && startButton.gameObject != null) Object.Destroy(startButton.gameObject);
            if (restartButton != null && restartButton.gameObject != null) Object.Destroy(restartButton.gameObject);
        }
        else
        {
            if (uiManagerGO != null) Object.DestroyImmediate(uiManagerGO);
            if (gameManagerGO != null) Object.DestroyImmediate(gameManagerGO);
            if (startButton != null && startButton.gameObject != null) Object.DestroyImmediate(startButton.gameObject);
            if (restartButton != null && restartButton.gameObject != null) Object.DestroyImmediate(restartButton.gameObject);
        }
    }

    [Test]
    public void GameUIManager_CanFindGameManagerInstance()
    {
        // Test that UIManager can find GameManager.Instance
        Assert.IsNotNull(GameManager.Instance);
        
        // Simulate Start() method behavior
        uiManager.Start();
        
        // UIManager should be able to work with GameManager
        Assert.DoesNotThrow(() => uiManager.UpdateUIState());
    }

    [Test]
    public void GameUIManager_StartButtonTriggersGameStart()
    {
        // Test that clicking start button calls GameManager.StartGame()
        Assert.AreEqual(GameManager.GameState.Start, gameManager.CurrentGameState);
        
        // Setup paddle for ball spawning
        GameObject paddleGO = new GameObject("Paddle");
        PlayerPaddle paddle = paddleGO.AddComponent<PlayerPaddle>();
        gameManager.RegisterPaddleForSpawning(paddleGO.transform);
        
        // Simulate button click via reflection (since we can't click in unit tests)
        var onStartMethod = typeof(GameUIManager).GetMethod("OnStartGameClicked",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onStartMethod?.Invoke(uiManager, null);
        
        // Game should now be in Playing state
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);

        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(paddleGO);
        else
            Object.DestroyImmediate(paddleGO);
    }

    [Test]
    public void GameUIManager_RestartButtonResetsAndStartsGame()
    {
        // Setup game state
        GameObject paddleGO = new GameObject("Paddle");
        PlayerPaddle paddle = paddleGO.AddComponent<PlayerPaddle>();
        gameManager.RegisterPaddleForSpawning(paddleGO.transform);
        
        // Start game and add some score
        gameManager.StartGame();
        gameManager.AddScore(100);
        gameManager.SetGameState(GameManager.GameState.GameOver);
        
        Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentGameState);
        Assert.AreEqual(100, gameManager.GetScore());
        
        // Simulate restart button click
        var onRestartMethod = typeof(GameUIManager).GetMethod("OnRestartGameClicked",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        onRestartMethod?.Invoke(uiManager, null);
        
        // Game should be playing with reset score
        Assert.AreEqual(GameManager.GameState.Playing, gameManager.CurrentGameState);
        Assert.AreEqual(0, gameManager.GetScore());

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
        
        if (Application.isPlaying)
            Object.Destroy(paddleGO);
        else
            Object.DestroyImmediate(paddleGO);
    }

    [Test]
    public void GameUIManager_UpdateUIStateDoesNotThrow()
    {
        // Test that UpdateUIState works without UI panels assigned
        Assert.DoesNotThrow(() => uiManager.UpdateUIState());
        Assert.DoesNotThrow(() => uiManager.ShowStartPanel());
    }

    [Test]
    public void GameUIManager_HandlesNullGameManagerGracefully()
    {
        // Test UIManager behavior when GameManager is null
        var gameManagerField = typeof(GameUIManager).GetField("gameManager", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        gameManagerField?.SetValue(uiManager, null);
        
        Assert.DoesNotThrow(() => uiManager.UpdateUIState());
        
        // Button clicks should not throw when GameManager is null
        var onStartMethod = typeof(GameUIManager).GetMethod("OnStartGameClicked",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.DoesNotThrow(() => onStartMethod?.Invoke(uiManager, null));
        
        var onRestartMethod = typeof(GameUIManager).GetMethod("OnRestartGameClicked",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.DoesNotThrow(() => onRestartMethod?.Invoke(uiManager, null));
    }
}