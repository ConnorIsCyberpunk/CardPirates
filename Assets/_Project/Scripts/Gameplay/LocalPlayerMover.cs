using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
public class LocalPlayerMover : MonoBehaviourPun
{
    [Header("Movement")]
    public float moveSpeed = 4.5f;
    public float turnSpeed = 12f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;
    
    private CharacterController controller;
    private float vy; 

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        if (!Application.isFocused) return; // Prevent moving when alt-tabbed
        if (!controller.enabled) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0, v);
        
        // Move relative to camera direction
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

        // Jump & Gravity
        if (controller.isGrounded) 
        {
            vy = -2f; 
            if (Input.GetButtonDown("Jump"))
            {
                vy = Mathf.Sqrt(-2f * gravity * jumpHeight);
            }
        }
        else 
        {
            vy += gravity * Time.deltaTime;
        }

        Vector3 velocity = input * moveSpeed + Vector3.up * vy;
        controller.Move(velocity * Time.deltaTime);
    }
}