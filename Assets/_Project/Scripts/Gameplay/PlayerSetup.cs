using Photon.Pun;
using UnityEngine;
using System; 

public class PlayerSetup : MonoBehaviourPun
{
    public Transform cameraAnchor;
    PlayerSkinController skin;

    void Awake()
    {
        skin = GetComponentInChildren<PlayerSkinController>(true);

        int skinIndex = 0;
        object[] data = photonView.InstantiationData;

        // data[0] comes in as an object, safe cast to int
        if (data != null && data.Length > 0 && data[0] != null)
        {
            try 
            {
                skinIndex = Convert.ToInt32(data[0]); 
            }
            catch
            {
                Debug.LogError("Error loading skin index, defaulting to 0");
            }
        }

        if (skin)
            skin.ApplySkin(skinIndex);
    }

    void Start()
    {
        if (!photonView.IsMine) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Camera.main)
        {
            var follow = Camera.main.GetComponent<FollowCamera>();
            if (follow)
            {
                follow.SetTarget(cameraAnchor);
            }
        }
    }
}