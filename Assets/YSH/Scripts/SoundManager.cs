using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static Enums;

public class SoundManager : Singleton<SoundManager>
{
    const string MIXER_PARAM_VOLUME = "_Volume";
    const string MIXER_PARAM_PITCH = "_Pitch";
    const string MIXER_PARAM_PITCHSHIFT = "_PitchShift";

    [SerializeField] AudioMixer mixer;

    AudioSource[] audioSources;
    AudioMixerGroup[] mixerGroup;

    protected override void Init()
    {
        audioSources = new AudioSource[(int)ESoundType.Length];
        mixerGroup = mixer.FindMatchingGroups("Master");

        GameObject temp;
        for (int i = 0; i < audioSources.Length; i++)
        {
            temp = new GameObject($"{(ESoundType)i}");
            audioSources[i] = temp.AddComponent<AudioSource>();
            audioSources[i].transform.SetParent(transform);
            audioSources[i].outputAudioMixerGroup = mixerGroup[i + 1];
        }

        // BGM
        audioSources[(int)ESoundType.BGM].loop = true;
        // SFX
        audioSources[(int)ESoundType.SFX].playOnAwake = false;
        audioSources[(int)ESoundType.SFX].loop = false;
    }

    public void Play(ESoundType playType, AudioClip clip, float tempo = 1.0f)
    {
        AudioSource audioSource = audioSources[(int)playType];

        switch(playType)
        {
            case ESoundType.BGM:
                {
                    if (audioSource.isPlaying)
                        audioSource.Stop();

                    ChangeTempo(ESoundType.BGM, tempo);

                    audioSource.clip = clip;
                    audioSource.Play();
                }
                break;
            case ESoundType.SFX:
                {
                    audioSource.PlayOneShot(clip);
                }
                break;
        }
    }

    public void Play(ESoundType playType, in string clipName, float tempo = 1.0f)
    {
        AudioClip clip = LoadAudio(clipName);
        if (clip == null)
        {
            Debug.Log($"[SOUND] Play Failed... / {clipName}");
            return;
        }

        Play(playType, clip, tempo);
    }

    public void PlayClipAtPoint(AudioClip clip, Vector3 pos)
    {
        AudioSource.PlayClipAtPoint(clip, pos);
    }

    public void PlayClipAtPoint(in string clipName, Vector3 pos)
    {
        AudioClip clip = LoadAudio(clipName);
        if (clip == null)
        {
            Debug.Log($"[SOUND] PlayClipAtPoint Failed... / {clipName}");
            return;
        }

        AudioSource.PlayClipAtPoint(clip, pos);
    }

    public AudioClip LoadAudio(string clipName)
    {
        AudioClip clip = null;
        clip = ResourceManager.Instance.Load<AudioClip>($"Audio/{clipName}");
        return clip;
    }

    public void Stop(ESoundType playType)
    {
        audioSources[(int)playType].Stop();
    }

    public void ChangeVolume(ESoundType playType, float volume)
    {
        AudioSource audioSource = audioSources[(int)playType];
        audioSource.outputAudioMixerGroup.audioMixer.SetFloat($"{playType}{MIXER_PARAM_VOLUME}", volume);
    }

    public void ChangeTempo(ESoundType playType, float tempo)
    {
        AudioSource audioSource = audioSources[(int)playType];
        // Change Tempo
        // 오디오 소스의 pitch만 바꾸면 음정이 변하므로 mixer의 pitch shifter를 같이 사용하여 보정
        audioSource.pitch = tempo;
        audioSource.outputAudioMixerGroup.audioMixer.SetFloat($"{playType}{MIXER_PARAM_PITCHSHIFT}", 1.0f / tempo);
    }
}
