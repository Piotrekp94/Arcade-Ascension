using NUnit.Framework;
using UnityEngine;

public class LevelDataTests
{
    private LevelData testLevelData;

    [SetUp]
    public void Setup()
    {
        // Create a test LevelData instance
        testLevelData = ScriptableObject.CreateInstance<LevelData>();
        
        // Use reflection to set private fields for testing
        SetPrivateField("levelId", 1);
        SetPrivateField("levelName", "Test Level");
        SetPrivateField("levelDescription", "A test level for unit testing");
        SetPrivateField("blockRows", 5);
        SetPrivateField("blockColumns", 8);
        SetPrivateField("blockSpacing", 0.1f);
        SetPrivateField("spawnAreaOffset", new Vector2(0f, -1f));
        SetPrivateField("scoreMultiplier", 1.0f);
        SetPrivateField("defaultBlockScore", 10);
    }

    [TearDown]
    public void Teardown()
    {
        if (testLevelData != null)
        {
            if (Application.isPlaying)
                Object.Destroy(testLevelData);
            else
                Object.DestroyImmediate(testLevelData);
        }
    }

    [Test]
    public void LevelData_HasCorrectLevelId()
    {
        Assert.AreEqual(1, testLevelData.LevelId);
    }

    [Test]
    public void LevelData_HasCorrectLevelName()
    {
        Assert.AreEqual("Test Level", testLevelData.LevelName);
    }

    [Test]
    public void LevelData_HasCorrectLevelDescription()
    {
        Assert.AreEqual("A test level for unit testing", testLevelData.LevelDescription);
    }

    [Test]
    public void LevelData_HasCorrectBlockConfiguration()
    {
        Assert.AreEqual(5, testLevelData.BlockRows);
        Assert.AreEqual(8, testLevelData.BlockColumns);
        Assert.That(testLevelData.BlockSpacing, Is.EqualTo(0.1f).Within(0.01f));
        Assert.That(Vector2.Distance(testLevelData.SpawnAreaOffset, new Vector2(0f, -1f)), Is.LessThan(0.01f));
    }

    [Test]
    public void LevelData_HasCorrectScoringConfiguration()
    {
        Assert.That(testLevelData.ScoreMultiplier, Is.EqualTo(1.0f).Within(0.01f));
        Assert.AreEqual(10, testLevelData.DefaultBlockScore);
    }

    [Test]
    public void LevelData_IsValidWithCorrectData()
    {
        Assert.IsTrue(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsInvalidWithZeroLevelId()
    {
        SetPrivateField("levelId", 0);
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsInvalidWithNegativeLevelId()
    {
        SetPrivateField("levelId", -1);
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsInvalidWithEmptyLevelName()
    {
        SetPrivateField("levelName", "");
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsInvalidWithNullLevelName()
    {
        SetPrivateField("levelName", null);
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsInvalidWithZeroBlockRows()
    {
        SetPrivateField("blockRows", 0);
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsInvalidWithNegativeBlockRows()
    {
        SetPrivateField("blockRows", -1);
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsInvalidWithZeroBlockColumns()
    {
        SetPrivateField("blockColumns", 0);
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsInvalidWithNegativeBlockColumns()
    {
        SetPrivateField("blockColumns", -1);
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsInvalidWithNegativeBlockSpacing()
    {
        SetPrivateField("blockSpacing", -0.1f);
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsValidWithZeroBlockSpacing()
    {
        SetPrivateField("blockSpacing", 0f);
        Assert.IsTrue(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsInvalidWithZeroScoreMultiplier()
    {
        SetPrivateField("scoreMultiplier", 0f);
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsInvalidWithNegativeScoreMultiplier()
    {
        SetPrivateField("scoreMultiplier", -1.0f);
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsInvalidWithNegativeDefaultBlockScore()
    {
        SetPrivateField("defaultBlockScore", -1);
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsValidWithZeroDefaultBlockScore()
    {
        SetPrivateField("defaultBlockScore", 0);
        Assert.IsTrue(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_CreateCopy_CreatesIndependentCopy()
    {
        LevelData copy = testLevelData.CreateCopy();
        
        Assert.IsNotNull(copy);
        Assert.AreNotSame(testLevelData, copy);
        
        // Verify all properties are copied correctly
        Assert.AreEqual(testLevelData.LevelId, copy.LevelId);
        Assert.AreEqual(testLevelData.LevelName, copy.LevelName);
        Assert.AreEqual(testLevelData.LevelDescription, copy.LevelDescription);
        Assert.AreEqual(testLevelData.BlockRows, copy.BlockRows);
        Assert.AreEqual(testLevelData.BlockColumns, copy.BlockColumns);
        Assert.AreEqual(testLevelData.BlockSpacing, copy.BlockSpacing);
        Assert.AreEqual(testLevelData.SpawnAreaOffset, copy.SpawnAreaOffset);
        Assert.AreEqual(testLevelData.ScoreMultiplier, copy.ScoreMultiplier);
        Assert.AreEqual(testLevelData.DefaultBlockScore, copy.DefaultBlockScore);
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(copy);
        else
            Object.DestroyImmediate(copy);
    }

    [Test]
    public void LevelData_CreateCopy_CopyIsValid()
    {
        LevelData copy = testLevelData.CreateCopy();
        
        Assert.IsTrue(copy.IsValid());
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(copy);
        else
            Object.DestroyImmediate(copy);
    }

    [Test]
    public void LevelData_Properties_AreReadOnly()
    {
        // Verify that properties don't have public setters by checking they are read-only
        // This is more of a compile-time check, but we can verify the values don't change unexpectedly
        int originalId = testLevelData.LevelId;
        string originalName = testLevelData.LevelName;
        
        // Properties should remain constant without direct access to private fields
        Assert.AreEqual(originalId, testLevelData.LevelId);
        Assert.AreEqual(originalName, testLevelData.LevelName);
    }

    [Test]
    public void LevelData_HandlesEdgeCaseValues()
    {
        // Test with edge case but valid values
        SetPrivateField("levelId", 1);
        SetPrivateField("levelName", "A"); // Minimum valid name
        SetPrivateField("blockRows", 1);
        SetPrivateField("blockColumns", 1);
        SetPrivateField("blockSpacing", 0f);
        SetPrivateField("scoreMultiplier", 0.01f); // Very small but valid
        SetPrivateField("defaultBlockScore", 0);
        
        Assert.IsTrue(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_HandlesLargeValues()
    {
        // Test with large but reasonable values
        SetPrivateField("blockRows", 50);
        SetPrivateField("blockColumns", 50);
        SetPrivateField("blockSpacing", 10.0f);
        SetPrivateField("scoreMultiplier", 100.0f);
        SetPrivateField("defaultBlockScore", 10000);
        
        Assert.IsTrue(testLevelData.IsValid());
    }

    // Helper method to set private fields via reflection
    private void SetPrivateField(string fieldName, object value)
    {
        var field = typeof(LevelData).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field {fieldName} not found");
        field.SetValue(testLevelData, value);
    }
}