using UnityEngine;
using Photon.Pun;

public class KillButton : MonoBehaviour
{
    public PlayerSetup targetPlayer;

    public void OnKillPressed()
    {
        if (GameRoundManager.Instance == null) return;
        if (!PhotonNetwork.IsMasterClient) return;

        if (targetPlayer != null)
        {
            GameRoundManager.Instance.CaptainEliminates(targetPlayer);
        }
    }
}
