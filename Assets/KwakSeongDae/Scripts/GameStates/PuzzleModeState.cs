using Photon.Pun;
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

    private Dictionary<int, Coroutine> finishRoutineDic;
    private Dictionary<int, bool> isBlockCheckDic;

    private Coroutine mainCollisionRoutine;

    public override void Enter()
    {
        SceneLoad(SceneIndex.Game);
        base.Enter();
        // Dictionary 초기 세팅
        finishRoutineDic = new Dictionary<int, Coroutine>();
        isBlockCheckDic = new Dictionary<int, bool>();
        // 플레이어 수만큼 미리 요소 추가
        foreach (var playerID in playerObjectDic.Keys)
        {
            finishRoutineDic.Add(playerID, null);
            isBlockCheckDic.Add(playerID, false);
        }

        // 충돌 감지 루틴 실행
        mainCollisionRoutine = StartCoroutine(CollisionCheckRoutine());
    }

    public override void Exit()
    {
        StopCoroutine(mainCollisionRoutine);
        // finishRoutine이 실행되고 있는 경우에는 해당 코루틴은 중지
        foreach (int i in finishRoutineDic.Keys)
        {
            if (finishRoutineDic[i] != null)
            {
                StopFinishRoutine(finishRoutineDic[i]);
            }
        }
        finishRoutineDic.Clear();
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
            var isBlockCheckKeys = isBlockCheckDic.Keys.ToArray();
            foreach (var playerID in isBlockCheckKeys)
            {
                isBlockCheckDic[playerID] = false;
            }

            // 2. Physics2D로 충돌체 검사
            // isEntered가 된 블럭만 감지해서 현재 FInish 지점 상태 업데이트
            Collider2D[] cols = Physics2D.OverlapBoxAll(detectorPos, detectorScale, 0, LayerMask.GetMask("Blocks"));
            print("충돌 감지 중");
            foreach (var collision in cols)
            {
                // 블럭이 존재하는 경우, 해당 소유자의 블럭이 있음을 체크
                // 충돌된 블럭이 있을때, 플레이어의 코루틴의 유무 판단 후, 코루틴 실행
                if (collision.GetComponent<Blocks>().IsEntered == false
                    && collision.TryGetComponent<PhotonView>(out var block))
                {
                    // 테스트 용
                    //int playerID = block.Owner.ActorNumber;
                    int playerID = collision.GetComponent<TestBlocks>().PlayerID;

                    if (isBlockCheckDic.ContainsKey(playerID))
                        isBlockCheckDic [playerID] = true;
                }
            }

            // 3. 현재 충돌된 블럭이 있는 플레이어들만 FInishRoutine 수행
            // 충돌된 블럭이 없는 플레이어들은 기존 수행되던 루틴을 해제
            var finishRoutineKeys = finishRoutineDic.Keys.ToArray();
            foreach (var playerID in finishRoutineKeys)
            {
                // 블럭체크에 해당 플레이어가 있으면서 true인 경우 => 현재 FInish지점이 블럭이 있음
                if (isBlockCheckDic.ContainsKey(playerID))
                {
                    if (isBlockCheckDic[playerID] == true)
                    {
                        print($"{playerID} 블럭 감지");
                        if (finishRoutineDic[playerID] == null)
                            finishRoutineDic[playerID] = StartCoroutine(FinishRoutine(playerID));
                    }
                    else
                    {
                        print($"{playerID} 블럭 없음");
                        if (finishRoutineDic[playerID] != null)
                        {
                            StopFinishRoutine(finishRoutineDic[playerID]);
                            finishRoutineDic[playerID] = null;
                        }
                    }
                }
                else
                {
                    print($"{playerID}의 비정상적인 접근");
                }
            }
            yield return delay;
        }
    }

    protected override IEnumerator FinishRoutine(int playerID)
    {
        yield return StartCoroutine(base.FinishRoutine(playerID));
        // 제한 시간이 지나면
        // 해당 플레이어는 더 이상 조작 불가
        PlayerStateChange(playerID);

        // 모든 플레이어 상태 체크 후, 집계 시작
        AllPlayerResult();
    }

    private void PlayerStateChange(int playerID)
    {
        if (playerObjectDic.ContainsKey(playerID)
            && playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
        {
            controller.IsGoal = true;
        }

        // 기존에 finishRoutineDic의 목록에서 해당 플레이어 삭제
        finishRoutineDic.Remove(playerID);
        isBlockCheckDic.Remove(playerID);

        print($"{playerID}는 이제 조작할 수 없습니다.");
    }

    private void AllPlayerResult()
    {
        if (finishRoutineDic.Count < 1)
        {
            //TODO: 각 플레이어가 쌓은 블럭의 개수를 집계하는 코드 필요
            List<Tuple<int,int>> result = new List<Tuple<int,int>>();
            foreach (var playerID in playerObjectDic.Keys)
            {
                //if (playerObjectDic[playerID].TryGetComponent<BlockCountManager>(out var manager))
                //{
                //    result.Add(new Tuple<int, int>(playerID, manager.BlockCount));
                //}

                //테스트 코드
                result.Add(new Tuple<int, int>(playerID, playerID));
            }
            //내림차순으로 블럭 개수 정렬
            result.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            result.ForEach(x => print($"{x.Item1}의 블럭개수: {x.Item2}"));

            print($"모든 플레이어의 블럭 개수 집계 및 게임 종료");
            print($"{result[0].Item1}이 퍼즐 모드의 우승자입니다!!!");

            manager.CurrentState = StateType.Stop;
        }
    }
}
