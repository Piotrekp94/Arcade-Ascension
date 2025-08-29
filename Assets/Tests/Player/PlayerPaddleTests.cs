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
    public void PlayerPaddle_HasCorrectInitialPosition()
    {
        // Test that paddle initializes at correct position
        Assert.AreEqual(Vector2.zero, (Vector2)playerPaddle.transform.position);
    }

    [Test]
    public void PlayerPaddle_HasCorrectInitialScale()
    {
        // Test that paddle has correct initial scale
        Assert.AreEqual(Vector3.one, playerPaddle.transform.localScale);
    }
}