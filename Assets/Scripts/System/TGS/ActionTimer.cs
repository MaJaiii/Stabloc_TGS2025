using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening; // DOTweenを追加

public class ActionTimer : MonoBehaviour
{
    [SerializeField] Image[] timerEdges;
    [SerializeField] TextMeshProUGUI blockCountText;
    [SerializeField] TextMeshProUGUI levelUpText;
    [SerializeField] public float timer;
    [SerializeField] BlockAction blockAction;
    [SerializeField] int maxBlock;
    int maxTime;
    [SerializeField] float fillAmount;
    [SerializeField] GameOver gameOver;
    [SerializeField] GameObject levelText;

    ScoreSystem scoreSystem;
    public int blockCount = 0;
    bool isRecovery = false;
    public bool isGameOver = false;
    int chain;
    int lastBlockCount = 0; // 前回のblockCountを保存

    int level = 0;
    public int[] border;

    // アニメーション用設定
    [SerializeField] private float throbDuration = 0.5f;
    [SerializeField] private float throbScale = 1.3f;
    private bool isLevelUpAnimPlaying = false;

    GameSettings settings;

    private void Start()
    {
        maxTime = CsvSettingsLoader.Load().timeLimitation;
        scoreSystem = GetComponent<ScoreSystem>();
        blockCountText.gameObject.SetActive(false);
        Gamepad.current?.SetMotorSpeeds(0, 0);
        if (maxBlock <= 0) maxBlock = 150;

        // アニメーション用テキストを非表示に初期化
        if (levelUpText != null)
        {
            levelUpText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isGameOver || blockAction.tutorial) return;
        settings = CsvSettingsLoader.Load();

        if (blockCount < 11) maxTime = 90;
        else if (blockCount < 51) maxTime = settings.timeLimitation;
        else if (blockCount < 71) maxTime = Mathf.CeilToInt(settings.timeLimitation / 2);
        else if (blockCount < 101) maxTime = Mathf.CeilToInt(settings.timeLimitation / 4);
        else maxTime = Mathf.CeilToInt(settings.timeLimitation / 10);

        if (!blockAction.tutorial && !blockCountText.gameObject.activeSelf && GameStatus.gameState == GAME_STATE.GAME_INGAME)
        {
            blockCountText.gameObject.SetActive(true);
        }

        if (gameOver.isGameOver && !isGameOver)
        {
            isGameOver = true;
            Gamepad.current?.SetMotorSpeeds(0, 0);
            blockCountText.gameObject.SetActive(false);
            return;
        }

        if (isGameOver) return;

        // レベルアップの条件が満たされたかチェックし、
        // かつ前回からのblockCountが変化した場合にのみ再生
        if ((blockCount == 11 || blockCount == 51 || blockCount == 71 || blockCount == 101) && blockCount != lastBlockCount)
        {
            // レベルアップの条件に達した瞬間にアニメーション再生
            PlayLevelUpAnimation();

            // アニメーションを再生したblockCountを記憶する
            lastBlockCount = blockCount;
        }


        if (isRecovery)
        {
            timer += Time.deltaTime * maxTime * 1.5f;
            if (timer > maxTime)
            {
                timer = maxTime;
                isRecovery = false;
            }
        }
        else if (timer > 0 && (blockAction.flagStatus & FlagsStatus.PressDownButton) != FlagsStatus.PressDownButton && blockAction.pivotObj != null)
        {
            timer -= Time.deltaTime;

            if (timer < 0)
            {
                timer = 0;
                blockAction.droppingPos = blockAction.ghostSystem.ghostBlock.position;
                blockAction.flagStatus |= FlagsStatus.Drop;
                blockAction.flagStatus |= FlagsStatus.PressDownButton;
            }
            if (Gamepad.current != null && timer < 3)
            {
                var gp = Gamepad.current;
                float motor = Mathf.Max(0, 3 - timer) / 3;
                gp.SetMotorSpeeds(motor, motor);

                foreach (var image in timerEdges)
                {
                    if (timer % .5f > .25f) image.color = Color.red;
                    else image.color = Color.white;
                }
            }
        }
        else Gamepad.current?.SetMotorSpeeds(0, 0);

        fillAmount = (timer / maxTime) * 5920;

        int[] segmentSizes = { 510, 1910, 1050, 1910, 540 };

        for (int i = 0; i < timerEdges.Length; i++)
            timerEdges[i].fillAmount = 0f;

        float remaining = fillAmount;

        for (int i = segmentSizes.Length - 1; i >= 0; i--)
        {
            if (remaining <= 0) break;

            if (timerEdges[i] == null) break;
            if (remaining < segmentSizes[i])
            {
                timerEdges[i].fillAmount = remaining / (float)segmentSizes[i];
                break;
            }
            else
            {
                timerEdges[i].fillAmount = 1f;
                remaining -= segmentSizes[i];
            }
        }

        blockCountText.SetText("{0:000}", blockCount);
    }

    public void RecoveryTimer()
    {
        isRecovery = true;
        foreach (var image in timerEdges)
        {
            image.color = Color.white;
        }
    }

    public bool AddPoint(float height, int score = 0)
    {
        if (scoreSystem == null)
        {
            Debug.LogError("scoreSystem is not assigned!");
            return false;
        }
        if (timer >= maxTime * .6f) chain++;
        else chain = 0;
        bool chainFlag = chain > 0 ? true : false;
        int gainScore = 0;
        if (score <= 0)
        {
            int timeScore = (int)Mathf.Ceil((timer / maxTime) * 5);
            int heightScore = (int)Mathf.Ceil((height) * 2);
            if (timeScore == 0) heightScore = 0;
            int chainScore = (int)Mathf.Ceil((timeScore + heightScore) * 0.25f * (chain + 1));

            gainScore = timeScore + heightScore + chainScore;
        }
        else gainScore += score;

        scoreSystem.ModifyScore(gainScore);
        return chainFlag;
    }

    // ▼ 新しいアニメーションメソッド
    private void PlayLevelUpAnimation()
    {
        // アニメーション中であることを示すフラグを立てる
        isLevelUpAnimPlaying = true;

        // テキストを可視化
        levelUpText.gameObject.SetActive(true);
        levelUpText.text = "LEVEL UP!!! \n Speed UP!!!";
        levelUpText.transform.localScale = Vector3.one;

        // DOTweenのシーケンスを作成
        Sequence sequence = DOTween.Sequence();

        // テキストのスケールをドクンと拡大・縮小
        sequence.Append(levelUpText.transform.DOScale(throbScale, throbDuration / 2).SetEase(Ease.OutQuad));
        sequence.Append(levelUpText.transform.DOScale(1, throbDuration / 2).SetEase(Ease.InQuad));

        // 拡大後にテキストをフェードアウト
        sequence.Append(levelUpText.DOFade(0, 1.0f));

        // アニメーション完了時にテキストを非表示にし、フラグをリセット
        sequence.OnComplete(() =>
        {
            levelUpText.gameObject.SetActive(false);
            levelUpText.alpha = 1.0f; // 次回のために透明度をリセット
            isLevelUpAnimPlaying = false;
        });
    }
}