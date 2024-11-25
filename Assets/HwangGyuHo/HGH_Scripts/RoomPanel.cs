using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class RoomPanel : MonoBehaviour
{
    [SerializeField] PlayerEntry[] playerEntries;
    [SerializeField] Button startButton;


    private void OnEnable()
    {
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
        // TODO : 플레이어 속성이 바뀌면 그것을 업데이트
        Debug.Log($"{targetPlayer.NickName} 정보변경!!");
    }

    public void StartGame()
    {
        // TODO : 플레이어들 READY가 모두 되면 게임시작 버튼으로 게임시작
    }

    public void LeaveRoom()
    {
        Debug.Log("방을 떠났습니다");
        PhotonNetwork.LeaveRoom();
    }

    public void AllPlayerReadyCheck()
    {
        // TODO : 모든 플레이어의 레디 체크
    }
}
