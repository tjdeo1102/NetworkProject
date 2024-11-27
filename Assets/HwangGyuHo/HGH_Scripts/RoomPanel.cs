using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class RoomPanel : MonoBehaviour
{
    [SerializeField] PlayerEntry[] playerEntries;
    [SerializeField] Button startButton;
    [SerializeField] private int gameScene;


    private void OnEnable()
    {
        UpdatePlayer();

        // PlayerNumbering 에 플레이어 추가
        PlayerNumbering.OnPlayerNumberingChanged += UpdatePlayer;

        PhotonNetwork.LocalPlayer.SetReady(false);
    }

    private void OnDisable()
    {
        // PlayerNumbering에 플레이어 빼기
        PlayerNumbering.OnPlayerNumberingChanged -= UpdatePlayer;
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

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startButton.interactable = AllPlayerReadyCheck();
        }
        else
        {
            startButton.interactable = false;
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
