using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Blocks : MonoBehaviour
{
    [SerializeField] private float basicFallSpeed;          // 하강 속도
    [SerializeField] private float fastFallSpeed;           // 빠른 하강 속도
    [SerializeField] private GameObject spotlightPrefab;    // spotlight UI 오브젝트
    [SerializeField] private float moveAmount;              // 일반 이동량
    [SerializeField] private float pushAmount;              // 밀치기 이동량
    [SerializeField] private float rotateAmount;            // 회전량
    [SerializeField] private float moveDelay;               // 이동 시 부여할 딜레이

    [SerializeField] private GameObject[] tiles;            // 블럭 타일들
    [SerializeField] private Transform tileParent;          // 타일들의 부모 트랜스폼
    [SerializeField] private Vector2[] tileRotPos;          // 회전시 적용할 위치값들

    private int rotIndex = 0;               // tileRotPos 배열에서 사용할 index값

    private Rigidbody2D rigid;              // Rigidbody2D 컴포넌트 참조                                          

    private Vector2 currentVelocity;        // 현재 하강 속도
    private Vector2 currentDirection;       // 현재 이동 방향
    private float currentAmount;            // 현재 이동량

    private bool isControllable;            // 제어 가능 여부
    private bool isFastDown;                // 빠른 하강 여부
    private bool isPushing;                 // 밀치기 여부

    private Coroutine moveRoutine;          // 이동 시 사용할 코루틴
    private WaitForSeconds wsMoveDelay;     // 이동 코루틴에서 활용할 WaitForSeconds 객체

    public UnityAction OnBlockFallen;       // 블럭이 맵밖으로 떨어질 때 Invoke (체력감소, 사망 처리에 활용)
    public UnityAction OnBlockEntered;      // 블럭이 안착했을 때 Invoke (블럭 카운팅에 활용)
    public UnityAction OnBlockExited;       // 블럭이 안착했다가 벗어날 때 (블럭 카운팅에 활용)

    public bool IsEntered { get { return isControllable; } }     // 외부에서 안착여부를 확인하기 위한 프로퍼티

    private void Awake()
    {
        // 컴포넌트 참조
        rigid = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        isControllable = true;
        wsMoveDelay = new WaitForSeconds(moveDelay);

        // 기본 하강 속도 설정
        currentVelocity = new Vector2(rigid.velocity.x, basicFallSpeed);
    }

    void Update()
    {
        // 타워나 블럭에 부딪힌 이후부터는 해당 블럭에 대한 조작 불가능
        if (!isControllable)
        {
            return;
        }

        // 이동, 밀치기 여부 확인하여 amount 설정
        currentAmount = isPushing ? pushAmount : moveAmount;
        // 이동 or 밀치기 진행
        ExecuteMove();

        // 빠른 하강 여부 확인하여 velocity 설정
        currentVelocity.y = isFastDown ? fastFallSpeed : basicFallSpeed;
        // 하강
        rigid.velocity = currentVelocity;

        // flag 초기화
        // 플레이어가 지속적으로 key를 누르고 있는다면 다시 true가 될 것이고
        // 플레이어가 key를 더이상 누르지 않는다면 false인 상태로 지속될 것이다.
        isPushing = false;
        isFastDown = false;
    }

    // Player의 빠른 하강 조작을 위한 Interface
    public void Down()
    {
        isFastDown = true;
    }

    // Player의 이동 조작을 위한 Interface
    public void Move(Vector2 dir)
    {
        currentDirection = dir;
    }

    // Player의 밀치기 조작을 위한 Interface
    public void Push(Vector2 dir)
    {
        currentDirection = dir;
        isPushing = true;
    }

    public void Rotate()
    {
        if (!isControllable)
            return;

        // z축으로 rotateAmount 만큼 회전
        transform.Rotate(Vector3.forward, rotateAmount);

        // 회전시 적용할 위치값을 설정하지 않았으면 여기서 return
        if (tileRotPos.Length == 0)
            return;

        // tile Parent의 위치를 적용
        tileParent.localPosition = tileRotPos[rotIndex++];

        // 360도 회전을 모두 진행했다면 다시 처음으로 
        if (rotIndex >= tileRotPos.Length)
            rotIndex = 0;
    }

    // 실제 이동, 밀치기를 진행
    private void ExecuteMove()
    {
        // 이동이 없는 경우
        if (currentDirection == Vector2.zero)
        {
            // 이동 코루틴 중지
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
                moveRoutine = null;
            }
            return;
        }

        // 아직 이동 코루틴이 실행되지 않았다면 실행
        if (moveRoutine == null)
        {
            moveRoutine = StartCoroutine(MoveRoutine());
        }
    }

    // 이동 코루틴을 통해 이동 타이밍을 조절
    // Update에서 바로 이동해버리면 이동속도 제어가 안됀다
    private IEnumerator MoveRoutine()
    {
        Vector2 movePos;

        // 제어가 가능할 동안 루프
        while (isControllable)
        {
            movePos = rigid.position + currentDirection * currentAmount;

            // 순간적으로 이동해야 하므로 position값을 변경한다.
            rigid.position = movePos;

            // delay만큼 대기
            yield return wsMoveDelay;

            rigid.velocity = new Vector2(0, rigid.velocity.y);
        }

        // 코루틴 변수 초기화
        moveRoutine = null;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isControllable)
            rigid.velocity = Vector2.zero;

        // 충돌한 면이 other의 윗면인지 확인
        if (other.contacts[0].normal.y >= 0.9f)
        {
            Debug.Log("entered");

            //// 충돌 시 flag 변경 (레이어로 특정 물체 구분 필요?)
            //isControllable = false;

            // 이벤트 발생
            // (타워의 경우 옆면충돌을 별도로 판정해야 하는가?)
            OnBlockEntered?.Invoke();
        }
        else
        {
            Debug.Log("not entered");
        }
        // 충돌 시 flag 변경 (레이어로 특정 물체 구분 필요?)
        isControllable = false;
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        // 이벤트 발생
        // 현재 블럭이 Enter 상태였었는지 확인할 수단이 필요? 
        OnBlockExited?.Invoke();
    }
}
