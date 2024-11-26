using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalModeState : GameState
{
    [Header("서바이벌 모드 설정")]
    [SerializeField] int winBlockCount;

    private Dictionary<int,Action<int>> hpMiddleware;
    private Dictionary<int,Action<int>> blockCountMiddleware;
    private Dictionary<int,Coroutine> winRoutineDic;

    //public override void Enter()
    //{
    //    SceneLoad(SceneIndex.Game);
    //    base.Enter();

    //    // Dictionary 초기 세팅
    //    hpMiddleware = new Dictionary<int,Action<int>>();
    //    blockCountMiddleware = new Dictionary<int,Action<int>>();
    //    winRoutineDic = new Dictionary<int, Coroutine>();
    //    var playerKeys = playerObjectDic.Keys;

    //    // 플레이어 수만큼 미리 요소 추가
    //    foreach (var playerID in playerKeys)
    //    {
    //        winRoutineDic.Add(playerID, null);

    //        // 각 플레이어 HP 및 BlockCount이벤트 구독 설정
    //        if (playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
    //        {
    //            hpMiddleware.Add(playerID, (newHP) => PlayerHPHandle(newHP, playerID));
    //            controller.OnChangeHp += hpMiddleware[playerID];
    //        }
    //        if (playerObjectDic[playerID].TryGetComponent<BlockCountManager>(out var manager))
    //        {
    //            blockCountMiddleware.Add(playerID, (newBlockCount) => PlayerBlockCountHandle(newBlockCount, playerID));
    //            manager.OnChangeBlockCount += blockCountMiddleware[playerID];
    //        }
    //    }
    //}

    //public override void Exit()
    //{
    //    var playerKeys = playerObjectDic.Keys;

    //    // 플레이어 수만큼 미리 요소 추가
    //    foreach (var playerID in playerKeys)
    //    {
    //        if (playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller)
    //            && hpMiddleware.ContainsKey(playerID))
    //        {
    //            controller.OnChangeHp -= hpMiddleware[playerID];
    //        }
    //        if (playerObjectDic[playerID].TryGetComponent<BlockCountManager>(out var manager)
    //            && blockCountMiddleware.ContainsKey(playerID))
    //        {
    //            manager.OnChangeBlockCount -= blockCountMiddleware[playerID];
    //        }
    //    }

    //    hpMiddleware.Clear();
    //    blockCountMiddleware.Clear();

    //    // winRoutineDic이 실행되고 있는 경우에는 해당 코루틴은 중지
    //    foreach (int i in winRoutineDic.Keys)
    //    {
    //        if (winRoutineDic[i] != null)
    //        {
    //            StopFinishRoutine(winRoutineDic[i]);
    //        }
    //    }
    //    winRoutineDic.Clear();

    //    base.Exit();
    //}

    public void PlayerHPHandle(int newHP, int playerID)
    {
        if(newHP < 1)
        {
            if (playerObjectDic.ContainsKey(playerID)
                && playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
            {
                controller.IsGoal = true;
                // 플레이어 컨트롤러 측, 관련 변수 업데이트 시 코드 추가
                print($"{playerID}님의 남은 목숨이 모두 소진되어 게임오버되었습니다.");
            }
        }
    }


    public void PlayerBlockCountHandle(int newBlockCount, int playerID) 
    { 
        // 목표 블럭 개수만큼 쌓였을 때, 승리 루틴 실행
        if (newBlockCount >= winBlockCount)
        {
            if (winRoutineDic.ContainsKey(playerID))
            {
                if (winRoutineDic[playerID] == null)
                    winRoutineDic[playerID] = StartCoroutine(FinishRoutine(playerID));
            }
            else
            {
                print("정상적으로 초기화되지 않은 플레이어가 존재");
            }
        }
        else
        {
            //실행되고 있는 승리 루틴이 존재하면, 해당 루틴 중지
            if (winRoutineDic[playerID] != null)
            {
                StopFinishRoutine(winRoutineDic[playerID]);
                winRoutineDic[(playerID)] = null;
            }
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

        // 기존에 winRoutineDic의 목록에서 해당 플레이어 삭제
        winRoutineDic.Remove(playerID);
        winRoutineDic.Remove(playerID);

        print($"{playerID}는 이제 조작할 수 없습니다.");
    }

    private void AllPlayerResult()
    {
        if (winRoutineDic.Count < 1)
        {
             List<Tuple<int, int>> result = new List<Tuple<int, int>>();
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
            result.ForEach((x) => {
                playerUI?.SetResultEntry(x.Item1.ToString(), x.Item2);
                playerUI?.SetResult();
            });
            print($"모든 플레이어의 블럭 개수 집계 및 게임 종료");
            print($"{result[0].Item1}이 퍼즐 모드의 우승자입니다!!!");

            Time.timeScale = 0f;
        }
    }
}
