using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SceneIndex
{
    Room, Game
}

/* 딕셔너리 관리: 방장만
 * 충돌 관리 및 그 외 로직: 방장만 
 * UI 관리: 각 플레이어
 */
public class GameState : MonoBehaviourPunCallbacks
{
    [Header("기본 설정")]
    public StateType StateType;

    [Header("게임 시작 & 종료 설정")]
    [SerializeField] protected float startDelayTime;
    [SerializeField] protected float finishDelayTime;
    [SerializeField] private PlayerGameCanvasUI uiPrefab;

    [Header("플레이어 스폰 설정")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector2 bottomLeft;            // 스폰 가능 지역의 좌하단 좌표
    [SerializeField] private Vector2 upRight;               // 스폰 가능 지역의 우상단 좌표

    [HideInInspector] public Dictionary<int, GameObject> playerObjectDic;
    protected PlayerGameCanvasUI playerUI;
    private WaitForSecondsRealtime startDelay;
    private WaitForSeconds finishDelay;

    // 활성화 시점에 모두 초기화
    protected virtual void OnEnable()
    {
        print($"{StateType}에 진입");

        // 시작 딜레이는 게임이 멈춰야되는 기능도 포함하므로 Realtime으로 계산
        startDelay = new WaitForSecondsRealtime(startDelayTime);
        finishDelay = new WaitForSeconds(finishDelayTime);
        playerObjectDic = new Dictionary<int, GameObject>();

        if (playerPrefab != null
            && uiPrefab != null)
        {
            var players = PhotonNetwork.PlayerList;
            var playerSpawnPos = PlayerSpawnStartPositions(bottomLeft, upRight, players.Length);
            print($"플레이어 수: {players.Length}");

            for (int i = 0; i < players.Length; i++)
            {
                var playerObj = Instantiate(playerPrefab, playerSpawnPos[i], Quaternion.identity, null);
                // 본인 오브젝트가 생성되는 경우에는 본인 UI도 같이 생성
                if (players[i] == PhotonNetwork.LocalPlayer)
                    playerUI = Instantiate(uiPrefab, playerObj.transform);

                playerObjectDic.Add(players[i].ActorNumber, playerObj);
            }
        }

        // RPC이용해서 시작 시간 동기화, 방장이 RPC날리기
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("StartRoutineMiddleware", RpcTarget.AllViaServer);
    }

    public virtual void Exit()
    {
        print($"{StateType}에서 탈출");

        // 모든 딕셔너리 초기화 과정 불필요
        Time.timeScale = 1f;

        SceneLoad(SceneIndex.Room);
    }

    [PunRPC]
    private void StartRoutineMiddleware()
    {
        // 각자 자신의 State에서만 처리
        if(photonView.IsMine)
            StartCoroutine(StartRoutine(PhotonNetwork.Time));
    }

    /// <summary>
    /// 모드 시작 시, 작동할 타이머 루틴
    /// </summary>
    private IEnumerator StartRoutine(double startTime)
    {
        var delay = PhotonNetwork.Time - startTime;
        // 지연보상 적용
        playerUI?.SetTimer(startDelayTime - (float)delay);
        Time.timeScale = 0f;
        yield return startDelay;
        playerUI?.SetTimer(0);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// 각 게임 모드 별, FinishRoutine 가상 함수
    /// </summary>
    protected virtual IEnumerator FinishRoutine(int playerID)
    {
        playerUI?.SetTimer(finishDelayTime);
        yield return finishDelay;
        playerUI?.SetTimer(0);
    }

    /// <summary>
    /// FinishRoutine종료시, StopCoroutine전에 한번 거쳐가는 미들함수
    /// </summary>
    protected void StopFinishRoutine(Coroutine routine)
    {
        // 종료 시, 공통적인 기능들 일괄 처리
        playerUI?.SetTimer(0);

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
