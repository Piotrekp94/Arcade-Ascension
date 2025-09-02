using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

[TestFixture]
public class LevelLifecycleManagerTests
{
    private GameObject lifecycleManagerGO;
    private LevelLifecycleManager lifecycleManager;
    private LevelData testLevelData;

    [SetUp]
    public void Setup()
    {
        // Create test GameObjects
        lifecycleManagerGO = new GameObject("LevelLifecycleManager");
        lifecycleManager = lifecycleManagerGO.AddComponent<LevelLifecycleManager>();
        
        // Set the instance for testing
        LevelLifecycleManager.SetInstanceForTesting(lifecycleManager);
        
        // Create test level data
        testLevelData = ScriptableObject.CreateInstance<LevelData>();
        
        // Use reflection to set private fields for testing
        SetPrivateField(testLevelData, "levelName", "TestLevel");
        SetPrivateField(testLevelData, "blockRows", 3);
        SetPrivateField(testLevelData, "blockColumns", 5);
        SetPrivateField(testLevelData, "blockSpacingX", 0.1f);
        SetPrivateField(testLevelData, "blockSpacingY", 0.1f);
        SetPrivateField(testLevelData, "spawnAreaOffset", Vector2.zero);
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up test objects
        if (lifecycleManagerGO != null)
        {
            Object.DestroyImmediate(lifecycleManagerGO);
        }
        
        if (testLevelData != null)
        {
            Object.DestroyImmediate(testLevelData);
        }
        
        // Reset singleton
        LevelLifecycleManager.SetInstanceForTesting(null);
    }

    [Test]
    public void LevelLifecycleManager_HasSingletonInstance()
    {
        // Test that singleton pattern works
        Assert.IsNotNull(LevelLifecycleManager.Instance);
        Assert.AreEqual(lifecycleManager, LevelLifecycleManager.Instance);
    }

    [Test]
    public void LevelLifecycleManager_InitiallyHasNoSpawnedObjects()
    {
        // Test initial state
        Assert.IsFalse(lifecycleManager.HasSpawnedObjects());
        Assert.AreEqual(0, lifecycleManager.GetSpawnedBlockCount());
        Assert.IsNull(lifecycleManager.GetPaddleComponent());
        Assert.IsNull(lifecycleManager.GetBallComponent());
        Assert.IsNull(lifecycleManager.GetBallGameObject());
    }

    [Test]
    public void LevelLifecycleManager_SpawnLevelObjects_RequiresLevelData()
    {
        // Expect error logs since no prefabs are assigned and we're trying to spawn objects
        LogAssert.Expect(LogType.Error, "LevelLifecycleManager: No wall prefab assigned!");
        LogAssert.Expect(LogType.Error, "LevelLifecycleManager: No paddle prefab assigned!");
        LogAssert.Expect(LogType.Error, "LevelLifecycleManager: No level data provided for block spawning!");
        LogAssert.Expect(LogType.Error, "LevelLifecycleManager: No ball prefab assigned!");
        
        // Test that spawning with null level data is handled gracefully
        Assert.DoesNotThrow(() => {
            lifecycleManager.SpawnLevelObjects(null);
        });
        
        // Should still have no objects since no prefabs are assigned
        Assert.IsFalse(lifecycleManager.HasSpawnedObjects());
    }

    [Test]
    public void LevelLifecycleManager_DestroyLevelObjects_HandlesEmptyState()
    {
        // Test that destroying objects when none exist is handled gracefully
        Assert.DoesNotThrow(() => {
            lifecycleManager.DestroyLevelObjects();
        });
        
        // Should still have no objects
        Assert.IsFalse(lifecycleManager.HasSpawnedObjects());
    }

    [Test]
    public void LevelLifecycleManager_SpawnLevelObjects_WithValidData()
    {
        // Expect error logs since no prefabs are assigned in test environment
        LogAssert.Expect(LogType.Error, "LevelLifecycleManager: No wall prefab assigned!");
        LogAssert.Expect(LogType.Error, "LevelLifecycleManager: No paddle prefab assigned!");
        LogAssert.Expect(LogType.Error, "LevelLifecycleManager: No block prefab assigned!");
        LogAssert.Expect(LogType.Error, "LevelLifecycleManager: No ball prefab assigned!");
        // Note: "Cannot spawn ball - no paddle available!" doesn't occur because 
        // ball spawning returns early when ballPrefab is null
        
        // Test that the method can be called without throwing exceptions
        Assert.DoesNotThrow(() => {
            lifecycleManager.SpawnLevelObjects(testLevelData);
        });
    }

    [Test]
    public void LevelLifecycleManager_GetSpawnedBlockCount_ReturnsZeroInitially()
    {
        // Test block count tracking
        Assert.AreEqual(0, lifecycleManager.GetSpawnedBlockCount());
    }

    [Test]
    public void LevelLifecycleManager_ComponentAccessors_ReturnNullInitially()
    {
        // Test that component accessors return null when no objects spawned
        Assert.IsNull(lifecycleManager.GetPaddleComponent());
        Assert.IsNull(lifecycleManager.GetBallComponent());
        Assert.IsNull(lifecycleManager.GetBallGameObject());
    }

    [Test]
    public void LevelLifecycleManager_HasSpawnedObjects_FalseInitially()
    {
        // Test spawned objects detection
        Assert.IsFalse(lifecycleManager.HasSpawnedObjects());
    }

    [Test]
    public void LevelLifecycleManager_DestroyAfterSpawn_ClearsState()
    {
        // Expect error logs since no prefabs are assigned in test environment
        LogAssert.Expect(LogType.Error, "LevelLifecycleManager: No wall prefab assigned!");
        LogAssert.Expect(LogType.Error, "LevelLifecycleManager: No paddle prefab assigned!");
        LogAssert.Expect(LogType.Error, "LevelLifecycleManager: No block prefab assigned!");
        LogAssert.Expect(LogType.Error, "LevelLifecycleManager: No ball prefab assigned!");
        // Note: "Cannot spawn ball - no paddle available!" doesn't occur because 
        // ball spawning returns early when ballPrefab is null
        
        // Test full spawn/destroy cycle
        lifecycleManager.SpawnLevelObjects(testLevelData);
        lifecycleManager.DestroyLevelObjects();
        
        // Should be back to initial state
        Assert.IsFalse(lifecycleManager.HasSpawnedObjects());
        Assert.AreEqual(0, lifecycleManager.GetSpawnedBlockCount());
        Assert.IsNull(lifecycleManager.GetPaddleComponent());
        Assert.IsNull(lifecycleManager.GetBallComponent());
        Assert.IsNull(lifecycleManager.GetBallGameObject());
    }

    // Integration tests would require actual prefabs and more complex setup
    // For now, these basic tests ensure the class structure works correctly

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field {fieldName} not found");
        field.SetValue(target, value);
    }
}