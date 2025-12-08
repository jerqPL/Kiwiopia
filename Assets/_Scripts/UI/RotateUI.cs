using UnityEngine;

public class RotateUI : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    void Update()
    {
        transform.Rotate((Vector3.right + Vector3.up) * rotationSpeed * Time.deltaTime);
    }
}
