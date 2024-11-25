using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private GameObject player;         // 소환된 플레이어
    [SerializeField] private float playerOffsetX = -3f; // 왼쪽에 배치할 오프셋
    [SerializeField] private BlockMaxHeightManager blockMaxHeightManager; // BlockMaxHeightManager를 참조

    private float previousHighestBlockY = 0f;  // 이전 최대 높이(처음 시작할 위치로 지정할 예정)

    private void Update()
    {
        if (player != null)
        {
            // 가장 높은 블럭의 y값을 BlockMaxHeightManager에서 가져오기
            float currentHighestBlockY = blockMaxHeightManager.GetHighestBlockPosition();

            // 최대 높이가 변경되었을 때만 위치 업데이트
            if (currentHighestBlockY != previousHighestBlockY)
            {
                SetPlayerPosition(currentHighestBlockY);  // 위치 업데이트
                previousHighestBlockY = currentHighestBlockY; // 이전 높이 갱신
            }
        }
    }

    // 플레이어 위치를 블록의 최대 높이에 맞게 조정하는 함수
    public void SetPlayerPosition(float highestBlockY)
    {
        // 플레이어의 위치를 가장 높은 블록 위치에서 왼쪽으로 오프셋을 적용하여 조정
        Vector2 playerNewPosition = new Vector2(player.transform.position.x + playerOffsetX, highestBlockY);

        // 플레이어의 위치 업데이트
        player.transform.position = playerNewPosition;
    }
}
