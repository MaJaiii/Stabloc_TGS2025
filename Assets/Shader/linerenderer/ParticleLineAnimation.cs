// ParticleAnimator.cs
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ParticleLineAnimation : MonoBehaviour
{
    // パーティクルエフェクトのプレハブ
    [SerializeField] private GameObject particlePrefab;
    // 上下移動のアニメーションにかかる時間
    [SerializeField] private float cycleDuration = 0.5f;
    // パーティクルが上下に移動する回数
    [SerializeField] private int cycleCount = 3;
    // パーティクルが到達する目標の高さ（オフセット）
    [SerializeField] private float targetHeightOffset = 5.0f;

    // BlockAction.csへの参照をインスペクターで設定
    [SerializeField] private BlockAction blockAction;

    // スポーンする場所のオフセット(２が中心、2.5は外側)
    [SerializeField] private float spawnoffset = 2.0f;

    // アニメーションを実行するキー
    public KeyCode animationKey = KeyCode.L;

    void Update()
    {
        // 一旦の確認用
        if (Input.GetKeyDown(animationKey))
        {
            PlayAnimation();
        }
    }


    /// <summary>
    /// アニメーションを再生
    /// このメソッドを呼び出すだけで一連のアニメーションが実行
    /// </summary>
    public void PlayAnimation()
    {
        // 必要なコンポーネントとプレハブが設定されているかを確認
        if (blockAction == null || particlePrefab == null)
        {
            Debug.LogError("Required data is missing to start the animation.");
            return;
        }

        // blockActionからlastGroundLevelの値を取得
        float lastGroundLevel = blockAction.lastGroundLevel;

        // BlockActionのoriginフィールドを直接参照して四隅の座標を計算
        float minX = blockAction.origin.x - spawnoffset; 
        float maxX = blockAction.origin.x + spawnoffset;
        float minZ = blockAction.origin.z - spawnoffset; 
        float maxZ = blockAction.origin.z + spawnoffset; 
        float y = lastGroundLevel;

        // 4つの角の座標を明示的に定義
        Vector3 corner1 = new Vector3(maxX, y, maxZ);
        Vector3 corner2 = new Vector3(minX, y, maxZ);
        Vector3 corner3 = new Vector3(maxX, y, minZ);
        Vector3 corner4 = new Vector3(minX, y, minZ);

        // 各角に1つずつパーティクルを生成し、それぞれのアニメーションを開始
        AnimateSingleParticle(Instantiate(particlePrefab, corner1, Quaternion.identity), lastGroundLevel);
        AnimateSingleParticle(Instantiate(particlePrefab, corner2, Quaternion.identity), lastGroundLevel);
        AnimateSingleParticle(Instantiate(particlePrefab, corner3, Quaternion.identity), lastGroundLevel);
        AnimateSingleParticle(Instantiate(particlePrefab, corner4, Quaternion.identity), lastGroundLevel);
    }

    private void AnimateSingleParticle(GameObject particleInstance, float startY)
    {
        // 生成されたパーティクルを管理するための子オブジェクトにする
        particleInstance.transform.SetParent(this.transform);

        // アニメーションのシーケンスを定義
        Sequence sequence = DOTween.Sequence();

        // 目標の高さ（lastGroundLevel + オフセット）を計算
        float targetY = startY + targetHeightOffset;

        // 上下移動アニメーションをループ
        for (int i = 0; i < cycleCount; i++)
        {
            // まず目標の高さまで上昇し、次に開始Y座標まで下降
            sequence.Append(particleInstance.transform.DOMoveY(targetY, cycleDuration / 2).SetEase(Ease.OutSine));
            sequence.Append(particleInstance.transform.DOMoveY(startY, cycleDuration / 2).SetEase(Ease.InSine));
        }

        // 全てのサイクルが完了したら、パーティクルオブジェクトを破棄
        sequence.OnComplete(() => Destroy(particleInstance.gameObject));
    }
}