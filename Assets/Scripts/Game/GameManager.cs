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
    public event Action OnBallSpawned;

    [SerializeField]
    private TextMeshProUGUI _scoreText; // Reference to UI TextMeshPro element
    private int score;
    [SerializeField]
    private float difficultyMultiplier = 1.0f; // Multiplier for game difficulty
    
    // Ball spawning system
    [SerializeField]
    private GameObject ballPrefab; // Ball prefab reference for spawning
    [SerializeField]
    private float respawnDelay = 3.0f; // Time before ball respawns after being lost
    
    private Transform registeredPaddle; // Paddle reference for ball spawning
    private bool respawnTimerActive = false;
    private float respawnTimer = 0f;

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

    void Update()
    {
        // Handle respawn timer
        if (respawnTimerActive)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                SpawnBallAtPaddle();
                respawnTimerActive = false;
            }
        }
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
        // Only handle ball loss during gameplay
        if (CurrentGameState == GameState.Playing)
        {
            // Start respawn timer instead of immediate game over
            StartRespawnTimer();
        }
    }

    public void ResetGame()
    {
        // Reset game state and score
        score = 0;
        UpdateScoreUI();
        SetGameState(GameState.Start);
        
        // Stop respawn timer if active
        respawnTimerActive = false;
        respawnTimer = 0f;
    }

    public void StartGame()
    {
        // Start a new game
        SetGameState(GameState.Playing);
        
        // Spawn initial ball attached to paddle
        SpawnInitialBallAtPaddle();
    }

    // Ball spawning system methods
    public void StartRespawnTimer()
    {
        if (CurrentGameState == GameState.Playing)
        {
            respawnTimerActive = true;
            respawnTimer = respawnDelay;
        }
    }

    public bool IsRespawnTimerActive()
    {
        return respawnTimerActive;
    }

    public float GetRespawnDelay()
    {
        return respawnDelay;
    }

    public void SetRespawnDelay(float delay)
    {
        respawnDelay = delay;
    }

    public void RegisterPaddleForSpawning(Transform paddle)
    {
        registeredPaddle = paddle;
    }

    public Transform GetRegisteredPaddle()
    {
        return registeredPaddle;
    }

    public GameObject GetBallPrefab()
    {
        return ballPrefab;
    }

    public void SetBallPrefab(GameObject prefab)
    {
        ballPrefab = prefab;
    }

    public GameObject SpawnBallAtPosition(Vector2 position)
    {
        if (ballPrefab == null)
        {
            // Create a basic ball if no prefab is assigned
            GameObject ball = new GameObject("Ball");
            ball.tag = "Ball";
            ball.AddComponent<CircleCollider2D>();
            ball.AddComponent<Rigidbody2D>();
            ball.AddComponent<Ball>();
            ball.transform.position = position;
            
            OnBallSpawned?.Invoke();
            return ball;
        }
        else
        {
            GameObject spawnedBall = Instantiate(ballPrefab, position, Quaternion.identity);
            OnBallSpawned?.Invoke();
            return spawnedBall;
        }
    }

    private void SpawnBallAtPaddle()
    {
        if (registeredPaddle != null)
        {
            Vector2 spawnPosition = registeredPaddle.position + Vector3.up * 0.5f; // Spawn slightly above paddle
            GameObject spawnedBall = SpawnBallAtPosition(spawnPosition);
            
            // Attach the spawned ball to the paddle
            if (spawnedBall != null)
            {
                Ball ballComponent = spawnedBall.GetComponent<Ball>();
                PlayerPaddle paddle = registeredPaddle.GetComponent<PlayerPaddle>();
                
                if (ballComponent != null && paddle != null)
                {
                    paddle.AttachBall(ballComponent);
                }
            }
        }
    }

    private void SpawnInitialBallAtPaddle()
    {
        // Same logic as SpawnBallAtPaddle but called at game start
        if (registeredPaddle != null)
        {
            Vector2 spawnPosition = registeredPaddle.position + Vector3.up * 0.5f; // Spawn slightly above paddle
            GameObject spawnedBall = SpawnBallAtPosition(spawnPosition);
            
            // Attach the spawned ball to the paddle
            if (spawnedBall != null)
            {
                Ball ballComponent = spawnedBall.GetComponent<Ball>();
                PlayerPaddle paddle = registeredPaddle.GetComponent<PlayerPaddle>();
                
                if (ballComponent != null && paddle != null)
                {
                    paddle.AttachBall(ballComponent);
                }
            }
        }
    }

    // Method to manually update respawn timer for testing
    public void UpdateRespawnTimer(float deltaTime)
    {
        if (respawnTimerActive)
        {
            respawnTimer -= deltaTime;
            if (respawnTimer <= 0f)
            {
                SpawnBallAtPaddle();
                respawnTimerActive = false;
            }
        }
    }

    // Manual testing and debugging methods
    [ContextMenu("Spawn Initial Ball")]
    public void ForceSpawnInitialBall()
    {
        SpawnInitialBallAtPaddle();
    }

    [ContextMenu("Force Spawn Ball")]
    public void ForceSpawnBall()
    {
        SpawnBallAtPaddle();
    }

    [ContextMenu("Log System Status")]
    public void LogSystemStatus()
    {
        Debug.Log($"GameManager Status:" +
                  $"\n- Current State: {CurrentGameState}" +
                  $"\n- Registered Paddle: {(registeredPaddle != null ? registeredPaddle.name : "NULL")}" +
                  $"\n- Ball Prefab: {(ballPrefab != null ? ballPrefab.name : "NULL")}" +
                  $"\n- Respawn Timer Active: {respawnTimerActive}" +
                  $"\n- Respawn Delay: {respawnDelay}");
                  
        if (registeredPaddle != null)
        {
            PlayerPaddle paddle = registeredPaddle.GetComponent<PlayerPaddle>();
            if (paddle != null)
            {
                Debug.Log($"Paddle Status:" +
                          $"\n- Has Attached Ball: {paddle.HasAttachedBall()}" +
                          $"\n- Attached Ball: {(paddle.GetAttachedBall() != null ? paddle.GetAttachedBall().name : "NULL")}");
            }
        }
    }
}