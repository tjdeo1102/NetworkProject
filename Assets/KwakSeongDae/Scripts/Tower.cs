using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviourPun
{
    void Start()
    {
        object[] data = photonView.InstantiationData;
        gameObject.name = $"{data[0]}";
    }
}
