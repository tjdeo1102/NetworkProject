using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMaxHeightManager : MonoBehaviour
{
    [SerializeField] private float boxCastHeight = 20f;
    [SerializeField] private LayerMask blockLayer;      // 블럭이 포함된 레이어
    [SerializeField] private Vector2 BoxSize = new Vector2(5f, 1f);
    [SerializeField] public float highestPoint = 0f;

    /*private void Start()
    {
        // 모든 블럭 오브젝트를 찾아서 이벤트 구독
        foreach (Blocks blocks in FindObjectsOfType<Blocks>())
        {
            blocks.OnBlockEntered += HandleBlockEntered;
            blocks.OnBlockExited += HandleBlockExited;
        }
    }

    private void HandleBlockEntered()
    {
        highestPoint = GetHighestBlockPosition();
        Debug.Log("가장 높은 블럭의 y값: " + highestPoint);
    }

    private void HandleBlockExited()
    {
        highestPoint = GetHighestBlockPosition();
        Debug.Log("가장 높은 블럭의 y값: " + highestPoint);
    }*/

    // 타워에서 가장 높은 블럭을 찾는 함수
    public float GetHighestBlockPosition()
    {
        RaycastHit2D hit = BoxCastToFindHighestBlock();

        if (hit.collider != null)
        {
            // 부모 오브젝트에서 y값을 가져옴 (자식에 Collider가 있는 경우)
            /*Transform parentTransform = hit.collider.transform.parent;
            if (parentTransform != null)
            {
                return parentTransform.position.y;
            }*/
            return hit.point.y;
        }

        return 0;  // 닿은 블럭이 없으면 매우 낮은 값을 반환(타워의 포지션 값을 받아올 예정)
    }

    private RaycastHit2D BoxCastToFindHighestBlock()
    {
        // 타워의 중심에서 위쪽으로 boxCastHeight만큼 올려 놓고, 아래 방향으로 쏘기 위한 시작점 설정
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * boxCastHeight;  // 타워 위에서 발사

        // 위에서 아래로 BoxCast를 쏴서 첫 번째로 닿는 블럭을 찾음
        RaycastHit2D hit = Physics2D.BoxCast(rayOrigin, BoxSize, 0f, Vector2.down, boxCastHeight, blockLayer);

        return hit;
    }

    private void OnDrawGizmos()
    {
        // BoxCast의 시작 위치 계산
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * boxCastHeight;
        Vector2 direction = Vector2.down; // 박스캐스트 방향
        float distance = boxCastHeight; // 박스캐스트 거리

        // Gizmos 색상 설정
        Gizmos.color = Color.red;

        // 충돌 결과를 검사
        RaycastHit2D hit = BoxCastToFindHighestBlock();

        if (hit.collider != null ) // 충돌 발생 시
        {
            // 충돌 지점까지 레이저 표시
            Gizmos.DrawRay(rayOrigin, direction * hit.distance);

            // 충돌 지점에 정확히 박스를 표시
            Gizmos.DrawWireCube(hit.point, BoxSize);
        }
        else // 충돌 없음
        {
            // 최대 거리까지 레이저 표시
            Gizmos.DrawRay(rayOrigin, direction * distance);
        }
    }
}
