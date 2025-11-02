using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerSkinController : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [Header("Assign in prefab")]
    public Transform modelRoot;       // empty child where the visual goes (e.g., “ModelRoot”)
    public SkinDatabase database;

    GameObject currentVisual;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        int skinIndex = 0;
        var data = photonView.InstantiationData; // sent by PhotonNetwork.Instantiate(...)
        if (data != null && data.Length > 0 && data[0] is int)
            skinIndex = (int)data[0];

        ApplySkin(skinIndex);
    }

    public void ApplySkin(int skinIndex)
    {
        if (currentVisual) Destroy(currentVisual);
        var prefab = database != null ? database.Get(skinIndex) : null;
        if (prefab == null)
        {
            Debug.LogWarning("PlayerSkinController: No skin prefab found for index " + skinIndex);
            return;
        }
        var parent = modelRoot != null ? modelRoot : transform;
        currentVisual = Instantiate(prefab, parent, false);
    }
}
