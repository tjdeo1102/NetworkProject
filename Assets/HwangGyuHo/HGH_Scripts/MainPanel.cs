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
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject createRoomPanel;
    [SerializeField] Button[] modeButton;
    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField maxPlayerInputField;

    [Header("모드선택 버튼")]
    [SerializeField] private bool isColored_0;
    [SerializeField] private bool isColored_1;
    [SerializeField] private bool isColored_2;

    ColorBlock colorBlock = new ColorBlock();

    /// <summary>
    /// 방제목과 인원수 정하는 패널을 OFF
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

    public void SelectModeButton1()
    {
        if (isColored_0 == true)
        {
            colorBlock.selectedColor = Color.white;
            colorBlock.normalColor = Color.white;
            colorBlock.highlightedColor = Color.white;

            modeButton[0].colors = colorBlock;
            colorBlock.colorMultiplier = 5;

            isColored_0 = false;

            return;
        }
        else if (isColored_1 == true || isColored_2 == true)
        {
            modeButton[0].interactable = false;
            return;
        }
        
        isColored_0 = true;
        ButtonColor();
    }

    public void SelectModeButton2()
    {
        if (isColored_1 == true)
        {
            isColored_1 = false;
            colorBlock.selectedColor = Color.white;
            colorBlock.normalColor = Color.white;
            colorBlock.highlightedColor = Color.white;

            modeButton[1].colors = colorBlock;
            colorBlock.colorMultiplier = 5;

            return;
        }
        else if (isColored_0 == true || isColored_2 == true)
        {
            modeButton[1].interactable = false;
            return;
        }

        isColored_1 = true;
        ButtonColor();
    }
    public void SelectModeButton3()
    {
        if (isColored_2 == true)
        {
            colorBlock.selectedColor = Color.white;
            colorBlock.normalColor = Color.white;
            colorBlock.highlightedColor = Color.white;

            modeButton[2].colors = colorBlock;
            colorBlock.colorMultiplier = 5;

            isColored_2 = false;

            return;
        }
        else if (isColored_0 == true || isColored_1 == true)
        {
            modeButton[2].interactable = false;
            return;
        }

        isColored_2 = true;
        ButtonColor();
    }


    public void ButtonColor()
    {
        ColorBlock colorBlock = new ColorBlock();
        colorBlock.selectedColor = Color.green;
        colorBlock.normalColor = Color.green;
        colorBlock.highlightedColor = Color.green;
        colorBlock.colorMultiplier = 3;

        if (isColored_0 == true)
        {
            modeButton[0].colors = colorBlock;
        }
        else if (isColored_1 == true)
        {
            modeButton[1].colors= colorBlock;
            
        }
        else if (isColored_2 == true)
        {
            modeButton[2].colors= colorBlock;
        }
    }

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
