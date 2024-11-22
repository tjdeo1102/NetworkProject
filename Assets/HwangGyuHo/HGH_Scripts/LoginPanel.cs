using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoginPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField idInputField;

    private void Start()
    {
        idInputField.text = $"Player{Random.Range(1000,10000)}";
    }

    public void Login()
    {
        Debug.Log("로그인 요청");
        if (idInputField.text == "")
        {
            Debug.LogWarning("아이디를 입력하셔야 접속이 가능합니다");
        }
        
        PhotonNetwork.LocalPlayer.NickName = idInputField.text;
        PhotonNetwork.ConnectUsingSettings();
    }
}
