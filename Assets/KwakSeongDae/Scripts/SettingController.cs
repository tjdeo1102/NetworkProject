using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingController : MonoBehaviour
{
    [Header("설정 윈도우 요소")]
    public Slider BGMSlider;
    public Slider SFXSlider;

    private void OnEnable()
    {
        BGMSlider.onValueChanged.AddListener(BGMVolumeChanged);
        SFXSlider.onValueChanged.AddListener(SFXVolumeChanged);
        float vol = 0f;
        SoundManager.Instance.GetVolume(Enums.ESoundType.BGM, out vol);
        BGMSlider.value = vol;
        SoundManager.Instance.GetVolume(Enums.ESoundType.SFX, out vol);
        SFXSlider.value = vol;
    }

    private void BGMVolumeChanged(float value)
    {
        SoundManager.Instance.ChangeVolume(Enums.ESoundType.BGM, value);
    }
    private void SFXVolumeChanged(float value)
    {
        SoundManager.Instance.ChangeVolume(Enums.ESoundType.SFX, value);
    }

    private void OnDisable()
    {
        BGMSlider.onValueChanged.RemoveListener(BGMVolumeChanged);
        SFXSlider.onValueChanged.RemoveListener(SFXVolumeChanged);
    }
}
