using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class SimpleNetworkTransform : MonoBehaviourPun, IPunObservable
{
    private Vector3 netPos; 
    private Quaternion netRot;

    void Awake()
    { 
        netPos = transform.position; 
        netRot = transform.rotation; 
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) 
        { 
            // We own this object: send our data to others
            stream.SendNext(transform.position); 
            stream.SendNext(transform.rotation); 
        }
        else 
        { 
            // Network player: receive data
            netPos = (Vector3)stream.ReceiveNext(); 
            netRot = (Quaternion)stream.ReceiveNext(); 
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            // Smoothly move network players to their real position
            transform.position = Vector3.Lerp(transform.position, netPos, 10f * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, netRot, 10f * Time.deltaTime);
        }
    }
}