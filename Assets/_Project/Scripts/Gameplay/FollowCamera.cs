// FollowCamera.cs
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public static FollowCamera Instance { get; private set; }

    [Header("Target")]
    [SerializeField] Transform target;           // set by PlayerSetup at runtime
    [SerializeField] Vector3 lookOffset = Vector3.zero;

    [Header("Orbit")]
    public float distance = 4f;
    public float minDistance = 2f;
    public float maxDistance = 6f;
    public float zoomSpeed = 5f;

    public float yaw = 0f;
    public float pitch = 20f;
    public float sensX = 2.5f;
    public float sensY = 2.0f;
    public float minPitch = -25f;
    public float maxPitch = 70f;

    [Header("Smoothing")]
    public float posSmooth = 0.05f;
    public float rotSmooth = 0.05f;

    [Header("Collision")]
    public bool useCollision = true;
    public float collisionRadius = 0.25f;
    public LayerMask collisionMask = ~0; // everything

    [Header("Cursor")]
    public bool lockCursor = true;
    public KeyCode toggleCursorKey = KeyCode.Escape;

    Vector3 posVel;
    Quaternion rotVel;

    void Awake()
    {
        Instance = this;
        if (lockCursor) SetCursorLocked(true);
    }

    public void SetTarget(Transform t)
    {
        target = t;
        if (target) yaw = target.eulerAngles.y;     // start behind player
    }

    void LateUpdate()
    {
        if (!target) return;

        // Toggle cursor lock
        if (Input.GetKeyDown(toggleCursorKey))
            SetCursorLocked(Cursor.lockState != CursorLockMode.Locked);

        // Mouse orbit + zoom
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw   += Input.GetAxis("Mouse X") * sensX;
            pitch -= Input.GetAxis("Mouse Y") * sensY;
            pitch  = Mathf.Clamp(pitch, minPitch, maxPitch);

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                distance = Mathf.Clamp(
                    distance - scroll * zoomSpeed,
                    minDistance, maxDistance
                );
            }
        }

        // Desired camera transform
        Quaternion wantRot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3   wantPos = target.position + lookOffset
                            - wantRot * Vector3.forward * distance;

        // Simple collision so cam doesn’t clip through walls
        if (useCollision)
        {
            Vector3 from = target.position + lookOffset;
            Vector3 dir  = (wantPos - from).normalized;
            float   len  = Vector3.Distance(from, wantPos);

            if (Physics.SphereCast(from, collisionRadius, dir, out RaycastHit hit, len, collisionMask, QueryTriggerInteraction.Ignore))
                wantPos = from + dir * (hit.distance - 0.05f);
        }

        // Smoothly move/rotate
        transform.position = Vector3.SmoothDamp(transform.position, wantPos, ref posVel, posSmooth);
        transform.rotation = SmoothDampRotation(transform.rotation, wantRot, ref rotVel, rotSmooth);
    }

    Quaternion SmoothDampRotation(Quaternion current, Quaternion target, ref Quaternion vel, float smoothTime)
    {
        return QuaternionUtil.SmoothDamp(current, target, ref vel, smoothTime);
    }

    public Vector3 PlanarForward
        => Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

    void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !locked;
    }
}

// Small helper because Quaternion doesn't have SmoothDamp built-in
static class QuaternionUtil
{
    public static Quaternion SmoothDamp(Quaternion current, Quaternion target, ref Quaternion deriv, float smoothTime)
    {
        if (Time.deltaTime < Mathf.Epsilon) return current;
        // cheap approach: slerp with exponential smoothing
        float t = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.0001f, smoothTime));
        return Quaternion.Slerp(current, target, t);
    }
}
