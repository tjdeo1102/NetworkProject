using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameTimer : MonoBehaviour
{
    [Header("타미어 기본 설정")]
    [SerializeField] private float timer;
    [SerializeField] private TextMeshProUGUI timerText;
    public float Timer
    {
        get
        {
            return timer;
        }
        set
        {
            timer = value;
            timerText?.SetText($"{(int)timer+1}");
            if (timer < 0.01f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        timerText?.SetText($"{timer}");
    }

    private void Update()
    {
        // 게임의 일시정지와 관련없이 진행되도록 설정
        Timer -= Time.unscaledDeltaTime;
    }
}
