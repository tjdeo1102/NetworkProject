using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SoundTest : MonoBehaviour
{
    [SerializeField] TMP_Text txtBgm;
    [SerializeField] TMP_Text txtTempo;
    [SerializeField] Button buttonPrefab;
    [SerializeField] Button btnStop;
    [SerializeField] Button btnReset;
    [SerializeField] RectTransform bgmContentArea;
    [SerializeField] RectTransform sfxContentArea;
    [SerializeField] Slider tempoSlider;

    private void Start()
    {
        btnStop.onClick.AddListener(() =>
        {
            SoundManager.Instance.Stop(Enums.ESoundType.BGM);
            txtBgm.text = "Select BGM : ";
        });

        btnReset.onClick.AddListener(() =>
        {
            tempoSlider.value = 1.0f;
        });

        tempoSlider.onValueChanged.AddListener((value) =>
        {
            txtTempo.text = $"Target Tempo : {value:F2}";
            SoundManager.Instance.ChangeTempo(Enums.ESoundType.BGM, value);
        });

        txtTempo.text = $"Target Tempo : {tempoSlider.value:F2}";
    }

    public void AddSound(Enums.ESoundType eType, AudioClip[] sounds)
    {
        Button btn;
        foreach (AudioClip clip in sounds)
        {
            btn = Instantiate(buttonPrefab);
            btn.GetComponentInChildren<TMP_Text>().text = clip.name;
            if (eType == Enums.ESoundType.BGM)
            {
                btn.onClick.AddListener(() =>
                {
                    SoundManager.Instance.Play(Enums.ESoundType.BGM, clip, tempoSlider.value);
                    txtBgm.text = $"Select BGM : {clip.name}";
                });
                btn.transform.SetParent(bgmContentArea);
            }
            else
            {
                btn.onClick.AddListener(() =>
                {
                    SoundManager.Instance.Play(Enums.ESoundType.SFX, clip);
                });
                btn.transform.SetParent(sfxContentArea);
            }
        }
    }
}
