using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNetwork_Jun : MonoBehaviourPunCallbacks
{
    public const string roomName = "TestRoom";

    void Start()
    {
        PhotonNetwork.NickName = $"Player_{Random.Range(0, 10000)}";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("<color=green>Connected To Master</color>");
        RoomOptions options = new RoomOptions();
        options.IsVisible = false;
        options.MaxPlayers = 4;

        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("<color=green>Joined Room</color>");
        StartCoroutine(WaitRoutine());
    }

    private IEnumerator WaitRoutine()
    {
        yield return new WaitForSeconds(1f);
        StartTest();
    }

    private void StartTest()
    {
        Debug.Log("<color=yellow>Start Test</color>");

        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity, data: new object[] { PhotonNetwork.LocalPlayer.NickName });
    }
}
