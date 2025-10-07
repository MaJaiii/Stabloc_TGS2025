using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// �ǉ�-----------------------------------------
using System.Collections;
// ---------------------------------------------

public class ScoreSystem_new : MonoBehaviour
{
    [SerializeField] BlockAction blockAction;
    [SerializeField] TextMeshProUGUI nowScore;
    int prevScore;
    public int score;
    int targetScore;
    bool isCountUp;
    Sequence sequence;

    ActionTimer actionTimer;

    // �ǉ�-----------------------------------------
    [SerializeField] TextMeshProUGUI shiningScore;  // ����X�R�A��\������UI�e�L�X�g

    [Header("���点�镶���̔{��")]
    [SerializeField] int textMultiple = 300;    // ���点�镶���̔{��
    int multipleCount = 0;                      // ���点�������̐����J�E���g
    int scoreAnimParse = 0;

    [Header("�����T�C�Y�ύX����")]
    [SerializeField] float biggerDuration = 0.4f;   // �傫���Ȃ鎞��
    [SerializeField] float smallerDuration = 0.2f;  // �������Ȃ鎞��

    float defaultFontSize;        // �ŏ��̕����T�C�Y
    float maxFontSize = 140f;   // �ő�̕����T�C�Y   
    // --------------------------------------------

    private void Start()
    {
        actionTimer = GetComponent<ActionTimer>();

        // �ǉ�-----------------------------------------
        // �ŏ��̃e�L�X�g�T�C�Y���擾
        defaultFontSize = shiningScore.fontSize;
        // ����X�R�A���A�N�e�B�u�ɂ���
        shiningScore.enabled = false;        
        // ---------------------------------------------
    }
    private void Update()
    {
        if (isCountUp) nowScore.SetText("{0:000000}", prevScore);

        // �ǉ�-----------------------------------------
        // �e�L�X�g�����点�邩����
        if (prevScore >= textMultiple + multipleCount && prevScore >= targetScore - textMultiple)
        {
            Debug.Log($"{prevScore} : {targetScore}");
            // �Ō�̌��点��X�R�A���v�Z����i�A���Ăяo���h�~�j
            multipleCount = targetScore - (targetScore % textMultiple);

            // �\������e�L�X�g�̃X�R�A��ݒ�
            shiningScore.SetText("{0:000000}", multipleCount);

            // �X�R�A�̕\���ؑ�
            StartCoroutine(SwitchScoreDisplay());
        }

        scoreAnimParse = int.Parse(nowScore.text);
        // ---------------------------------------------
    }

    public void ModifyScore(int gain)
    {
        prevScore = score;
        score += gain;
        targetScore = score + gain;
        if (isCountUp) sequence.Kill(true);
        CountUpAnim();
    }

    public void ProductScore(float product)
    {
        prevScore = score;
        score = Mathf.CeilToInt(prevScore * product);
        targetScore = Mathf.CeilToInt(prevScore * product);
        if (isCountUp) sequence.Kill(true);
        Debug.Log(targetScore);
        CountUpAnim();
    }

    void CountUpAnim()
    {
        isCountUp = true;
        sequence = DOTween.Sequence().Append(DOTween.To(() => prevScore, num => prevScore = num, score, .5f)).AppendInterval(.1f).AppendCallback(() => isCountUp = false);
    }

    // �ǉ�-----------------------------------------
    // �X�R�A�̕\���ؑ�
    IEnumerator SwitchScoreDisplay()
    {
        // �����ҋ@����i�X�R�A�A�j���[�V�����Đ��^�C�~���O�����̂��߁j
        yield return new WaitUntil(() => scoreAnimParse >= multipleCount);

        // �X�R�A�\���̐؂�ւ�
        SetColor(nowScore, false);           // ���݂̃X�R�A���\��
        SetColor(shiningScore, true);   // ����X�R�A��\��

        //�ꎞ�I�Ɍ��炷�X�R�A�̒l(�X�R�A�A�j���[�V���������̂���)
        int delayScore = score - multipleCount;
        // �X�R�A���ꎞ�I�ɕۑ�
        int oldScore = score;
        score -= delayScore;
        prevScore = oldScore;

        // �t�H���g�T�C�Y��傫������R���[�`��
        yield return AnimationFontSize(defaultFontSize, maxFontSize, biggerDuration);
        // �t�H���g�T�C�Y������������R���[�`��
        yield return AnimationFontSize(maxFontSize, defaultFontSize, smallerDuration);

        // �X�R�A�\���̐؂�ւ��i���ɖ߂��j
        SetColor(shiningScore, false);   // ���݂̃X�R�A���\��
        SetColor(nowScore, true);           // ���݂̃X�R�A��\��

        // �X�R�A�����ɖ߂�
        score += delayScore;
        prevScore = score - delayScore;
        // �X�R�A�A�j���[�V�������Đ�
        if (isCountUp) sequence.Kill(true);
        CountUpAnim();
    }

    // �t�H���g�T�C�Y��ύX
    IEnumerator AnimationFontSize(float startSize, float finishSize, float duration)
    {
        // �o�ߎ��Ԃ��J�E���g����ϐ�
        float elapsedTimer = 0.0f;

        //�ݒ莞�Ԍo�߂���܂ŌJ��Ԃ�
        while (elapsedTimer < duration)
        {
            // ���Ԃ��J�E���g
            elapsedTimer += Time.deltaTime;
            // �i�s�x�̌v�Z
            float time = Mathf.Clamp01(elapsedTimer / duration);

            // �Ȃ߂炩�Ƀt�H���g�T�C�Y��ύX
            float newSize = Mathf.SmoothStep(startSize, finishSize, time);
            shiningScore.fontSize = newSize;

            yield return null;
        }
    }

    // �F��ݒ肷�郁�\�b�h
    void SetColor(TextMeshProUGUI meshProUGUI, bool active)
    {
        meshProUGUI.enabled = active;
    }
    // ---------------------------------------------
}
