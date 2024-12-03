using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour
{
    // 버튼 컴포넌트
    Button buttonComponent;

    void Awake()
    {
        // 버튼 컴포넌트를 찾는다.
        buttonComponent = GetComponent<Button>();
    }

    private void Start()
    {
        // 버튼 클릭시 효과음 재생하도록 콜백 추가
        buttonComponent.onClick.AddListener(() => {
            SoundManager.Instance.Play(Enums.ESoundType.SFX, SoundManager.SFX_CLICK);
        });
    }
}
