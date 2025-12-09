using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;

public class BoatTeleport : MonoBehaviour
{
    [Header("Link")]
    public Transform targetArrivalPoint; 

    [Header("Settings")]
    public float interactionRange = 4.0f; // NEW SETTING

    [Header("UI (Optional)")]
    public Image fadeImage; 

    private bool isTeleporting = false;
    private Transform localPlayerTransform;

    void Update()
    {
        if (isTeleporting) return;

        // 1. Find Local Player (Cache it)
        if (localPlayerTransform == null)
        {
            foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
            {
                PhotonView pv = p.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    localPlayerTransform = p.transform;
                    break;
                }
            }
            return;
        }

        // 2. MATH CHECK (No Physics needed)
        float distance = Vector3.Distance(transform.position, localPlayerTransform.position);
        
        if (distance < interactionRange)
        {
            StartCoroutine(TeleportSequence(localPlayerTransform));
        }
    }

    IEnumerator TeleportSequence(Transform player)
    {
        isTeleporting = true;

        // Fade Out
        if (fadeImage)
        {
            fadeImage.enabled = true;
            fadeImage.color = Color.black; // Instant black for test
        }

        // Move
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        player.position = targetArrivalPoint.position;
        player.rotation = targetArrivalPoint.rotation;

        yield return new WaitForSeconds(0.2f); 

        if (cc) cc.enabled = true;

        // Fade In
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