using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera mainCamera;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float maxZoomOut = 10f;
    [SerializeField] private float minZoomIn = 5f;
    [SerializeField] private List<Player> players;
    [SerializeField] private GameState gameState;
    public float cameraSize;

    [SerializeField] private float highestPlayerHeight = 0f;
    [SerializeField] private float lowestPlayerHeight = 0f;

    private void Update()
    {
        GetHighestTowerHeight();
        ModulateCameraZoom();
        //Debug.Log($"<color=Greed>Camera Pos: {transform.position} </color>");
    }

    public void GetHighestTowerHeight()
    {
        // 예외 처리
        if (gameState == null || gameState.towerObjectDic == null) return;

        // 초기화
        highestPlayerHeight = float.MinValue;
        lowestPlayerHeight = float.MaxValue;

        //플레이어 GetCommponent

        foreach (var player in gameState.towerObjectDic.Values)
        {
            var blockMaxHeightManager = player.GetComponent<BlockMaxHeightManager>();
            if (blockMaxHeightManager != null)
            {
                float height = blockMaxHeightManager.highestPoint;

                if (height > highestPlayerHeight)
                {
                    highestPlayerHeight = height;
                }
                if ( height < lowestPlayerHeight)
                {
                    lowestPlayerHeight = height;
                }
            }
        }
    }

    private void ModulateCameraZoom()
    {
        // 높이 차이가 작으면 줌인, 크면 줌아웃
        float heightDifference = highestPlayerHeight - lowestPlayerHeight;
        float targetZoom = Mathf.Lerp(minZoomIn, maxZoomOut, heightDifference / 10f); // 높이에 비례하여 줌

        // 현재 카메라 줌을 목표 값에 맞게 천천히 변화
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
        
        // 현재 카메라 사이즈를 변수에 저장
        cameraSize = Camera.main.orthographicSize;
    }
}
