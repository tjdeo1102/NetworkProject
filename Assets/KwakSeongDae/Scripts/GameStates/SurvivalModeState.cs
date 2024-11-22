using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalModeState : GameState
{
    [Header("서바이벌 모드 설정")]
    [SerializeField] float winTimer;
    [SerializeField] int winBlockCount;

    private Action<int> hpMiddleware;
    private Action<int> bulletCountMiddleware;
    private Dictionary<int,Coroutine> winRoutineDic;
    private WaitForSeconds timer;

    public override void Enter()
    {
        base.Enter();

        // Dictionary 초기 세팅
        winRoutineDic = new Dictionary<int, Coroutine>();
        var playerKeys = playerObjectDic.Keys;

        // 플레이어 수만큼 미리 요소 추가
        foreach (var playerID in playerKeys)
        {
            winRoutineDic.Add(playerID, null);

            if (playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
            {
                // TODO: 컨트롤러 측, 관련 이벤트 구현시 참조 설정
                hpMiddleware = (newHP) => PlayerHPHandle(newHP,playerID);
                bulletCountMiddleware = (newBlockCount) => PlayerBlockCountHandle(newBlockCount, playerID);
                // controller.OnChangeHp += hpMiddleware;
                // controller.OnChangeBlockCount += bulletCountMiddleware;
                print($"{playerID}에 대한 HP 및 BlockCount이벤트 구독 설정");
            }
        }

        // Timer 초기 세팅
        timer = new WaitForSeconds(winTimer);
    }

    public override void Exit()
    {
        // 이벤트 구독 해제
        // controller.OnChangeHp -= hpMiddleware;
        // controller.OnChangeBlockCount -= bulletCountMiddleware;

        // winRoutineDic이 실행되고 있는 경우에는 해당 코루틴은 중지
        foreach (int i in winRoutineDic.Keys)
        {
            if (winRoutineDic[i] != null)
            {
                StopCoroutine(winRoutineDic[i]);
            }
        }
        winRoutineDic.Clear();

        base.Exit();
    }

    public void PlayerHPHandle(int newHP, int playerID)
    {
        if(newHP < 1)
        {
            if (playerObjectDic.ContainsKey(playerID)
                && playerObjectDic[playerID].TryGetComponent<PlayerController>(out var controller))
            {
                // TODO: 플레이어가 죽은 상황일 때, 해당 플레이어는 조작 못하도록 설정
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
                    winRoutineDic[playerID] = StartCoroutine(WinRoutine(playerID));
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
                StopCoroutine(winRoutineDic[playerID]);
                winRoutineDic[(playerID)] = null;
            }
        }

    }

    private IEnumerator WinRoutine(int playerID)
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
