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
using UnityEngine.Events;
using WebSocketSharp;
using static UnityEditor.Progress;

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
    [HideInInspector] public Dictionary<int, GameObject> towerObjectDic;
    [HideInInspector] public float playerWidth;
    protected PlayerGameCanvasUI playerUI;
    private WaitForSecondsRealtime startDelay;
    private WaitForSeconds finishDelay;

    // 활성화 시점에 모두 초기화
    private void OnEnable()
    {
        print("진입");
        StartCoroutine(NetworkWaitRoutine());
    }
    protected virtual void Init()
    {
        // 시작 딜레이는 게임이 멈춰야되는 기능도 포함하므로 Realtime으로 계산
        startDelay = new WaitForSecondsRealtime(startDelayTime);
        finishDelay = new WaitForSeconds(finishDelayTime);
        // 플레이어 오브젝트 딕셔너리는 모든 클라이언트가 가질수 있도록 설정
        playerObjectDic = new Dictionary<int, GameObject>();
        towerObjectDic = new Dictionary<int, GameObject>();

        if (playerPrefabPath.IsNullOrEmpty() == false
            && uiPrefab != null)
        {
            var players = PhotonNetwork.PlayerList;
            var playerSpawnPos = PlayerSpawnStartPositions(bottomLeft, upRight, players.Length);
            print($"플레이어 수: {players.Length}");

            var playerNum = PhotonNetwork.LocalPlayer.GetPlayerNumber();
            // 타워 생성
            var towerObj = PhotonNetwork.Instantiate(towerPrefabPath, playerSpawnPos[playerNum], Quaternion.identity, data: new object[] { playerNum });
            // 네트워크 플레이어 오브젝트를 생성하기
            var playerObj = PhotonNetwork.Instantiate(playerPrefabPath, playerSpawnPos[playerNum], Quaternion.identity, data: new object[] { players[playerNum].NickName });

            var playerView = playerObj.GetComponent<PhotonView>();
            var towerView = towerObj.GetComponent<PhotonView>();
            photonView.RPC("SetPlayerObjectDic", RpcTarget.All, playerView.ViewID);
            photonView.RPC("SetTowerObjectDic", RpcTarget.All, towerView.ViewID);
            // 본인 오브젝트가 생성되는 경우에는 본인 UI도 같이 생성
            playerUI = Instantiate(uiPrefab);
            playerUI.GetComponent<PlayerGameCanvasUI>().gameState = gameObject;
        }

        // RPC이용해서 시작 시간 동기화, 방장이 RPC날리기
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("StartRoutineWrap", RpcTarget.All);
    }
    
    private IEnumerator NetworkWaitRoutine()
    {
        var delay = new WaitForSeconds(1f);
        yield return delay;
        Init();
    }

    [PunRPC]
    protected void SetPlayerObjectDic(int viewID)
    {
        var obj = PhotonView.Find(viewID);
        playerObjectDic.Add(obj.Owner.ActorNumber, obj.gameObject);
    }

    [PunRPC]
    protected void SetTowerObjectDic(int viewID)
    {
        var obj = PhotonView.Find(viewID);
        towerObjectDic.Add(obj.Owner.ActorNumber, obj.gameObject);
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
        // 개인 플레이어 영역은 0.25단위로 움직일 수 있도록 조정
        var rawWidth = MathF.Abs(upRight.x - bottomLeft.x) / playerNum;
        playerWidth = Mathf.Ceil(rawWidth / 0.5f) * 0.5f;
        // 조정된 width에 따라, 좌하단 좌표 수정, 
        var widthRemain = MathF.Abs(upRight.x - bottomLeft.x) - (playerWidth * playerNum);
        // 가운데 정렬
        bottomLeft = new Vector2(bottomLeft.x + widthRemain / 2, bottomLeft.y);

        // 투명 벽 수 = 플레이어 수 + 1
        // 투명 벽 위치 (x값) = bottomLeft.x + 투명 벽 인덱스 * width
        // 투명 벽 위치 (y값) = bottomLeft.y
        for (int i = 0; i < playerNum+1; i++)
        {
            PhotonNetwork.Instantiate(wallPrefabPath, new Vector2(bottomLeft.x + (i * playerWidth), bottomLeft.y), Quaternion.identity);
        }

        // 플레이어 스폰 위치 (x값) =
        // (bottomLeft + 개인 너비 * 플레이어 인덱스 = 각 플레이어 영역의 bottomLeft)
        // + (개인너비 / 2 = 각 플레이어 영역의 중심) 
        // 플레이어 스폰 위치 (y값) = bottomLeft.y
        var playerPositions = new Vector2[playerNum];
        for (int i = 0; i < playerPositions.Length; i++)
        {
            playerPositions[i] = new Vector2((bottomLeft.x + playerWidth * i) + (playerWidth / 2), bottomLeft.y);
        }
        return playerPositions;
    }
}
