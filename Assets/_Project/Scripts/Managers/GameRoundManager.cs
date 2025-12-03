using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GameRoundManager : MonoBehaviourPunCallbacks
{
    public static GameRoundManager Instance;

    [Header("Round Settings")]
    public float roundDuration = 120f; // 2 minutes
    public Transform shipReturnPoint;

    List<PlayerSetup> allPlayers = new List<PlayerSetup>();
    PlayerSetup captain;
    PlayerSetup imposter;

    float timer;
    bool roundActive = false;

    void Awake()
    {
        Instance = this;
    }

    public void RegisterPlayer(PlayerSetup setup)
    {
        allPlayers.Add(setup);

        // When all players spawned, start the game (master client only)
        if (PhotonNetwork.IsMasterClient && allPlayers.Count == PhotonNetwork.PlayerList.Length)
        {
            photonView.RPC("RPC_StartGame", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void RPC_StartGame()
    {
        AssignRoles();
        StartCoroutine(RoundRoutine());
    }

    void AssignRoles()
    {
        int captainIndex = Random.Range(0, allPlayers.Count);
        captain = allPlayers[captainIndex];
        captain.SetCaptain(true);

        // pick imposter but not the captain
        PlayerSetup candidate;
        do
        {
            candidate = allPlayers[Random.Range(0, allPlayers.Count)];
        }
        while (candidate == captain);

        imposter = candidate;
        imposter.SetImposter(true);
    }

    IEnumerator RoundRoutine()
    {
        roundActive = true;
        timer = roundDuration;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        // time is up → teleport everyone back
        ReturnEveryoneToShip();
        roundActive = false;

        // now Captain gets the voting UI
        captain.ShowVotingUI(allPlayers);
    }

    void ReturnEveryoneToShip()
    {
        foreach (var p in allPlayers)
        {
            p.TeleportTo(shipReturnPoint.position);
        }
    }

    public void CaptainEliminates(PlayerSetup target)
    {
        if (target == imposter)
        {
            photonView.RPC("RPC_CrewWins", RpcTarget.All);
        }
        else
        {
            // Innocent → continue
            allPlayers.Remove(target);
            target.Eliminate();

            photonView.RPC("RPC_StartNextRound", RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_CrewWins()
    {
        Debug.Log("Crew wins!");
        // TODO: Show end-game UI
    }

    [PunRPC]
    void RPC_StartNextRound()
    {
        StartCoroutine(RoundRoutine());
    }

    public bool IsImposter(PlayerSetup p) => p == imposter;
}
