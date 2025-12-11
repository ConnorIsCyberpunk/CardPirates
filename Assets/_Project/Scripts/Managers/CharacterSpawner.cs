using UnityEngine;
using Photon.Pun;
using System.Collections; 

public class CharacterSpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawn Settings")]
    public string playerPrefabName = "Player";
    public Transform spawnPoint;
    public float spawnOffsetY = 1.0f;
    public bool showGizmo = true;

    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            SpawnCharacter();
        }
    }

    void SpawnCharacter()
    {
        Vector3 pos = spawnPoint ? spawnPoint.position : Vector3.zero;
        pos.y += spawnOffsetY;

        // Check for skin data from previous scene
        object[] data = null;
        if (photonView.InstantiationData != null)
            data = photonView.InstantiationData;

        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, pos, Quaternion.identity);
        
        Debug.Log("Player spawned at " + pos);

        // Assign camera target
        if (Camera.main)
        {
            var cam = Camera.main.GetComponent<FollowCamera>();
            var setup = player.GetComponent<PlayerSetup>();
            
            if (cam && setup && setup.cameraAnchor)
            {
                cam.SetTarget(setup.cameraAnchor);
            }
        }

        // Master client handles role distribution after spawn
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(TriggerRoleDistribution());
        }
    }

    IEnumerator TriggerRoleDistribution()
    {
        yield return new WaitForSeconds(0.5f);

        if (RoleManager.Instance != null)
        {
            RoleManager.Instance.DistributeRoles();
        }
        else
        {
            Debug.LogError("RoleManager missing from scene");
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