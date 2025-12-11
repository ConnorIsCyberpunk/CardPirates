using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ImposterAbility : MonoBehaviourPun
{
    public float killRange = 3.0f;
    public KeyCode killKey = KeyCode.F;

    private GameObject killUI; 
    private bool hasKilled = false;
    private bool isSaboteur = false;

    void Start()
    {
        // Find the prompt UI
        GameObject found = GameObject.Find("KillPromptText");
        if (found) 
        {
            killUI = found;
            killUI.SetActive(false); 
        }

        // Check role once at start
        if (photonView.IsMine)
        {
             object myRole;
             if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out myRole))
             {
                 if ((string)myRole == "SABOTEUR") isSaboteur = true;
             }
        }
    }

    void Update()
    {
        if (!photonView.IsMine || !isSaboteur) return;
        if (hasKilled) return;

        CheckForVictim();
    }

    void CheckForVictim()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, killRange))
        {
            PhotonView targetView = hit.collider.GetComponent<PhotonView>();
            
            if (targetView != null && !targetView.IsMine)
            {
                string targetRole = "";
                if (targetView.Owner.CustomProperties.ContainsKey("Role"))
                    targetRole = (string)targetView.Owner.CustomProperties["Role"];
                
                // Can't kill the Captain
                if (targetRole == "CAPTAIN") 
                {
                    if(killUI) killUI.SetActive(false);
                    return; 
                }

                if(killUI) killUI.SetActive(true); 

                if (Input.GetKeyDown(killKey))
                {
                    DoKill(targetView);
                }
                return;
            }
        }

        if(killUI) killUI.SetActive(false);
    }

    void DoKill(PhotonView target)
    {
        hasKilled = true;
        if(killUI) killUI.SetActive(false);

        Debug.Log("Kill confirmed on " + target.Owner.NickName);
        target.RPC("RpcGetKilled", RpcTarget.All);
    }

    [PunRPC]
    public void RpcGetKilled()
    {
        gameObject.SetActive(false); 
    }
}