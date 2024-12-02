using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SoundTest : MonoBehaviour
{
    [SerializeField] TMP_Text txtBgm;
    [SerializeField] Button buttonPrefab;
    [SerializeField] Button btnStop;
    [SerializeField] RectTransform bgmContentArea;
    [SerializeField] RectTransform sfxContentArea;

    private void Start()
    {
        btnStop.onClick.AddListener(() =>
        {
            SoundManager.Instance.Stop(Enums.ESoundType.BGM);
            txtBgm.text = "Select BGM : ";
        });
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
                    SoundManager.Instance.Play(Enums.ESoundType.BGM, clip);
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
