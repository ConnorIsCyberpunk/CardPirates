using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public static FollowCamera Instance { get; private set; }

    [Header("Target")]
    [SerializeField] private Transform target;          // Set by PlayerSetup at runtime
    [SerializeField] private Vector3 lookOffset = Vector3.zero;

    [Header("Orbit & Distance")]
    public float distance = 4f;
    public float minDistance = 2f;
    public float maxDistance = 6f;
    public float zoomSpeed = 5f;

    [Header("Rotation")]
    public float yaw = 0f;
    public float pitch = 20f;

    [Header("Mouse Sensitivity")]
    public float sensX = 8f;          // horizontal sensitivity
    public float sensY = 8f;          // vertical sensitivity
    public float sensitivityMultiplier = 1.0f;
    public float minPitch = -25f;
    public float maxPitch = 70f;
    public bool invertY = false;

    [Header("Smoothing")]
    public float posSmooth = 0.03f;   // lower = snappier
    public float rotSmooth = 0.03f;

    [Header("Collision")]
    public bool useCollision = true;
    public float collisionRadius = 0.25f;
    public LayerMask collisionMask = ~0; // everything

    [Header("Cursor")]
    public bool lockCursor = true;
    public KeyCode toggleCursorKey = KeyCode.Escape;

    // internals
    private Vector3 posVel;
    private Quaternion rotVel;

    private void Awake()
    {
        Instance = this;

        if (lockCursor)
            SetCursorLocked(true);
    }

    /// <summary>Called by PlayerSetup when the local player spawns.</summary>
    public void SetTarget(Transform t)
    {
        target = t;
        if (target != null)
        {
            // start looking roughly the same direction as the player
            yaw = target.eulerAngles.y;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Toggle cursor lock
        if (Input.GetKeyDown(toggleCursorKey))
        {
            bool nowLocked = Cursor.lockState != CursorLockMode.Locked;
            SetCursorLocked(nowLocked);
        }

        // Mouse orbit + zoom only when cursor is locked
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            float ySign = invertY ? 1f : -1f;

            yaw   += mx * sensX * sensitivityMultiplier;
            pitch += my * sensY * sensitivityMultiplier * ySign;
            pitch  = Mathf.Clamp(pitch, minPitch, maxPitch);

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                distance = Mathf.Clamp(
                    distance - scroll * zoomSpeed,
                    minDistance,
                    maxDistance
                );
            }
        }

        // Desired camera transform
        Quaternion wantRot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 from = target.position + lookOffset;
        Vector3 wantPos = from - wantRot * Vector3.forward * distance;

        // Simple collision so cam doesn’t clip through walls
        if (useCollision)
        {
            Vector3 dir = (wantPos - from).normalized;
            float len = Vector3.Distance(from, wantPos);

            if (Physics.SphereCast(from, collisionRadius, dir, out RaycastHit hit, len,
                    collisionMask, QueryTriggerInteraction.Ignore))
            {
                // pull camera closer to the hit point
                wantPos = from + dir * (hit.distance - 0.05f);
            }
        }

        // Smooth move/rotate
        if (posSmooth <= 0f)
        {
            transform.position = wantPos;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, wantPos,
                ref posVel, posSmooth
            );
        }

        if (rotSmooth <= 0f)
        {
            transform.rotation = wantRot;
        }
        else
        {
            transform.rotation = QuaternionUtil.SmoothDamp(
                transform.rotation, wantRot,
                ref rotVel, rotSmooth
            );
        }
    }

    /// <summary>
    /// Forward direction flattened on the XZ plane.
    /// Used by your movement script: FollowCamera.Instance.PlanarForward
    /// </summary>
    public Vector3 PlanarForward =>
        Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

    private void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !locked;
    }
}

/// <summary>
/// Small helper because Quaternion doesn't have SmoothDamp built-in.
/// This version just does an exponential slerp (feels like SmoothDamp).
/// </summary>
static class QuaternionUtil
{
    public static Quaternion SmoothDamp(
        Quaternion current,
        Quaternion target,
        ref Quaternion deriv,
        float smoothTime)
    {
        if (Time.deltaTime < Mathf.Epsilon)
            return current;

        float t = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.0001f, smoothTime));
        return Quaternion.Slerp(current, target, t);
    }
}
