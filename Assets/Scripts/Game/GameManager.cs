using UnityEngine;
using TMPro; // Assuming TextMeshPro is used for UI text
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    // Method to set Instance for testing purposes
    public static void SetInstanceForTesting(GameManager testInstance)
    {
        Instance = testInstance;
    }

    public enum GameState
    {
        Start,
        Playing,
        GameOver
    }

    public GameState CurrentGameState { get; private set; }
    
    // Events for game state changes
    public event Action OnGameOver;

    [SerializeField]
    private TextMeshProUGUI _scoreText; // Reference to UI TextMeshPro element
    private int score;
    [SerializeField]
    private float difficultyMultiplier = 1.0f; // Multiplier for game difficulty

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        score = 0;
        UpdateScoreUI();
        SetGameState(GameState.Start); // Initial game state
    }

    public float GetDifficultyMultiplier()
    {
        return difficultyMultiplier;
    }

    public void SetDifficultyMultiplier(float newMultiplier)
    {
        difficultyMultiplier = newMultiplier;
        // Potentially re-evaluate game parameters based on new difficulty
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }


    public int GetScore()
    {
        return score;
    }

    void UpdateScoreUI()
    {
        if (_scoreText != null)
        {
            _scoreText.text = "Score: " + score;
        }
    }


    public void SetGameState(GameState newGameState)
    {
        GameState previousState = CurrentGameState;
        CurrentGameState = newGameState;
        
        switch (CurrentGameState)
        {
            case GameState.Start:
                Debug.Log("Game State: Start");
                // Potentially show start menu, reset game elements
                break;
            case GameState.Playing:
                Debug.Log("Game State: Playing");
                // Hide start menu, start ball movement, etc.
                break;
            case GameState.GameOver:
                Debug.Log("Game State: Game Over");
                // Show game over screen, stop ball, etc.
                
                // Trigger game over event if transitioning from a different state
                if (previousState != GameState.GameOver)
                {
                    OnGameOver?.Invoke();
                }
                break;
        }
    }

    public void OnBallLost()
    {
        Debug.Log($"GameManager OnBallLost called. Current state: {CurrentGameState}");
        
        // Only handle ball loss during gameplay
        if (CurrentGameState == GameState.Playing)
        {
            Debug.Log("GameManager: Ball lost during gameplay, setting state to GameOver");
            SetGameState(GameState.GameOver);
        }
        else
        {
            Debug.Log($"GameManager: Ball lost ignored because game state is {CurrentGameState}");
        }
    }

    public void ResetGame()
    {
        // Reset game state and score
        score = 0;
        UpdateScoreUI();
        SetGameState(GameState.Start);
    }

    public void StartGame()
    {
        // Start a new game
        SetGameState(GameState.Playing);
    }
}