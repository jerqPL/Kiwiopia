using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    private Vector3 startingPosition;
    private Quaternion startingRotation;

    [SerializeField] private float movementSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float rotationSpeed;

    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;

    [SerializeField] private TilesHandler tilesHandler;
 
    private InputActionMap cameraMap;

    private InputAction cameraReset;
    private InputAction cameraMoveForward;
    private InputAction cameraMoveBackward;
    private InputAction cameraMoveLeft;
    private InputAction cameraMoveRight;

    private InputAction cameraZoom;

    private InputAction cameraRotateUp;
    private InputAction cameraRotateDown;
    private InputAction cameraRotateLeft;
    private InputAction cameraRotateRight;
    private InputAction cameraMove;
    
    private Vector2 scrollDelta;

    private float xRotation;
    private float yRotation;

    Vector3 moveStartPosition;

    private Coroutine cameraMovement;

    void Start()
    {
        startingPosition = transform.position;
        startingRotation = transform.rotation;

        cameraMap = InputSystem.actions.FindActionMap("Camera");

        cameraReset = cameraMap.FindAction("Reset");

        cameraMoveForward = cameraMap.FindAction("Move Forward");
        cameraMoveBackward = cameraMap.FindAction("Move Backward");
        cameraMoveLeft = cameraMap.FindAction("Move Left");
        cameraMoveRight = cameraMap.FindAction("Move Right");

        cameraZoom = cameraMap.FindAction("Zoom");

        cameraRotateUp = cameraMap.FindAction("Rotate Up");
        cameraRotateDown = cameraMap.FindAction("Rotate Down");
        cameraRotateLeft = cameraMap.FindAction("Rotate Left");
        cameraRotateRight = cameraMap.FindAction("Rotate Right");
        cameraMove = cameraMap.FindAction("Move");
        xRotation = transform.rotation.eulerAngles.x;
        yRotation = transform.rotation.eulerAngles.y;

        cameraMove.started += setMoveStartPosition;
        cameraZoom.performed += ctx => scrollDelta = ctx.ReadValue<Vector2>();
        cameraReset.started += ctx => { transform.position = startingPosition;
                                        transform.rotation = startingRotation;
                                        xRotation = transform.rotation.eulerAngles.x;
                                        yRotation = transform.rotation.eulerAngles.y; };
    }

    private void setMoveStartPosition(InputAction.CallbackContext c)
    {
        moveStartPosition = Global.selectionHandler.getPositionOnPlaneOnMouse();
    }

    void Update()
    {

        float MoveSpeedModifier = transform.position.y/minDistance;
        if (cameraMoveForward.IsPressed())
        {
            transform.position += Global.ZeroYVector3(Camera.main.transform.forward).normalized * movementSpeed * MoveSpeedModifier * Time.deltaTime;
        }
        if (cameraMoveBackward.IsPressed())
        {
            transform.position += Global.ZeroYVector3(Camera.main.transform.forward).normalized * movementSpeed * MoveSpeedModifier * Time.deltaTime * -1;
        }

        if (cameraMoveLeft.IsPressed())
        {
            transform.position += Global.ZeroYVector3(Camera.main.transform.right).normalized * movementSpeed * MoveSpeedModifier * Time.deltaTime * -1;
        }
        if (cameraMoveRight.IsPressed())
        {
            transform.position += Global.ZeroYVector3(Camera.main.transform.right).normalized * movementSpeed * MoveSpeedModifier * Time.deltaTime;
        }

        transform.position += transform.forward * scrollDelta.y * zoomSpeed * Time.deltaTime;
        if (transform.position.y < minDistance)
        {
            transform.position += transform.forward * ((minDistance - transform.position.y) / transform.forward.y);
        }
        if (transform.position.y > maxDistance)
        {
            transform.position += transform.forward * ((maxDistance - transform.position.y) / transform.forward.y);
        }

        scrollDelta = Vector2.zero;

        if (cameraRotateUp.IsPressed())
        {
            xRotation += rotationSpeed * Time.deltaTime * -1;
        }
        if (cameraRotateDown.IsPressed())
        {
            xRotation += rotationSpeed * Time.deltaTime;
        }
        if (cameraRotateLeft.IsPressed())
        {
            yRotation += rotationSpeed * Time.deltaTime * 2f * -1;
        }
        if (cameraRotateRight.IsPressed())
        {
            yRotation += rotationSpeed * Time.deltaTime * 2f;
        }

        xRotation = Mathf.Clamp(xRotation, 45f, 89f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        if (cameraMove.IsPressed())
        {
            Vector3 moveCurrentPosition = Global.selectionHandler.getPositionOnPlaneOnMouse();
            Vector3 delta = moveStartPosition - moveCurrentPosition;
            transform.position += delta;
        }
    }

    public void focousOnTile(Tile tile)
    {
        if (cameraMovement != null)
        {
            StopCoroutine(cameraMovement);
        }
        cameraMovement = StartCoroutine(MovementAnimation(tile.transform.position, 0.5f));
    }

    IEnumerator MovementAnimation(Vector3 position, float duration)
    {
        Vector3 middleOnPlane = Global.selectionHandler.getPositionOnPlaneOnMiddle();
        Vector3 delta = position - middleOnPlane;
        Vector3 startingPos = transform.position;
        float time = 0f;
        while (time < duration)
        {
            yield return null;
            time += Time.deltaTime;
            transform.position = startingPos + Vector3.Lerp(Vector3.zero, delta, time/duration);
        }
        transform.position = startingPos + delta;
    }
}
