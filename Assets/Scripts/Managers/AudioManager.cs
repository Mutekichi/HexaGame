using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    public AudioClip mainButtonSound;
    public AudioClip subButtonSound;
    public AudioClip tileFlipSound;
    public AudioClip StageClearSound;

    [Header("Audio Sources")]
    public AudioSource musicAudioSource;
    public AudioSource sfxAudioSource;

    [Header("SFX Volumes")]
    [Range(0f, 1f)]
    public float mainSfxVolume = 1f;
    [Range(0f, 1f)]
    public float subSfxVolume = 1f;
    [Range(0f, 1f)]
    public float tileFlipSfxVolume = 1f;
    [Range(0f, 1f)]
    public float stageClearSfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicAudioSource == null)
        {
            musicAudioSource = gameObject.AddComponent<AudioSource>();
            musicAudioSource.loop = true;
        }

        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        Debug.Log("Playing music");
        if (clip != null)
        {
            musicAudioSource.clip = clip;
            musicAudioSource.Play();
            Debug.Log("Music is playing: " + musicAudioSource.isPlaying);
        }
        else
        {
            Debug.LogWarning("Clip is null!");
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Clip is null!");
        }
    }

    public void PlayMainButtonSound()
    {
        if (mainButtonSound != null)
        {
            sfxAudioSource.PlayOneShot(mainButtonSound, mainSfxVolume);
        }
        else
        {
            Debug.LogWarning("Main Button Sound is not assigned!");
        }
    }

    public void PlaySubButtonSound()
    {
        if (subButtonSound != null)
        {
            sfxAudioSource.PlayOneShot(subButtonSound, subSfxVolume);
        }
        else
        {
            Debug.LogWarning("Sub Button Sound is not assigned!");
        }
    }

    public void PlayTileFlipSound()
    {
        if (tileFlipSound != null)
        {
            sfxAudioSource.PlayOneShot(tileFlipSound, subSfxVolume);
        }
        else
        {
            Debug.LogWarning("Tile Flip Sound is not assigned!");
        }
    }

    public void PlayStageClearSound()
    {
        if (StageClearSound != null)
        {
            sfxAudioSource.PlayOneShot(StageClearSound, stageClearSfxVolume);
        }
        else
        {
            Debug.LogWarning("Stage Clear Sound is not assigned!");
        }
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