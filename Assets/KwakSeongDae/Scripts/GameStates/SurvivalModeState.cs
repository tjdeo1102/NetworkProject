using Photon.Pun;
using Photon.Realtime;
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
    private Coroutine deadRoutine;
    private Action<int> hpAction;
    private Action<int> blockCountAction;
    private List<int> DeadPlayers;

    private GameObject selfPlayer;

    protected override void Init()
    {
        base.Init();
        DeadPlayers = new List<int>();
        var playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        selfPlayer = playerObjectDic[playerID];

        // 현재 자신만 이벤트 등록
        var controller = selfPlayer.GetComponent<PlayerController>();
        hpAction = (newHP) => PlayerHPHandle(newHP, playerID);
        controller.OnChangeHp += hpAction;


        blockCountAction = (newBlockCount) => PlayerBlockCountHandle(newBlockCount, playerID);
        controller.OnChangeBlockCount += blockCountAction;
    }
    private void OnDisable()
    {
        // Routine이 실행되고 있는 경우에는 해당 코루틴은 중지
        if (deadRoutine != null)
            StopCoroutine(deadRoutine);
        if (winRoutine != null)
            StopCoroutine(winRoutine);

        ReturnScene();
        Time.timeScale = 1f;
    }

    public void PlayerHPHandle(int newHP, int playerID)
    {
        print($"{playerID} 체력 감소: {newHP}");
        // 자신 이벤트인 경우에만 호출
        if (PhotonNetwork.LocalPlayer.ActorNumber != playerID) return;

        // 블럭이 동시에 떨어질 때, 중복 호출 방지
        if (deadRoutine == null)
            deadRoutine = StartCoroutine(DeadRoutine(newHP));
    }


    public void PlayerBlockCountHandle(int newBlockCount, int playerID)
    {
        // 자신 이벤트인 경우에만 호출
        if (PhotonNetwork.LocalPlayer.ActorNumber != playerID) return;

        // 목표 블럭 개수만큼 쌓였을 때, 승리 루틴 실행
        if (newBlockCount >= winBlockCount)
        {
            if (winRoutine == null)
                winRoutine = StartCoroutine(FinishRoutine(photonView.Owner.ActorNumber));
        }
        else
        {
            //실행되고 있는 승리 루틴이 존재하면, 해당 루틴 중지
            if (winRoutine != null)
            {
                StopFinishRoutine(winRoutine);
                winRoutine = null;
            }
        }

    }

    private IEnumerator DeadRoutine(int newHP)
    {
        if (newHP < 1)
        {
            if (selfPlayer.TryGetComponent<PlayerController>(out var controller))
            {
                // 해당 플레이어는 더 이상 조작 불가
                controller.ReachGoal();
                controller.OnChangeHp -= hpAction;
                controller.OnChangeBlockCount -= blockCountAction;

                print($"{photonView.Owner.NickName}님의 남은 목숨이 모두 소진되어 게임오버되었습니다.");
                photonView.RPC("AllPlayerStateCheck", RpcTarget.MasterClient, false, photonView.Owner.ActorNumber);
            }
        }
        // 블럭이 동시에 떨어지는 경우, 중복 실행 방지
        yield return new WaitForSeconds(1f);
        deadRoutine = null;
    }

    protected override IEnumerator FinishRoutine(int playerID)
    {
        yield return StartCoroutine(base.FinishRoutine(playerID));

        // 제한 시간이 지나면
        // 해당 플레이어는 더 이상 조작 불가
        if (selfPlayer.TryGetComponent<PlayerController>(out var controller))
        {
            controller.ReachGoal();
            controller.OnChangeHp -= hpAction;
            controller.OnChangeBlockCount -= blockCountAction;

            print($"{playerID}는 이제 조작할 수 없습니다.");

            // 이미 승리한 플레이어가 나타났으므로, 게임 끝
            // 모든 플레이어 상태 체크 후, 집계 시작
            photonView.RPC("AllPlayerStateCheck", RpcTarget.MasterClient, true, photonView.Owner.ActorNumber);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // 방장이 강제 종료 예외 처리
        if (PhotonNetwork.IsMasterClient == false) return;
        playerObjectDic.Remove(otherPlayer.ActorNumber);
        towerObjectDic.Remove(otherPlayer.ActorNumber);

        AllPlayerStateCheck(false);
    }

    [PunRPC]
    private void AllPlayerStateCheck(bool isFinishGame,int playerID = -1)
    {
        // 죽은 플레이어가 있는 경우, 방장은 초기화 진행
        if (playerID > -1)
        {
            // 기존 Dic의 목록에서 해당 플레이어 추가
            DeadPlayers.Add(playerID);
        }

        if (isFinishGame)
        {
            foreach (var player in playerObjectDic.Keys)
            {
                if(!DeadPlayers.Contains(player))
                {
                    DeadPlayers.Add(player);
                }
            }
        }

        if (DeadPlayers.Count >= playerObjectDic.Count)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();
            foreach (var id in playerObjectDic.Keys)
            {
                if (playerObjectDic[id].TryGetComponent<PlayerController>(out var controller))
                {
                    result.Add(new Tuple<int, int>(id,controller.BlockCount));
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
    private void UpdateUI(int[] playerIDs, int[] blockCounts)
    {
        for (int i = 0; i < playerIDs.Length; i++)
        {
            playerUI?.AddResultEntry(playerIDs[i], blockCounts[i]);
        }
        playerUI?.SetResult();

        print($"모든 플레이어의 블럭 개수 집계 및 게임 종료");
        print($"{playerIDs[0]}이 서바이벌 모드의 우승자입니다!!!");

        Time.timeScale = 0f;
    }
}
