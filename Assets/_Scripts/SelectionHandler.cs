using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static Global;

public class SelectionHandler : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    public SelectionHandlerState state = SelectionHandlerState.None; 
    //0 - none; 1 - unit moving

    public Tile lastClickedTile;

    private InputActionMap selectionMap;
    private InputAction select;
    private InputAction mousePosition;

    private bool clicked = false;

    void Start()
    {
        selectionMap = InputSystem.actions.FindActionMap("Selection");
        select = selectionMap.FindAction("Select");
        mousePosition = selectionMap.FindAction("Mouse Position");
        select.performed += ctx => clicked = true;
    }

    void OnSelect()
    {
        Tile tile = getTileOnMouse();
        if (tile == null)
        {
            return;
        }

        if (state == SelectionHandlerState.UnitMoving)
        {
            state = SelectionHandlerState.None;
            if (lastClickedTile.unit == null)
            {
                uIHandler.ClickedTile(lastClickedTile);
                return;
            }
            if (lastClickedTile.unit != null && lastClickedTile.unit.unitMovement.isMoving.Value)
            {
                uIHandler.ClickedTile(lastClickedTile);
                return;
            }
            unitsHandler.RequesUnitMovement(Global.unitsHandler.GetIndexOf(lastClickedTile.unit));
            state = SelectionHandlerState.None;
            uIHandler.ClickedTile(lastClickedTile);
            return;
        }

        uIHandler.ClickedTile(tile);
        lastClickedTile = tile;   
    }

    public Tile getTileOnMouse()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return null;
        }
        Vector2 mousePos = mousePosition.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                return tile;
            }
        }
        return null;
    }

    public Vector3 getPositionOnPlaneOnMouse()
    {
        Vector2 mousePos = mousePosition.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            return hitPoint;
        }

        return Vector3.zero;
    }

    public Vector3 getPositionOnPlaneOnMiddle()
    {
        // ray from the center of the camera instead of mouse
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = mainCamera.ScreenPointToRay(center);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    void Update()
    {
        if (clicked)
        {
            OnSelect();
            clicked = false;
        }
    }
}
