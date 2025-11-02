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
        PhotonNetwork.JoinOrCreateRoom(
            roomName,
            new RoomOptions { MaxPlayers = maxPlayers },
            TypedLobby.Default
        );
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
        => Debug.LogError($"Join failed {returnCode}: {message}");

    public override void OnDisconnected(DisconnectCause cause)
        => Debug.LogWarning($"Disconnected: {cause}");
}
