using NUnit.Framework;
using UnityEngine;

public class GlobalGameConfigTests
{
    private GlobalGameConfig testConfig;

    [SetUp]
    public void Setup()
    {
        // Create a test config instance
        testConfig = ScriptableObject.CreateInstance<GlobalGameConfig>();
    }

    [TearDown]
    public void Teardown()
    {
        if (testConfig != null)
        {
            if (Application.isPlaying)
                Object.Destroy(testConfig);
            else
                Object.DestroyImmediate(testConfig);
        }
        
        // Clean up singleton for next test
        GlobalGameConfig.SetInstanceForTesting(null);
    }

    [Test]
    public void GlobalGameConfig_HasDefaultValues()
    {
        Assert.AreEqual(120f, testConfig.GlobalTimeLimit);
    }

    [Test]
    public void GlobalGameConfig_IsValidWithDefaults()
    {
        Assert.IsTrue(testConfig.IsValid());
    }

    [Test]
    public void GlobalGameConfig_SingletonAccessWorks()
    {
        GlobalGameConfig.SetInstanceForTesting(testConfig);
        
        Assert.AreSame(testConfig, GlobalGameConfig.Instance);
        Assert.AreEqual(120f, GlobalGameConfig.Instance.GlobalTimeLimit);
    }

    [Test]
    public void GlobalGameConfig_HandlesEdgeCaseValues()
    {
        // Use reflection to test with edge case values
        SetPrivateField("globalTimeLimit", 1f);
        
        Assert.IsTrue(testConfig.IsValid());
    }

    [Test]
    public void GlobalGameConfig_IsInvalidWithNegativeValues()
    {
        // Test negative time limit
        SetPrivateField("globalTimeLimit", -1f);
        Assert.IsFalse(testConfig.IsValid());
        
        // Test zero time limit
        SetPrivateField("globalTimeLimit", 0f);
        Assert.IsFalse(testConfig.IsValid());
    }

    private void SetPrivateField(string fieldName, object value)
    {
        var field = typeof(GlobalGameConfig).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field {fieldName} not found");
        field.SetValue(testConfig, value);
    }
}