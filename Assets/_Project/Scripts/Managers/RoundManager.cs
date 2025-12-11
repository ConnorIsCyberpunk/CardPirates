using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro; 

public class RoundManager : MonoBehaviourPunCallbacks
{
    [Header("Settings")]
    public float roundTime = 120f;
    public Transform teleportTarget; 

    [Header("UI")]
    public GameObject courtPanel;
    public GameObject buttonPrefab; 
    public TMP_Text timerText;      

    private bool isCourtSession = false;
    private float currentTime;

    void Start()
    {
        currentTime = roundTime;
        if(courtPanel) courtPanel.SetActive(false);
        
        if (!teleportTarget) 
        {
            GameObject sp = GameObject.Find("SpawnPoint");
            if(sp) teleportTarget = sp.transform;
        }
    }

    void Update()
    {
        if (!isCourtSession)
        {
            currentTime -= Time.deltaTime;
            
            if (timerText) timerText.text = "TIME: " + Mathf.Ceil(currentTime);

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

        // Move local player to court
        GameObject myPlayer = GetLocalPlayerObject();
        if (myPlayer && teleportTarget)
        {
            CharacterController cc = myPlayer.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            
            myPlayer.transform.position = teleportTarget.position;
            
            if (cc) cc.enabled = true;
        }

        GenerateCourtUI();
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void GenerateCourtUI()
    {
        if (!courtPanel || !buttonPrefab) return;
        courtPanel.SetActive(true);

        foreach (Transform child in courtPanel.transform) Destroy(child.gameObject);

        // Check if I am captain
        object roleObj;
        bool isCaptain = false;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out roleObj))
        {
            isCaptain = ((string)roleObj == "CAPTAIN");
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p == PhotonNetwork.LocalPlayer) continue; 

            GameObject btnObj = Instantiate(buttonPrefab, courtPanel.transform);
            
            TMP_Text t = btnObj.GetComponentInChildren<TMP_Text>();
            if (t) t.text = isCaptain ? "EJECT " + p.NickName : "ACCUSE " + p.NickName;

            Button b = btnObj.GetComponent<Button>();
            if (isCaptain)
            {
                Player target = p; 
                b.onClick.AddListener(() => EjectPlayer(target));
                b.GetComponent<Image>().color = new Color(1, 0.5f, 0.5f);
            }
            else
            {
                b.interactable = false; 
            }
        }
    }

    void EjectPlayer(Player target)
    {
        photonView.RPC("RpcEjectPlayer", RpcTarget.All, target);
        
        courtPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    [PunRPC]
    void RpcEjectPlayer(Player targetPlayer)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); 
        
        foreach (GameObject pObj in players)
        {
            PhotonView pv = pObj.GetComponent<PhotonView>();
            if (pv && pv.Owner == targetPlayer)
            {
                Debug.Log(targetPlayer.NickName + " was ejected.");
                
                // Add physics to throw them off ship
                Rigidbody rb = pObj.GetComponent<Rigidbody>();
                if (!rb) rb = pObj.AddComponent<Rigidbody>(); 
                
                CharacterController cc = pObj.GetComponent<CharacterController>();
                if (cc) cc.enabled = false; 
                
                rb.isKinematic = false;
                rb.useGravity = true;
                
                rb.AddForce((Vector3.right * 500f) + (Vector3.up * 200f));
                rb.AddTorque(Random.insideUnitSphere * 100f);
                return;
            }
        }
    }

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