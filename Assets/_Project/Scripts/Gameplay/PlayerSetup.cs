using Photon.Pun;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPun
{
    public Transform cameraAnchor;               // assign child "CameraAnchor"
    PlayerSkinController skin;

    void Awake()
    {
        skin = GetComponentInChildren<PlayerSkinController>(true);

        int skinIndex = 0;
        var data = photonView.InstantiationData;
        if (data != null && data.Length > 0) skinIndex = (int)data[0];

        if (skin)
            skin.ApplySkin(skinIndex);          // run for BOTH local and remote
        else
            Debug.LogWarning("[PlayerSetup] Missing PlayerSkinController.");
    }

    void Start()
    {
        if (!photonView.IsMine) return;

        // lock the cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // attach a simple camera follow (example)
        var cam = Camera.main;
        if (cam)
        {
            var follow = cam.GetComponent<ThirdPersonCamera>();
            if (!follow) follow = cam.gameObject.AddComponent<ThirdPersonCamera>();
            follow.target = cameraAnchor;
            follow.EnableControl(true);
        }
    }
}
