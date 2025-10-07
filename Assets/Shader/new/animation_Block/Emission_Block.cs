using System.Collections;
using UnityEngine;

public class Emission_Block : MonoBehaviour
{
    // 各プロセスの時間設定
    [Header("フェードインの時間")]
    [SerializeField]
    private float fadeTime = 2f;

    [Header("待機時間")]
    [SerializeField]
    private float waitTime = 1f;

    [Header("ディゾルブの時間")]
    [SerializeField]
    private float dissolveTime = 3f;

    private Renderer myRenderer;
    private Material myMaterial;

    // シェーダープロパティのIDをキャッシュ
    private int glowFalloffID;
    private int dissolveAmountID;

    private void Awake()
    {
        // レンダラーとマテリアルを取得
        myRenderer = GetComponent<Renderer>();
        myMaterial = myRenderer.material; // .materialは新しいインスタンスを作成します

        // シェーダープロパティのIDを取得し、パフォーマンスを向上させる
        glowFalloffID = Shader.PropertyToID("_GlowFalloff");
        dissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    }

    private void Start()
    {
        // フェードアウトとディゾルブのコルーチンを開始
        StartCoroutine(GlowDissolveSequence());
    }

    private IEnumerator GlowDissolveSequence()
    {
        // ステップ1: _GlowFalloffを50から0へアニメーション
        float startGlowFalloff = 1f;
        float endGlowFalloff = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            float newGlowFalloff = Mathf.Lerp(startGlowFalloff, endGlowFalloff, elapsedTime / fadeTime);
            myMaterial.SetFloat(glowFalloffID, newGlowFalloff);
            elapsedTime += Time.deltaTime;
            yield return null; // 次のフレームまで待機
        }
        myMaterial.SetFloat(glowFalloffID, endGlowFalloff); // 最終値を確定

        // ステップ2: 少し待機
        yield return new WaitForSeconds(waitTime);

        // ステップ3: _DissolveAmountを0から1へアニメーション
        float startDissolveAmount = 0f;
        float endDissolveAmount = 1f;
        elapsedTime = 0f;

        while (elapsedTime < dissolveTime)
        {
            float newDissolveAmount = Mathf.Lerp(startDissolveAmount, endDissolveAmount, elapsedTime / dissolveTime);
            myMaterial.SetFloat(dissolveAmountID, newDissolveAmount);
            elapsedTime += Time.deltaTime;
            yield return null; // 次のフレームまで待機
        }
        myMaterial.SetFloat(dissolveAmountID, endDissolveAmount); // 最終値を確定

        // ステップ4: オブジェクトを破壊
        Destroy(gameObject);
    }
}