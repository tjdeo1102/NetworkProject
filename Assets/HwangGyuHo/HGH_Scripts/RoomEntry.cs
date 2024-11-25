using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomEntry : MonoBehaviour
{
    [SerializeField] TMP_Text roomName;
    [SerializeField] TMP_Text currentPlayer;
    [SerializeField] Button joinRoomButton;

    // 방에 대한 정보 세팅
    public void SetRoomInfo(RoomInfo info)
    {
        roomName.text = info.Name;
        currentPlayer.text = $"{info.PlayerCount} / {info.MaxPlayers}";
        joinRoomButton.interactable = info.PlayerCount < info.MaxPlayers;
    }

    public void JoinRoom()
    {
        // 방 들어갈거니까 로비는 나가는 구문
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.JoinRoom(roomName.text);
    }
}
