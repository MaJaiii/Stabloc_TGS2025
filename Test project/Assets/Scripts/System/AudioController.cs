using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [Header("Settings")]
    [SerializeField] private int poolSize = 10;

    private List<AudioSource> sfxPool = new List<AudioSource>();
    private int nextIndex = 0;

    private AudioSource bgmSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        // Create pooled SFX sources
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            sfxPool.Add(src);
        }

        // Create dedicated BGM source
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
    }

    //  --- BGM Methods ---
    public void PlayBGM(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        volume /= 100;
        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.pitch = pitch;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PauseBGM(bool pause)
    {
        if (pause) bgmSource.Pause();
        else bgmSource.UnPause();
    }

    //  --- SFX Methods ---
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        volume /= 100;
        AudioSource src = GetNextSource();
        src.clip = clip;
        src.volume = volume;
        src.pitch = pitch;
        src.loop = false;
        src.Play();

        // Auto-release when clip ends
        StartCoroutine(ReleaseAfterPlay(src));
    }

    public AudioSource PlaySFXLoop(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        volume /= 100;
        AudioSource src = GetNextSource();
        src.clip = clip;
        src.volume = volume;
        src.pitch = pitch;
        src.loop = true;
        src.Play();
        return src; // return reference so caller can stop later
    }

    public void StopSFX(AudioSource src)
    {
        if (src != null && src.isPlaying)
            src.Stop();
    }

    // --- Helpers ---
    private AudioSource GetNextSource()
    {
        AudioSource src = sfxPool[nextIndex];
        nextIndex = (nextIndex + 1) % sfxPool.Count;
        return src;
    }

    private IEnumerator ReleaseAfterPlay(AudioSource src)
    {
        yield return new WaitWhile(() => src.isPlaying);
        src.clip = null; // optional cleanup
    }
}
