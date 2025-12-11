using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public Button char1Button;
    public Button char2Button;
    public Button char3Button;

    [Header("Settings")]
    public string playerPrefabName = "Player";   
    public Vector3 spawnPos = Vector3.zero;

    void Awake()
    {
        // Simple listeners for the buttons
        if (char1Button) char1Button.onClick.AddListener(() => SpawnWithSkin(0));
        if (char2Button) char2Button.onClick.AddListener(() => SpawnWithSkin(1));
        if (char3Button) char3Button.onClick.AddListener(() => SpawnWithSkin(2));
    }

    public override void OnJoinedRoom()
    {
        // Unlock cursor so we can click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        gameObject.SetActive(true);
    }

    void SpawnWithSkin(int skinIndex)
    {
        // Pass the skin index to the new player
        object[] data = new object[] { skinIndex };
        
        PhotonNetwork.Instantiate(playerPrefabName, spawnPos, Quaternion.identity, 0, data);
        
        // Disable this UI
        gameObject.SetActive(false); 
    }
}