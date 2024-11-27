using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using WebSocketSharp;
using static UnityEditor.Progress;

public enum SceneIndex
{
    Room, Game
}

[RequireComponent(typeof(PhotonView))]
public class GameState : MonoBehaviourPun
{
    private const int maxPlayer = 4;

    [Header("게임 시작 & 종료 설정")]
    [SerializeField] protected float startDelayTime;
    [SerializeField] protected float finishDelayTime;
    [SerializeField] private PlayerGameCanvasUI uiPrefab;

    [Header("플레이어 스폰 설정")]
    [SerializeField] private string playerPrefabPath;
    [SerializeField] private string towerPrefabPath;
    [SerializeField] private string wallPrefabPath;
    [SerializeField] private Vector2 bottomLeft;            // 스폰 가능 지역의 좌하단 좌표
    [SerializeField] private Vector2 upRight;               // 스폰 가능 지역의 우상단 좌표

    [HideInInspector] public Dictionary<int, GameObject> playerObjectDic;
    protected PlayerGameCanvasUI playerUI;
    private WaitForSecondsRealtime startDelay;
    private WaitForSeconds finishDelay;

    // 활성화 시점에 모두 초기화
    protected virtual void OnEnable()
    {
        //print($"퍼즐 모드에 진입");

        // 시작 딜레이는 게임이 멈춰야되는 기능도 포함하므로 Realtime으로 계산
        startDelay = new WaitForSecondsRealtime(startDelayTime);
        finishDelay = new WaitForSeconds(finishDelayTime);
        // 플레이어 오브젝트 딕셔너리는 모든 클라이언트가 가질수 있도록 설정
        playerObjectDic = new Dictionary<int, GameObject>();

        // 방장이 모든 플레이어 오브젝트 생성 작업 진행
        if (PhotonNetwork.IsMasterClient
            && playerPrefabPath.IsNullOrEmpty() == false
            && uiPrefab != null)
        {
            var players = PhotonNetwork.PlayerList;
            var playerSpawnPos = PlayerSpawnStartPositions(bottomLeft, upRight, players.Length);
            print($"플레이어 수: {players.Length}");

            // 플레이어 오브젝트 담을 배열
            var playerObjViewIDs = new int[players.Length];

            for (int i = 0; i < players.Length; i++)
            {
                // 타워 생성
                var towerObj = PhotonNetwork.Instantiate(towerPrefabPath, playerSpawnPos[i], Quaternion.identity, data: new object[] { players[i].GetPlayerNumber() });
                // 네트워크 플레이어 오브젝트를 생성하기
                var playerObj = PhotonNetwork.Instantiate(playerPrefabPath, playerSpawnPos[i], Quaternion.identity, data: new object[] { players[i].NickName });
                // 각 플레이어 오브젝트의 소유권을 해당되는 클라이언트로 변경하기

                var playerView = playerObj.GetComponent<PhotonView>();
                playerView.TransferOwnership(players[i]);
                playerObjViewIDs[i] = playerView.ViewID;
            }
            photonView.RPC("SetPlayerObjectDic", RpcTarget.All, playerObjViewIDs,players);
        }

        // 본인 오브젝트가 생성되는 경우에는 본인 UI도 같이 생성
        playerUI = Instantiate(uiPrefab);

        // RPC이용해서 시작 시간 동기화, 방장이 RPC날리기
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("StartRoutineWrap", RpcTarget.All);
    }

    public virtual void Exit()
    {
        print($"퍼즐 모드 종료");

        // 모든 딕셔너리 초기화 과정 불필요
        Time.timeScale = 1f;

        SceneLoad(SceneIndex.Room);
    }

    [PunRPC]
    protected void SetPlayerObjectDic(int[] viewIDs, Player[] players)
    {
        for(int i = 0; i< viewIDs.Length; i++)
        {
            var obj = PhotonView.Find(viewIDs[i]);
            playerObjectDic.Add(players[i].ActorNumber, obj.gameObject);

        }
    }

    [PunRPC]
    protected void StartRoutineWrap()
    {
        StartCoroutine(StartRoutine(PhotonNetwork.Time));
    }

    /// <summary>
    /// 모드 시작 시, 작동할 타이머 루틴
    /// </summary>
    protected IEnumerator StartRoutine(double startTime)
    {
        var delay = PhotonNetwork.Time - startTime;
        print($"방장이 보낸 RPC를 수신까지 딜레이 {delay}");
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
        playerUI?.SetTimer(0);
        StopCoroutine(routine);
    }

    /// <summary>
    /// 플레이어가 스폰할 위치 반환 및 블럭 제한구역 설정
    /// </summary>
    /// <param name="bottomLeft"> 맵 좌하단 위치</param>
    /// <param name="upRight"> 맵 우상단 위치</param>
    /// <param name="playerNum"> 총 플레이어 수</param>
    /// <returns></returns>
    private Vector2[] PlayerSpawnStartPositions(Vector2 bottomLeft, Vector2 upRight, int playerNum)
    {
        if (playerNum < 1 || playerNum >= maxPlayer) return null;

        // 개인 플레이어 너비 = 전체 너비 / 플레이어 수
        var width = MathF.Abs(upRight.x - bottomLeft.x) / playerNum;

        // 투명 벽 수 = 플레이어 수 + 1
        // 투명 벽 위치 (x값) = bottomLeft.x + 투명 벽 인덱스 * width
        // 투명 벽 위치 (y값) = bottomLeft.y
        for (int i = 0; i < playerNum+1; i++)
        {
            PhotonNetwork.Instantiate(wallPrefabPath, new Vector2(bottomLeft.x + (i * width), bottomLeft.y), Quaternion.identity);
        }

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
