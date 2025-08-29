using UnityEngine;
using TMPro; // Assuming TextMeshPro is used for UI text

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Start,
        Playing,
        GameOver
    }

    public GameState CurrentGameState { get; private set; }

    [SerializeField]
    private TextMeshProUGUI _scoreText; // Reference to UI TextMeshPro element
    private int score;
    [SerializeField]
    private float difficultyMultiplier = 1.0f; // Multiplier for game difficulty

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
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
                break;
        }
    }
}