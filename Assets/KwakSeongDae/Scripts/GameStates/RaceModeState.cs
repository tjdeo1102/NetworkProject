using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceModeState : GameState
{
    [Header("레이스 모드 설정")]
    [SerializeField] private float goalTimer;
    [SerializeField] private BoxCollider2D boxDetector;
    private Dictionary<int, Coroutine> goalRoutineDic;
    private Dictionary<int, bool> isBlockCheckDic;
    private WaitForSeconds timer;

    private Coroutine mainCollisionRoutine;

    public override void Enter()
    {
        base.Enter();
        // Dictionary 초기 세팅
        goalRoutineDic = new Dictionary<int, Coroutine>();
        isBlockCheckDic = new Dictionary<int, bool>();
        // 플레이어 수만큼 미리 요소 추가
        foreach (var playerID in playerObjectDic.Keys)
        {
            goalRoutineDic.Add(playerID, null);
            isBlockCheckDic.Add(playerID, false);
        }

        // Timer 초기 세팅
        timer = new WaitForSeconds(goalTimer);

        // 충돌 감지 루틴 실행
        mainCollisionRoutine = StartCoroutine(CollisionCheckRoutine());
    }

    public override void Exit()
    {
        StopCoroutine(mainCollisionRoutine);
        // finishRoutine이 실행되고 있는 경우에는 해당 코루틴은 중지
        foreach (int i in goalRoutineDic.Keys)
        {
            if (goalRoutineDic[i] != null)
            {
                StopCoroutine(goalRoutineDic[i]);
            }
        }
        goalRoutineDic.Clear();
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
            Collider2D[] cols = Physics2D.OverlapBoxAll(detectorPos, detectorScale, 0);
            print("충돌 감지 중");
            foreach (var collision in cols)
            {
                // TODO: 블럭에 해당하는 태그로 바꿔주기

                // 블럭이 존재하는 경우, 해당 소유자의 블럭이 있음을 체크
                // 충돌된 블럭이 있을때, 플레이어의 코루틴의 유무 판단 후, 코루틴 실행
                if (collision.CompareTag("Player")
                    && collision.GetComponent<Blocks>().IsEntered == false
                    && collision.TryGetComponent<PhotonView>(out var block))
                {
                    // 테스트용 
                    //int playerID = block.Owner.ActorNumber;
                    int playerID = collision.GetComponent<TestBlocks>().PlayerID;

                    if (isBlockCheckDic.ContainsKey(playerID))
                        isBlockCheckDic[playerID] = true;
                }
            }

            // 3. 현재 충돌된 블럭이 있는 플레이어들만 FInishRoutine 수행
            // 충돌된 블럭이 없는 플레이어들은 기존 수행되던 루틴을 해제
            var goalRoutineKeys = goalRoutineDic.Keys.ToArray();
            foreach (var playerID in goalRoutineKeys)
            {
                // 블럭체크에 해당 플레이어가 있으면서 true인 경우 => 현재 FInish지점이 블럭이 있음
                if (isBlockCheckDic.ContainsKey(playerID))
                {
                    if (isBlockCheckDic[playerID] == true)
                    {
                        print($"{playerID} 블럭 감지");
                        if (goalRoutineDic[playerID] == null)
                            goalRoutineDic[playerID] = StartCoroutine(GoalRoutine(playerID));
                    }
                    else
                    {
                        print($"{playerID} 블럭 없음");
                        if (goalRoutineDic[playerID] != null)
                        {
                            StopCoroutine(goalRoutineDic[playerID]);
                            goalRoutineDic[playerID] = null;
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

    private IEnumerator GoalRoutine(int playerID)
    {
        // 제한 시간이 지나면
        yield return timer;

        AllPlayerStop();

        print($"{playerID}는 우승자입니다.");
        
        manager.CurrentState = StateType.Stop;
    }

    private void AllPlayerStop()
    {
        // TODO: 모든 플레이어가 조작할 수 없는 상태로 진입
        print("모든 플레이어의 행동이 중지되었습니다.");
    }
}
