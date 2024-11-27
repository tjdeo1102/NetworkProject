using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject player;         // 소환된 플레이어
    [SerializeField] private BlockMaxHeightManager blockMaxHeightManager; // BlockMaxHeightManager를 참조

    private float previousHighestBlockY = 0f;  // 이전 최대 높이(처음 시작할 위치로 지정할 예정)

    public void SetMaxHeightManager(BlockMaxHeightManager blockMaxHeightManager)
    {
        this.blockMaxHeightManager = blockMaxHeightManager;
        if (blockMaxHeightManager != null)
        {
            blockMaxHeightManager.OnHeightChanged += SetPlayerPosition; // 이벤트 구독
        }
    }


    private void Start()
    {

    }

    private void OnDestroy()
    {
        if (blockMaxHeightManager != null)
        {
            blockMaxHeightManager.OnHeightChanged -= SetPlayerPosition; // 이벤트 구독 해제
        }
    }

    // 플레이어 위치를 블록의 최대 높이에 맞게 조정하는 함수
    public void SetPlayerPosition(float highestPoint)
    {
        // 플레이어의 위치를 가장 높은 블록 위치로 조정
        Vector2 playerNewPosition = new Vector2(player.transform.position.x, highestPoint);

        // 플레이어의 위치 업데이트
        player.transform.position = playerNewPosition;
    }
}
