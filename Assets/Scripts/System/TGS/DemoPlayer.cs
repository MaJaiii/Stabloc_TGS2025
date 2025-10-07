using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class DemoPlayer : MonoBehaviour
{

    bool isPressed = false;
    RawImage image;
    [SerializeField] VideoPlayer player;
    [SerializeField] VideoClip[] clips;
    [SerializeField] TextMeshProUGUI textMeshPro;

    int index = 0;
    float volume = .8f;

    float timer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image = GetComponent<RawImage>();
        player.clip = clips[Random.Range(0, clips.Length)];
        player.SetDirectAudioVolume(0, volume);
        player.Play();
        player.isLooping = true;
    }


    void OnVideoEnd()
    {
        {
            isPressed = true;
            player.Pause();
            textMeshPro.text = "";
            AudioController.Instance.StopBGM();
            image.DOFade(0, .5f).OnComplete(() => SceneManager.LoadScene("LogoScene"));
        }
    }
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            index++;
            if (index == clips.Length) index = 0;
            player.Stop();
            player.clip = clips[(int)index];
            player.Play();
            return;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            volume -= .05f;
            if (volume < 0) volume = 0;
            player.SetDirectAudioVolume(0, volume);
            return;
        }
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            volume += .05f;
            if (volume > 1) volume = 1; 
            player.SetDirectAudioVolume(0, volume);
            return;
        }

        if (!isPressed && timer >= 180)
        {
            OnVideoEnd();
        }

        InputSystem.onAnyButtonPress.CallOnce(ctrl =>
        {
            if (!isPressed && ctrl.device is Gamepad)
            {
                isPressed = true;
                player.Pause();
                textMeshPro.text = "";
                image.DOFade(0, .5f).OnComplete(() => SceneManager.LoadScene("LogoScene"));
            }
        });

    }
}
