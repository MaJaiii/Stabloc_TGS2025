using UnityEngine;
using System.Collections;

public class Cahin_Block : MonoBehaviour
{
    // アニメーションのパラメータ
    [SerializeField] private float m_pulseDuration = 0.5f; // 鼓動の1拍の長さ
    [SerializeField] private float m_maxScale = 1.5f; // 拡大時の最大スケール値

    private Material m_material;

    private void Start()
    {
        // MeshRendererからマテリアルインスタンスを取得
        // このインスタンスは複製されるため、他のオブジェクトのマテリアルに影響を与えません。
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            m_material = meshRenderer.material;
            // アニメーションコルーチンを開始
            StartCoroutine(HeartbeatAnimation());
        }
        else
        {
            Debug.LogError("MeshRenderer not found on this GameObject.", this);
            Destroy(gameObject); // メッシュレンダラーがない場合は自身を破棄
        }
    }

    private IEnumerator HeartbeatAnimation()
    {
        // 鼓動（拡大）
        float elapsedTime = 0f;
        while (elapsedTime < m_pulseDuration)
        {
            elapsedTime += Time.deltaTime;
            // 0から1へ向かう補間値
            float t = elapsedTime / m_pulseDuration;
            // スムーズな加速・減速のために二次関数を適用
            float scaleValue = Mathf.Lerp(1.0f, m_maxScale, t * t);
            m_material.SetFloat("_ObjectScale", scaleValue);
            yield return null;
        }

        // 縮小（元のサイズに戻る）
        elapsedTime = 0f;
        while (elapsedTime < m_pulseDuration)
        {
            elapsedTime += Time.deltaTime;
            // 1から0へ向かう補間値
            float t = elapsedTime / m_pulseDuration;
            float scaleValue = Mathf.Lerp(m_maxScale, 1.0f, t);
            m_material.SetFloat("_ObjectScale", scaleValue);
            yield return null;
        }

        // アニメーション終了後、自身を破棄
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // シーンからオブジェクトが破棄されるときに、マテリアルもクリーンアップする
        if (m_material != null)
        {
            Destroy(m_material);
        }
    }
}