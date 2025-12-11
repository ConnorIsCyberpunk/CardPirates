using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro; 
using UnityEngine.UI; 

public class RoleManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public GameObject rolePanel;
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
        // Randomize list
        List<Player> shuffled = players.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < shuffled.Count; i++)
        {
            string roleName = "Crewmate";
            string colorHex = "#FFFFFF"; // White

            if (i == 0)
            {
                roleName = "CAPTAIN";
                colorHex = "#FFD700"; // Gold
            }
            else if (i == 1 && shuffled.Count >= 2)
            {
                roleName = "SABOTEUR";
                colorHex = "#FF0000"; // Red
            }
            
            photonView.RPC("RpcAnnounceRole", shuffled[i], roleName, colorHex);
        }
    }

    [PunRPC]
    void RpcAnnounceRole(string role, string colorHex)
    {
        if (rolePanel) rolePanel.SetActive(true);
        
        if (roleTextObject) 
        {
            string msg = "YOU ARE THE\n<size=120%>" + role + "</size>";
            
            // Check for TextMeshPro or standard Text
            TMP_Text tmp = roleTextObject.GetComponent<TMP_Text>();
            Text legacy = roleTextObject.GetComponent<Text>();

            Color c = Color.white;
            ColorUtility.TryParseHtmlString(colorHex, out c);

            if (tmp != null)
            {
                tmp.text = msg;
                tmp.color = c;
            }
            else if (legacy != null)
            {
                legacy.text = msg;
                legacy.color = c;
            }
        }

        // Save Role to player properties
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["Role"] = role;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Invoke("HideUI", revealDuration);
    }

    void HideUI()
    {
        if (rolePanel) rolePanel.SetActive(false);
    }
}