using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Blocks : MonoBehaviourPun, IPunObservable
{
    #region Serialize Field
    [SerializeField] private float basicFallSpeed;          // 하강 속도
    [SerializeField] private float fastFallSpeed;           // 빠른 하강 속도
    [SerializeField] private GameObject spotlightObject;    // spotlight 오브젝트
    [SerializeField] private float moveAmount;              // 일반 이동량
    [SerializeField] private float pushAmount;              // 밀치기 이동량
    [SerializeField] private float rotateAmount;            // 회전량
    [SerializeField] private float moveDelay;               // 이동 시 부여할 딜레이
    [SerializeField] private GameObject[] tiles;            // 블럭 타일들
    [SerializeField] private LayerMask castLayer;           // 레이캐스트용 layermask
    [SerializeField] private float rotateSpeed;             // 회전 속도
    [SerializeField] private Vector2 blockSize;             // 블럭 사이즈 (타일 하나당 0.5로 계산)  
    [SerializeField] private float panaltyDrag;             // 타워 상승 시 적용할 Drag
    #endregion

    #region Private Field
    private Rigidbody2D rigid;              // Rigidbody2D 컴포넌트 참조                                          
    private Vector2 currentVelocity;        // 현재 하강 속도
    private Vector2 currentDirection;       // 현재 이동 방향
    private float currentAmount;            // 현재 이동량
    private float targetAngle;              // 목표 회전각              

    private bool isControllable;            // 제어 가능 여부
    private bool isFastDown;                // 빠른 하강 여부
    private bool isPushing;                 // 밀치기 여부
    private bool isRotate;                  // 회전 여부
    private bool isEntered;                 // 블럭 안착 여부
    private bool isVertical = false;        // 회전 상태를 확인하기 위한 flag

    private int collisionCount = 0;         // 현재 블럭과 충돌해있는 충돌체의 수 (exit 판정에 사용)
    private float normalDrag;               // 일반 상태의 Drag 수치 (최초 설정된 Drag) 

    private Coroutine moveRoutine;          // 이동 시 사용할 코루틴
    private WaitForSeconds wsMoveDelay;     // 이동 코루틴에서 활용할 WaitForSeconds 객체

    private PlayerController owner;         // 자신을 생성한 Player

    private Coroutine freezeRotateRoutine;  // 회전 처리를 위한 코루틴
    #endregion

    #region Public Field
    public UnityAction<Blocks> OnBlockFallen;       // 블럭이 맵밖으로 떨어질 때 Invoke (체력감소, 사망 처리에 활용)
    public UnityAction<Blocks> OnBlockEntered;      // 블럭이 안착했을 때 Invoke (블럭 카운팅에 활용)
    public UnityAction<Blocks> OnBlockExited;       // 블럭이 안착했다가 벗어날 때 (블럭 카운팅에 활용)
  
    public bool IsControllable { get { return isControllable; } } // 외부에서 제어 가능여부를 확인하기 위한 프로퍼티
    public bool IsEntered { get { return isEntered; } }     // 외부에서 안착여부를 확인하기 위한 프로퍼티
    public PlayerController Owner { get { return owner; } } // 외부에서 블럭 Owner를 확인하기 위한 프로퍼티
    #endregion

    private void Awake()
    {
        // 컴포넌트 참조
        rigid = GetComponent<Rigidbody2D>();

        // 최초 Drag 저장
        normalDrag = rigid.drag;
    }

    private void Start()
    {
        isControllable = true;
        wsMoveDelay = new WaitForSeconds(moveDelay);

        // 네트워크 or local 처리
        if (PhotonNetwork.IsConnected)
        {
            // 자신의 블럭이 아니면 spotlight를 보이지 않도록 한다.
            if (photonView.IsMine)
                SetSpotlight();
            else
                spotlightObject.SetActive(false);
        }
        else
        {
            SetSpotlight();
        }

        // 기본 하강 속도 설정
        currentVelocity = new Vector2(rigid.velocity.x, basicFallSpeed);
    }

    private void Update()
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

    private void FixedUpdate()
    {
        if (!isControllable)
            return;

        if (isRotate)
        {
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (rigid.rotation >= targetAngle)
                rigid.rotation = targetAngle;
            else
                rigid.rotation += rotateSpeed;

            if (freezeRotateRoutine == null)
                freezeRotateRoutine = StartCoroutine(FreezeRotateRoutine());
        }
    }

    private IEnumerator FreezeRotateRoutine()
    {
        var delay = new WaitForSeconds(0.2f);
        yield return delay;
        // spotlight의 크기 설정
        SetSpotlight();

        rigid.constraints = RigidbodyConstraints2D.None;

        spotlightObject.SetActive(isControllable);

        // 다시 회전이 가능하도록 회전 완료 처리
        isRotate = false;
        freezeRotateRoutine = null;
    }

    public void ChangeDrag(bool bPanalty)
    {
        // 타워 상승 진행 시
        if (bPanalty)
        {
            // 안착해있는 블럭인 경우
            if (isEntered)
            {
                rigid.drag = panaltyDrag;
                rigid.velocity = Vector2.zero;
            }
        }
        // 타워 상승 종료 시
        else
        {
            rigid.drag = normalDrag;
            rigid.velocity = Vector2.zero;
        }
    }

    public void SetOwner(PlayerController player)
    {
        // owner 참조
        owner = player;
        // 이벤트 구독
        owner.OnPlayerDone += Freeze;
        owner.OnProcessPanalty += ChangeDrag;
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

        // 회전 중이면 return
        if (isRotate)
            return;

        // 현재 회전각 저장
        targetAngle += 90;

        // 회전중 spotlight 비활성화
        spotlightObject.SetActive(false);

        // flag set
        isRotate = true;
    }

    private void Freeze()
    {
        // 조작 불가 상태로 변경
        isControllable = false;

        if (rigid != null)
        {
            // 더 이상 물리처리 하지 않도록 변경
            rigid.simulated = false;

            // velocity 초기화
            rigid.velocity = Vector2.zero;
            rigid.angularVelocity = 0;
        }

        SpriteRenderer sr;
        foreach (GameObject tile in tiles)
        {
            sr = tile.GetComponent<SpriteRenderer>();
            // sprite color를 변경하여 freeze 상태를 확인할 수 있도록 함
            sr.color = Color.gray;
        }
    }

    private void SetSpotlight()
    {
        if (isVertical)
        {
            spotlightObject.transform.localScale = new Vector3(blockSize.y, spotlightObject.transform.localScale.y, spotlightObject.transform.localScale.z);

            // 블럭이 회전될 때 같이 회전되어 방향이 틀어지므로 보정을 위해 재회전 해준다.
            spotlightObject.transform.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else
        {
            spotlightObject.transform.localScale = new Vector3(blockSize.x, spotlightObject.transform.localScale.y, spotlightObject.transform.localScale.z);

            // 블럭이 회전될 때 같이 회전되어 방향이 틀어지므로 보정을 위해 재회전 해준다.
            spotlightObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        // 회전 현황 업데이트
        isVertical = !isVertical;
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

        bool hitWall = false;

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

                if (resultHit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
                {
                    //rigid.rotation = targetAngle;
                    //print($"이동 중에 회전처리{rigid.rotation}");

                    Debug.Log("Hit Wall (MoveRoutine)");
                    hitWall = true;
                    break;
                }

                // 제어권을 해제
                isControllable = false;

                // 최초 충돌 시 충돌 효과음 재생
                SoundManager.Instance.Play(Enums.ESoundType.SFX, SoundManager.SFX_FIRSTCOLLISION);

                // spotlight 비활성화
                spotlightObject.SetActive(false);

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

                // 충돌 시 충돌한 위치를 기준으로 자신에게 반대방향으로 힘을 가한다.
                rigid.AddForceAtPosition(-lastDir * lastAmount, resultHit.point, ForceMode2D.Impulse);

                // 충돌한 상대 블럭에 대해서도 힘을 가해준다.                
                // 블럭과 충돌한 경우 리지드바디는 부모에 있기 때문에 GetComponentInParent를 사용
                Rigidbody2D otherRigid = resultHit.collider.GetComponentInParent<Rigidbody2D>();

                // 결과적으로 상대 오브젝트에서 rigidbody를 찾았으면 힘을 가해준다.
                if (otherRigid != null)
                    otherRigid.AddForceAtPosition(lastDir * lastAmount, resultHit.point, ForceMode2D.Impulse);

                // for debug
                Debug.Log($"Horizontal Collision : {tiles[i].name} > {resultHit.collider.name}");
                Debug.Log($"origin : {(Vector2)tiles[i].transform.position}, direction : {lastDir}");

                yield break;
            }

            if (!hitWall)
            {
                // 순간적으로 이동해야 하므로 position값을 변경한다.
                rigid.position += moveDist;
            }

            // delay만큼 대기
            yield return wsMoveDelay;
        }

        // 코루틴 변수 초기화
        moveRoutine = null;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            //rigid.rotation = targetAngle;
            //print($"벽 충돌 시 회전처리{rigid.rotation}");

            Debug.Log("Hit Wall (OnCollision)");
            return;
        }

        spotlightObject.SetActive(false);

        if (isControllable)
        {
            rigid.simulated = false;
            rigid.velocity = Vector2.zero;
            rigid.angularVelocity = 0;
            rigid.simulated = true;

            // 충돌 시 flag 변경 
            isControllable = false;

            // 최초 충돌 시 충돌 효과음 재생
            SoundManager.Instance.Play(Enums.ESoundType.SFX, SoundManager.SFX_FIRSTCOLLISION);
        }

        // 충돌체 카운트 증가
        collisionCount++;

        // 이미 enter된 상태면 return
        if (isEntered)
            return;

        // enter flag set
        isEntered = true;

        // 이벤트 발생
        OnBlockEntered?.Invoke(this);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (!photonView.IsMine)
            return;

        // 골인한 상태면 exit 처리 안함
        if (owner.IsGoal)
            return;

        // 충돌체 카운트 감소
        collisionCount--;

        // 아직 enter된 상태가 아니면 return
        if (!isEntered)
            return;

        // 충돌중인 물체가 있다면 exit가 아닌것으로 간주
        if (collisionCount > 0)
            return;

        // enter flag set
        isEntered = false;

        // 이벤트 발생
        OnBlockExited?.Invoke(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 블럭 추락 여부 확인
        if (other.CompareTag("FallTrigger"))
        {
            // 이벤트 발생
            OnBlockFallen?.Invoke(this);
            // 바로 삭제
            Destroy(gameObject);
        }
    }

    // 이벤트 해제
    private void OnDestroy()
    {
        if (owner != null)
        {
            owner.OnPlayerDone -= Freeze;
            owner.OnProcessPanalty -= ChangeDrag;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (tiles.Length <= 0)
            return;

        for (int i = 0; i < tiles.Length; i++)
        {
            Gizmos.DrawRay((tiles[i].transform.position+Vector3.left*0.25f) + (Vector3.up * 0.22f), Vector3.left * 0.2f);
            Gizmos.DrawRay((tiles[i].transform.position+Vector3.left*0.25f) + (Vector3.down * 0.22f), Vector3.left * 0.2f);
            Gizmos.DrawRay((tiles[i].transform.position+Vector3.right*0.25f) + (Vector3.up * 0.22f), Vector3.right * 0.2f);
            Gizmos.DrawRay((tiles[i].transform.position+Vector3.right*0.25f) + (Vector3.down * 0.22f), Vector3.right * 0.2f);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rigid.simulated);
            stream.SendNext(rigid.rotation);
            stream.SendNext(rigid.position);
            stream.SendNext(rigid.drag);
        }
        else
        {
            rigid.simulated = (bool)stream.ReceiveNext();
            rigid.rotation = (float)stream.ReceiveNext();
            rigid.position = (Vector2)(stream.ReceiveNext());
            rigid.drag = (float)stream.ReceiveNext();
        }
    }
}
