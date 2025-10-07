using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{

    float timer = 0;

    public bool isGameOver = false;
    bool isPressed = false;

    [SerializeField]
    BlockAction blockAction;
    [SerializeField]
    ActionTimer actionTimer;
    [SerializeField]
    ScoreSystem scoreSystem;

    [Header("Canvas_CameraOverlay")]
    [SerializeField]
    Image background;
    [SerializeField]
    Transform title;
    [SerializeField]
    TextMeshProUGUI scoreText;
    [SerializeField]
    TextMeshProUGUI blockCount;
    [SerializeField]
    TextMeshProUGUI height;
    [SerializeField]
    TextMeshProUGUI restart;
    [SerializeField]
    Image blank;

    [Header("Audio")]
    [SerializeField] Animator bgm;
    [SerializeField] AudioClip gameOver_SE;
    [SerializeField] AudioClip gameOver_scoreSE;
    [SerializeField] AudioClip gameOver_resultSE;
    [SerializeField] AudioClip gameOver_typingSE;

	// ’Ç‰Á9/17---------------------------------------
	[SerializeField]
	GameObject titleGameOver;
    // ’Ç‰Á---------------------------------------


    GameSettings settings;

	private void Start()
    {
        settings = CsvSettingsLoader.Load();

        Color alpha = Color.white;
        Color black = Color.black;

        alpha.a = 0;
        black.a = 0;

        background.color = black;
        foreach(Transform obj in title) obj.GetComponent<Image>().color = alpha;
        blockCount.color = alpha;
        height.color = alpha;
        scoreText.color = alpha;
        restart.gameObject.SetActive(false);
        blank.color = black;
    }

    private void Update()
    {
        if (isGameOver && GameStatus.gameState == GAME_STATE.GAME_INGAME)
        {
            timer += Time.deltaTime;
            if (timer > 1)
            {
                timer = Mathf.NegativeInfinity;
                actionTimer.isGameOver = true;
                GameStatus.gameState = GAME_STATE.TRANSITIONING; // Optional: prevent multiple calls
                scoreSystem.ProductScore(Mathf.CeilToInt((blockAction.height + 1) * .9f + (actionTimer.blockCount) * .1f));
                StartCoroutine(GameOverSequence()); // Run the proper coroutine
                bgm.SetBool("isPitchDown", true);
            }
        }

        if (GameStatus.gameState == GAME_STATE.GAME_OVER)
        {
            InputSystem.onAnyButtonPress.CallOnce( ctrl =>           
            {
                if (!isPressed)
                {
                    isPressed = true;
                    StartCoroutine(Restart());
                }
            });

        }
    }

    IEnumerator Restart()
    {
        while (blank.color.a < 1)
        {
            Color color = blank.color;
            color.a += .1f;
            blank.color = color;
            yield return new WaitForSeconds(.1f);
        }
        yield return new WaitForSeconds(.5f);
        SceneManager.LoadScene("SampleScene_Ma");

    }

    IEnumerator GameOverSequence()
    {

        // Fade background
        yield return StartCoroutine(changeAlpha(background, .8f, 0.5f));
        AudioController.Instance.StopBGM();
        yield return new WaitForSeconds(1);

        //// Fade in each title object one by one
        //foreach (Transform obj in title)
        //{
        //    Image img = obj.GetComponent<Image>();
        //    if (img != null)
        //    {
        //        yield return StartCoroutine(changeAlpha(img, 1, 0.1f));
        //    }
        //}

		// ’Ç‰Á9/17---------------------------------------
		titleGameOver.GetComponent<Animator>().SetBool("isFalling", true);
        AudioController.Instance.PlaySFX(gameOver_SE, settings.masterVolume * 1.5f, 1, false);
		// ’Ç‰Á---------------------------------------

		yield return new WaitForSeconds(5f);

		// Update texts
		scoreText.SetText("{0:000000}", scoreSystem.score);

		blockCount.text = $"You have placed {actionTimer.blockCount} blocks";
        height.text = $"It reached a height of {blockAction.height} meters";

        // Fade in score
        //AudioController.Instance.PlaySFX(gameOver_scoreSE);
        yield return StartCoroutine(changeAlpha(scoreText, 1, 0.5f));

        // Fade in block count
        yield return new WaitForSeconds(.5f);
        yield return StartCoroutine(changeAlpha(blockCount, 1, 0.3f));

        // Wait, then fade in height
        yield return new WaitForSeconds(.5f);
        yield return StartCoroutine(changeAlpha(height, 1, 0.3f));

        // Wait, then show restart button
        yield return new WaitForSeconds(.5f);
        restart.gameObject.SetActive(true);

        GameStatus.gameState = GAME_STATE.GAME_OVER;
    }


    IEnumerator changeAlpha(TextMeshProUGUI text, float alpha, float duration = 0)
    {
        Color color = text.color;
        float rate = .1f;
        if (duration == 0)
        {
            color.a = alpha;
            text.color = color;
        }
        else if (color.a < alpha)
        {
            var str = text.text ?? "";
            text.text = "";
            color.a = alpha;
            text.color = color;
            for (int i = 0; i < str.Length; i++)
            {
                text.text += str[i];
                if (!char.IsDigit(str[i])) 
                {
                    AudioController.Instance.PlaySFX(gameOver_typingSE, settings.masterVolume * .5f);
                    yield return new WaitForSeconds(.03f); 
                }
            }

        }
        else if (color.a > alpha)
        {
            do
            {
                color.a -= rate;
                text.color = color;
                yield return new WaitForSeconds(duration * Time.deltaTime);
            }
            while (color.a > alpha);

        }
    }

    IEnumerator changeAlpha (Image image, float alpha, float duration = 0)
    {
        Color color = image.color;
        float rate = .1f;
        if (duration == 0)
        {
            color.a = alpha;
            image.color = color;
        }
        else if (color.a < alpha)
        {
            do
            {
                color.a += rate;
                image.color = color;
                yield return new WaitForSeconds(duration * Time.deltaTime);
            }
            while (color.a < alpha);

        }
        else if (color.a > alpha)
        {
            do
            {
                color.a -= rate;
                image.color = color;
                yield return new WaitForSeconds(duration * Time.deltaTime);
            }
            while (color.a > alpha);

        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.CompareTag("Ground") && !isGameOver)
        { 
            isGameOver = true; 
            blockAction.TowerCollapse(); 
        }
    }
}
