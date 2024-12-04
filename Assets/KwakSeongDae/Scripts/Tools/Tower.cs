using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviourPun, IPunObservable
{
    private Rigidbody2D rigid;

    void Awake()
    {
        object[] data = photonView.InstantiationData;
        gameObject.name = $"Tower{data[0]}";

        GetComponent<BlockMaxHeightManager>().gameState = PhotonView.Find((int)data[1]).GetComponent<GameState>();
        rigid = GetComponent<Rigidbody2D>();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rigid.position);
        }
        else
        {
            rigid.position = (Vector2)(stream.ReceiveNext());
        }
    }
}
