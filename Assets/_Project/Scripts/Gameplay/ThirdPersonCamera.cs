using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Targets")]
    public Transform target;

    [Header("Settings")]
    public float distance = 5f;
    public float height = 2f;
    public float mouseSensitivity = 5.0f; // Adjusted to be normal range
    public float smoothSpeed = 10f;
    
    [Header("Controls")]
    public bool invertY = false;

    private float yaw;
    private float pitch;

    // Helper to toggle input
    public void EnableControl(bool isOn) 
    { 
        this.enabled = isOn; 
    }

    void LateUpdate()
    {
        if (!target) return;

        // Basic Input
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mx;
        if (invertY) pitch += my;
        else pitch -= my;

        pitch = Mathf.Clamp(pitch, -60f, 75f);

        // Calculate Position
        Quaternion currentRotation = Quaternion.Euler(pitch, yaw, 0f);
        
        Vector3 offset = (Vector3.back * distance) + (Vector3.up * height);
        Vector3 desiredPosition = target.position + (currentRotation * offset);

        // Smooth Movement (Standard Student Lerp)
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Smooth Rotation
        Vector3 lookDir = target.position - transform.position;
        Quaternion lookRot = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, smoothSpeed * Time.deltaTime);
    }
}