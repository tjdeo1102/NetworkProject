using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTester : MonoBehaviour
{
    [SerializeField] AudioClip[] bgms;
    [SerializeField] AudioClip[] sfxs;

    [SerializeField] UI_SoundTest ui;

    private void Awake()
    {
        ui.AddSound(Enums.ESoundType.BGM, bgms);
        ui.AddSound(Enums.ESoundType.SFX, sfxs);
    }
}
