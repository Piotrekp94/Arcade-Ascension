using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class PlayerPaddleTests
{
    private PlayerPaddle playerPaddle;
    private GameObject paddleGO;

    [SetUp]
    public void Setup()
    {
        paddleGO = new GameObject();
        playerPaddle = paddleGO.AddComponent<PlayerPaddle>();
        // Initialize paddle position and scale for consistent testing
        playerPaddle.transform.position = Vector2.zero;
        playerPaddle.transform.localScale = Vector3.one;
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(paddleGO);
    }

    [Test]
    public void PlayerPaddle_SetSpeedChangesSpeed()
    {
        float newSpeed = 10f;
        playerPaddle.SetSpeed(newSpeed);
        Assert.AreEqual(newSpeed, playerPaddle.speed);
    }

    [Test]
    public void PlayerPaddle_SetSizeChangesScaleX()
    {
        float newSize = 2f;
        playerPaddle.SetSize(newSize);
        Assert.AreEqual(newSize, playerPaddle.transform.localScale.x);
        Assert.AreEqual(1f, playerPaddle.transform.localScale.y); // Y should remain 1
    }
}