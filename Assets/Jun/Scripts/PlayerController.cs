using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviourPun
{

    [Header("Block")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private Blocks currentBlock;


    [Header("PlayerStat")]
    [SerializeField] private int curHp;
    [SerializeField] private int maxHp;
    public event Action<int> OnChangeHp;

    [Header("PlayerCheck")]
    public bool IsGoal = false;

    private void Start()
    {
        //PhotonNetwork.ConnectUsingSettings();
        if (spawnPoint == null || blockPrefabs == null || blockPrefabs.Length == 0)
        {
            Debug.LogError("스폰지점, 블럭프리팹이 잘못 설정 되었습니다.");
            return;
        }

        //Start가 아닌 방에 참가 했을 때 스폰하는 것으로 수정 예정
        SpawnBlock();
        /*if (photonView.IsMine)
        {

            if (spawnPoint == null || blockPrefabs == null || blockPrefabs.Length == 0)
            {
                Debug.LogError("스폰지점, 블럭프리팹이 잘못 설정 되었습니다.");
                return;
            }

            //Start가 아닌 방에 참가 했을 때 스폰하는 것으로 수정 예정
            SpawnBlock();

            currentBlock.OnBlockEntered += BlockEntered;
        }*/
    }

    private void OnDestroy()
    {
        if (photonView.IsMine)
        {
            //Block.OnBlockEntered -= BlockEntered;
        }
    }

    private void Update()
    {
        if (/*photonView.IsMine && */currentBlock != null )
        {
            PlayerInput();
        }
    }

    private void PlayerInput()
    {
        if (IsGoal)
            return;

        if (Input.GetKey(KeyCode.DownArrow))
            currentBlock.Down();

        if (Input.GetKey(KeyCode.U))
        {
            currentBlock.Push(Vector2.left);
        }
        else if (Input.GetKey(KeyCode.I))
        {
            currentBlock.Push(Vector2.right);
        }
        else
        {
            currentBlock.Move(Vector2.right * Input.GetAxisRaw("Horizontal"));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentBlock.Rotate();
        }
    }
    private void BlockEntered()
    {
        // 기존 블럭의 제어 해제
        if (currentBlock != null)
        {
            currentBlock = null;
        }

        // 새로운 블럭 생성
        SpawnBlock();
    }

    public void SpawnBlock()
    {
        /*if (!PhotonNetwork.IsConnected) 
        {
            Debug.LogError("Photon Network에 연결되어 있지 않습니다. 블럭을 생성할 수 없습니다.");
            return;
        }*/

        int randomIndex = Random.Range(0, blockPrefabs.Length);
        //SpawnPoint는 플레이어 위치 or 블럭의 쌓인 y값 최대치 or 타워의 높이 상대치를 통해 정해질 예정
        GameObject newBlock = Instantiate(blockPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);
        //GameObject newBlock = PhotonNetwork.Instantiate(blockPrefabs[randomIndex].name, spawnPoint.position, Quaternion.identity);
        currentBlock = newBlock.GetComponent<Blocks>();
        //currentBlock.OnDisableControl += BlockEntered;
    }

    public void TakeDamage(int damage)
    {
        curHp -= damage;
        Debug.Log($"현재 체력 : {curHp}");

        OnChangeHp?.Invoke(curHp);

        // 플레이어의 체력이 0 이하가 되면 그 이후의 상황을 처리
        if (curHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //플레이어 죽음 애니메이션 추가
        //한명의 플레이어가 생존할 때 까지 대기
    }

    public void ReachGoal()
    {
        IsGoal = true;
        //골인 했을 때 추가적인 구현
        //ex) 원작 게임처럼 큰 나무집이 떨어져 1등이 엔딩을 장식할 수 있도록
    }
}
