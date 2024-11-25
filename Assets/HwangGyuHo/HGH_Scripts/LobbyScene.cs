using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class LobbyScene : MonoBehaviourPunCallbacks
{
    public enum Panel { Login, Menu, Lobby, Room }

    [SerializeField] LoginPanel loginPanel;
    [SerializeField] MainPanel menuPanel;
    [SerializeField] RoomPanel roomPanel;
    [SerializeField] LobbyPanel lobbyPanel;

    private void Start()
    {
        SetActivePanel(Panel.Login);
    }
    /// <summary>
    /// 마스터 서버에 접속을 허락해달라는 요청을 받은 후의 반응
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("접속에 성공했다!");
        SetActivePanel(Panel.Menu);
    }

    /// <summary>
    /// 게임 접속이 끊어졌을 때 보내는 반응
    /// </summary>
    /// <param name="cause"></param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"접속이 끊겼다. cause : {cause}");
        SetActivePanel(Panel.Login);
    }

    /// <summary>
    /// 방 생성 요청에 대한 반응
    /// </summary>
    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성 성공");
    }

    /// <summary>
    /// 방 생성을 실패했을 때 보내주는 반응
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"방 생성 실패, 사유 : {message}");
    }

    /// <summary>
    /// 방을 입장하는데 성공했을때 보내는 반응
    /// </summary>
    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 성공");
        SetActivePanel(Panel.Room);
    }

    /// <summary>
    /// 다른 플레이어가 방으로 입장했을 때 보내는 반응
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomPanel.EnterPlayer(newPlayer);
    }

    /// <summary>
    /// 다른 플레이어가 방을 떠났을때 보내는 반응
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomPanel.ExitPlayer(otherPlayer);
    }

    /// <summary>
    /// 플레이어의 상태가 변경됐을때 그 정보를 다른 플레이어게 동기화 시키는 반응
    /// </summary>
    /// <param name="targetPlayer"></param>
    /// <param name="changedProps"></param>
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        roomPanel.PlayerPropertiesUpdate(targetPlayer, changedProps);
    }

    // public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    // {
    //    
    // }

    /// <summary>
    /// 방을 입장하는데 실패했을때 보내는 반응
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"방 입장 실패, 사유 : {message}");
    }

    /// <summary>
    /// 랜덤매칭을 실패했을때 보내는 반응
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"랜덤 매칭 실패, 사유 : {message}");
    }

    /// <summary>
    /// 자신이 방을 떠났을때 보내는 반응
    /// </summary>
    public override void OnLeftRoom()
    {
        Debug.Log("방 퇴장 성공");
        SetActivePanel(Panel.Menu);
    }

    /// <summary>
    /// 로비로 들어왔을때 보내는 반응
    /// </summary>
    public override void OnJoinedLobby()
    {
        Debug.Log("로비 입장 성공");
        SetActivePanel(Panel.Lobby);
    }

    /// <summary>
    /// 로비를 떠났을때 보내는 반응
    /// </summary>
    public override void OnLeftLobby()
    {
        Debug.Log("로비 퇴장 성공");
        lobbyPanel.ClearRoomEntries();
        SetActivePanel(Panel.Menu);
    }

    /// <summary>
    /// 룸 리스트에 변경이 있었을때 보내는 반응
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 방의 목록이 변경이 있는 경우 서버에서 보내는 정보들
        // 주의사항
        // 1. 처음 로비 입장 시 : 모든 방 목록을 전달
        // 2. 입장 중 방 목록이 변경되는 경우 : 변경된 방 목록만 전달
        lobbyPanel.UpdateRoomList(roomList);
    }

    /// <summary>
    /// 방장이 떠났을때 그 권한을 인계받는 반응
    /// </summary>
    /// <param name="newMasterClient"></param>
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"{newMasterClient.NickName} 플레이어가 방장이 되었습니다.");

         // PhotonNetwork.SetMasterClient(newMasterClient); 저 사람한테 방장 주기, 마스터 클라이언트만 된다
    }

    /// <summary>
    /// 활성화 시킬려는 패널이 맞으면 활성화 시켜주고, 안맞으면 비활성화 시켜주는 함수
    /// </summary>
    /// <param name="panel"></param>
    private void SetActivePanel(Panel panel)
    {
        loginPanel.gameObject.SetActive(panel == Panel.Login);
        menuPanel.gameObject.SetActive(panel == Panel.Menu);
        roomPanel.gameObject.SetActive(panel == Panel.Room);
        lobbyPanel.gameObject.SetActive(panel == Panel.Lobby);
    }
}
