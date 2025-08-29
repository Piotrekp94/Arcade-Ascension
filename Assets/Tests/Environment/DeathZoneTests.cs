using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class DeathZoneTests
{
    private DeathZone deathZone;
    private GameObject deathZoneGO;
    private BoxCollider2D deathZoneCollider;
    private GameManager gameManager;

    [SetUp]
    public void Setup()
    {
        // Setup GameManager for testing
        GameObject gameManagerGO = new GameObject();
        gameManager = gameManagerGO.AddComponent<GameManager>();
        GameManager.SetInstanceForTesting(gameManager);

        // Setup DeathZone
        deathZoneGO = new GameObject();
        deathZoneCollider = deathZoneGO.AddComponent<BoxCollider2D>();
        deathZone = deathZoneGO.AddComponent<DeathZone>();
        
        // Ensure proper initialization for unit tests
        // In unit tests, Awake() might not be called automatically
        deathZone.EnsureProperInitialization();
    }

    [TearDown]
    public void Teardown()
    {
        GameManager.SetInstanceForTesting(null);
        
        if (Application.isPlaying)
        {
            if (deathZoneGO != null) Object.Destroy(deathZoneGO);
            if (gameManager != null && gameManager.gameObject != null) Object.Destroy(gameManager.gameObject);
        }
        else
        {
            if (deathZoneGO != null) Object.DestroyImmediate(deathZoneGO);
            if (gameManager != null && gameManager.gameObject != null) Object.DestroyImmediate(gameManager.gameObject);
        }
    }

    [Test]
    public void DeathZone_HasRequiredComponents()
    {
        // Test that DeathZone component has BoxCollider2D
        Assert.IsNotNull(deathZoneCollider);
        Assert.IsNotNull(deathZone);
    }

    [Test]
    public void DeathZone_ColliderIsTrigger()
    {
        // DeathZone should be a trigger to detect when ball enters
        Assert.IsTrue(deathZoneCollider.isTrigger);
    }

    [Test]
    public void DeathZone_CanDetectBallEntering()
    {
        // Test that DeathZone can detect when a ball enters its trigger
        GameObject ballGO = new GameObject("Ball");
        ballGO.tag = "Ball"; // Ball should be tagged as "Ball"
        ballGO.AddComponent<CircleCollider2D>();
        ballGO.AddComponent<Rigidbody2D>();

        bool ballDetected = false;
        deathZone.OnBallLost += () => ballDetected = true;

        // Simulate trigger enter
        deathZone.SimulateBallEntry(ballGO);
        
        Assert.IsTrue(ballDetected);

        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(ballGO);
        else
            Object.DestroyImmediate(ballGO);
    }

    [Test]
    public void DeathZone_OnlyDetectsBallObjects()
    {
        // Test that DeathZone only reacts to objects tagged as "Ball"
        GameObject nonBallGO = new GameObject("NotBall");
        nonBallGO.tag = "Player"; // Different tag
        nonBallGO.AddComponent<CircleCollider2D>();

        bool ballDetected = false;
        deathZone.OnBallLost += () => ballDetected = true;

        // Simulate trigger enter with non-ball object
        deathZone.SimulateBallEntry(nonBallGO);
        
        Assert.IsFalse(ballDetected);

        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(nonBallGO);
        else
            Object.DestroyImmediate(nonBallGO);
    }

    [Test]
    public void DeathZone_TriggersGameOverWhenBallLost()
    {
        // Test that DeathZone triggers game over when ball is lost
        GameObject ballGO = new GameObject("Ball");
        ballGO.tag = "Ball";
        ballGO.AddComponent<CircleCollider2D>();

        // Initial game state should not be GameOver
        Assert.AreNotEqual(GameManager.GameState.GameOver, gameManager.CurrentGameState);

        // Simulate ball entering death zone
        deathZone.SimulateBallEntry(ballGO);

        // Game state should now be GameOver
        Assert.AreEqual(GameManager.GameState.GameOver, gameManager.CurrentGameState);

        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(ballGO);
        else
            Object.DestroyImmediate(ballGO);
    }

    [Test]
    public void DeathZone_DestroysBallOnEntry()
    {
        // Test that ball is destroyed when it enters the death zone
        GameObject ballGO = new GameObject("Ball");
        ballGO.tag = "Ball";
        ballGO.AddComponent<CircleCollider2D>();

        Assert.IsNotNull(ballGO);

        // Simulate ball entering death zone
        deathZone.SimulateBallEntry(ballGO);

        // Ball should be marked for destruction (we can't easily test actual destruction in unit tests)
        // But we can verify the DeathZone attempted to destroy it
        Assert.IsTrue(deathZone.WasBallDestroyed());
    }

    [Test]
    public void DeathZone_HandlesNullGameManagerGracefully()
    {
        // Test that DeathZone doesn't crash when GameManager is null
        GameManager.SetInstanceForTesting(null);
        
        GameObject ballGO = new GameObject("Ball");
        ballGO.tag = "Ball";
        ballGO.AddComponent<CircleCollider2D>();

        Assert.DoesNotThrow(() => {
            deathZone.SimulateBallEntry(ballGO);
        });

        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(ballGO);
        else
            Object.DestroyImmediate(ballGO);
    }
}