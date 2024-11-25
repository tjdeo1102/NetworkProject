using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : MonoBehaviourPunCallbacks
{
    [SerializeField] Button gameoverButton;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            gameoverButton.interactable = true;
        }
        else
        {
            gameoverButton.interactable = false;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel("HGH_Scene");
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("HGH_Scene");
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            gameoverButton.interactable = true;
        }
        else
        {
            gameoverButton.interactable = false;
        }
    }
    public void GameOver()
    {
        // 자신이 방장이 아니라면 실행안한다
        if (PhotonNetwork.IsMasterClient == false)
            return;

        // 게임이 끝나면 방 안으로 들어올 수 있게 한다
        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.LoadLevel("HGH_Scene");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
