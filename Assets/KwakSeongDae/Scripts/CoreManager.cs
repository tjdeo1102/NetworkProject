using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[Serializable]
//public enum StateType
//{
//    Stop,Puzzle,Race,Survival,Size
//}

public class CoreManager : Singleton<CoreManager>
{
//    [Header("게임 상태 관리")]
//    [SerializeField] GameState[] states;
//    [SerializeField] private StateType currentState;
//    public StateType CurrentState
//    {
//        get => currentState;
//        set
//        {
//            currentState = value;
//            // 모든 클라이언트 각자 매니저에서 상태 변경 수행
//            photonView.RPC("ChangeState",RpcTarget.All,value);
//        }
//    }

//    [HideInInspector]public GameState state;
//    private Dictionary<StateType, GameState> stateDic;

//    [Header("참가 플레이어 관리")]
//    public Dictionary<int, Player> PlayerDic;

//    protected override void Init()
//    {
//        stateDic = new Dictionary<StateType, GameState>();
//        PlayerDic = new Dictionary<int, Player>();
//        foreach (var s in states)
//        {
//            stateDic.Add(s.StateType, s);
//        }
//        print("INIT");
//        CurrentState = StateType.Stop;
//    }

//    private void Update()
//    {
//        state?.OnUpdate();
//    }

//    [PunRPC]
//    private void ChangeState(StateType changeType)
//    {
//        if (stateDic != null 
//            && stateDic.ContainsKey(changeType))
//        {
//            state?.Exit();
//            state?.gameObject.SetActive(false);

//            state = stateDic[changeType];

//            state.gameObject.SetActive(true);
//            state?.Enter();
//        }
//    }

//    /// <summary>
//    /// 인스펙터에서 State를 임의로 바꿨을 때, 적용되도록 설정.
//    /// 추후,배포시 삭제 필요
//    /// </summary>
//    private void OnValidate()
//    {
//#if UNITY_EDITOR
//        // 모든 클라이언트 각자 매니저에서 상태 변경 수행
//            photonView.RPC("ChangeState",RpcTarget.All,currentState);
//#endif
//    }
}
