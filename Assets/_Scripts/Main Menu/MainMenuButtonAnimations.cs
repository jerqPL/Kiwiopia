using Newtonsoft.Json.Bson;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuButtonAnimations : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Slider backgroundSlider;
    public TMP_Text buttonText;

    public Color normalTextColor = Color.black;
    public Color highlightedTextColor = Color.white;
    public float animationTime = 0.2f;

    private float animationProgress = 0f;
    private bool isHovered = false;

    void Update()
    {
        if (isHovered)
        {
            animationProgress += Time.deltaTime / animationTime;
        }
        else
        {
            animationProgress -= Time.deltaTime / animationTime;
        }

        animationProgress = Mathf.Clamp01(animationProgress);

        // Quadratic easing (ease-in)
        float easedValue = animationProgress * animationProgress;
        backgroundSlider.value = easedValue;

        // Update text color
        buttonText.faceColor = Color.Lerp(normalTextColor, highlightedTextColor, easedValue);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
