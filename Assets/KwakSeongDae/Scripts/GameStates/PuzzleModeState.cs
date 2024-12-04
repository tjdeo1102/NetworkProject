using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PuzzleModeState : GameState
{
    [Header("퍼즐 모드 설정")]
    [SerializeField] private BoxCollider2D boxDetector;
    [SerializeField] float towerHeightStep;
    [SerializeField] float towerSpeed;

    private Coroutine finishRoutine;
    private Dictionary<int, bool> isBlockCheckDic;
    private Coroutine mainCollisionRoutine;

    private Action<int> hpAction;
    private GameObject selfPlayer;
    private Coroutine panaltyRoutine;
    private Vector2 targetTowerPos;
    private bool isMoveTower;

    private int playerID;

    public override void OnEnable()
    {
        base.OnEnable();

        // 배경음악 재생 (Puzzle)
        SoundManager.Instance.Play(Enums.ESoundType.BGM, SoundManager.BGM_PUZZLE);
    }

    protected override void Init()
    {
        base.Init();

        // Dictionary 초기 세팅
        isBlockCheckDic = new Dictionary<int, bool>();
        
        // 플레이어 수만큼 미리 요소 추가
        foreach (var player in PhotonNetwork.PlayerList)
        {
            isBlockCheckDic.Add(player.ActorNumber, false);
        }

        playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        selfPlayer = playerObjectDic[playerID];
        //hpAction = (newHP) => PlayerHPHandle(newHP, playerID);
        selfPlayer.GetComponent<PlayerController>().OnChangeHp += PlayerHPHandle;


        // 방장만 충돌 감지 루틴 실행
        if (PhotonNetwork.IsMasterClient)
            mainCollisionRoutine = StartCoroutine(CollisionCheckRoutine());
    }

    private void Finish()
    {
        // 기존 작업 마무리
        if (PhotonNetwork.IsMasterClient
            && mainCollisionRoutine != null)
            StopCoroutine(mainCollisionRoutine);

        // finishRoutine이 실행되고 있는 경우에는 해당 코루틴은 중지
        if (finishRoutine != null)
            StopCoroutine(finishRoutine);

        isBlockCheckDic?.Clear();

        selfPlayer.GetComponent<PlayerController>().OnChangeHp -= PlayerHPHandle;
    }

    private void PlayerHPHandle(int newHP)
    {
        print("체력 변화");
        //// 자신 이벤트인 경우에만 호출
        //if (PhotonNetwork.LocalPlayer.ActorNumber != playerID) return;

        // 블럭이 동시에 떨어질 때, 중복 호출 방지
        if (panaltyRoutine == null)
            panaltyRoutine = StartCoroutine(PanaltyRoutine());
    }

    private IEnumerator PanaltyRoutine()
    {
        // 내 타워의 높이를 towerHegithStep만큼 상승
        // 해당 타워는 photonView transform이므로 자동으로 위치 동기화
        targetTowerPos = (Vector2)towerObjectDic[playerID].transform.position + Vector2.up * towerHeightStep;
        towerObjectDic[playerID].GetComponent<Rigidbody2D>().velocity = Vector2.up * towerSpeed;
        // 플레이어에 패널티 처리 알림
        playerObjectDic[playerID].GetComponent<PlayerController>().ProcessPanalty(true);
        isMoveTower = true;
        // 블럭이 동시에 떨어지는 경우, 중복 실행 방지
        yield return new WaitForSeconds(1f);
        panaltyRoutine = null;
    }

    private void FixedUpdate()
    {
        if (towerObjectDic != null && towerObjectDic.ContainsKey(playerID))
        {
            if (isMoveTower)
            {
                var curPos = (Vector2)towerObjectDic[playerID].transform.position;
                var dif = targetTowerPos - curPos;
                // 거리 차이가 미미해지거나, 현재 y값이 목표치의 y값을 넘어간 경우에는 위치 조정 및 속도 0
                if (Vector2.Distance(targetTowerPos, curPos) < 0.01f
                    || dif.y < -0.01f)
                {
                    towerObjectDic[playerID].GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    towerObjectDic[playerID].transform.position = targetTowerPos;
                    isMoveTower = false;
                    // 플레이어에 패널티 처리 종료 알림
                    playerObjectDic[playerID].GetComponent<PlayerController>().ProcessPanalty(false);
                }
            }
            else
            {
                // 타겟 타워 위치 초기화
                targetTowerPos = towerObjectDic[playerID].transform.position;
            }
        }
    }

    private IEnumerator CollisionCheckRoutine()
    {
        var detectorPos = (Vector2)boxDetector.transform.position + boxDetector.offset;
        var detectorScale = Vector2.Scale(boxDetector.transform.localScale, boxDetector.size);
        var delay = new WaitForSeconds(0.1f);

        while (true)
        {
            // 1. 현재 블럭 충돌 Check를 False로 초기화
            var IDs = isBlockCheckDic.Keys.ToArray();
            foreach (var ID in IDs)
            {
                isBlockCheckDic[ID] = false;
            }

            // 2. Physics2D로 충돌체 검사
            // isEntered가 된 블럭만 감지해서 현재 FInish 지점 상태 업데이트
            Collider2D[] cols = Physics2D.OverlapBoxAll(detectorPos, detectorScale, 0, LayerMask.GetMask("Blocks"));
            print("방장 충돌 감지 중");
            foreach (var collision in cols)
            {
                var blockTrans = collision.transform.parent;
                var block = blockTrans.GetComponent<Blocks>();
                // 블럭이 존재하는 경우, 해당 소유자의 블럭이 있음을 체크
                // 충돌된 블럭이 있을때, 플레이어의 코루틴의 유무 판단 후, 코루틴 실행
                if (block.IsControllable == false
                    && blockTrans.TryGetComponent<PhotonView>(out var view))
                {
                    int ID = view.Owner.ActorNumber;

                    if (isBlockCheckDic.ContainsKey(ID))
                        isBlockCheckDic [ID] = true;
                }
            }

            // 타워가 충돌된 경우도 감지
            Collider2D[] towerCols = Physics2D.OverlapBoxAll(detectorPos, detectorScale, 0, LayerMask.GetMask("Ground"));
            foreach (var collision in towerCols)
            {
                var towerTrans = collision.transform.parent;
                var tower = towerTrans.GetComponent<Tower>();
                // 타워가 충돌된 경우는 그 즉시 종료
                if (towerTrans.TryGetComponent<PhotonView>(out var view))
                {
                    int ID = view.Owner.ActorNumber;

                    // 해당 PlayerStateChange은 개인적으로 동작
                    PlayerStateChange(ID);

                    // 이후, 모든 플레이어는 상태 체크 후, 집계까지 진행
                    AllPlayerStateCheck(ID);
                }
            }

            // 3. 현재 충돌된 블럭이 있는 플레이어에서 FInishRoutine을 RPC로 수행하도록 만들기
            // 충돌된 블럭이 없는 플레이어들은 기존 수행되던 루틴을 해제
            IDs = isBlockCheckDic.Keys.ToArray();
            foreach (var ID in IDs)
            {
                // 블럭체크에 해당 플레이어가 있으면서 true인 경우 => 현재 FInish지점이 블럭이 있음
                if (isBlockCheckDic[ID] == true)
                {
                    print($"{ID} 블럭 감지");
                    photonView.RPC("FinishRoutineWrap", RpcTarget.AllViaServer, ID, true);
                }
                else
                {
                    print($"{playerID} 블럭 없음");
                    photonView.RPC("FinishRoutineWrap", RpcTarget.AllViaServer, ID, false);
                }
            }
            yield return delay;
        }
    }

    [PunRPC]
    private void FinishRoutineWrap(int ID, bool isPlay)
    {
        // 해당 플레이어에서만 루틴 실행
        if (playerID != ID) return;

        if (isPlay)
        {
            // 기존에 실행중이면 무시
            if (finishRoutine == null)
                finishRoutine = StartCoroutine(FinishRoutine(ID));
        }
        else
        {
            if (finishRoutine != null)
                StopFinishRoutine(finishRoutine);

            finishRoutine = null;
        }
    }

    protected override IEnumerator FinishRoutine(int playerID)
    {
        yield return StartCoroutine(base.FinishRoutine(playerID));
        // 제한 시간이 지나면 해당 플레이어는 더 이상 조작 불가

        // 해당 PlayerStateChange은 개인적으로 동작
        PlayerStateChange(playerID);

        // 이후, 모든 플레이어는 상태 체크 후, 집계까지 진행
        photonView.RPC("AllPlayerStateCheck", RpcTarget.MasterClient,playerID);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        print($"{otherPlayer.ActorNumber}가 나감");

        // 방장이 강제 종료 예외 처리
        if (PhotonNetwork.IsMasterClient == false) return;

        if (playerObjectDic.ContainsKey(otherPlayer.ActorNumber)
            && playerObjectDic[otherPlayer.ActorNumber].TryGetComponent<PlayerController>(out var controller))
        {
            controller.ReachGoal();
            playerObjectDic.Remove(otherPlayer.ActorNumber);
        }

        if (towerObjectDic.ContainsKey(otherPlayer.ActorNumber))
            towerObjectDic.Remove(otherPlayer.ActorNumber);

        AllPlayerStateCheck(otherPlayer.ActorNumber);
    }

    private void PlayerStateChange(int playerID)
    {
        if (playerObjectDic.ContainsKey(playerID)
            && playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
        {
            controller.ReachGoal();
        }

        print($"{playerID}는 이제 조작할 수 없습니다.");
    }

    [PunRPC]
    private void AllPlayerStateCheck(int playerID = -1)
    {
        // 딕셔너리에서 초기화할 플레이어가 있는 경우, 방장은 초기화 진행
        if (playerID > -1)
        {
            // 기존에 Dic의 목록에서 해당 플레이어 삭제
            isBlockCheckDic.Remove(playerID);
        }

        // 퍼즐 모드: 모든 플레이어가 끝난 경우에는 집계 진행 
        if (isBlockCheckDic.Count < 1)
        {
            List<Tuple<int,int>> result = new List<Tuple<int,int>>();
            foreach (var playerKey in playerObjectDic.Keys)
            {
                if (playerObjectDic[playerKey].TryGetComponent<PlayerController>(out var controller))
                {
                    result.Add(new Tuple<int, int>(playerKey, controller.BlockCount));
                }
            }

            //내림차순으로 블럭 개수 정렬
            result.Sort((x, y) => y.Item2.CompareTo(x.Item2));

            //각 클라이언트에서, 각각의 UI에 해당 내용 반영되도록 설정
            var players = new int[result.Count];
            var blockCounts = new int[result.Count];
            for (int i = 0; i < result.Count; i++)
            {
                players[i] = result[i].Item1;
                blockCounts[i] = result[i].Item2;
            }

            // UI 업데이트 작업 및 게임 정지기능은 모든 클라이언트 진행
            photonView.RPC("UpdateUI", RpcTarget.All, players, blockCounts);
        }
    }

    [PunRPC]
    private void UpdateUI(int[] playerIDs,int[] blockCounts)
    {
        for (int i = 0; i < playerIDs.Length; i++)
        {
            playerUI?.AddResultEntry(playerIDs[i], blockCounts[i]);
        }
        playerUI?.SetResult();

        // 마무리 작업
        Finish();

        print($"모든 플레이어의 블럭 개수 집계 및 게임 종료");
        print($"{playerIDs[0]}이 퍼즐 모드의 우승자입니다!!!");

        //Time.timeScale = 0f;
    }
}
