using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerGameCanvasUI : MonoBehaviour
{
    [Header("기본 UI 설정")]
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private GameObject button;
    [SerializeField] private GameTimer gameTimer;

    [Header("스코어 패널 설정")]
    [SerializeField] private GameObject scoreView;
    [SerializeField] private GameObject resultEntryPrefab;


    [HideInInspector]public GameObject gameState;

    public void AddResultEntry(int playerID, int score)
    {
        //네트워크 오브젝트로 사용할 오브젝트가 아님
        var obj = Instantiate(resultEntryPrefab,scoreView.transform);
        if (obj.TryGetComponent<ResultScoreEntry>(out var entry))
        {
            entry.SetEntry(playerID.ToString(), score);
        }
    }
    public void AddResultEntry(int playerID, float score)
    {
        //네트워크 오브젝트로 사용할 오브젝트가 아님
        var obj = Instantiate(resultEntryPrefab, scoreView.transform);
        if (obj.TryGetComponent<ResultScoreEntry>(out var entry))
        {
            entry.SetEntry(playerID.ToString(), score);
        }
    }

    public void SetTimer(float time)
    {
        if(gameTimer == null) return;

        if (time > 0)
        {
            gameTimer.gameObject.SetActive(true);
            gameTimer.Timer = time;
        }
        else
        {
            gameTimer.Timer = 0f;
        }
    }

    public void SetResult()
    {
        // 방장만 떠나는 버튼 활성화
        if (PhotonNetwork.IsMasterClient)
            button.SetActive(true);

        scorePanel.SetActive(true); 
    }

    public void ReturnScene()
    {
        gameState?.SetActive(false);
    }
}
