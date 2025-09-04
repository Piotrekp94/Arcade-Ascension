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
    public event Action OnGameStarted;
    public event Action OnLevelCompleted;
    public event System.Action<GameState> OnGameStateChanged;
    
    // Score events
    public event System.Action<int> OnScoreChanged;
    public event System.Action<int, int> OnScoreAdded; // (amount added, new total)
    
    // Timer events
    public event System.Action<float> OnTimerUpdated; // (time remaining)
    public event System.Action OnTimerExpired;

    [SerializeField]
    private TextMeshProUGUI _scoreText; // Reference to UI TextMeshPro element
    private int score;
    [SerializeField]
    private float difficultyMultiplier = 1.0f; // Multiplier for game difficulty
    [SerializeField]
    private float scoreMultiplier = 1.0f; // Multiplier for all score additions
    [SerializeField]
    private int defaultBlockScore = 10; // Default score value for new blocks
    
    // Ball spawning system
    [SerializeField]
    private GameObject ballPrefab; // Ball prefab reference for spawning
    [SerializeField]
    private float respawnDelay = 3.0f; // Time before ball respawns after being lost
    
    private Transform registeredPaddle; // Paddle reference for ball spawning
    private bool respawnTimerActive = false;
    private float respawnTimer = 0f;
    
    // Timer system
    [SerializeField]
    private float levelTimeLimit = 120f; // Default 2 minutes per level
    private float currentTimeRemaining;
    private bool isTimerActive = false;
    
    // Level completion tracking
    private int blocksRemaining = -1; // -1 means no level active, 0 means level complete

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
        
        // Initialize timer
        currentTimeRemaining = levelTimeLimit;
        isTimerActive = false;
        
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
        
        // Handle level timer
        if (isTimerActive && CurrentGameState == GameState.Playing)
        {
            currentTimeRemaining -= Time.deltaTime;
            OnTimerUpdated?.Invoke(currentTimeRemaining);
            
            if (currentTimeRemaining <= 0f)
            {
                OnTimerExpirationCleanup();
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

    public float GetScoreMultiplier()
    {
        return scoreMultiplier;
    }

    public void SetScoreMultiplier(float newMultiplier)
    {
        scoreMultiplier = Mathf.Max(0.0f, newMultiplier); // Ensure non-negative multiplier
    }

    public void SetScoreMultiplierWithDifficulty(float baseMultiplier)
    {
        // Apply difficulty scaling to score multiplier
        float finalMultiplier = baseMultiplier * difficultyMultiplier;
        SetScoreMultiplier(finalMultiplier);
    }

    public int GetDefaultBlockScore()
    {
        return defaultBlockScore;
    }

    public void SetDefaultBlockScore(int newScore)
    {
        defaultBlockScore = Mathf.Max(0, newScore); // Ensure non-negative score
    }

    public void AddScore(int amount)
    {
        int multipliedAmount = Mathf.RoundToInt(amount * scoreMultiplier);
        int previousScore = score;
        score += multipliedAmount;
        
        UpdateScoreUI();
        
        // Trigger score events
        OnScoreChanged?.Invoke(score);
        OnScoreAdded?.Invoke(multipliedAmount, score);
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
        
        // Fire general state change event
        OnGameStateChanged?.Invoke(CurrentGameState);
        
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
        
        // Reset level completion tracking
        blocksRemaining = -1; // No level active
        
        // Stop respawn timer if active
        respawnTimerActive = false;
        respawnTimer = 0f;
        
        // Reset timer
        StopTimer();
        currentTimeRemaining = levelTimeLimit;
    }

    public void RestartGame()
    {
        // Reset game state first
        ResetGame();
        
        // Then immediately start a new game
        StartGame();
    }

    public void StartGame()
    {
        StartGame(null); // Use default time limit
    }
    
    public void StartGame(LevelData levelData)
    {
        // Only start if in Start state to prevent multiple starts
        if (CurrentGameState != GameState.Start)
            return;
            
        // Set level-specific time limit if provided
        if (levelData != null)
        {
            SetTimeLimit(levelData.LevelTimeLimit);
            Debug.Log($"GameManager: Using level time limit of {levelData.LevelTimeLimit} seconds for {levelData.LevelName}");
        }
            
        // Start a new game
        SetGameState(GameState.Playing);
        
        // Start the level timer
        StartTimer();
        
        // Level objects should be spawned by LevelLifecycleManager before calling StartGame
        // Just ensure we have the required components
        if (LevelLifecycleManager.Instance != null)
        {
            var lifecycleManager = LevelLifecycleManager.Instance;
            
            // Update block count from spawned blocks
            int blockCount = lifecycleManager.GetSpawnedBlockCount();
            SetBlocksRemaining(blockCount);
            
            Debug.Log($"GameManager: Started game with {blockCount} blocks");
        }
        else
        {
            Debug.LogWarning("GameManager: LevelLifecycleManager not found - falling back to old ball spawning");
            // Fallback to old method if LevelLifecycleManager not available
            SpawnInitialBallAtPaddle();
        }
        
        // Fire game started event for UI updates
        OnGameStarted?.Invoke();
    }

    // Timer system methods
    public float GetTimeLimit()
    {
        return levelTimeLimit;
    }
    
    public void SetTimeLimit(float newTimeLimit)
    {
        levelTimeLimit = Mathf.Max(0f, newTimeLimit); // Ensure non-negative
        if (!isTimerActive)
        {
            currentTimeRemaining = levelTimeLimit; // Reset current time if timer not active
        }
    }
    
    public float GetTimeRemaining()
    {
        return currentTimeRemaining;
    }
    
    public bool IsTimerActive()
    {
        return isTimerActive;
    }
    
    public void StartTimer()
    {
        currentTimeRemaining = levelTimeLimit;
        isTimerActive = true;
        
        // Fire initial timer update to ensure UI displays the starting time
        OnTimerUpdated?.Invoke(currentTimeRemaining);
    }
    
    public void StopTimer()
    {
        isTimerActive = false;
    }
    
    public void UpdateTimer(float deltaTime)
    {
        // Manual timer update for testing
        if (isTimerActive && CurrentGameState == GameState.Playing)
        {
            currentTimeRemaining -= deltaTime;
            OnTimerUpdated?.Invoke(currentTimeRemaining);
            
            if (currentTimeRemaining <= 0f)
            {
                OnTimerExpirationCleanup();
            }
        }
    }
    
    public string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds < 0f)
            timeInSeconds = 0f;
            
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    private void OnTimerExpirationCleanup()
    {
        // Set timer to exactly zero and disable it
        currentTimeRemaining = 0f;
        isTimerActive = false;
        
        // Fire timer expiration event
        OnTimerExpired?.Invoke();
        
        // Clean up all level objects (same as level completion)
        if (LevelLifecycleManager.Instance != null)
        {
            LevelLifecycleManager.Instance.DestroyLevelObjects();
            Debug.Log("GameManager: Level objects destroyed after timer expiration");
        }
        
        // Return to level selection
        SetGameState(GameState.Start);
        
        // Directly notify UI as fallback (in case event subscription isn't working)
        GameUIManager uiManager = FindFirstObjectByType<GameUIManager>();
        if (uiManager != null)
        {
            Debug.Log("GameManager: Directly updating UI to show level selection");
            uiManager.UpdateUIState();
        }
        else
        {
            Debug.LogWarning("GameManager: Could not find GameUIManager for direct UI update");
        }
        
        Debug.Log("GameManager: Timer expired - returning to level selection");
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

    // Level completion system methods
    public bool CheckLevelCompletion()
    {
        // Level is complete only if we had blocks set (>= 0) and now have 0 remaining
        return blocksRemaining == 0;
    }

    public void OnBlockDestroyed()
    {
        blocksRemaining--;
        
        if (blocksRemaining <= 0 && CurrentGameState == GameState.Playing)
        {
            OnAllBlocksDestroyed();
        }
    }

    public void OnAllBlocksDestroyed()
    {
        if (CurrentGameState == GameState.Playing)
        {
            // Stop the timer on level completion
            StopTimer();
            
            // Change game state to allow level selection UI to show
            CurrentGameState = GameState.Start;
            
            // Destroy all level objects using LevelLifecycleManager
            if (LevelLifecycleManager.Instance != null)
            {
                LevelLifecycleManager.Instance.DestroyLevelObjects();
                Debug.Log("GameManager: Level objects destroyed after completion");
            }
            
            OnLevelCompleted?.Invoke();
        }
    }

    public void SetBlocksRemainingForTesting(int count)
    {
        blocksRemaining = count;
    }

    public void SetBlocksRemaining(int count)
    {
        blocksRemaining = count;
    }

    public int GetBlocksRemaining()
    {
        return blocksRemaining;
    }
}