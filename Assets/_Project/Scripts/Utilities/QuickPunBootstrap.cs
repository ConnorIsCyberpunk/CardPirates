using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class QuickPunBootstrap : MonoBehaviourPunCallbacks
{
    [SerializeField] string roomName = "TestRoom";
    [SerializeField] byte maxPlayers = 8;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = maxPlayers;

        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join failed: " + message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected: " + cause);
    }
}