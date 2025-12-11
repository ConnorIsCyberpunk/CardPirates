using UnityEngine;

public class MenuCursorKeeper : MonoBehaviour
{
    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    void LateUpdate()
    {
        // Force unlock if something else tries to lock it
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }
}