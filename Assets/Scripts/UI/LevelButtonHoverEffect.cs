using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections; // Required for Coroutines

public class LevelButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float scaleDuration = 0.2f;

    [Header("Color Settings")]
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private float colorDuration = 0.2f;

    private Vector3 originalScale;
    private Color originalColor;
    private Image buttonImage;
    private Text buttonText;
    private Coroutine currentAnimationCoroutine;

    void Awake()
    {
        originalScale = transform.localScale;
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        buttonText = GetComponentInChildren<Text>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        currentAnimationCoroutine = StartCoroutine(AnimateHover(hoverScale, hoverColor));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        currentAnimationCoroutine = StartCoroutine(AnimateHover(originalScale.x, originalColor));
    }

    private IEnumerator AnimateHover(float targetScale, Color targetColor)
    {
        Vector3 startScale = transform.localScale;
        Color startColor = buttonImage != null ? buttonImage.color : Color.white;
        Color startTextColor = buttonText != null ? buttonText.color : Color.white;

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime / scaleDuration; // Using scaleDuration for both for simplicity
            transform.localScale = Vector3.Lerp(startScale, originalScale * targetScale, timer);

            if (buttonImage != null)
            {
                buttonImage.color = Color.Lerp(startColor, targetColor, timer);
            }
            if (buttonText != null)
            {
                buttonText.color = Color.Lerp(startTextColor, targetColor, timer);
            }
            yield return null;
        }

        // Ensure final state is set precisely
        transform.localScale = originalScale * targetScale;
        if (buttonImage != null)
        {
            buttonImage.color = targetColor;
        }
        if (buttonText != null)
        {
            buttonText.color = targetColor;
        }
    }
}