using UnityEngine;
using Photon.Pun;

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
        Vector3 pos = spawnPoint ? spawnPoint.position : Vector3.zero;
        pos.y += spawnOffsetY;

        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, pos, Quaternion.identity);
        Debug.Log($"[CharacterSpawner] Spawned player at {pos}");

        // Optional: automatically link FollowCamera if available
        FollowCamera cam = Camera.main ? Camera.main.GetComponent<FollowCamera>() : null;
        if (cam && player)
        {
            PlayerSetup setup = player.GetComponent<PlayerSetup>();
            if (setup && setup.cameraAnchor)
            {
                cam.SetTarget(setup.cameraAnchor);
                Debug.Log("[CharacterSpawner] FollowCamera target set to player anchor.");
            }
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
