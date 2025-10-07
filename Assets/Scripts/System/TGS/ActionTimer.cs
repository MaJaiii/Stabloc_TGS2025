using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening; // DOTween��ǉ�

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
    int lastBlockCount = 0; // �O���blockCount��ۑ�

    int level = 0;
    public int[] border;

    // �A�j���[�V�����p�ݒ�
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

        // �A�j���[�V�����p�e�L�X�g���\���ɏ�����
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

        // ���x���A�b�v�̏������������ꂽ���`�F�b�N���A
        // ���O�񂩂��blockCount���ω������ꍇ�ɂ̂ݍĐ�
        if ((blockCount == 11 || blockCount == 51 || blockCount == 71 || blockCount == 101) && blockCount != lastBlockCount)
        {
            // ���x���A�b�v�̏����ɒB�����u�ԂɃA�j���[�V�����Đ�
            PlayLevelUpAnimation();

            // �A�j���[�V�������Đ�����blockCount���L������
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

    // �� �V�����A�j���[�V�������\�b�h
    private void PlayLevelUpAnimation()
    {
        // �A�j���[�V�������ł��邱�Ƃ������t���O�𗧂Ă�
        isLevelUpAnimPlaying = true;

        // �e�L�X�g������
        levelUpText.gameObject.SetActive(true);
        levelUpText.text = "LEVEL UP!!! \n Speed UP!!!";
        levelUpText.transform.localScale = Vector3.one;

        // DOTween�̃V�[�P���X���쐬
        Sequence sequence = DOTween.Sequence();

        // �e�L�X�g�̃X�P�[�����h�N���Ɗg��E�k��
        sequence.Append(levelUpText.transform.DOScale(throbScale, throbDuration / 2).SetEase(Ease.OutQuad));
        sequence.Append(levelUpText.transform.DOScale(1, throbDuration / 2).SetEase(Ease.InQuad));

        // �g���Ƀe�L�X�g���t�F�[�h�A�E�g
        sequence.Append(levelUpText.DOFade(0, 1.0f));

        // �A�j���[�V�����������Ƀe�L�X�g���\���ɂ��A�t���O�����Z�b�g
        sequence.OnComplete(() =>
        {
            levelUpText.gameObject.SetActive(false);
            levelUpText.alpha = 1.0f; // ����̂��߂ɓ����x�����Z�b�g
            isLevelUpAnimPlaying = false;
        });
    }
}