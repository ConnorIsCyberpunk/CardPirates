using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class QuickPunBootstrap : MonoBehaviourPunCallbacks
{
    [SerializeField] string roomName = "TestRoom";
    [SerializeField] string playerPrefabName = "Player";            // must be under a Resources/ folder
    [SerializeField] Vector3 spawnPos = Vector3.zero;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = 16 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        var go = PhotonNetwork.Instantiate(playerPrefabName, spawnPos, Quaternion.identity);

        // Optional: ensure the player's Recorder is on
        var rec = go.GetComponent<Photon.Voice.Unity.Recorder>();
        if (rec != null)
        {
            rec.RecordingEnabled = true;
            rec.TransmitEnabled  = true;
        }
    }
}
