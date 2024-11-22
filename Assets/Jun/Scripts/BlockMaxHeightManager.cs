using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMaxHeightManager : MonoBehaviour
{
    public float boxCastHeight = 20f;                      
    public LayerMask blockLayer;                     // 블럭이 포함된 레이어
    public Vector2 BoxSize = new Vector2(5f, 1f);

    // 타워에서 가장 높은 블럭을 찾는 함수
    public float GetHighestBlockPosition()
    {
        RaycastHit2D hit = BoxCastToFindHighestBlock();

        if (hit.collider != null)
        {
            // 부모 오브젝트에서 y값을 가져옴 (자식에 Collider가 있는 경우)
            Transform parentTransform = hit.collider.transform.parent;
            if (parentTransform != null)
            {
                return parentTransform.position.y;
            }
        }

        return 0;  // 닿은 블럭이 없으면 매우 낮은 값을 반환(타워의 포지션 값을 받아올 예정)
    }

    private RaycastHit2D BoxCastToFindHighestBlock()
    {
        // 타워의 중심에서 위쪽으로 BoxCast를 쏘기 위한 시작점 설정
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * boxCastHeight;  // 타워 위에서 발사

        // 위에서 아래로 BoxCast를 쏴서 첫 번째로 닿는 블럭을 찾음
        RaycastHit2D hit = Physics2D.BoxCast(rayOrigin, BoxSize, 0f, Vector2.down, boxCastHeight, blockLayer);

        return hit;
    }

    private void OnDrawGizmos()
    {
        // BoxCast의 시작 위치 계산 (위쪽에서 시작)
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * boxCastHeight;

        // Gizmos로 BoxCast의 경로를 시각화하여 디버깅
        Gizmos.color = Color.red;

        // BoxCast의 크기에 맞춰서 박스를 그려줌
        // "rayOrigin"은 BoxCast가 시작하는 위치, Size는 박스의 크기
        Gizmos.DrawWireCube(rayOrigin + Vector2.down * boxCastHeight / 2, BoxSize);
    }

    private void Update()
    {
        float highestBlockY = GetHighestBlockPosition();
        Debug.Log("가장 높은 블럭의 y값: " + highestBlockY);
    }
}
