using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Procedural;

public class PlayerController : MonoBehaviourPun
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private Block currentBlock;


    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        if (photonView.IsMine)
        {

            if (spawnPoint == null || blockPrefabs == null || blockPrefabs.Length == 0)
            {
                Debug.LogError("스폰지점, 블럭프리팹이 잘못 설정 되었습니다.");
                return;
            }

            //Start가 아닌 방에 참가 했을 때 스폰하는 것으로 수정 예정
            SpawnBlock();

            //Block.OnBlockEntered += BlockEntered;
        }
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
        if (photonView.IsMine && currentBlock != null)
        {
            PlayerInput();
        }
    }

    private void PlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //currentBlock.Move();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //currentBlock.Move();
        }


        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
        {
            //currentBlock.Rotate(90);
        }


        if (Input.GetKey(KeyCode.DownArrow))
        {
            //currentBlock.Down();
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
        if (!PhotonNetwork.IsConnected) 
        {
            Debug.LogError("Photon Network에 연결되어 있지 않습니다. 블럭을 생성할 수 없습니다.");
            return;
        }

        int randomIndex = Random.Range(0, blockPrefabs.Length);
        GameObject newBlock = PhotonNetwork.Instantiate(blockPrefabs[randomIndex].name, spawnPoint.position, Quaternion.identity);
        currentBlock = newBlock.GetComponent<Block>();
    }
}
