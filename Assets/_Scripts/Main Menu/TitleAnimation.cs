using UnityEngine;
using UnityEngine.InputSystem;

public class TitleAnimation : MonoBehaviour
{
    [SerializeField] private float minRotation = 170;
    [SerializeField] private float maxRotation = 190;

    private float currentRotation;

    private InputActionMap selectionMap;
    private InputAction mousePosition;

    void Start()
    {
        selectionMap = InputSystem.actions.FindActionMap("Selection");
        mousePosition = selectionMap.FindAction("Mouse Position");

        Vector2 mousePos = mousePosition.ReadValue<Vector2>();


        currentRotation = Mathf.Lerp(maxRotation, minRotation, mousePos.x / Screen.width);

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
                                              currentRotation,
                                              transform.rotation.eulerAngles.z);
    }


    void Update()
    {
        Vector2 mousePos = mousePosition.ReadValue<Vector2>();


        currentRotation = Mathf.Lerp(maxRotation, minRotation, mousePos.x / Screen.width);

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
                                              currentRotation,
                                              transform.rotation.eulerAngles.z);
    }
}
