using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
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
    [SerializeField] TMP_Text roomTitle;

    //[Header("Mode Select")]
    private bool[] isMode;
    //[SerializeField] private bool isMode_0;           // 모드1 버튼
    //[SerializeField] private bool isMode_1;           // 모드2 버튼
    //[SerializeField] private bool isMode_2;           // 모드3 버튼

    private void Awake()
    {
        isMode = new bool[3];
    }

    private void OnEnable()
    {
        UpdatePlayer();

        // PlayerNumbering 에 플레이어 추가
        PlayerNumbering.OnPlayerNumberingChanged += UpdatePlayer;

        PhotonNetwork.LocalPlayer.SetReady(false);
        int mode = PhotonNetwork.CurrentRoom.GetMode();

        if (PhotonNetwork.IsMasterClient == true)
        {
            for (int i = 0; i < modeButton.Length; i++)
            {
                modeButton[i].animationTriggers.highlightedTrigger = "Highlighted";
            }
            ModeSelctWrapper(mode);
        }
        else
        {
            for (int i = 0; i < modeButton.Length; i++)
            {
                print($"{i}번째: {isMode[i]}");
                modeButton[i].animationTriggers.highlightedTrigger = "";
            }
            ModeSelect(mode);
        }

        roomTitle?.SetText(PhotonNetwork.CurrentRoom.Name);

    }

    
    public void ModeSelctWrapper(int modeIndex)
    {
        if (PhotonNetwork.IsMasterClient == false) return;

        photonView.RPC("ModeSelect", RpcTarget.AllBuffered, modeIndex);
    }

    [PunRPC]
    public void ModeSelect(int modeIndex)
    {
        gameScene = modeIndex + 1;

        for (int i = 0; i < modeButton.Length; i++)
        {
            if (i == modeIndex)
            {
                isMode[modeIndex] = !isMode[modeIndex];
                PhotonNetwork.CurrentRoom.SetMode(modeIndex);
            }
            else
            {
                isMode[modeIndex] = false;
            }
        }

        for (int i = 0; i < modeButton.Length; i++)
        {
            if (i == modeIndex)
            {
                modeButton[i].animationTriggers.normalTrigger = "";
                modeButton[i].animator.CrossFade("Highlighted", 0.1f);
            }
            else
            {
                modeButton[i].animationTriggers.normalTrigger = "Normal";
                modeButton[i].animator.CrossFade("Disabled", 0.1f);
            }
        }
        print($"{PhotonNetwork.LocalPlayer.NickName}: {modeIndex.ToString()}\n {modeButton[modeIndex].animationTriggers.highlightedTrigger.ToString()}");
    }

    private void OnDisable()
    {
        // PlayerNumbering에 플레이어 빼기
        PlayerNumbering.OnPlayerNumberingChanged -= UpdatePlayer;

        for (int i = 0; i < modeButton.Length; i++)
        {
            modeButton[i].animator.writeDefaultValuesOnDisable = true;
        }
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

    public void PlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable properties)
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
