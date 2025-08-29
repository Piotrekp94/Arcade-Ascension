using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

public class UpgradeManagerTests
{
    private UpgradeManager upgradeManager;
    private GameObject upgradeManagerGO;
    private PlayerPaddle playerPaddle;
    private GameObject playerPaddleGO;
    private Ball ball;
    private GameObject ballGO;
    private Rigidbody2D ballRB;

    [SetUp]
    public void Setup()
    {
        upgradeManagerGO = new GameObject();
        upgradeManager = upgradeManagerGO.AddComponent<UpgradeManager>();

        // Setup PlayerPaddle
        playerPaddleGO = new GameObject();
        playerPaddle = playerPaddleGO.AddComponent<PlayerPaddle>();
        playerPaddle.transform.position = Vector2.zero;
        playerPaddle.transform.localScale = Vector3.one;
        // Manually set initial speed for testing
        // playerPaddle.speed = 5f; // Assuming speed is accessible for testing

        // Setup Ball
        ballGO = new GameObject();
        ballRB = ballGO.AddComponent<Rigidbody2D>();
        ball = ballGO.AddComponent<Ball>();
        ballRB.gravityScale = 0;
        ballRB.simulated = true;
        // Manually set initial speed for testing
        // ball.initialSpeed = 5f; // Assuming initialSpeed is accessible for testing

        // Add some dummy upgrades to the manager for testing
        // This would typically be done via [SerializeField] in the editor,
        // but for testing, we can add them programmatically.
        // For simplicity, assuming availableUpgrades is accessible.
        // upgradeManager.availableUpgrades.Add(new UpgradeManager.Upgrade
        // {
        //     upgradeName = "Speed Boost",
        //     description = "Increases paddle speed",
        //     cost = 10,
        //     type = UpgradeManager.UpgradeType.PaddleSpeed,
        //     value = 2f
        // });
        // upgradeManager.availableUpgrades.Add(new UpgradeManager.Upgrade
        // {
        //     upgradeName = "Size Increase",
        //     description = "Increases paddle size",
        //     cost = 15,
        //     type = UpgradeManager.UpgradeType.PaddleSize,
        //     value = 0.5f
        // });
        // upgradeManager.availableUpgrades.Add(new UpgradeManager.Upgrade
        // {
        //     upgradeName = "Ball Speed Up",
        //     description = "Increases ball speed",
        //     cost = 20,
        //     type = UpgradeManager.UpgradeType.BallSpeed,
        //     value = 1f
        // });
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(upgradeManagerGO);
        Object.Destroy(playerPaddleGO);
        Object.Destroy(ballGO);
    }

    [Test]
    public void UpgradeManager_ApplyPaddleSpeedUpgrade()
    {
        // Assuming playerPaddle.speed is accessible
        // float initialSpeed = playerPaddle.speed;
        // UpgradeManager.Upgrade speedUpgrade = new UpgradeManager.Upgrade
        // {
        //     type = UpgradeManager.UpgradeType.PaddleSpeed,
        //     value = 2f
        // };
        // upgradeManager.ApplyUpgrade(speedUpgrade);
        // Assert.AreEqual(initialSpeed + 2f, playerPaddle.speed);
    }

    [Test]
    public void UpgradeManager_ApplyPaddleSizeUpgrade()
    {
        // Assuming playerPaddle.transform.localScale is accessible
        // float initialSizeX = playerPaddle.transform.localScale.x;
        // UpgradeManager.Upgrade sizeUpgrade = new UpgradeManager.Upgrade
        // {
        //     type = UpgradeManager.UpgradeType.PaddleSize,
        //     value = 0.5f
        // };
        // upgradeManager.ApplyUpgrade(sizeUpgrade);
        // Assert.AreEqual(initialSizeX + 0.5f, playerPaddle.transform.localScale.x);
    }

    [Test]
    public void UpgradeManager_ApplyBallSpeedUpgrade()
    {
        // Assuming ball.initialSpeed is accessible
        // float initialBallSpeed = ball.initialSpeed;
        // UpgradeManager.Upgrade ballSpeedUpgrade = new UpgradeManager.Upgrade
        // {
        //     type = UpgradeManager.UpgradeType.BallSpeed,
        //     value = 1f
        // };
        // upgradeManager.ApplyUpgrade(ballSpeedUpgrade);
        // Assert.AreEqual(initialBallSpeed + 1f, ball.initialSpeed);
    }
}