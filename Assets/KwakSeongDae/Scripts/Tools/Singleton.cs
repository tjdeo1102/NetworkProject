using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviourPunCallbacks
{
    public static T Instance { get; protected set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = gameObject.GetComponent<T>();
            DontDestroyOnLoad(gameObject);

            photonView.RPC("Init", RpcTarget.All);
            //Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [PunRPC]
    protected virtual void Init() { }
}