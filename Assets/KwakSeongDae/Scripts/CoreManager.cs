using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum StateType
{
    Stop,Puzzle,Race,Survival,Size
}

public class CoreManager : Singleton<CoreManager>
{
    [Header("게임 상태 관리")]
    [SerializeField] GameState[] states;
    [SerializeField] private StateType currentState;
    public StateType CurrentState
    {
        get => currentState;
        set
        {
            currentState = value;
            ChangeState(value);
        }
    }

    private GameState state;
    private Dictionary<StateType, GameState> stateDic;

    [Header("참가 플레이어 관리")]
    public Dictionary<int, GameObject> PlayerDic;

    protected override void Init()
    {
        stateDic = new Dictionary<StateType, GameState>();
        PlayerDic = new Dictionary<int, GameObject>();
        foreach (var s in states)
        {
            stateDic.Add(s.StateType, s);
        }
        print("INIT");
        CurrentState = StateType.Stop;
    }

    private void Update()
    {
        state?.OnUpdate();
    }

    private void ChangeState(StateType changeType)
    {
        if (stateDic != null 
            && stateDic.ContainsKey(changeType))
        {
            state?.Exit();
            state?.gameObject.SetActive(false);

            state = stateDic[changeType];

            state.gameObject.SetActive(true);
            state?.Enter();
        }
    }

    /// <summary>
    /// 편집기에서 State를 임의로 바꿨을 때, 적용되도록 설정
    /// </summary>
    private void OnValidate()
    {
        ChangeState(currentState);
    }

    /// <summary>
    /// 포톤 뷰가 설정된 플레이어만 플레이어로 추가 가능
    /// </summary>
    public void SetPlayer(GameObject player)
    {
        if (player.TryGetComponent<PhotonView>(out var view))
        {
            PlayerDic.Add(view.Owner.ActorNumber, player);
        }
    }
    public void SetPlayer(GameObject[] player)
    {
        foreach (var p in player)
        {
            if (p.TryGetComponent<PhotonView>(out var view))
            {
                PlayerDic.Add(view.Owner.ActorNumber, p);
            }
        }
    }

    /// <summary>
    /// 만약, 각 플레이어에 대한 초기화가 따로 필요한 경우, PlayerDic를 이용해 직접 초기화 진행
    /// </summary>
    public void ResetPlayer()
    {
        PlayerDic.Clear();
    }
}
