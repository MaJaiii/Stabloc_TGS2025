using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [Header("Audio Pool Settings")]
    [SerializeField] private int poolSize = 10;

    private AudioSource bgmSource;
    private AudioSource[] sfxPool;
    private int nextIndex = 0;

    // --- Beat Sync ---
    public event Action<int> OnBeat;   // Fires every beat (1, 2, 3...)
    public event Action<int> OnBar;    // Fires every 4 beats (1 bar, 2 bars...)

    [HideInInspector]public float bpm = 120f;
    private float secPerBeat;
    private double nextBeatTime;
    private int beatCount = 0;
    string lastScene;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        // Init pool
        sfxPool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            sfxPool[i] = gameObject.AddComponent<AudioSource>();
            sfxPool[i].playOnAwake = false;
        }

        // Dedicated BGM source
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = false;
        lastScene = SceneManager.GetActiveScene().name;
    }

    private void Update()
    {
        if (lastScene != SceneManager.GetActiveScene().name)
        {
            lastScene = SceneManager.GetActiveScene().name;
            bgmSource.Stop();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log(bgmSource.clip != null);
            bgmSource.Play();
        }

        for (int i = 0;i < sfxPool.Length; i++)
        {
            var clip = sfxPool[i];
            if (clip == null) continue;
            if (!clip.isPlaying && !clip.loop)
            {
                clip.clip = null;
            }
        }
        
        if (bgmSource.isPlaying)
        {
            double dspTime = AudioSettings.dspTime;

            if (dspTime >= nextBeatTime)
            {
                beatCount++;
                OnBeat?.Invoke(beatCount);

                if (beatCount % 4 == 0)
                    OnBar?.Invoke(beatCount / 4);

                nextBeatTime += secPerBeat;
            }
        }
        else if (bgmSource.clip != null && !bgmSource.isPlaying)
        {
            PlayBGM(bgmSource.clip, bgmSource.volume * 100, bgmSource.pitch, bpm);
        }
    }

    // --- BGM ---
    public void PlayBGM(AudioClip clip, float volume = 1f, float pitch = 1f, float bpm = 120f)
    {
        volume = volume == 0 ? .5f : volume / 2;
        this.bpm = bpm;
        secPerBeat = 60f / bpm;
        beatCount = 0;

        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.pitch = pitch;
        bgmSource.loop = true;

        double startTime = AudioSettings.dspTime + 0.1f; // small delay for sync
        bgmSource.Play();

        nextBeatTime = startTime + secPerBeat;
    }



	public void StopBGM()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
    }


    // --- SFX ---
    public AudioSource PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
    {
        Debug.Log($"Play SFX: {clip.name} with volume {volume}");
        volume /= 2;
        AudioSource src = GetNextSource();
        src.clip = clip;
        src.volume = volume;
        src.pitch = pitch;
        src.loop = loop;
        src.Play();
        return src;
    }

    public void StopSFX (AudioSource src)
    {
        src.Stop();
        src.clip = null;
    }

    private AudioSource GetNextSource()
    {
        for (int i = 0; i < sfxPool.Length; i++)
        {
            if (sfxPool[i].clip == null)
            {
                nextIndex = i;
                break;
            }
        }
        AudioSource src = sfxPool[nextIndex];
        return src;
    }
}
