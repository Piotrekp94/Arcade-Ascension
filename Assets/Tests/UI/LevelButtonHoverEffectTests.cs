using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using UnityEngine.EventSystems;
using System.Collections;

public class LevelButtonHoverEffectTests
{
    private LevelButtonHoverEffect hoverEffect;
    private GameObject testObject;
    private Image testImage;
    private Text testText;

    [SetUp]
    public void Setup()
    {
        testObject = new GameObject("TestLevelButton");
        testObject.transform.localScale = Vector3.one; // Ensure scale is set to (1,1,1)
        
        hoverEffect = testObject.AddComponent<LevelButtonHoverEffect>();
        testImage = testObject.AddComponent<Image>();
        
        GameObject textChildObject = new GameObject("Text");
        testText = textChildObject.AddComponent<Text>();
        testText.transform.SetParent(testObject.transform);
        
        testImage.color = Color.white;
        testText.color = Color.white;
        
        hoverEffect.enabled = true;
        
        // Manually call Awake to ensure initialization
        var awakeMethod = typeof(LevelButtonHoverEffect).GetMethod("Awake", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        awakeMethod?.Invoke(hoverEffect, null);
    }

    [TearDown]
    public void Teardown()
    {
        if (testObject != null)
        {
            if (Application.isPlaying)
                Object.Destroy(testObject);
            else
                Object.DestroyImmediate(testObject);
        }
    }

    [Test]
    public void LevelButtonHoverEffect_InitializesCorrectlyOnAwake()
    {
        var originalScale = typeof(LevelButtonHoverEffect).GetField("originalScale", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var buttonImage = typeof(LevelButtonHoverEffect).GetField("buttonImage", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var buttonText = typeof(LevelButtonHoverEffect).GetField("buttonText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var originalColor = typeof(LevelButtonHoverEffect).GetField("originalColor", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Vector3 storedOriginalScale = (Vector3)originalScale.GetValue(hoverEffect);
        Image storedButtonImage = (Image)buttonImage.GetValue(hoverEffect);
        Text storedButtonText = (Text)buttonText.GetValue(hoverEffect);
        Color storedOriginalColor = (Color)originalColor.GetValue(hoverEffect);

        Assert.AreEqual(testObject.transform.localScale, storedOriginalScale);
        Assert.AreEqual(testImage, storedButtonImage);
        Assert.AreEqual(testText, storedButtonText);
        Assert.AreEqual(Color.white, storedOriginalColor);
    }

    [Test]
    public void LevelButtonHoverEffect_OnPointerEnterStartsHoverAnimation()
    {
        var currentAnimationCoroutineField = typeof(LevelButtonHoverEffect).GetField("currentAnimationCoroutine", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.IsNull(currentAnimationCoroutineField.GetValue(hoverEffect));

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        hoverEffect.OnPointerEnter(eventData);

        Coroutine runningCoroutine = (Coroutine)currentAnimationCoroutineField.GetValue(hoverEffect);
        Assert.IsNotNull(runningCoroutine);
    }

    [Test]
    public void LevelButtonHoverEffect_OnPointerExitStartsExitAnimation()
    {
        var currentAnimationCoroutineField = typeof(LevelButtonHoverEffect).GetField("currentAnimationCoroutine", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        hoverEffect.OnPointerEnter(eventData);
        
        Coroutine firstCoroutine = (Coroutine)currentAnimationCoroutineField.GetValue(hoverEffect);
        Assert.IsNotNull(firstCoroutine);

        hoverEffect.OnPointerExit(eventData);
        
        Coroutine secondCoroutine = (Coroutine)currentAnimationCoroutineField.GetValue(hoverEffect);
        Assert.IsNotNull(secondCoroutine);
        Assert.AreNotEqual(firstCoroutine, secondCoroutine);
    }

    [Test]
    public void LevelButtonHoverEffect_StopsCurrentAnimationWhenNewAnimationStarts()
    {
        var currentAnimationCoroutineField = typeof(LevelButtonHoverEffect).GetField("currentAnimationCoroutine", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        
        hoverEffect.OnPointerEnter(eventData);
        Coroutine firstCoroutine = (Coroutine)currentAnimationCoroutineField.GetValue(hoverEffect);
        Assert.IsNotNull(firstCoroutine);

        hoverEffect.OnPointerEnter(eventData);
        Coroutine secondCoroutine = (Coroutine)currentAnimationCoroutineField.GetValue(hoverEffect);
        
        Assert.IsNotNull(secondCoroutine);
        Assert.AreNotEqual(firstCoroutine, secondCoroutine);
    }

    [UnityTest]
    public IEnumerator LevelButtonHoverEffect_ScaleChangesOnHover()
    {
        var hoverScaleField = typeof(LevelButtonHoverEffect).GetField("hoverScale", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var scaleDurationField = typeof(LevelButtonHoverEffect).GetField("scaleDuration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var originalScaleField = typeof(LevelButtonHoverEffect).GetField("originalScale", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        hoverScaleField.SetValue(hoverEffect, 1.2f);
        scaleDurationField.SetValue(hoverEffect, 0.01f); // Very fast animation

        Vector3 originalScale = testObject.transform.localScale;
        Vector3 storedOriginalScale = (Vector3)originalScaleField.GetValue(hoverEffect);
        
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        hoverEffect.OnPointerEnter(eventData);
        
        // Wait much longer to ensure animation completes
        yield return new WaitForSeconds(1.0f);
        
        Vector3 finalScale = testObject.transform.localScale;
        Vector3 expectedFinalScale = storedOriginalScale * 1.2f;
        
        // Test that scale increased from original
        Assert.Greater(finalScale.x, originalScale.x, $"Final scale ({finalScale.x}) should be greater than original ({originalScale.x}). Stored original: {storedOriginalScale.x}, Expected: {expectedFinalScale.x}");
        
        // Test that scale moved in the right direction - use more tolerant comparison
        float scaleIncrease = finalScale.x - originalScale.x;
        Assert.Greater(scaleIncrease, 0.02f, $"Scale should increase by at least 0.02. Actual increase: {scaleIncrease}, Final: {finalScale.x}, Original: {originalScale.x}");
        
        // Verify it's moving toward the expected scale - the scale should be much closer to target (1.2) than to original (1.0)
        float distanceToTarget = Mathf.Abs(finalScale.x - expectedFinalScale.x);
        float distanceToOriginal = Mathf.Abs(finalScale.x - originalScale.x);
        Assert.Less(distanceToTarget, distanceToOriginal, $"Should be closer to target ({expectedFinalScale.x}) than original ({originalScale.x}). Distance to target: {distanceToTarget}, Distance to original: {distanceToOriginal}, Final scale: {finalScale.x}");
    }

    [UnityTest]
    public IEnumerator LevelButtonHoverEffect_ColorChangesOnHover()
    {
        var hoverColorField = typeof(LevelButtonHoverEffect).GetField("hoverColor", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var scaleDurationField = typeof(LevelButtonHoverEffect).GetField("scaleDuration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        hoverColorField.SetValue(hoverEffect, Color.yellow);
        scaleDurationField.SetValue(hoverEffect, 0.01f); // Very fast animation
        
        Color originalColor = testImage.color; // White
        
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        hoverEffect.OnPointerEnter(eventData);
        
        // Wait much longer to ensure animation completes
        yield return new WaitForSeconds(1.0f);
        
        Color finalImageColor = testImage.color;
        Color finalTextColor = testText.color;
        
        // Test that colors have changed from original white
        Assert.AreNotEqual(originalColor, finalImageColor, "Image color should change from original");
        Assert.AreNotEqual(originalColor, finalTextColor, "Text color should change from original");
        
        // Test that colors are closer to yellow than white
        float imageDistanceToYellow = Vector4.Distance(finalImageColor, Color.yellow);
        float imageDistanceToWhite = Vector4.Distance(finalImageColor, Color.white);
        float textDistanceToYellow = Vector4.Distance(finalTextColor, Color.yellow);
        float textDistanceToWhite = Vector4.Distance(finalTextColor, Color.white);
        
        Assert.Less(imageDistanceToYellow, imageDistanceToWhite, $"Image should be closer to yellow. Final color: {finalImageColor}");
        Assert.Less(textDistanceToYellow, textDistanceToWhite, $"Text should be closer to yellow. Final color: {finalTextColor}");
    }

    [UnityTest]
    public IEnumerator LevelButtonHoverEffect_ReturnsToOriginalStateOnPointerExit()
    {
        var hoverScaleField = typeof(LevelButtonHoverEffect).GetField("hoverScale", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var hoverColorField = typeof(LevelButtonHoverEffect).GetField("hoverColor", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var scaleDurationField = typeof(LevelButtonHoverEffect).GetField("scaleDuration", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        hoverScaleField.SetValue(hoverEffect, 1.2f);
        hoverColorField.SetValue(hoverEffect, Color.yellow);
        scaleDurationField.SetValue(hoverEffect, 0.01f); // Very fast animation

        Vector3 originalScale = testObject.transform.localScale;
        Color originalColor = testImage.color;
        
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        
        // Enter hover state
        hoverEffect.OnPointerEnter(eventData);
        yield return new WaitForSeconds(1.0f); // Wait for hover animation
        
        // Exit hover state
        hoverEffect.OnPointerExit(eventData);
        yield return new WaitForSeconds(1.0f); // Wait for exit animation
        
        Vector3 finalScale = testObject.transform.localScale;
        Color finalImageColor = testImage.color;
        Color finalTextColor = testText.color;
        
        // Debug the values to understand what's happening
        var originalScaleField = typeof(LevelButtonHoverEffect).GetField("originalScale", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Vector3 storedOriginalScale = (Vector3)originalScaleField.GetValue(hoverEffect);
        
        // Test scale restoration with tolerance
        Assert.AreEqual(originalScale.x, finalScale.x, 0.05f, $"Scale should return to original. Original: {originalScale.x}, Final: {finalScale.x}, Stored: {storedOriginalScale.x}");
        
        // Test color restoration using distance-based approach
        float imageDistanceToOriginal = Vector4.Distance(finalImageColor, originalColor);
        float imageDistanceToHover = Vector4.Distance(finalImageColor, Color.yellow);
        float textDistanceToOriginal = Vector4.Distance(finalTextColor, originalColor);
        float textDistanceToHover = Vector4.Distance(finalTextColor, Color.yellow);
        
        Assert.Less(imageDistanceToOriginal, imageDistanceToHover, $"Image should be closer to original white than hover yellow. Final: {finalImageColor}, Original: {originalColor}");
        Assert.Less(textDistanceToOriginal, textDistanceToHover, $"Text should be closer to original white than hover yellow. Final: {finalTextColor}, Original: {originalColor}");
    }

    [Test]
    public void LevelButtonHoverEffect_WorksWithoutImageComponent()
    {
        Object.DestroyImmediate(testImage);
        
        GameObject newTestObject = new GameObject("TestWithoutImage");
        LevelButtonHoverEffect newHoverEffect = newTestObject.AddComponent<LevelButtonHoverEffect>();
        
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        
        Assert.DoesNotThrow(() => newHoverEffect.OnPointerEnter(eventData));
        Assert.DoesNotThrow(() => newHoverEffect.OnPointerExit(eventData));
        
        Object.DestroyImmediate(newTestObject);
    }

    [Test]
    public void LevelButtonHoverEffect_WorksWithoutTextComponent()
    {
        Object.DestroyImmediate(testText.gameObject);
        
        GameObject newTestObject = new GameObject("TestWithoutText");
        newTestObject.AddComponent<Image>();
        LevelButtonHoverEffect newHoverEffect = newTestObject.AddComponent<LevelButtonHoverEffect>();
        
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        
        Assert.DoesNotThrow(() => newHoverEffect.OnPointerEnter(eventData));
        Assert.DoesNotThrow(() => newHoverEffect.OnPointerExit(eventData));
        
        Object.DestroyImmediate(newTestObject);
    }
}