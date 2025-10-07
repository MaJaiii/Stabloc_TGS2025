using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// 追加-----------------------------------------
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

    // 追加-----------------------------------------
    [SerializeField] TextMeshProUGUI shiningScore;  // 光るスコアを表示するUIテキスト

    [Header("光らせる文字の倍数")]
    [SerializeField] int textMultiple = 300;    // 光らせる文字の倍数
    int multipleCount = 0;                      // 光らせた文字の数をカウント
    int scoreAnimParse = 0;

    [Header("文字サイズ変更時間")]
    [SerializeField] float biggerDuration = 0.4f;   // 大きくなる時間
    [SerializeField] float smallerDuration = 0.2f;  // 小さくなる時間

    float defaultFontSize;        // 最初の文字サイズ
    float maxFontSize = 140f;   // 最大の文字サイズ   
    // --------------------------------------------

    private void Start()
    {
        actionTimer = GetComponent<ActionTimer>();

        // 追加-----------------------------------------
        // 最初のテキストサイズを取得
        defaultFontSize = shiningScore.fontSize;
        // 光るスコアを非アクティブにする
        shiningScore.enabled = false;        
        // ---------------------------------------------
    }
    private void Update()
    {
        if (isCountUp) nowScore.SetText("{0:000000}", prevScore);

        // 追加-----------------------------------------
        // テキストを光らせるか判定
        if (prevScore >= textMultiple + multipleCount && prevScore >= targetScore - textMultiple)
        {
            Debug.Log($"{prevScore} : {targetScore}");
            // 最後の光らせるスコアを計算する（連続呼び出し防止）
            multipleCount = targetScore - (targetScore % textMultiple);

            // 表示するテキストのスコアを設定
            shiningScore.SetText("{0:000000}", multipleCount);

            // スコアの表示切替
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

    // 追加-----------------------------------------
    // スコアの表示切替
    IEnumerator SwitchScoreDisplay()
    {
        // 少し待機する（スコアアニメーション再生タイミング調整のため）
        yield return new WaitUntil(() => scoreAnimParse >= multipleCount);

        // スコア表示の切り替え
        SetColor(nowScore, false);           // 現在のスコアを非表示
        SetColor(shiningScore, true);   // 光るスコアを表示

        //一時的に減らすスコアの値(スコアアニメーション調整のため)
        int delayScore = score - multipleCount;
        // スコアを一時的に保存
        int oldScore = score;
        score -= delayScore;
        prevScore = oldScore;

        // フォントサイズを大きくするコルーチン
        yield return AnimationFontSize(defaultFontSize, maxFontSize, biggerDuration);
        // フォントサイズを小さくするコルーチン
        yield return AnimationFontSize(maxFontSize, defaultFontSize, smallerDuration);

        // スコア表示の切り替え（元に戻す）
        SetColor(shiningScore, false);   // 現在のスコアを非表示
        SetColor(nowScore, true);           // 現在のスコアを表示

        // スコアを元に戻す
        score += delayScore;
        prevScore = score - delayScore;
        // スコアアニメーションを再生
        if (isCountUp) sequence.Kill(true);
        CountUpAnim();
    }

    // フォントサイズを変更
    IEnumerator AnimationFontSize(float startSize, float finishSize, float duration)
    {
        // 経過時間をカウントする変数
        float elapsedTimer = 0.0f;

        //設定時間経過するまで繰り返し
        while (elapsedTimer < duration)
        {
            // 時間をカウント
            elapsedTimer += Time.deltaTime;
            // 進行度の計算
            float time = Mathf.Clamp01(elapsedTimer / duration);

            // なめらかにフォントサイズを変更
            float newSize = Mathf.SmoothStep(startSize, finishSize, time);
            shiningScore.fontSize = newSize;

            yield return null;
        }
    }

    // 色を設定するメソッド
    void SetColor(TextMeshProUGUI meshProUGUI, bool active)
    {
        meshProUGUI.enabled = active;
    }
    // ---------------------------------------------
}
