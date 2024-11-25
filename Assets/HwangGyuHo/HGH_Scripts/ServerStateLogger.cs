using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ServerStateLogger : MonoBehaviourPunCallbacks
{
    // 현재 클라이언트의 상태를 알 수 있는 클래스
    [SerializeField] ClientState state;

    // 서버 상황을 알려주는 코드
    private void Update()
    {
        // 상태가 바뀌는 것이 확인되면 로그를 찍어준다
        if (state == PhotonNetwork.NetworkClientState)
            return;

        // 서버에게 나의 상태가 어떤지 요청하는 구문
        state = PhotonNetwork.NetworkClientState;
        Debug.Log($"[Pun] {state}");
    }

}
