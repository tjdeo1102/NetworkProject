using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceModeState : GameState
{
    [Header("레이스 모드 설정")]
    [SerializeField] private BoxCollider2D boxDetector;
    
    private Coroutine goalRoutine;
    private Dictionary<int, bool> isBlockCheckDic;

    private Coroutine mainCollisionRoutine;

    protected override void OnEnable()
    {
        base.OnEnable();
        // Dictionary 초기 세팅
        isBlockCheckDic = new Dictionary<int, bool>();
        // 플레이어 수만큼 미리 요소 추가
        foreach (var playerID in playerObjectDic.Keys)
        {
            isBlockCheckDic.Add(playerID, false);
        }

        // 방장만 충돌 감지 루틴 실행
        if (PhotonNetwork.IsMasterClient)
            mainCollisionRoutine = StartCoroutine(CollisionCheckRoutine());
    }

    public override void Exit()
    {
        if (PhotonNetwork.IsMasterClient)
            StopCoroutine(mainCollisionRoutine);

        // goalRoutine이 실행되고 있는 경우에는 해당 코루틴은 중지
        StopCoroutine(goalRoutine);

        isBlockCheckDic.Clear();

        // Exit호출은 Enter의 역순
        base.Exit();
    }
    private IEnumerator CollisionCheckRoutine()
    {
        var detectorPos = (Vector2)boxDetector.transform.position + boxDetector.offset;
        var detectorScale = Vector2.Scale(boxDetector.transform.localScale, boxDetector.size);
        var delay = new WaitForSeconds(0.1f);
        // 코루틴 실행 즉시 실행 하지 말기 => 로그가 더러워짐
        yield return null;

        while (true)
        {
            // 1. 현재 블럭 충돌 Check를 False로 초기화
            var playerIDs = isBlockCheckDic.Keys.ToArray();
            foreach (var playerID in playerIDs)
            {
                isBlockCheckDic[playerID] = false;
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

                // TODO: 블럭이 닿은경우를 체크하는 것이 아닌, 컨트롤 여부를 체크해야함
                if (block.IsEntered == false
                    //&& block.IsControllable
                    && blockTrans.TryGetComponent<PhotonView>(out var view))
                {
                    int playerID = view.Owner.ActorNumber;

                    if (isBlockCheckDic.ContainsKey(playerID))
                        isBlockCheckDic[playerID] = true;
                }
            }

            // 3. 현재 충돌된 블럭이 있는 플레이어들만 FInishRoutine 수행
            // 충돌된 블럭이 없는 플레이어들은 기존 수행되던 루틴을 해제
            playerIDs = isBlockCheckDic.Keys.ToArray();
            foreach (var playerID in playerIDs)
            {
                // 블럭체크에 해당 플레이어가 있으면서 true인 경우 => 현재 FInish지점이 블럭이 있음
                if (isBlockCheckDic[playerID] == true)
                {
                    print($"{playerID} 블럭 감지");
                    photonView.RPC("FinishRoutineWrap", RpcTarget.AllViaServer, playerID, true);
                }
                else
                {
                    print($"{playerID} 블럭 없음");
                    photonView.RPC("FinishRoutineWrap", RpcTarget.AllViaServer, playerID, false);
                }
            }
            yield return delay;
        }
    }

    protected override IEnumerator FinishRoutine(int playerID)
    {
        yield return StartCoroutine(base.FinishRoutine(playerID));
        // 제한 시간이 지나면
        // 모든 플레이어 작동 멈추고 집계
        AllPlayerStateChange();
        AllPlayerResult();
    }

    private void AllPlayerStateChange()
    {
        foreach (var playerID in playerObjectDic.Keys)
        {
            if (playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controlller))
            {
                controlller.IsGoal = true;
            }
            // 기존에 finishRoutineDic의 목록에서 해당 플레이어 삭제
            goalRoutineDic.Remove(playerID);
            isBlockCheckDic.Remove(playerID);
            print($"{playerID}는 이제 조작할 수 없습니다.");
        }
        print("모든 플레이어의 행동이 중지되었습니다.");
    }

    private void AllPlayerResult()
    {
        if (goalRoutineDic.Count < 1)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();
            foreach (var playerID in playerObjectDic.Keys)
            {
                //TODO: 각 플레이어의 가장 높은 블럭을 집계하는 코드 필요

                //테스트 코드
                result.Add(new Tuple<int, int>(playerID, playerID));
            }
            //내림차순으로 블럭 개수 정렬
            result.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            //result.ForEach((x) => {
            //    playerUI?.SetResultEntry(x.Item1.ToString(), x.Item2);
            //    playerUI?.SetResult();
            //});

            print($"모든 플레이어의 블럭 개수 집계 및 게임 종료");
            print($"{result[0].Item1}이 퍼즐 모드의 우승자입니다!!!");

            Time.timeScale = 0f;
        }
    }
}
