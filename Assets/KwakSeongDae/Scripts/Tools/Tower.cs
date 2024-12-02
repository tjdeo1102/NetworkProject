using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviourPun
{
    void Awake()
    {
        object[] data = photonView.InstantiationData;
        gameObject.name = $"Tower{data[0]}";

        GetComponent<BlockMaxHeightManager>().gameState = PhotonView.Find((int)data[1]).GetComponent<GameState>();
    }
}
