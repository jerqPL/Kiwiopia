using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float maxDistance;
    [SerializeField] private float minDistance;

    [SerializeField] private float rotationSpeed;

    [SerializeField] private TilesHandler tilesHandler;

    public Transform focusPoint;
    private Coroutine movementCoroutine;
    private bool coroutineEnded = true;
    private Vector3 targetDestination;

 
    private InputActionMap cameraMap;
    private InputAction cameraForward;
    private InputAction cameraBackward;
    private InputAction cameraRotateLeft;
    private InputAction cameraRotateRight;
    private InputAction cameraRotateUp;
    private InputAction cameraRotateDown;

    void Start()
    {
        cameraMap = InputSystem.actions.FindActionMap("Camera");
        cameraForward = cameraMap.FindAction("Forward");
        cameraBackward = cameraMap.FindAction("Backward");
        cameraRotateLeft = cameraMap.FindAction("Rotate Left");
        cameraRotateRight = cameraMap.FindAction("Rotate Right");
        cameraRotateUp = cameraMap.FindAction("Rotate Up");
        cameraRotateDown = cameraMap.FindAction("Rotate Down");
    }

    void Update()
    {
        if (cameraForward.IsPressed() && Vector3.Distance(transform.position, focusPoint.position) > minDistance)
        {
            transform.position += (focusPoint.position - transform.position).normalized * movementSpeed * Time.deltaTime;
        }
        if (cameraBackward.IsPressed() && Vector3.Distance(transform.position, focusPoint.position) < maxDistance)
        {
            transform.position += (focusPoint.position - transform.position).normalized * movementSpeed * Time.deltaTime * -1;
        }

        if (cameraRotateLeft.IsPressed())
        {
            transform.RotateAround(focusPoint.position, Vector3.up, rotationSpeed * Time.deltaTime);
        }
        if (cameraRotateRight.IsPressed())
        {
            transform.RotateAround(focusPoint.position, Vector3.up, rotationSpeed * Time.deltaTime * -1);
        }

        if (cameraRotateUp.IsPressed())
        {
            transform.Rotate(new Vector3(rotationSpeed * Time.deltaTime * -1, 0 ,0));
        }
        if (cameraRotateDown.IsPressed())
        {
            transform.Rotate(new Vector3(rotationSpeed * Time.deltaTime, 0, 0));
        }

        Vector3 angles = transform.rotation.eulerAngles;
        float x = angles.x;
        x = Mathf.Clamp(x, 45f, 75f);
        
        transform.rotation = Quaternion.Euler(x, angles.y, angles.z);

    }

    public void UpdateFocusPoint(Transform newFocusPoint)
    {
        if (focusPoint == null)
        {
            focusPoint = tilesHandler.centerTile.transform;
        }

        Vector3 deltaCamPosition = transform.position - focusPoint.position;

        if (!coroutineEnded)
        {
            deltaCamPosition = targetDestination - focusPoint.position;
        }

        
        focusPoint = newFocusPoint;

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        targetDestination = newFocusPoint.position + deltaCamPosition;
        movementCoroutine = StartCoroutine(animateCameraMovement(newFocusPoint.position + deltaCamPosition, newFocusPoint));
        coroutineEnded = false;
    }

    private IEnumerator animateCameraMovement(Vector3 destination, Transform target)
    {
        float duration = .4f;
        float time = 0f;

        Vector3 source = transform.position;
        Vector3 targetPos = target.position;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(source, destination, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = destination;
        //transform.LookAt(targetPos);
        coroutineEnded = true;
    }
}
