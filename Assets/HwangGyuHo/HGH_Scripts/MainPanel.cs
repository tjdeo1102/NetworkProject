using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class MainPanel : MonoBehaviour
{
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject createRoomPanel;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField maxPlayerInputField;
    
    /// <summary>
    /// 방제목과 인원수 정하는 패널을 OFF
    /// </summary>
    private void OnEnable()
    {
        createRoomPanel.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void CreateRoomMenu()
    {
        createRoomPanel.SetActive(true);
        roomNameInputField.text = "";
        maxPlayerInputField.text = $"4";
    }

    public void CreateRoomConfirm()
    {
        Debug.Log("방 만들기 성공했습니다.");
        string roomName = roomNameInputField.text;
        if (roomName == "")
        {
            Debug.LogWarning("방 이름을 입력해야 방을 생성할 수 있습니다.");
            return;
        }
        int maxPlayer = int.Parse(maxPlayerInputField.text);
        maxPlayer = Mathf.Clamp(maxPlayer,1,4);

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = maxPlayer;
        PhotonNetwork.CreateRoom(roomName, options);

    }

    public void CreateRoomCancel()
    {
        Debug.Log("방 만들기를 취소했습니다");
        createRoomPanel.SetActive(false);
    }

    public void RandomMatching()
    {
        Debug.Log("랜덤매칭 요청");
        // 비어있는 방이 없으면 들어가지 않는 방식
        // PhotonNetwork.JoinRandomRoom();

        // 비어있는 방이 없으면 새로 방을 만들어서 들어가는 방식
        // 그래서 얘는 방을 만들기 위한 내용도 써줘야 한다
        string Name = $"Room {Random.Range(1000, 10000)}";
        RoomOptions options = new RoomOptions() { MaxPlayers = 4 };
        PhotonNetwork.JoinRandomOrCreateRoom(roomName: name, roomOptions: options);
    }

    public void JoinLobby()
    {
        Debug.Log("로비 입장 요청");
        PhotonNetwork.JoinLobby();
    }

    public void Logout()
    {
        Debug.Log("로그아웃 요청");
        PhotonNetwork.Disconnect();
    }
}
