using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine.UI;
using UnityEditor;
using Unity.VisualScripting;

public class MainPanel : MonoBehaviour
{
    [SerializeField] GameObject menuPanel;               // 메뉴패널 오브젝트
    [SerializeField] GameObject createRoomPanel;         // 방만드는 오브젝트
    [SerializeField] Button[] modeButton;                // 모드 선택하는 버튼 3개
    [SerializeField] TMP_InputField roomNameInputField;  // 방이름 적는 InputField
    [SerializeField] TMP_InputField maxPlayerInputField; // 최대 플레이어수 적는 InputField

    [Header("모드선택버튼 색깔")]
    [SerializeField] public bool isMode_0;           // 모드1 버튼의 색깔
    [SerializeField] public bool isMode_1;           // 모드2 버튼의 색깔
    [SerializeField] public bool isMode_2;           // 모드3 버튼의 색깔

    /// <summary>
    /// 방 만들기창 ON
    /// </summary>
    private void OnEnable()
    {
        createRoomPanel.SetActive(false);
    }

    /// <summary>
    /// 방 만드는 창 메뉴
    /// </summary>
    public void CreateRoomMenu()
    {
        createRoomPanel.SetActive(true);
        roomNameInputField.text = "";
        maxPlayerInputField.text = $"4";
    }

    /// <summary>
    /// 방 만들기
    /// </summary>
    public void CreateRoomConfirm()
    {
        if (isMode_0 == true)
        {
            Debug.Log("방 만들기 성공했습니다.");
            string roomName = roomNameInputField.text;
            if (roomName == "")
            {
                Debug.LogWarning("방 이름을 입력해야 방을 생성할 수 있습니다.");
                return;
            }
            int maxPlayer = int.Parse(maxPlayerInputField.text);
            maxPlayer = Mathf.Clamp(maxPlayer, 1, 4);

            RoomOptions options = new RoomOptions();
            options.MaxPlayers = maxPlayer;
            PhotonNetwork.CreateRoom(roomName, options);
        }
        else if (isMode_1 == true)
        {
            Debug.Log("방 만들기 성공했습니다.");
            string roomName = roomNameInputField.text;
            if (roomName == "")
            {
                Debug.LogWarning("방 이름을 입력해야 방을 생성할 수 있습니다.");
                return;
            }
            int maxPlayer = int.Parse(maxPlayerInputField.text);
            maxPlayer = Mathf.Clamp(maxPlayer, 1, 4);

            RoomOptions options = new RoomOptions();
            options.MaxPlayers = maxPlayer;
            PhotonNetwork.CreateRoom(roomName, options);
        }
        else if (isMode_2 == true)
        {
            Debug.Log("방 만들기 성공했습니다.");
            string roomName = roomNameInputField.text;
            if (roomName == "")
            {
                Debug.LogWarning("방 이름을 입력해야 방을 생성할 수 있습니다.");
                return;
            }
            int maxPlayer = int.Parse(maxPlayerInputField.text);
            maxPlayer = Mathf.Clamp(maxPlayer, 1, 4);

            RoomOptions options = new RoomOptions();
            options.MaxPlayers = maxPlayer;
            PhotonNetwork.CreateRoom(roomName, options);
        }
        else
            return;
    }

    /// <summary>
    /// 방 만들때 취소하기
    /// </summary>
    public void CreateRoomCancel()
    {
        Debug.Log("방 만들기를 취소했습니다");
        createRoomPanel.SetActive(false);
    }

    /// <summary>
    /// 랜덤매칭
    /// </summary>
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

    /// <summary>
    /// 로비 입장
    /// </summary>
    public void JoinLobby()
    {
        Debug.Log("로비 입장 요청");
        PhotonNetwork.JoinLobby();
    }

    /// <summary>
    /// 로그아웃
    /// </summary>
    public void Logout()
    {
        Debug.Log("로그아웃 요청");
        PhotonNetwork.Disconnect();
    }

    /// <summary>
    /// 버튼1을 선택했을때 함수
    /// </summary>
    public void SelectModeButton1()
    {
        if (isMode_0 == true)
        {
            // 선택을 해제했을때 각 버튼을 누를 수 있게 활성화
            isMode_0 = false;
            modeButton[0].interactable = true;
            modeButton[1].interactable = true;
            modeButton[2].interactable = true;
            return;
        }
        else if (isMode_1 == true || isMode_2 == true)
        {
            // 다른 2개의 버튼 중 하나라도 선택되어 있다면 버튼 비활성화
            modeButton[0].interactable = false;
            return;
        }
        
        // 칼라 bool값이 트루가 되면 버튼칼라가 바꾸는 함수 실행
        isMode_0 = true;
    }

    /// <summary>
    /// 모드버튼2 를 눌렀을때
    /// </summary>
    public void SelectModeButton2()
    {
        if (isMode_1 == true)
        {
            isMode_1 = false;
            
            modeButton[0].interactable = true;
            modeButton[1].interactable = true;
            modeButton[2].interactable = true;
            return;
        }
        else if (isMode_0 == true || isMode_2 == true)
        {
            modeButton[1].interactable = false;
            return;
        }
        

        isMode_1 = true;
        
    }

    /// <summary>
    /// 모드버튼3 을 눌렀을때
    /// </summary>
    public void SelectModeButton3()
    {
        if (isMode_2 == true)
        {
            isMode_2 = false;
            modeButton[0].interactable = true;
            modeButton[1].interactable = true;
            modeButton[2].interactable = true;
            return;
        }
        else if (isMode_0 == true || isMode_1 == true)
        {
            modeButton[2].interactable = false;
            return;
        }

        isMode_2 = true;
        
    }

    /// <summary>
    /// 유저ID를 파이어베이스에서 삭제하는 함수
    /// </summary>
    public void DeleteUser()
    {
        FirebaseUser user = BackendManager.Auth.CurrentUser;
        user.DeleteAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("DeleteAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("DeleteAsync encountered an error: " + task.Exception);
                return;
            }

            Debug.Log("User deleted successfully.");
            PhotonNetwork.Disconnect();
        });
    }
}
