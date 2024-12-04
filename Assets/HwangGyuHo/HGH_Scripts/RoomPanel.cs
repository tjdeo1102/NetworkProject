using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class RoomPanel : MonoBehaviour
{
    [SerializeField] PlayerEntry[] playerEntries;
    [SerializeField] Button[] modeButton;                // 모드 선택하는 버튼 3개
    [SerializeField] Button startButton;
    // [SerializeField] MainPanel gameMode;
    // [SerializeField] private TMP_InputField gameModeText;
    [SerializeField] private int gameScene;
    [SerializeField] PhotonView photonView;
    [SerializeField] TMP_Text modeText;

    [Header("Mode Select")]
    [SerializeField] private bool isMode_0;           // 모드1 버튼
    [SerializeField] private bool isMode_1;           // 모드2 버튼
    [SerializeField] private bool isMode_2;           // 모드3 버튼


    private void OnEnable()
    {
        UpdatePlayer();

        // PlayerNumbering 에 플레이어 추가
        PlayerNumbering.OnPlayerNumberingChanged += UpdatePlayer;

        PhotonNetwork.LocalPlayer.SetReady(false);

        if (PhotonNetwork.IsMasterClient == true)
            SendSelectMode1();
    }

    private void OnDisable()
    {
        // PlayerNumbering에 플레이어 빼기
        PlayerNumbering.OnPlayerNumberingChanged -= UpdatePlayer;
    }

    // public void GameModeUI()
    // {
    //     if(gameMode.isMode_0 == true)
    //     {
    //         gameModeText.text = "Mode 1";
    //     }
    //     else if(gameMode.isMode_1 == true)
    //     {
    //         gameModeText.text = "Mode 2";
    //     }
    //     else if (gameMode.isMode_2 == true)
    //     {
    //         gameModeText.text = "Mode 3";
    //     }
    // }

    /// <summary>
    /// 버튼1을 선택했을때 함수
    /// </summary>
    [PunRPC]
    public void SelectModeButton1()
    {
        gameScene = 1;
        modeText.text = $"Puzzle";
        //ColorBlock colorBlock = modeButton[0].colors;
        //colorBlock.normalColor = Color.green;
        //modeButton[0].colors = colorBlock;
        
        if (isMode_0 == true)
        {
            // 선택을 해제했을때 각 버튼을 누를 수 있게 활성화
            isMode_0 = false;
            //modeButton[0].interactable = true;
            //modeButton[1].interactable = true;
            //modeButton[2].interactable = true;

            //colorBlock.normalColor = Color.white;
            //modeButton[0].colors = colorBlock;
            Debug.Log($"isMode_0: {isMode_0}, isMode_1: {isMode_1}, isMode_2: {isMode_2}");

            return;
        }
        // 모드2, 모드3 버튼이 true 라면 모드1의 버튼을 하얗게, ismode_1,2 을 false로.
        else if (isMode_1 == true || isMode_2 == true)
        {
            //colorBlock.normalColor = Color.white;
            //modeButton[1].colors = colorBlock;
            //modeButton[2].colors = colorBlock;

            isMode_1 = false;
            isMode_2 = false;

            Debug.Log($"isMode_0: {isMode_0}, isMode_1: {isMode_1}, isMode_2: {isMode_2}");
        }
        isMode_0 = true;
        modeButton[0].interactable = false;
        if (PhotonNetwork.IsMasterClient == true)
        {
            modeButton[1].interactable = true;
            modeButton[2].interactable = true;
        }
        else
        {
            modeButton[1].interactable = false;
            modeButton[2].interactable = false;
        }
    }

    public void SendSelectMode1()
    {
        if (photonView.IsMine == false)
            return;

        if (PhotonNetwork.IsMasterClient == false) return;

        photonView.RPC("SelectModeButton1", RpcTarget.AllBuffered);
    }

    public void SendSelectMode2()
    {
        if (photonView.IsMine == false)
            return;

        if (PhotonNetwork.IsMasterClient == false) return;

        photonView.RPC("SelectModeButton2", RpcTarget.AllBuffered);
    }

    public void SendSelectMode3()
    {
        if (photonView.IsMine == false)
            return;

        if (PhotonNetwork.IsMasterClient == false) return;

        photonView.RPC("SelectModeButton3", RpcTarget.AllBuffered);
    }

    /// <summary>
    /// 모드버튼2 를 눌렀을때
    /// </summary>
    [PunRPC]
    public void SelectModeButton2()
    {
        gameScene = 2;
        modeText.text = $"Race";
        //ColorBlock colorBlock = modeButton[1].colors;
        //colorBlock.normalColor = Color.green;
        //modeButton[1].colors = colorBlock;

        if (isMode_1 == true)
        {
            isMode_1 = false;

            //modeButton[0].interactable = true;
            //modeButton[1].interactable = true;
            //modeButton[2].interactable = true;

            //colorBlock.normalColor = Color.white;
            //modeButton[1].colors = colorBlock;
            
            return;
        }
        else if (isMode_0 == true || isMode_2 == true)
        {
            //colorBlock.normalColor = Color.white;
            //modeButton[0].colors = colorBlock;
            //modeButton[2].colors = colorBlock;

            isMode_0 = false;
            isMode_2 = false;
        }
        isMode_1 = true;
        modeButton[1].interactable = false;
        if (PhotonNetwork.IsMasterClient == true)
        {
            modeButton[0].interactable = true;
            modeButton[2].interactable = true;
        }
        else
        {
            modeButton[0].interactable = false;
            modeButton[2].interactable = false;
        }

        Debug.Log($"isMode_0: {isMode_0}, isMode_1: {isMode_1}, isMode_2: {isMode_2}");
    }

    /// <summary>
    /// 모드버튼3 을 눌렀을때
    /// </summary>
    [PunRPC]
    public void SelectModeButton3()
    {
        gameScene = 3;
        modeText.text = $"Surviver";
        //ColorBlock colorBlock = modeButton[2].colors;
        //colorBlock.normalColor = Color.green;
        //modeButton[2].colors = colorBlock;

        if (isMode_2 == true)
        {
            isMode_2 = false;
            //modeButton[0].interactable = true;
            //modeButton[1].interactable = true;
            //modeButton[2].interactable = true;

            //colorBlock.normalColor = Color.white;

            //modeButton[2].colors = colorBlock;
            return;
        }
        else if (isMode_0 == true || isMode_1 == true)
        {
            //colorBlock.normalColor = Color.white;
            //modeButton[0].colors = colorBlock;
            //modeButton[1].colors = colorBlock;

            isMode_1 = false;
            isMode_0 = false;
        }

        isMode_2 = true;
        modeButton[2].interactable = true;
        if (PhotonNetwork.IsMasterClient == true)
        {
            modeButton[0].interactable = true;
            modeButton[1].interactable = true;
        }
        else
        {
            modeButton[0].interactable = false;
            modeButton[1].interactable = false;
        }

        Debug.Log($"isMode_0: {isMode_0}, isMode_1: {isMode_1}, isMode_2: {isMode_2}");
    }


    public void UpdatePlayer()
    {
        
        foreach (PlayerEntry entry in playerEntries)
        {
            entry.SetEmpty();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.GetPlayerNumber() == -1)
                return;

            int number = player.GetPlayerNumber();
            playerEntries[number].SetPlayer(player);
        }
        Debug.Log($"LocalPlayer: {PhotonNetwork.LocalPlayer.NickName}");
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startButton.interactable = AllPlayerReadyCheck();
            Debug.Log($"photonView:{photonView.ViewID}");
        }
        else
        {
            startButton.interactable = false;
            //for (int i = 0; i < modeButton.Length; i++)
            //{
            //    modeButton[i].gameObject.SetActive(false);
            //}
        }
    }

    public void EnterPlayer(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} 입장!");
        UpdatePlayer();
    }

    public void ExitPlayer(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} 퇴장!");
        UpdatePlayer();
    }

    public void PlayerPropertiesUpdate(Player targetPlayer, Hashtable properties)
    {
        // 레디 커스텀 프로퍼티를 변경한 경우면 RREADY 키가 있음
        // TODO : 플레이어 속성이 바뀌면 그것을 업데이트
        Debug.Log($"{targetPlayer.NickName} 정보변경!!");
        if (properties.ContainsKey(CustomPropert.READY))
        {
            UpdatePlayer();
        }
    }

    public void StartGame()
    {
        
        // 플레이어들 READY가 모두 되면 게임시작 버튼으로 게임시작
        // 씬 이름이 일치해야 함
        PhotonNetwork.LoadLevel(gameScene);
        // 게임 플레이 중에는 방에 들어올 수 없게
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    public void LeaveRoom()
    {
        Debug.Log("방을 떠났습니다");
        PhotonNetwork.LeaveRoom();
    }

    public bool AllPlayerReadyCheck()
    {
        // 모든 플레이어의 레디 체크
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            if (player.GetReady() == false)
                return false;
        }
        return true;
    }
}
