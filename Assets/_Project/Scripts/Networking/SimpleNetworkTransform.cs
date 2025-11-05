using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class SimpleNetworkTransform : MonoBehaviourPun, IPunObservable
{
    Vector3 netPos; Quaternion netRot;

    void Awake(){ netPos = transform.position; netRot = transform.rotation; }

    public void OnPhotonSerializeView(PhotonStream s, PhotonMessageInfo i)
    {
        if (s.IsWriting) { s.SendNext(transform.position); s.SendNext(transform.rotation); }
        else { netPos = (Vector3)s.ReceiveNext(); netRot = (Quaternion)s.ReceiveNext(); }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, netPos, 10f * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, netRot, 10f * Time.deltaTime);
        }
    }
    }
