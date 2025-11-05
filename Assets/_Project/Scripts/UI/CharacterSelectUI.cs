using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviourPunCallbacks
{
    [Header("Hook these in the Inspector")]
    public Button char1Button;
    public Button char2Button;
    public Button char3Button;

    [Header("Spawn settings")]
    public string playerPrefabName = "Player";   // the Player prefab in Resources/
    public Vector3 spawnPos = Vector3.zero;

    void Awake()
    {
        if (char1Button) char1Button.onClick.AddListener(() => SpawnWithSkin(0));
        if (char2Button) char2Button.onClick.AddListener(() => SpawnWithSkin(1));
        if (char3Button) char3Button.onClick.AddListener(() => SpawnWithSkin(2));
    }

    public override void OnJoinedRoom()
    {
        // show UI when joined (in case this object starts disabled)
        gameObject.SetActive(true);
    }

    void SpawnWithSkin(int skinIndex)
    {
        // InstantiationData sends skin index to every client
        PhotonNetwork.Instantiate(playerPrefabName, spawnPos, Quaternion.identity, 0, new object[] { skinIndex });
        gameObject.SetActive(false); // hide selector after spawning
    }
}
