using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class LocalPlayerMover : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 4.5f;
    public float sprintMultiplier = 1.5f;
    public float turnSpeed = 12f;           // higher = snappier facing

    [Header("Jump/Gravity")]
    public float jumpHeight = 1.2f;
    public float gravity = -20f;            // world gravity
    public float groundedGravity = -2f;     // stick-to-ground

    CharacterController controller;
    PhotonView pv;

    float vy; // vertical velocity

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        pv = GetComponentInParent<PhotonView>();
        if (pv != null && !pv.IsMine)
        {
            enabled = false;    // remote players don't run local controls
        }
    }

    void Update()
    {
        // Camera-relative input
        Vector3 fwd = Vector3.forward;
        Vector3 right = Vector3.right;
        if (Camera.main)
        {
            fwd = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
            right = Vector3.ProjectOnPlane(Camera.main.transform.right,   Vector3.up).normalized;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = (right * h + fwd * v);
        input = input.sqrMagnitude > 1f ? input.normalized : input;

        bool sprint = Input.GetKey(KeyCode.LeftShift);
        float speed = moveSpeed * (sprint ? sprintMultiplier : 1f);

        // Face where we move
        if (input.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(input, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        // Ground / jump / gravity
        if (controller.isGrounded)
        {
            if (vy < 0f) vy = groundedGravity;
            if (Input.GetButtonDown("Jump"))
            {
                vy = Mathf.Sqrt(-2f * gravity * Mathf.Max(0.01f, jumpHeight));
            }
        }
        else
        {
            vy += gravity * Time.deltaTime;
        }

        Vector3 motion = input * speed + Vector3.up * vy;
        controller.Move(motion * Time.deltaTime);
    }
}
