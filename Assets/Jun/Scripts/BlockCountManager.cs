using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCountManager : MonoBehaviour
{
    private Blocks blocks;
    public int BlockCount { get; private set; } = 0;  // 블럭 개수

    public event System.Action<int> OnChangeBlockCount;

    private void OnEnable()
    {
        // 이벤트 구독
        //blocks.OnBlockEntered += BlockPlaced;
        //blocks.OnBlockExited += BlockRemoved;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        //blocks.OnBlockEntered -= BlockPlaced;
        //blocks.OnBlockExited -= BlockRemoved;
    }

    private void BlockPlaced(Block block)
    {
        BlockCount++;
        Debug.Log($"블럭 개수: {BlockCount}");

        OnChangeBlockCount?.Invoke(BlockCount);
    }

    private void BlockRemoved(Block block)
    {
        BlockCount--;
        Debug.Log($"블럭 개수: {BlockCount}");

        OnChangeBlockCount?.Invoke(BlockCount);
    }
}
