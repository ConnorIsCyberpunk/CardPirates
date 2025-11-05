using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5f;
    public float height = 2f;
    public float mouseSensitivity = 180f;
    public float smooth = 10f;
    public bool invertY = false;

    float yaw, pitch;

    public void EnableControl(bool on) { enabled = on; }

    void LateUpdate()
    {
        if (!target) return;

        yaw   += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        pitch += (invertY ? my : -my);
        pitch = Mathf.Clamp(pitch, -60f, 75f);

        var rot = Quaternion.Euler(pitch, yaw, 0f);
        var desired = target.position + rot * (Vector3.back * distance) + Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position, Vector3.up), 1f - Mathf.Exp(-smooth * Time.deltaTime));
    }
}
