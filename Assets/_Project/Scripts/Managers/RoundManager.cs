using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro; // Use TextMeshPro for the buttons

public class RoundManager : MonoBehaviourPunCallbacks
{
    [Header("Settings")]
    public float roundTime = 120f;
    public Transform teleportTarget; // Assign your SpawnPoint here

    [Header("UI References")]
    public GameObject courtPanel;
    public GameObject buttonPrefab; // Drag your Button Prefab here
    public TMP_Text timerText;      // Optional: Drag a text to see the countdown

    private bool isCourtSession = false;
    private float currentTime;

    void Start()
    {
        currentTime = roundTime;
        if(courtPanel) courtPanel.SetActive(false);
        
        // Auto-find SpawnPoint if not set
        if (!teleportTarget) 
        {
            GameObject sp = GameObject.Find("SpawnPoint");
            if(sp) teleportTarget = sp.transform;
        }
    }

    void Update()
    {
        // Only Master Client controls the official game time, 
        // but for a simple prototype, everyone running their own timer is safer/easier
        if (!isCourtSession)
        {
            currentTime -= Time.deltaTime;
            
            // Update Visual Timer
            if (timerText) timerText.text = $"TIME: {Mathf.Ceil(currentTime)}";

            if (currentTime <= 0)
            {
                StartCourtSession();
            }
        }
    }

    void StartCourtSession()
    {
        if (isCourtSession) return;
        isCourtSession = true;

        // 1. TELEPORT TO DECK
        // We move our own local player
        GameObject myPlayer = GetLocalPlayerObject();
        if (myPlayer && teleportTarget)
        {
            // Disable CharacterController briefly to allow teleport
            CharacterController cc = myPlayer.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            
            myPlayer.transform.position = teleportTarget.position;
            
            if (cc) cc.enabled = true;
        }

        // 2. SHOW UI
        GenerateCourtUI();
        
        // 3. UNLOCK CURSOR
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void GenerateCourtUI()
    {
        if (!courtPanel || !buttonPrefab) return;
        courtPanel.SetActive(true);

        // Clean up old buttons
        foreach (Transform child in courtPanel.transform) Destroy(child.gameObject);

        // Am I the Captain?
        object roleObj;
        bool isCaptain = false;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out roleObj))
        {
            isCaptain = ((string)roleObj == "CAPTAIN");
        }

        // Generate a button for every OTHER player
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p == PhotonNetwork.LocalPlayer) continue; // Don't eject yourself

            GameObject btnObj = Instantiate(buttonPrefab, courtPanel.transform);
            
            // Setup Text
            TMP_Text t = btnObj.GetComponentInChildren<TMP_Text>();
            if (t) t.text = isCaptain ? $"EJECT {p.NickName}" : $"ACCUSE {p.NickName}";

            // Setup Click Action
            Button b = btnObj.GetComponent<Button>();
            if (isCaptain)
            {
                // CAPTAIN POWER: Clicking actually kills them
                Player target = p; // Cache for lambda
                b.onClick.AddListener(() => EjectPlayer(target));
                // Color it Red for danger
                b.GetComponent<Image>().color = new Color(1, 0.5f, 0.5f);
            }
            else
            {
                // CREW POWER: Clicking does nothing in MVP (Voice Chat is used instead)
                b.interactable = false; 
            }
        }
    }

    // --- CAPTAIN ACTIONS ---

    void EjectPlayer(Player target)
    {
        // Send RPC to everyone to execute this player
        photonView.RPC("RpcEjectPlayer", RpcTarget.All, target);
        
        // Close UI
        courtPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    [PunRPC]
    void RpcEjectPlayer(Player targetPlayer)
    {
        // Find the player object belonging to that player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); // Make sure Player Prefab is tagged "Player"
        
        foreach (GameObject pObj in players)
        {
            PhotonView pv = pObj.GetComponent<PhotonView>();
            if (pv && pv.Owner == targetPlayer)
            {
                Debug.Log($"{targetPlayer.NickName} WALKS THE PLANK!");
                
                // PHYSICS YEET
                Rigidbody rb = pObj.GetComponent<Rigidbody>();
                if (!rb) rb = pObj.AddComponent<Rigidbody>(); // Add RB if missing
                
                CharacterController cc = pObj.GetComponent<CharacterController>();
                if (cc) cc.enabled = false; // Disable CC so Physics can take over
                
                rb.isKinematic = false;
                rb.useGravity = true;
                
                // Throw them sideways (Off the ship) + Up
                rb.AddForce((Vector3.right * 500f) + (Vector3.up * 200f));
                
                // Add spin for drama
                rb.AddTorque(Random.insideUnitSphere * 100f);
                
                return;
            }
        }
    }

    // Helper to find my own object
    GameObject GetLocalPlayerObject()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            PhotonView pv = p.GetComponent<PhotonView>();
            if (pv && pv.IsMine) return p;
        }
        return null;
    }
}