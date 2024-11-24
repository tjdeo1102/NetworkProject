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
    [SerializeField] private LayerMask castLayer;           // 레이캐스트용 layermask

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
    // 추가로 좌우 이동 시 충돌 감지 후 충돌을 처리한다.
    private IEnumerator MoveRoutine()
    {
        Vector2 lastDir; // 움직일 방향
        float lastAmount; // 움직일 양
        Vector2 moveDist; // 움직일 거리

        RaycastHit2D hit1; // 위쪽 체크 결과
        RaycastHit2D hit2; // 아래쪽 체크 결과
        Vector2 startPos1; // 위쪽 시작 위치
        Vector2 startPos2; // 아래쪽 시작 위치

        RaycastHit2D resultHit; // 최종적으로 선정된 충돌 결과 (위쪽과 아래쪽을 판별해서 최종적으로 선정)
        Vector2 resultStartPos; // 최종적으로 선정된 시작 위치 (동일)

        Vector2 toHit; // 감지 시작 위치 -> 감지된 위치로 향하는 벡터

        // 제어가 가능할 동안 루프
        while (isControllable)
        {
            moveDist = currentDirection * currentAmount;
            lastDir = currentDirection;
            lastAmount = currentAmount;

            // 충돌 사전 감지
            // position 이동 시에 도착위치에 감지되는 물체가 있으면 실제 이동하지 않고 별도 처리
            for (int i = 0; i < tiles.Length; i++)
            {
                // 각 타일의 위치로부터 이동할 방향으로 raycast
                startPos1 = (Vector2)tiles[i].transform.position + (lastDir * 0.25f) + (Vector2.up * 0.22f);
                startPos2 = (Vector2)tiles[i].transform.position + (lastDir * 0.25f) + (Vector2.down * 0.22f);
                hit1 = Physics2D.Raycast(startPos1, lastDir, lastAmount - 0.05f, castLayer);
                hit2 = Physics2D.Raycast(startPos2, lastDir, lastAmount - 0.05f, castLayer);

                // 충돌감지가 하나라도 됐는지 확인
                if (hit1.collider == null && hit2.collider == null)
                    continue;

                // 둘다 감지 된 경우
                if (hit1.collider != null && hit2.collider != null)
                {
                    resultHit = hit1.distance <= hit2.distance ? hit1 : hit2;
                    resultStartPos = hit1.distance <= hit2.distance ? startPos1 : startPos2;
                }
                // hit1이 감지된 경우
                else if (hit1.collider != null)
                {
                    resultHit = hit1;
                    resultStartPos = startPos1;
                }
                // hit2가 감지된 경우
                else
                {
                    resultHit = hit2;
                    resultStartPos = startPos2;
                }
                
                // 부모 오브젝트가 hit된다면 해당 타일은 중간에 끼인 타일이므로 넘어간다.
                if (resultHit.transform == transform)
                    continue;

                // 제어권을 해제
                isControllable = false;

                // 충돌 검사를 시작한 위치로 부터 충돌한 지점까지의 거리를 확인
                toHit = resultHit.point - resultStartPos;
                // 충돌은 감지했지만 위치가 딱 붙어있지 않는 경우
                // (밀치기의 경우 왼쪽 2칸까지 (1f) 범위이기 때문에 필요한 조건)
                if (toHit.magnitude > 0.01f)
                {
                    Debug.Log("pos calibration");
                    // 블록의 위치를 보정해준다.
                    transform.position += new Vector3(toHit.x, 0, 0);
                }

                // for debug
                Debug.Log($"Horizontal Collision : {tiles[i].name} > {resultHit.collider.name}");
                Debug.Log($"origin : {(Vector2)tiles[i].transform.position}, direction : {lastDir}");
                SpriteRenderer sr = tiles[i].GetComponent<SpriteRenderer>();
                sr.color = Color.red;

                yield break;
            }

            Debug.Log("No Collision");

            // 순간적으로 이동해야 하므로 position값을 변경한다.
            rigid.position += moveDist;

            // delay만큼 대기
            yield return wsMoveDelay;
        }

        // 코루틴 변수 초기화
        moveRoutine = null;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
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

        if (isControllable)
        {
            rigid.velocity = Vector2.zero;

            // 충돌 시 flag 변경 
            isControllable = false;
        }   
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        // 이벤트 발생
        // 현재 블럭이 Enter 상태였었는지 확인할 수단이 필요? 
        OnBlockExited?.Invoke();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (tiles.Length <= 0)
            return;

        for (int i = 0; i < tiles.Length; i++)
        {
            Gizmos.DrawRay((tiles[i].transform.position+Vector3.left*0.25f) + (Vector3.up * 0.22f), Vector3.left * 0.45f);
            Gizmos.DrawRay((tiles[i].transform.position+Vector3.left*0.25f) + (Vector3.down * 0.22f), Vector3.left * 0.45f);
            Gizmos.DrawRay((tiles[i].transform.position+Vector3.right*0.25f) + (Vector3.up * 0.22f), Vector3.right * 0.45f);
            Gizmos.DrawRay((tiles[i].transform.position+Vector3.right*0.25f) + (Vector3.down * 0.22f), Vector3.right * 0.45f);
        }
    }
}
