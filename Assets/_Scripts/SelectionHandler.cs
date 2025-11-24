using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionHandler : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraMovement cameraMovement;

    private InputActionMap selectionMap;
    private InputAction select;
    private InputAction mousePosition;
    void Start()
    {
        selectionMap = InputSystem.actions.FindActionMap("Selection");
        select = selectionMap.FindAction("Select");
        mousePosition = selectionMap.FindAction("Mouse Position");
        select.performed += ctx => OnSelect();
    }

    void OnSelect()
    {
        
        Vector2 mousePos = mousePosition.ReadValue<Vector2>();
        Debug.Log($"pressd: {mousePos.x},{mousePos.y}");
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                cameraMovement.UpdateFocusPoint(tile.transform);
            }
        }
    }
    void Update()
    {
    }
}
