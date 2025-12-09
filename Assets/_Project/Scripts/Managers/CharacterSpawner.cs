using UnityEngine;
using Photon.Pun;
using System.Collections; // Needed for Coroutines

public class CharacterSpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawn Settings")]
    [Tooltip("Name of the prefab to spawn (must be inside a Resources folder).")]
    public string playerPrefabName = "Player";
    [Tooltip("Optional Transform where the player will spawn.")]
    public Transform spawnPoint;
    [Tooltip("Extra height offset to prevent spawning inside geometry.")]
    public float spawnOffsetY = 1.0f;

    [Header("Debug")]
    public bool showGizmo = true;

    void Start()
    {
        // Ensure we are connected and ready before spawning
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            SpawnCharacter();
        }
        else
        {
            Debug.LogWarning("[CharacterSpawner] Not connected to Photon room; no spawn performed.");
        }
    }

    void SpawnCharacter()
    {
        // 1. Calculate Position
        Vector3 pos = spawnPoint ? spawnPoint.position : Vector3.zero;
        pos.y += spawnOffsetY;

        // 2. Get the Skin Index (passed from CharacterSelect)
        object[] data = null;
        if (photonView.InstantiationData != null)
            data = photonView.InstantiationData;

        // 3. Instantiate Networked Player
        // Note: We use the data passed from the previous scene if available
        // If this script is on an object that doesn't hold data, we rely on the prefab defaults
        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, pos, Quaternion.identity);
        
        Debug.Log($"[CharacterSpawner] Spawned player at {pos}");

        // 4. Link Camera
        FollowCamera cam = Camera.main ? Camera.main.GetComponent<FollowCamera>() : null;
        if (cam && player)
        {
            PlayerSetup setup = player.GetComponent<PlayerSetup>();
            if (setup && setup.cameraAnchor)
            {
                cam.SetTarget(setup.cameraAnchor);
            }
        }

        // --- NEW LOGIC FOR ROLE REVEAL ---
        // If I am the Master Client, now that I have spawned, I trigger the role assignment.
        // We wait a tiny bit to ensure everyone's connection is stable.
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(TriggerRoleDistribution());
        }
    }

    IEnumerator TriggerRoleDistribution()
    {
        // Wait 0.5 seconds to ensure any other immediate joiners are ready
        yield return new WaitForSeconds(0.5f);

        if (RoleManager.Instance != null)
        {
            RoleManager.Instance.DistributeRoles();
        }
        else
        {
            Debug.LogError("[CharacterSpawner] RoleManager not found! Make sure GameManager is in the scene.");
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Gizmos.color = Color.green;
        Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
        pos.y += spawnOffsetY;
        Gizmos.DrawSphere(pos, 0.25f);
        Gizmos.DrawLine(pos, pos + Vector3.down * spawnOffsetY * 0.5f);
    }
}