using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceModeState : GameState
{
    [SerializeField] private float goalAcceptTime;
    private Dictionary<int, Coroutine> goalRoutineDic;
    private Dictionary<int, int> collisionBlockCountDic;
    private WaitForSeconds timer;

    public override void Enter()
    {
        base.Enter();
        // Dictionary 초기 세팅
        goalRoutineDic = new Dictionary<int, Coroutine>();
        collisionBlockCountDic = new Dictionary<int, int>();
        // Timer 초기 세팅
        timer = new WaitForSeconds(goalAcceptTime);
    }

    public override void Exit()
    {
        base.Exit();
        // finishRoutine이 실행되고 있는 경우에는 해당 코루틴은 중지
        foreach (int i in goalRoutineDic.Keys)
        {
            if (goalRoutineDic[i] != null)
            {
                StopCoroutine(goalRoutineDic[i]);
            }
        }
        goalRoutineDic.Clear();
        collisionBlockCountDic.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // TODO: 블럭에 해당되는 태그로 교체,
        // 블럭이 존재하는 경우, 블럭과 디텍터끼리 충돌 감지되면 BlockCount 증가
        // 충돌된 블럭이 있을때, 플레이어의 코루틴의 유무 판단 후, 코루틴 실행
        if (collision.CompareTag("Block")
            && collision.TryGetComponent<PhotonView>(out var block))
        {
            int playerID = block.Owner.ActorNumber;

            if (collisionBlockCountDic.ContainsKey(playerID))
                collisionBlockCountDic[playerID]++;
            else
                collisionBlockCountDic.Add(playerID, 1);

            if (goalRoutineDic.ContainsKey(playerID) == false)
                goalRoutineDic.Add(playerID, StartCoroutine(FinishRoutine(playerID)));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // TODO: 블럭이 존재하는 경우, 해당 태그를 가진 오브젝트와의 충돌 감지 처리
        if (collision.CompareTag("Block") && collision.TryGetComponent<PhotonView>(out var block))
        {
            int playerID = block.Owner.ActorNumber;
            if (collisionBlockCountDic.ContainsKey(playerID))
            {
                collisionBlockCountDic[playerID]--;

                if (collisionBlockCountDic[playerID] < 1
                    && goalRoutineDic.ContainsKey(playerID))
                {
                    StopCoroutine(goalRoutineDic[playerID]);
                    goalRoutineDic.Remove(playerID);
                }
            }
            // blockCount에 집계되지 않는 경우는 비정상적인 집계로 판단하고 finishRoutine을 중지
            // 비정상 집계 이유: 블럭이 추가되지 않고, 갑자기 블럭이 Exit되거나, 블럭 Enter가 정상적으로 처리되지 못함
            else
            {
                print("비정상적인 블럭");
                if (goalRoutineDic.ContainsKey(playerID))
                {
                    StopCoroutine(goalRoutineDic[playerID]);
                    goalRoutineDic.Remove(playerID);
                }
            }
        }
    }

    private IEnumerator FinishRoutine(int playerID)
    {
        // 제한 시간이 지나면
        yield return timer;

        // TODO: 모든 플레이어가 조작할 수 없는 상태로 진입 및 해당 모드 종료
        print($"{playerID}는 우승자입니다.");
        
        manager.CurrentState = StateType.Stop;
    }
}
