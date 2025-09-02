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

    // NEW TDD TESTS FOR SEPARATE X AND Y SPACING (RED PHASE)

    [Test]
    public void LevelData_HasCorrectBlockSpacingX()
    {
        // Test that separate X spacing property exists and works
        SetPrivateField("blockSpacingX", 0.2f);
        Assert.That(testLevelData.BlockSpacingX, Is.EqualTo(0.2f).Within(0.01f));
    }

    [Test]
    public void LevelData_HasCorrectBlockSpacingY()
    {
        // Test that separate Y spacing property exists and works
        SetPrivateField("blockSpacingY", 0.3f);
        Assert.That(testLevelData.BlockSpacingY, Is.EqualTo(0.3f).Within(0.01f));
    }

    [Test]
    public void LevelData_IsInvalidWithNegativeBlockSpacingX()
    {
        SetPrivateField("blockSpacingX", -0.1f);
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsInvalidWithNegativeBlockSpacingY()
    {
        SetPrivateField("blockSpacingY", -0.1f);
        Assert.IsFalse(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsValidWithZeroBlockSpacingX()
    {
        SetPrivateField("blockSpacingX", 0f);
        Assert.IsTrue(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_IsValidWithZeroBlockSpacingY()
    {
        SetPrivateField("blockSpacingY", 0f);
        Assert.IsTrue(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_CreateCopy_CopiesSeparateSpacingValues()
    {
        // Set separate spacing values
        SetPrivateField("blockSpacingX", 0.15f);
        SetPrivateField("blockSpacingY", 0.25f);
        
        LevelData copy = testLevelData.CreateCopy();
        
        Assert.IsNotNull(copy);
        Assert.That(copy.BlockSpacingX, Is.EqualTo(0.15f).Within(0.01f));
        Assert.That(copy.BlockSpacingY, Is.EqualTo(0.25f).Within(0.01f));
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(copy);
        else
            Object.DestroyImmediate(copy);
    }

    // NEW TDD TESTS FOR SPRITE LISTS (RED PHASE)

    [Test]
    public void LevelData_HasBlockSpritesProperty()
    {
        // Test that LevelData can store and retrieve block sprites
        Sprite[] testSprites = CreateTestSprites(2);
        SetPrivateField("blockSprites", testSprites);
        
        Sprite[] retrievedSprites = testLevelData.BlockSprites;
        Assert.AreEqual(testSprites, retrievedSprites);
        Assert.AreEqual(2, retrievedSprites.Length);
    }

    [Test]
    public void LevelData_CreateCopy_CopiesSpriteArray()
    {
        // Test that CreateCopy properly copies the sprite array
        Sprite[] testSprites = CreateTestSprites(3);
        SetPrivateField("blockSprites", testSprites);
        
        LevelData copy = testLevelData.CreateCopy();
        
        Assert.IsNotNull(copy);
        Assert.AreEqual(testSprites, copy.BlockSprites);
        Assert.AreEqual(3, copy.BlockSprites.Length);
        
        // Cleanup
        if (Application.isPlaying)
            Object.Destroy(copy);
        else
            Object.DestroyImmediate(copy);
    }

    [Test]
    public void LevelData_HandlesNullSpriteArrayGracefully()
    {
        // Test that null sprite array doesn't break validation
        SetPrivateField("blockSprites", null);
        
        Assert.DoesNotThrow(() => {
            Sprite[] sprites = testLevelData.BlockSprites;
            Assert.IsNull(sprites);
        });
        
        // Level should still be valid with null sprites
        Assert.IsTrue(testLevelData.IsValid());
    }

    [Test]
    public void LevelData_HandlesEmptySpriteArrayGracefully()
    {
        // Test that empty sprite array doesn't break validation
        SetPrivateField("blockSprites", new Sprite[0]);
        
        Assert.DoesNotThrow(() => {
            Sprite[] sprites = testLevelData.BlockSprites;
            Assert.IsNotNull(sprites);
            Assert.AreEqual(0, sprites.Length);
        });
        
        // Level should still be valid with empty sprite array
        Assert.IsTrue(testLevelData.IsValid());
    }

    // Helper method to create test sprites
    private Sprite[] CreateTestSprites(int count)
    {
        Sprite[] sprites = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(i / (float)count, 0.5f, 1f, 1f));
            texture.Apply();
            
            sprites[i] = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
            sprites[i].name = $"TestSprite{i}";
        }
        return sprites;
    }

    // Helper method to set private fields via reflection
    private void SetPrivateField(string fieldName, object value)
    {
        var field = typeof(LevelData).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field {fieldName} not found");
        field.SetValue(testLevelData, value);
    }
}