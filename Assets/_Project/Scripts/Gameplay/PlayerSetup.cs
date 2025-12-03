using Photon.Pun;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPun
{
    public Transform cameraAnchor;

    public bool isCaptain = false;
    public bool isImposter = false;

    public PlayerBio myBio;

    void Start()
    {
        if (photonView.IsMine)
        {
            AttachCamera();
            GenerateBio();
            GameRoundManager.Instance.RegisterPlayer(this);
        }
    }

    void AttachCamera()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var cam = Camera.main;
        var follow = cam.gameObject.GetComponent<ThirdPersonCamera>();
        if (!follow) follow = cam.gameObject.AddComponent<ThirdPersonCamera>();

        follow.target = cameraAnchor;
        follow.EnableControl(true);
    }

    void GenerateBio()
    {
        myBio = PlayerBioGenerator.Instance.GenerateBio(PhotonNetwork.NickName);
        BioPanelUI.Instance.ShowBio(myBio);
    }

    // ---------- ROLE ASSIGNMENTS ----------
    public void SetCaptain(bool value)
    {
        isCaptain = value;
        if (photonView.IsMine && value)
            Debug.Log("YOU ARE THE CAPTAIN");
    }

    public void SetImposter(bool value)
    {
        isImposter = value;
        if (photonView.IsMine && value)
            Debug.Log("YOU ARE THE IMPOSTER");
    }

    // ---------- ROUND ACTIONS ----------
    public void TeleportTo(Vector3 pos)
    {
        if (photonView.IsMine)
            transform.position = pos;
    }

    public void Eliminate()
    {
        if (photonView.IsMine)
        {
            gameObject.SetActive(false);
            Debug.Log("You have been eliminated.");
        }
    }

    // ---------- CAPTAIN VOTING ----------
    public void ShowVotingUI(System.Collections.Generic.List<PlayerSetup> players)
    {
        if (photonView.IsMine && isCaptain)
            VoteUI.Instance.Open(players);
    }

    // ---------- IMPOSTER KILL ----------
    public void TryImposterKill(PlayerSetup target)
    {
        if (!isImposter || !photonView.IsMine) return;

        photonView.RPC("RPC_Kill", RpcTarget.All, target.photonView.ViewID);
    }

    [PunRPC]
    void RPC_Kill(int viewID)
    {
        PlayerSetup target = PhotonView.Find(viewID).GetComponent<PlayerSetup>();
        target.Eliminate();
    }
}
