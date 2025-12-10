using Photon.Pun;
using UnityEngine;
using System; // CRITICAL: Needed for Convert.ToInt32

public class PlayerSetup : MonoBehaviourPun
{
    public Transform cameraAnchor;
    PlayerSkinController skin;

    void Awake()
    {
        skin = GetComponentInChildren<PlayerSkinController>(true);

        int skinIndex = 0;
        object[] data = photonView.InstantiationData;

        // --- THE FIX ---
        // We use Convert.ToInt32. This works for Byte, Int, Short, everything.
        // The old code (int)data[0] is what caused the Red Error crash.
        if (data != null && data.Length > 0 && data[0] != null)
        {
            try 
            {
                skinIndex = Convert.ToInt32(data[0]); 
            }
            catch (Exception) 
            {
                Debug.LogError("[PlayerSetup] Cast failed, defaulting to 0");
            }
        }

        if (skin)
            skin.ApplySkin(skinIndex);
        else
            Debug.LogWarning("[PlayerSetup] Missing PlayerSkinController.");
    }

    void Start()
    {
        if (!photonView.IsMine) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var cam = Camera.main;
        if (cam)
        {
            var follow = cam.GetComponent<FollowCamera>();
            if (follow)
            {
                follow.SetTarget(cameraAnchor);
            }
        }
    }
}