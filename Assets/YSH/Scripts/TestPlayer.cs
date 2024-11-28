using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TestPlayer : MonoBehaviourPun
{
    [SerializeField] private Blocks currentBlock;

    [SerializeField] private Blocks[] testBlocks;

    private int blockIndex = -1;

    bool isGoal = false;

    bool IsGoal 
    { 
        get { return isGoal; }
        set 
        {
            isGoal = value;
            if (isGoal == true)
                OnPlayerDone?.Invoke(); 
        } 
    }

    public UnityAction OnPlayerDone; 

    //private void Awake()
    //{
    //    if (testBlocks.Length <= 0)
    //        return;

    //    foreach (Blocks block in testBlocks)
    //    {
    //        block.SetOwner(this);
    //    }
    //}

    private void Start()
    {
        ChangeBlock();
    }

    // for test
    private void ChangeBlock()
    {
        if (testBlocks.Length <= 0)
            return;

        blockIndex++;

        if (blockIndex >= testBlocks.Length)
            blockIndex = 0;

        currentBlock = testBlocks[blockIndex];

        if (!currentBlock.gameObject.activeSelf)
            currentBlock.gameObject.SetActive(true);
    }

    void Update()
    {
        if (isGoal)
            return;

        // for test
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ChangeBlock();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            IsGoal = true;
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
}
