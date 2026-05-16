using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static Global;

public class SelectionHandler : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    public SelectionHandlerState state = SelectionHandlerState.None;
    //0 - none; 1 - unit moving; 2- unit dragging;

    public Unit draggedUnit;
    public List<Tile> draggedUnitPath = new List<Tile>();

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
        select.started += ctx => DragOnBoard();
        select.canceled += ctx => EndDragOnBoard();
    }

    private void DragOnBoard()
    {
        if (getTileOnMouse() != null)
        {
            Tile tile = getTileOnMouse();
            if (tile.unit != null)
            {
                draggedUnit = getTileOnMouse().unit;
                state = SelectionHandlerState.UnitDragging;
                draggedUnitPath.Clear();
            }
        }
    }

    private void EndDragOnBoard()
    {
        if (state == SelectionHandlerState.UnitDragging)
        {
            state = SelectionHandlerState.None;
            unitsHandler.RequestUnitMovementPath(unitsHandler.GetIndexOf(draggedUnit), draggedUnitPath);
            draggedUnit = null;
        }
    }

    public void SetLastClickedTile(Tile tile)
    {
        lastClickedTile = tile;
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
            unitsHandler.RequestUnitMovement(unitsHandler.GetIndexOf(lastClickedTile.unit), tilesHandler.GetIndexOf(tile));
            state = SelectionHandlerState.None;
            uIHandler.ClickedTile(lastClickedTile);
            return;
        }

        uIHandler.ClickedTile(tile);
        selectionHandler.SetLastClickedTile(tile);
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

        if (state == SelectionHandlerState.UnitDragging)
        {
            Tile tile = getTileOnMouse();
            if (tile != null && (draggedUnitPath.Count == 0 || draggedUnitPath[draggedUnitPath.Count-1] != tile))
            {
                List<Tile> path = tilesHandler.shortestPathSeeingVisible(draggedUnitPath.Count == 0 ? draggedUnit.tile : draggedUnitPath[draggedUnitPath.Count - 1], tile);
                draggedUnitPath.AddRange(path);
                // Remove duplicate neighbors from the end (up to 3 steps deep)
                if (draggedUnitPath.Count >= 2)
                {
                    int start = Mathf.Max(0, draggedUnitPath.Count - 4);

                    int i = draggedUnitPath.Count - 2;
                    while (i >= start)
                    {
                        if (draggedUnitPath[i] == draggedUnitPath[i + 1])
                        {
                            draggedUnitPath.RemoveAt(i + 1);
                        }

                        i--;
                    }
                }

                // Remove middle points if first and third are close enough
                if (draggedUnitPath.Count >= 3)
                {
                    int start = Mathf.Max(0, draggedUnitPath.Count - 5);

                    int i = draggedUnitPath.Count - 3;
                    while (i >= start)
                    {
                        if (tilesHandler.Distance(draggedUnitPath[i], draggedUnitPath[i + 2]) <= 1)
                        {
                            draggedUnitPath.RemoveAt(i + 1);
                        }

                        i--;
                    }
                }
            }
        }
    }
}
