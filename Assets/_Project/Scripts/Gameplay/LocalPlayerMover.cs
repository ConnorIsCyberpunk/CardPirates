using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class LocalPlayerMover : MonoBehaviourPun
{
    [Header("Movement Settings")]
    public float moveSpeed = 4.5f;
    public float turnSpeed = 12f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;
    
    // Internal variables
    private CharacterController controller;
    private float vy; 

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1. NETWORK CHECK: If I don't own this player, stop.
        if (!photonView.IsMine) return;

        // 2. INPUT FOCUS CHECK: If I'm clicking on another window, stop.
        // (Fixes the "One keyboard moves two players" glitch)
        if (!Application.isFocused) return;

        // 3. TELEPORT CHECK: If the Boat turned off my controller, stop.
        // (Fixes the "Move called on inactive controller" error)
        if (!controller.enabled) return;

        // --- MOVEMENT LOGIC ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0, v);
        
        // Make movement relative to the camera
        if (Camera.main)
        {
            Vector3 camFwd = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;
            input = (camRight * h + camFwd * v);
        }

        if (input.sqrMagnitude > 1f) input.Normalize();

        if (input.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(input, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        // Gravity & Jump
        if (controller.isGrounded) 
        {
            vy = -2f; // Small stick-to-ground force
            if (Input.GetButtonDown("Jump"))
            {
                vy = Mathf.Sqrt(-2f * gravity * jumpHeight);
            }
        }
        else 
        {
            vy += gravity * Time.deltaTime;
        }

        // Apply Move
        Vector3 velocity = input * moveSpeed + Vector3.up * vy;
        controller.Move(velocity * Time.deltaTime);
    }
}