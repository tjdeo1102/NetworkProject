using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntry : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text readyText;
    [SerializeField] Button readyButton;

    public void SetPlayer(Player player)
    {
        // 방에 들어갔을때
        // 닉네임은 nameText.text로
        nameText.text= player.NickName;
        // 레디버튼은 활성화 시키고
        readyButton.gameObject.SetActive(true);
        // 상호작용은 주체가 자기자신, LocalPlayer일때 가능
        readyButton.interactable = player == PhotonNetwork.LocalPlayer;

        if (player.GetReady())
        {
            readyText.text = "Ready";
        }
        else
        {
            readyText.text = "";
        }
    }

    public void SetEmpty()
    {
        readyText.text = "";
        nameText.text = "None";
        readyButton.gameObject.SetActive(false);
    }

    public void Ready()
    {
        bool ready = PhotonNetwork.LocalPlayer.GetReady();
        if (ready)
        {
            PhotonNetwork.LocalPlayer.SetReady(false);
        }
        else
        {
            PhotonNetwork.LocalPlayer.SetReady(true);
        }
    }
}
