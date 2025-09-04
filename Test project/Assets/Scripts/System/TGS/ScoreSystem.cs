using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ScoreSystem : MonoBehaviour
{
    [SerializeField] BlockAction blockAction;
    [SerializeField] TextMeshProUGUI nowScore;
    int prevScore;
    int tempBreak = 0;
    public float tempBreakduration;
    public int score;
    bool isCountUp;
    Sequence sequence;
    public int borderScore = 300;

    ActionTimer actionTimer;

    private void Start()
    {
        actionTimer = GetComponent<ActionTimer>();
    }
    private void Update()
    {
        if (isCountUp) nowScore.SetText("{0:000000}", prevScore);
    }

    public void ModifyScore (int gain)
    {
        prevScore = score;
        score += gain;
        if (Mathf.FloorToInt(prevScore / borderScore) < Mathf.FloorToInt(score / borderScore))
        {
            tempBreak = score - score % borderScore;
        }
        if (isCountUp) sequence.Kill(true);
        CountUpAnim();
    }

    public void ProductScore (float product)
    {
        prevScore = score;
        score = Mathf.CeilToInt(prevScore * product);
        if (isCountUp) sequence.Kill(true);
        CountUpAnim();
    }

    void CountUpAnim()
    {
        isCountUp = true;
        if (tempBreak == 0)
        {
            sequence = DOTween.Sequence().Append(DOTween.To(() => prevScore, num => prevScore = num, score, .5f)).AppendInterval(.1f).AppendCallback(() => isCountUp = false);
            return;
        }
        sequence = DOTween.Sequence().Append(DOTween.To(() => prevScore, num => prevScore = num, tempBreak, .5f)).AppendInterval(.1f).AppendCallback(() => DOTween.Sequence().Append(DOTween.To(() => tempBreak, num => tempBreak = num, score, .5f)).AppendInterval(.1f).AppendCallback(() => isCountUp = false));
        tempBreak = 0;
    }
}
