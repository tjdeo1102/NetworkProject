using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [Header("기본 설정")]
    public StateType StateType;

    protected CoreManager manager;

    public virtual void Enter() 
    {
        print($"{StateType}에 진입");
        manager = CoreManager.Instance;
    }
    public virtual void OnUpdate() 
    {
        //print($"{StateType}에서 업데이트 중");
    }
    public virtual void Exit() 
    {
        print($"{StateType}에서 탈출");
    }
}
