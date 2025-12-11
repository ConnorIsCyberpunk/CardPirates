using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;

public class BoatTeleport : MonoBehaviour
{
    public Transform targetArrivalPoint; 
    public float interactionRange = 4.0f;
    public Image fadeImage; 

    private bool isTeleporting = false;
    private Transform localPlayer;

    void Start()
    {
        StartCoroutine(FindPlayerRoutine());
    }

    // Keep trying to find the player until they spawn
    IEnumerator FindPlayerRoutine() 
    {
        while (localPlayer == null) 
        {
            foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
            {
                PhotonView pv = p.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    localPlayer = p.transform;
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        if (isTeleporting || localPlayer == null) return;

        float dist = Vector3.Distance(transform.position, localPlayer.position);
        
        if (dist < interactionRange)
        {
            StartCoroutine(TeleportSequence());
        }
    }

    IEnumerator TeleportSequence()
    {
        isTeleporting = true;

        if (fadeImage)
        {
            fadeImage.enabled = true;
            fadeImage.color = Color.black; 
        }

        CharacterController cc = localPlayer.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        localPlayer.position = targetArrivalPoint.position;
        localPlayer.rotation = targetArrivalPoint.rotation;

        yield return new WaitForSeconds(0.2f); 

        if (cc) cc.enabled = true;

        if (fadeImage)
        {
            yield return new WaitForSeconds(0.5f);
            fadeImage.enabled = false;
        }

        yield return new WaitForSeconds(2.0f);
        isTeleporting = false;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}