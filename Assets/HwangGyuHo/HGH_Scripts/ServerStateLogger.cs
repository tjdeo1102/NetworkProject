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
        // 상태가 바뀐 상황에서만 확인하는 코드
        // 상태가 안바뀌었다면 확인안한다
        // Update 프레임마다 확인할 수는 없으니까
        // 로그를 찍어주는 코드
        if (state == PhotonNetwork.NetworkClientState)
            return;

        // 서버에게 나의 상태가 어떤지 요청하는 구문
        state = PhotonNetwork.NetworkClientState;
        Debug.Log($"[Pun] {state}");
    }

}
