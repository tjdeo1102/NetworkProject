using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer_Network : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private Blocks currentBlock;

    [SerializeField] Blocks[] testBlockPrefabs;

    private Blocks[] testBlockInstances;

    private int blockIndex = -1;

    [SerializeField] private int blockCount = 0;

    private void Start()
    {
        object[] data = photonView.InstantiationData;
        gameObject.name = $"{data[0]}";

        testBlockInstances = new Blocks[testBlockPrefabs.Length];
    }

    private void ChangeBlock()
    {
        blockIndex++;

        if (blockIndex >= testBlockPrefabs.Length)
            blockIndex = 0;

        if (testBlockInstances[blockIndex] != null)
        {
            currentBlock = testBlockInstances[blockIndex];
        }
        else
        {
            GameObject blockObj = PhotonNetwork.Instantiate(testBlockPrefabs[blockIndex].name, new Vector3(Random.Range(-3, 3), 3.75f, 0), Quaternion.identity);
            testBlockInstances[blockIndex] = blockObj.GetComponent<Blocks>();
            currentBlock = testBlockInstances[blockIndex];
            currentBlock.OnBlockEntered += BlockEnter;
            currentBlock.OnBlockExited += BlockExit;
            Debug.Log($"<color=yellow>{photonView.Owner.NickName} Instantiated Block</color>");
        }    

        if (!currentBlock.gameObject.activeSelf)
            currentBlock.gameObject.SetActive(true);
    }

    private void BlockEnter(Blocks block)
    {
        blockCount++;
    }

    private void BlockExit(Blocks block)
    {
        blockCount--;
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ChangeBlock();
        }

        if (currentBlock == null)
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(blockCount);
        }
        else
        {
            blockCount = (int)stream.ReceiveNext();
        }
    }
}
