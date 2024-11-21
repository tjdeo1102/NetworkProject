using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Procedural;

public class PlayerController : MonoBehaviourPun
{
    private Transform spawnPoint;
    private GameObject[] blockPrefabs;
    private Block currentBlock;

    private void Start()
    {
        if (photonView.IsMine)
        {
            SpawnBlock();
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

    public void SpawnBlock()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        int randomIndex = Random.Range(0, blockPrefabs.Length);
        GameObject newBlock = PhotonNetwork.Instantiate(blockPrefabs[randomIndex].name, spawnPoint.position, Quaternion.identity);
        currentBlock = newBlock.GetComponent<Block>();
    }
}
