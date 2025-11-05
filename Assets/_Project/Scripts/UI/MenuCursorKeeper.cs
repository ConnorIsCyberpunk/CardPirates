using UnityEngine;

public class MenuCursorKeeper : MonoBehaviour
{
    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    // If any other script tries to lock during the menu, undo it.
    void LateUpdate()
    {
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }
}
