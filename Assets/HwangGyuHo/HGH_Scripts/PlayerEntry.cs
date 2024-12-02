using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerEntry : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text readyText;
    [SerializeField] Toggle readyToggle;

    public void SetPlayer(Player player)
    {
        if (player.IsMasterClient)
        {
            // 방장이면 이렇게 표시되게
            nameText.text = $"zZ{player.NickName}Zz";
        }
        else
        {
            // 방에 들어갔을때
            // 닉네임은 nameText.text로
            nameText.text = player.NickName;
        }

        // 레디버튼은 활성화 시키고
        readyToggle.gameObject.SetActive(true);
        // 상호작용은 주체가 자기자신, LocalPlayer일때 가능
        readyToggle.interactable = player == PhotonNetwork.LocalPlayer;

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
        readyToggle.gameObject.SetActive(false);
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
