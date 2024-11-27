using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGameManager : MonoBehaviourPunCallbacks
{
    private const string roomName = "test";
    private const int gameSceneNum = 2;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = $"{Random.Range(0, 10000)}";
        PhotonNetwork.ConnectUsingSettings();

        DontDestroyOnLoad(gameObject);
    }

    public void JoinRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = false;
        roomOptions.MaxPlayers = 4;
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);

        print("¹æ Âü°¡");
    }

    public void SceneLoad()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        PhotonNetwork.LoadLevel(gameSceneNum);
    }
}
