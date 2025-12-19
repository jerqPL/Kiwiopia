using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class BarUI : MonoBehaviour
{
    [SerializeField] private RectTransform fillArea;
    [SerializeField] private RectTransform background;

    private Slider bar;
    void Awake()
    {
        bar = GetComponent<Slider>();
        fillArea.gameObject.SetActive(false);
        background.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        UpdateRotation();   
    }

    public void UpdateRotation()
    {
        // get camera forward direction
        Vector3 camDir = Camera.main.transform.forward;

        // flatten (remove vertical influence)
        camDir.y = 0f;
        camDir.Normalize();

        // create rotation only around Y
        transform.transform.rotation = Quaternion.LookRotation(camDir);
    }

    public void UpdateValue(float value)
    {
        bar.value = value;
    }

    public void Disable()
    {
        fillArea.gameObject.SetActive(false);
        background.gameObject.SetActive(false);
    }

    public void Enable()
    {
        fillArea.gameObject.SetActive(true);
        background.gameObject.SetActive(true);
    }
}
