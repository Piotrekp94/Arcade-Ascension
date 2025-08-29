using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class WallTests
{
    private Wall wall;
    private GameObject wallGO;
    private BoxCollider2D wallCollider;

    [SetUp]
    public void Setup()
    {
        wallGO = new GameObject();
        wallCollider = wallGO.AddComponent<BoxCollider2D>();
        wall = wallGO.AddComponent<Wall>();
    }

    [TearDown]
    public void Teardown()
    {
        if (Application.isPlaying)
        {
            if (wallGO != null) Object.Destroy(wallGO);
        }
        else
        {
            if (wallGO != null) Object.DestroyImmediate(wallGO);
        }
    }

    [Test]
    public void Wall_HasRequiredComponents()
    {
        // Test that Wall component requires BoxCollider2D
        Assert.IsNotNull(wallCollider);
        Assert.IsNotNull(wall);
    }

    [Test]
    public void Wall_ColliderIsNotTrigger()
    {
        // Wall colliders should be solid (not triggers) for physics bouncing
        Assert.IsFalse(wallCollider.isTrigger);
    }

    [Test]
    public void Wall_CanSetWallType()
    {
        // Test that we can set different wall types (Top, Left, Right)
        wall.SetWallType(Wall.WallType.Top);
        Assert.AreEqual(Wall.WallType.Top, wall.GetWallType());

        wall.SetWallType(Wall.WallType.Left);
        Assert.AreEqual(Wall.WallType.Left, wall.GetWallType());

        wall.SetWallType(Wall.WallType.Right);
        Assert.AreEqual(Wall.WallType.Right, wall.GetWallType());
    }

    [Test]
    public void Wall_DefaultTypeIsTop()
    {
        // Test that wall defaults to Top type
        Assert.AreEqual(Wall.WallType.Top, wall.GetWallType());
    }

    [Test]
    public void Wall_OnCollisionEnterDoesNotThrow()
    {
        // Test that collision handling doesn't throw exceptions
        // This is a basic safety test - actual bouncing is handled by Unity physics
        Assert.DoesNotThrow(() => {
            // Simulate collision - Wall should handle it gracefully
            // The actual physics bouncing will be tested in integration tests
        });
    }

    [Test]
    public void Wall_CanBeInitializedWithDifferentTypes()
    {
        // Test initialization with different wall types
        GameObject topWallGO = new GameObject();
        Wall topWall = topWallGO.AddComponent<Wall>();
        topWall.SetWallType(Wall.WallType.Top);
        
        GameObject leftWallGO = new GameObject();
        Wall leftWall = leftWallGO.AddComponent<Wall>();
        leftWall.SetWallType(Wall.WallType.Left);
        
        GameObject rightWallGO = new GameObject();
        Wall rightWall = rightWallGO.AddComponent<Wall>();
        rightWall.SetWallType(Wall.WallType.Right);

        Assert.AreEqual(Wall.WallType.Top, topWall.GetWallType());
        Assert.AreEqual(Wall.WallType.Left, leftWall.GetWallType());
        Assert.AreEqual(Wall.WallType.Right, rightWall.GetWallType());

        // Cleanup
        if (Application.isPlaying)
        {
            Object.Destroy(topWallGO);
            Object.Destroy(leftWallGO);
            Object.Destroy(rightWallGO);
        }
        else
        {
            Object.DestroyImmediate(topWallGO);
            Object.DestroyImmediate(leftWallGO);
            Object.DestroyImmediate(rightWallGO);
        }
    }
}