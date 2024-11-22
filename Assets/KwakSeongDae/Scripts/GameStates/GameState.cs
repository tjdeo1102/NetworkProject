using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameState : MonoBehaviour
{
    
    [Header("기본 설정")]
    public StateType StateType;

    [Header("플레이어 스폰 설정")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Vector2 bottomLeft;            // 스폰 가능 지역의 좌하단 좌표
    [SerializeField] Vector2 upRight;               // 스폰 가능 지역의 우상단 좌표

    protected CoreManager manager;
    protected Dictionary<int, GameObject> playerObjectDic;

    public virtual void Enter() 
    {
        print($"{StateType}에 진입");
        manager = CoreManager.Instance;
        playerObjectDic = new Dictionary<int, GameObject>();
        if (playerPrefab != null )
        {
            var playerKeys = manager.PlayerDic.Keys.ToArray();
            print($"플레이어 수: {playerKeys.Length}");
            var playerSpawnPos = PlayerSpawnStartPositions(bottomLeft, upRight, playerKeys.Length);
            for (int i = 0; i < playerKeys.Length; i++)
            {
                playerObjectDic.Add(playerKeys[i], Instantiate(playerPrefab, playerSpawnPos[i], Quaternion.identity, null));
            }
        }
    }
    public virtual void OnUpdate() 
    {
        //print($"{StateType}에서 업데이트 중");
    }
    public virtual void Exit() 
    {
        print($"{StateType}에서 탈출");
        // TODO: 게임 모드 끝났을 때, 어떻게 처리?
        // 기본적으로 해당 게임 모드가 끝나면 방이 해체된다고 가정하고, 초기화 진행
        var playerObjectKeys =playerObjectDic.Keys.ToArray();
        // 플레이어 오브젝트들 삭제
        foreach (var key in playerObjectKeys)
        {
            if (playerObjectDic[key] != null)
                Destroy(playerObjectDic[key]);
        }
        playerObjectDic.Clear();
        manager?.ResetPlayer();
    }

    private Vector2[] PlayerSpawnStartPositions(Vector2 bottomLeft, Vector2 upRight, int playerNum)
    {
        if (playerNum < 1 || playerNum > 4) return null;

        // 개인 플레이어 너비 = 전체 너비 / 플레이어 수
        var width = MathF.Abs(upRight.x - bottomLeft.x) / playerNum;

        // 플레이어 스폰 위치 (x값) =
        // (bottomLeft + 개인 너비 * 플레이어 인덱스 = 각 플레이어 영역의 bottomLeft)
        // + (개인너비 / 2 = 각 플레이어 영역의 중심) 
        // 플레이어 스폰 위치 (y값) = bottomLeft.y
        var playerPositions = new Vector2[playerNum];
        for (int i = 0; i < playerPositions.Length; i++)
        {
            playerPositions[i] = new Vector2((bottomLeft.x + width * i) + (width / 2), bottomLeft.y);
        }
        return playerPositions;
    }
}
