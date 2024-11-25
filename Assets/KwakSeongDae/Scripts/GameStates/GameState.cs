using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SceneIndex
{
    Room, Game
}

public class GameState : MonoBehaviour
{
    [Header("기본 설정")]
    public StateType StateType;

    [Header("게임 시작 & 종료 설정")]
    [SerializeField] protected float startDelayTime;
    [SerializeField] protected float finishDelayTime;
    [SerializeField] private GameTimer timerUI;

    [Header("플레이어 스폰 설정")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector2 bottomLeft;            // 스폰 가능 지역의 좌하단 좌표
    [SerializeField] private Vector2 upRight;               // 스폰 가능 지역의 우상단 좌표

    protected CoreManager manager;
    protected Dictionary<int, GameObject> playerObjectDic;
    private WaitForSecondsRealtime startDelay;
    private WaitForSeconds finishDelay;

    private void OnEnable()
    {
        // 시작 딜레이는 게임이 멈춰야되는 기능도 포함하므로 Realtime으로 계산
        startDelay = new WaitForSecondsRealtime(startDelayTime);
        finishDelay = new WaitForSeconds(finishDelayTime);
    }

    public virtual void Enter()
    {
        print($"{StateType}에 진입");
        manager = CoreManager.Instance;
        playerObjectDic = new Dictionary<int, GameObject>();
        if (playerPrefab != null)
        {
            var playerKeys = manager.PlayerDic.Keys.ToArray();
            print($"플레이어 수: {playerKeys.Length}");
            var playerSpawnPos = PlayerSpawnStartPositions(bottomLeft, upRight, playerKeys.Length);
            for (int i = 0; i < playerKeys.Length; i++)
            {
                playerObjectDic.Add(playerKeys[i], Instantiate(playerPrefab, playerSpawnPos[i], Quaternion.identity, null));
            }
        }
        StartCoroutine(StartRoutine());
    }
    public virtual void OnUpdate()
    {
        //print($"{StateType}에서 업데이트 중");
    }
    public virtual void Exit()
    {
        print($"{StateType}에서 탈출");

        // 기본적으로 해당 게임 모드가 끝나면 방이 해체된다고 가정하고, 초기화 진행
        var playerObjectKeys = playerObjectDic.Keys.ToArray();
        // 플레이어 오브젝트들 삭제
        foreach (var playerID in playerObjectKeys)
        {
            if (playerObjectDic[playerID] != null)
                Destroy(playerObjectDic[playerID]);
        }
        playerObjectDic.Clear();
        manager?.ResetPlayer();

        SceneLoad(SceneIndex.Room);
    }

    /// <summary>
    /// 모드 시작 시, 작동할 타이머 루틴
    /// </summary>
    private IEnumerator StartRoutine()
    {
        timerUI.Timer = startDelayTime;
        timerUI.transform.gameObject.SetActive(true);
        Time.timeScale = 0f;
        yield return startDelay;
        timerUI.transform.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// 각 게임 모드 별, FinishRoutine 가상 함수
    /// </summary>
    protected virtual IEnumerator FinishRoutine(int playerID)
    {
        timerUI.Timer = finishDelayTime;
        timerUI.transform.gameObject.SetActive(true);
        yield return finishDelay;
        timerUI.transform.gameObject.SetActive(false);
    }

    /// <summary>
    /// FinishRoutine종료시, StopCoroutine전에 한번 거쳐가는 미들함수
    /// </summary>
    protected void StopFinishRoutine(Coroutine routine)
    {
        // 종료 시, 공통적인 기능들 일괄 처리
        timerUI.transform.gameObject.SetActive(false);
        StopCoroutine(routine);
    }

    private Vector2[] PlayerSpawnStartPositions(Vector2 bottomLeft, Vector2 upRight, int playerNum)
    {
        if (playerNum < 1 || playerNum > 4) return null;

        // 개인 플레이어 너비 = 전체 너비 / 플레이어 수
        var width = MathF.Abs(upRight.x - bottomLeft.x) / playerNum;

        // 플레이어 스폰 위치 (x값) =
        // (bottomLeft + 개인 너비 * 플레이어 인덱스 = 각 플레이어 영역의 bottomLeft)
        // + (개인너비 / 2 = 각 플레이어 영역의 중심) 
        // 플레이어 스폰 위치 (y값) = bottomLeft.y
        var playerPositions = new Vector2[playerNum];
        for (int i = 0; i < playerPositions.Length; i++)
        {
            playerPositions[i] = new Vector2((bottomLeft.x + width * i) + (width / 2), bottomLeft.y);
        }
        return playerPositions;
    }

    protected void SceneLoad(SceneIndex sceneIndex)
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        // 씬 전환 테스트 할때는 주석처리
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonNetwork.LoadLevel((int)sceneIndex);
    }
}
