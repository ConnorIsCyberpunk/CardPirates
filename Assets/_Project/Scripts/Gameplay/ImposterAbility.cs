using UnityEngine;
using UnityEngine.UI; // If using Legacy Text
using TMPro;          // If using TextMeshPro
using Photon.Pun;
using Photon.Realtime;

public class ImposterAbility : MonoBehaviourPun
{
    [Header("Settings")]
    public float killRange = 3.0f;
    public KeyCode killKey = KeyCode.F;

    private GameObject killUI; // The UI object we toggle
    private bool hasKilledThisRound = false;

    void Start()
    {
        // AUTO-FIND THE UI so you don't have to link it
        // It looks for the object named "KillPromptText" inside GameUI
        GameObject found = GameObject.Find("KillPromptText");
        if (found) 
        {
            killUI = found;
            killUI.SetActive(false); // Ensure hidden at start
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // 1. Role Check: Am I a Saboteur?
        object myRole;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out myRole))
        {
            if ((string)myRole != "SABOTEUR") return; 
        }
        else return; 

        // 2. Cooldown Check
        if (hasKilledThisRound) 
        {
            if(killUI) killUI.SetActive(false);
            return;
        }

        // 3. Raycast for Victim
        CheckForVictim();
    }

    void CheckForVictim()
    {
        RaycastHit hit;
        // Shoot ray from chest height
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, killRange))
        {
            PhotonView targetView = hit.collider.GetComponent<PhotonView>();
            
            // Is it a player and NOT me?
            if (targetView != null && !targetView.IsMine)
            {
                // Check Role: Cannot kill Captain
                string targetRole = "";
                if (targetView.Owner.CustomProperties.ContainsKey("Role"))
                    targetRole = (string)targetView.Owner.CustomProperties["Role"];
                
                if (targetRole == "CAPTAIN") 
                {
                    if(killUI) killUI.SetActive(false); // Hide if looking at Boss
                    return; 
                }

                // VALID TARGET FOUND!
                if(killUI) killUI.SetActive(true); // SHOW "PRESS F"

                if (Input.GetKeyDown(killKey))
                {
                    DoKill(targetView);
                }
                return;
            }
        }

        // If we hit nothing or a wall, hide UI
        if(killUI) killUI.SetActive(false);
    }

    void DoKill(PhotonView target)
    {
        hasKilledThisRound = true;
        if(killUI) killUI.SetActive(false);

        Debug.Log($"Killing {target.Owner.NickName}!");
        target.RPC("RpcGetKilled", RpcTarget.All);
    }

    [PunRPC]
    public void RpcGetKilled()
    {
        // Visuals for death
        gameObject.SetActive(false); 
        // Optional: Instantiate a dead body prefab here later!
    }
}