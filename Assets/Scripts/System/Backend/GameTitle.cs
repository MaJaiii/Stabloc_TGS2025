using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;
using Unity.VisualScripting;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameTitle : MonoBehaviour
{
    [Header("UI Element")]
    [SerializeField] RectTransform logo_Base_L;
    [SerializeField] RectTransform logo_Base_R;
    [SerializeField] GameObject logo;
    [SerializeField] GameObject input_Drop;
    [SerializeField] Image blank;

    [Header("Sprite Object")]
    [SerializeField] Sprite[] input_Drop_sprites;
    [SerializeField] Image input_Drop_image;


    bool isReadyToPlay;
    bool isDropping;

    float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isReadyToPlay = false;  
        isDropping = false;
        timer = 0;

        input_Drop.SetActive(false);
        AudioController.Instance.OnBar += ShowDrop;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (!isReadyToPlay && GameStatus.gameState == GAME_STATE.GAME_READYTOPLAY)
        {
			isReadyToPlay = true;

		}

        if (!isDropping && GameStatus.gameState == GAME_STATE.GAME_INGAME)
        {
            isDropping = true;
            input_Drop.AddComponent<Rigidbody2D>().gravityScale = 10000;

            logo_Base_L.DORotate(new Vector3(0, 0, -80), .2f);
            logo_Base_R.DORotate(new Vector3(0, 0, 80), .2f);
            for (int i = 0; i < logo.transform.childCount; i++)
            {
                logo.transform.GetChild(i).GetComponent<Rigidbody2D>().gravityScale = 10000;
            }
        }

        if (GameStatus.gameState == GAME_STATE.GAME_INGAME)
        {
			Destroy(gameObject);
		}

        if (timer > 60)
        {
            timer = 0;
            GameStatus.gameState = GAME_STATE.GAME_TITLE;
            AudioController.Instance.StopBGM();
            blank.DOFade(1, .5f).OnComplete(() => SceneManager.LoadScene("DemoPlay"));
        }
    }


    void ShowDrop(int bar)
    {
        if (!isReadyToPlay || bar < 1) return;
		input_Drop.SetActive(true);

		AudioController.Instance.OnBar -= ShowDrop;
	}
}
