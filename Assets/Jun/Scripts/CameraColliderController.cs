using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraColliderController : MonoBehaviour
{
    public Camera mainCamera; // 카메라를 Inspector에서 할당하거나 자동으로 찾습니다.

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // 카메라가 할당되지 않았으면 MainCamera를 자동으로 찾습니다.
        }

        if (mainCamera == null)
        {
            Debug.LogError("메인 카메라가 등록되지 않았습니다.");
            return;
        }
    }

    private void Update()
    {
        SetColliderToCamera();
    }

    private void SetColliderToCamera()
    {
        // 카메라 크기 계산
        if (!mainCamera.orthographic)
        {
            Debug.LogError("카메라 Projection이 Orthographic이 아닙니다.");
            return;
        }

        float cameraHeight = mainCamera.orthographicSize * 2f; // 카메라의 세로 크기
        float cameraWidth = cameraHeight * mainCamera.aspect;  // 카메라의 가로 크기

        // 자식 오브젝트에 있는 콜라이더 가져오기
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogError("이 오브젝트에 Collider2D 컴포넌트가 없습니다.");
            return;
        }

        boxCollider.size = new Vector2(cameraWidth, cameraHeight);


    }
}
