using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    public AudioClip mainButtonSound;
    public AudioClip subButtonSound;

    [Header("Audio Sources")]
    public AudioSource musicAudioSource;
    public AudioSource sfxAudioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(AudioClip clip)
    {
        musicAudioSource.clip = clip;
        musicAudioSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxAudioSource.clip = clip;
        sfxAudioSource.Play();
    }

    public void PlayMainButtonSound()
    {
        PlaySFX(mainButtonSound);
    }

    public void PlaySubButtonSound()
    {
        PlaySFX(subButtonSound);
    }

    public void StopMusic()
    {
        musicAudioSource.Stop();
    }

    public void StopSFX()
    {
        sfxAudioSource.Stop();
    }

    public void StopAllAudio()
    {
        StopMusic();
        StopSFX();
    }
}