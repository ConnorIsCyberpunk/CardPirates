using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro; // Keep this just in case
using UnityEngine.UI; // Needed for Legacy Text fallback

public class RoleManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public GameObject rolePanel;
    
    // CHANGE THIS: We now accept the raw GameObject so you can definitely drag it in
    public GameObject roleTextObject; 

    [Header("Settings")]
    public float revealDuration = 5f;

    public static RoleManager Instance;
    private bool hasDistributed = false;

    void Awake()
    {
        Instance = this;
        if(rolePanel) rolePanel.SetActive(false); 
    }

    public void DistributeRoles()
    {
        if (!PhotonNetwork.IsMasterClient || hasDistributed) return;
        hasDistributed = true;

        Player[] players = PhotonNetwork.PlayerList;
        List<Player> shuffled = players.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < shuffled.Count; i++)
        {
            string roleName = (i == 0) ? "CAPTAIN" : (i == 1 && shuffled.Count >= 2 ? "SABOTEUR" : "Crewmate");
            string colorHex = (i == 0) ? "#FFD700" : (i == 1 && shuffled.Count >= 2 ? "#FF0000" : "#FFFFFF");
            
            photonView.RPC("RpcAnnounceRole", shuffled[i], roleName, colorHex);
        }
    }

    [PunRPC]
    void RpcAnnounceRole(string role, string colorHex)
    {
        Debug.Log($"[RoleManager] Role Received: {role}"); // Debug check

        if (rolePanel) rolePanel.SetActive(true);
        
        if (roleTextObject) 
        {
            string msg = "YOU ARE THE\n<size=120%>" + role + "</size>";
            
            // TRY BOTH TYPES (Foolproof)
            TMP_Text tmp = roleTextObject.GetComponent<TMP_Text>();
            Text legacy = roleTextObject.GetComponent<Text>();

            if (tmp != null)
            {
                tmp.text = msg;
                if(ColorUtility.TryParseHtmlString(colorHex, out Color c)) tmp.color = c;
            }
            else if (legacy != null)
            {
                legacy.text = msg;
                if(ColorUtility.TryParseHtmlString(colorHex, out Color c)) legacy.color = c;
            }
        }

        // Save Role
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["Role"] = role;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Invoke(nameof(HideUI), revealDuration);
    }

    void HideUI()
    {
        if (rolePanel) rolePanel.SetActive(false);
    }
}