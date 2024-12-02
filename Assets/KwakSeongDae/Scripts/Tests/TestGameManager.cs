using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGameManager : MonoBehaviourPunCallbacks
{
    private const string roomName = "test";
    private bool canJoin = false;
    private void Start()
    {
        if (PhotonNetwork.IsConnected == false )
        {
            PhotonNetwork.NickName = $"{Random.Range(0, 10000)}";
            PhotonNetwork.ConnectUsingSettings();
        }     
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        canJoin = true;
    }

    public override void OnJoinedRoom()
    {
        print($"방 참가 {PhotonNetwork.CurrentRoom.Name}");
    }

    public void JoinRoom()
    {
        if (canJoin)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = false;
            roomOptions.MaxPlayers = 4;
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (returnCode == ErrorCode.GameClosed)
        {
            Debug.Log("다른 이름의 방으로 자동 생성 참가할 수 없습니다.");
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = false;
            roomOptions.MaxPlayers = 4;
            var newRoomName = roomName + "_" + System.Guid.NewGuid().ToString();
            PhotonNetwork.JoinOrCreateRoom(newRoomName, roomOptions, TypedLobby.Default);
        }
    }

    public void SceneLoad(int gameSceneNum)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        print("씬 로드");

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(gameSceneNum);
    }
}
