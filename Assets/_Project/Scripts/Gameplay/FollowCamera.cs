using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 lookOffset = Vector3.zero;

    [Header("Settings")]
    public float distance = 4f;
    public float minDistance = 2f;
    public float maxDistance = 6f;
    public float zoomSpeed = 5f;

    public float sensX = 5f;
    public float sensY = 5f;
    public float minPitch = -25f;
    public float maxPitch = 70f;

    [Header("Collision")]
    public LayerMask collisionMask; 

    private float yaw = 0f;
    private float pitch = 20f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetTarget(Transform t)
    {
        target = t;
        if (target != null)
            yaw = target.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Mouse Input
        yaw += Input.GetAxis("Mouse X") * sensX;
        pitch -= Input.GetAxis("Mouse Y") * sensY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Calculate Rotation & Position
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 targetPos = target.position + lookOffset;
        Vector3 direction = rotation * -Vector3.forward;

        // Simple Collision Check
        Vector3 finalPos = targetPos + direction * distance;
        
        RaycastHit hit;
        if (Physics.Linecast(targetPos, finalPos, out hit, collisionMask))
        {
            finalPos = hit.point + hit.normal * 0.1f; // Push out slightly
        }

        // Apply
        transform.position = Vector3.Lerp(transform.position, finalPos, 20f * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 20f * Time.deltaTime);
    }
}