using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SelectionHandler : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private UIHandler uIHandler;

    public Tile lastClickedTile;
    private int howManyTimesClickedSame;

    private InputActionMap selectionMap;
    private InputAction select;
    private InputAction mousePosition;
    void Start()
    {
        howManyTimesClickedSame = 0;
        selectionMap = InputSystem.actions.FindActionMap("Selection");
        select = selectionMap.FindAction("Select");
        mousePosition = selectionMap.FindAction("Mouse Position");
        select.performed += ctx => OnSelect();
    }

    void OnSelect()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        Vector2 mousePos = mousePosition.ReadValue<Vector2>();
        Debug.Log($"pressd: {mousePos.x},{mousePos.y}");
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                if (tile == lastClickedTile)
                {
                    howManyTimesClickedSame++;
                    //////TO-DO
                }
                else
                {
                    howManyTimesClickedSame = 0;
                }

                uIHandler.ClickedTile(tile, howManyTimesClickedSame);

                cameraMovement.UpdateFocusPoint(tile.transform);
                lastClickedTile = tile;
            }
        }
    }
    void Update()
    {
    }
}
