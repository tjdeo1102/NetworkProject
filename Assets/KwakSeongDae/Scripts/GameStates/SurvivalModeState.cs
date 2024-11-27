using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SurvivalModeState : GameState
{
    [Header("서바이벌 모드 설정")]
    [SerializeField] int winBlockCount;

    private Coroutine winRoutine;
    private Action<int> hpAction;
    private Action<int> blockCountAction;
    private List<int> FinishPlayers;

    protected override void OnEnable()
    {
        base.OnEnable();
        FinishPlayers = new List<int>();

        if (photonView.IsMine == false) return;

        // 현재 자신만 이벤트 등록
        var controller = selfPlayer.GetComponent<PlayerController>();
        hpAction = (newHP) => PlayerHPHandle(newHP);
        controller.OnChangeHp += hpAction;
        blockCountAction = (newBlockCount) => PlayerBlockCountHandle(newBlockCount);
        //controller.OnChangeBlockCount += blockCountAction;
    }

    public override void Exit()
    {
        // winRoutine이 실행되고 있는 경우에는 해당 코루틴은 중지
        StopCoroutine(winRoutine);
        var controller = selfPlayer.GetComponent<PlayerController>();
        controller.OnChangeHp -= hpAction;
        //controller.OnChangeBlockCount -= blockCountAction;

        base.Exit();
    }

    public void PlayerHPHandle(int newHP)
    {
        // 자신 이벤트인 경우에만 호출
        if (photonView.IsMine == false) return;

        if (newHP < 1)
        {
            if (selfPlayer.TryGetComponent<PlayerController>(out var controller))
            {
                controller.IsGoal = true;
                // 플레이어 컨트롤러 측, 관련 변수 업데이트 시 코드 추가
                print($"{photonView.Owner.NickName}님의 남은 목숨이 모두 소진되어 게임오버되었습니다.");
                photonView.RPC("AllPlayerStateCheck", RpcTarget.MasterClient, false, photonView.Owner.ActorNumber);
            }
        }
    }


    public void PlayerBlockCountHandle(int newBlockCount)
    {
        // 자신 이벤트인 경우에만 호출
        if (photonView.IsMine == false) return;

        // 목표 블럭 개수만큼 쌓였을 때, 승리 루틴 실행
        if (newBlockCount >= winBlockCount
            && winRoutine == null)
        {
            winRoutine = StartCoroutine(FinishRoutine(photonView.Owner.ActorNumber));
        }
        else
        {
            //실행되고 있는 승리 루틴이 존재하면, 해당 루틴 중지
            if (winRoutine != null)
            {
                StopFinishRoutine(winRoutine);
            }
        }

    }

    protected override IEnumerator FinishRoutine(int playerID)
    {
        yield return StartCoroutine(base.FinishRoutine(playerID));

        // 제한 시간이 지나면
        // 해당 플레이어는 더 이상 조작 불가
        print($"{playerID}는 이제 조작할 수 없습니다.");

        // 모든 플레이어 상태 체크 후, 집계 시작
        photonView.RPC("AllPlayerStateCheck", RpcTarget.MasterClient, true, photonView.Owner.ActorNumber);
    }

    [PunRPC]
    private void AllPlayerStateCheck(bool isFinishGame,int playerID = -1)
    {
        // 딕셔너리에서 초기화할 플레이어가 있는 경우, 방장은 초기화 진행
        if (playerID > -1)
        {
            // 기존에 Dic의 목록에서 해당 플레이어 추가
            FinishPlayers.Add(playerID);
        }

        if (isFinishGame)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();
            int score = 0;
            for (score = 0; score < FinishPlayers.Count; score++)
            {
                result.Add(new Tuple<int, int>(FinishPlayers[score], score + 1));
            }
            //내림차순으로 블럭 개수 정렬
            result.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            //result.ForEach((x) => {
            //    playerUI?.SetResultEntry(x.Item1.ToString(), x.Item2);
            //    playerUI?.SetResult();
            //});
            print($"모든 플레이어의 블럭 개수 집계 및 게임 종료");
            print($"{result[0].Item1}이 서바이벌 모드의 우승자입니다!!!");

            var players = new int[result.Count];
            var blockCounts = new int[result.Count];
            for (int i = 0; i < result.Count; i++)
            {
                players[i] = result[i].Item1;
                blockCounts[i] = result[i].Item2;
            }
            Time.timeScale = 0f;

            // UI 업데이트 작업 및 게임 정지기능은 모든 클라이언트 진행
            photonView.RPC("UpdateUI", RpcTarget.All, players, blockCounts);
        }
        else
        {
            if (FinishPlayers.Count < playerObjectDic.Count)
            {
                List<Tuple<int, int>> result = new List<Tuple<int, int>>();
                foreach (var id in playerObjectDic.Keys)
                {
                    //if (playerObjectDic[id].TryGetComponent<BlockCountManager>(out var manager))
                    //{
                    //    result.Add(new Tuple<int, int>(id manager.BlockCount));
                    //}

                    //테스트 코드
                    result.Add(new Tuple<int, int>(id, id));
                }
                //내림차순으로 블럭 개수 정렬
                result.Sort((x, y) => y.Item2.CompareTo(x.Item2));

                //각 클라이언트에서, 각각의 UI에 해당 내용 반영되도록 설정
                print($"모든 플레이어의 블럭 개수 집계 및 게임 종료");
                print($"성공한 사람은 없지만.. {result[0].Item1}이 서바이벌 모드의 우승자입니다!!!");

                var players = new int[result.Count];
                var blockCounts = new int[result.Count];
                for (int i = 0; i < result.Count; i++)
                {
                    players[i] = result[i].Item1;
                    blockCounts[i] = result[i].Item2;
                }
                Time.timeScale = 0f;

                // UI 업데이트 작업 및 게임 정지기능은 모든 클라이언트 진행
                photonView.RPC("UpdateUI", RpcTarget.All, players, blockCounts);
            }
        }
    }
}
