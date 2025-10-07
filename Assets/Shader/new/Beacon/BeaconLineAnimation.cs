// ParticleLineAnimation.cs

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BeaconLineAnimation : MonoBehaviour
{
    // アニメーションのパターンを定義する列挙型
    public enum AnimationPattern
    {
        Pattern1_XZShrink,          // パターン1: XZ軸が縮小する
        Pattern2_YGrowAndXZShrink   // パターン2: Y軸が伸びながらXZ軸が縮小する
    }

    [Header("Dependencies")]
    // BlockActionスクリプトへの参照。ブロックの配置情報（地面の高さなど）を取得するために使用
    [SerializeField] private BlockAction blockAction;

    [Header("Animation Settings")]
    // アニメーションに使用するキューブのプレハブ
    [SerializeField] private GameObject cubePrefab;
    // インスペクターで設定する現在のアニメーションパターン
    [SerializeField] private AnimationPattern currentPattern = AnimationPattern.Pattern2_YGrowAndXZShrink;
    // アニメーション全体の所要時間
    [SerializeField] private float animationDuration = 2.0f;
    // キューブをスポーンする位置のオフセット（中心からの距離）
    [SerializeField] private float spawnoffset = 2.0f;
    // スタート時のXZのスケール(1だと回転中に飛び出る)
    [SerializeField] private float XZ_scale = 0.7f;

    [Header("Y Scale Settings")]
    // Y軸の最大スケール値。パターン1では最初からこの値、パターン2ではこの値まで伸びる
    [SerializeField] private float yGrowOffset = 80.0f;

    [Header("Debug")]
    // デバッグ用にアニメーションを開始するためのキー
    public KeyCode animationKey = KeyCode.L;

    void Update()
    {
        // デバッグキーが押されたらアニメーションを開始する
        if (Input.GetKeyDown(animationKey))
        {
            PlayAnimation();
        }
    }

    /// <summary>
    /// アニメーションを再生するパブリックメソッド
    /// このメソッドを外部から呼び出すことで、アニメーションを開始できる
    /// </summary>
    public void PlayAnimation()
    {
        if (blockAction == null || cubePrefab == null)
        {
            Debug.LogError("Required data is missing to start the animation.");
            return;
        }

        // 4つの角の基準座標を定義
        Vector3 origin = blockAction.origin;
        List<Vector2> cornersXZ = new List<Vector2>
        {
            new Vector2(origin.x + spawnoffset, origin.z + spawnoffset),
            new Vector2(origin.x - spawnoffset, origin.z + spawnoffset),
            new Vector2(origin.x + spawnoffset, origin.z - spawnoffset),
            new Vector2(origin.x - spawnoffset, origin.z - spawnoffset)
        };

        // シーン内のすべてのコアオブジェクトを直接検索して、その位置を取得する
        CheckCore[] allCoresInScene = FindObjectsByType<CheckCore>(FindObjectsSortMode.None);

        // アニメーションに必要な情報を取得
        Color currentColor = blockAction.GetCurrentBlockColor();
        float lastGroundLevel = blockAction.lastGroundLevel + 0.5f;

        // 各角について、コアブロックの存在をチェック
        for (int i = 0; i < cornersXZ.Count; i++)
        {
            Vector2 cornerXZ = cornersXZ[i];
            bool isCorePresentAtCorner = false;

            // シーン内の全コアをループし、現在の角の位置に該当するコアがあるか確認
            foreach (CheckCore core in allCoresInScene)
            {
                // タグが"Placed"のコアのみを対象とする
                if (core.CompareTag("Placed"))
                {
                    Vector2 corePosXZ = new Vector2(core.transform.position.x, core.transform.position.z);

                    // Vector2.Distanceを使って誤差を許容しながら位置を比較
                    if (Vector2.Distance(cornerXZ, corePosXZ) < 0.1f)
                    {
                        isCorePresentAtCorner = true;
                        break;
                    }
                }
            }

            // コアがその角に存在しない場合にのみ、アニメーションを開始
            if (!isCorePresentAtCorner)
            {
                Vector3 animationSpawnPos = new Vector3(cornerXZ.x, lastGroundLevel, cornerXZ.y);

                AnimateSingleCube(Instantiate(cubePrefab, animationSpawnPos, Quaternion.identity), currentPattern, true, lastGroundLevel, currentColor);
            }
        }
    }

    /// <summary>
    /// 単一のキューブに対してアニメーションを適用するメソッド
    /// </summary>
    private void AnimateSingleCube(GameObject cubeInstance, AnimationPattern pattern, bool isPositiveRotation, float startY, Color color)
    {
        // キューブの親子関係を設定し、管理しやすくする
        cubeInstance.transform.SetParent(this.transform);

        // マテリアルを取得して、シェーダープロパティを操作できるようにする
        Material cubeMaterial = cubeInstance.GetComponent<Renderer>().material;

        // _BeaconLineColor
        cubeMaterial.SetColor("_BeaconLineColor", color);

        // 初期プロパティを設定
        cubeMaterial.SetFloat("_RotationAngle", 0);
        cubeMaterial.SetFloat("_XZScale", XZ_scale);

        // Transformのローカルスケールを操作
        cubeInstance.transform.localScale = new Vector3(XZ_scale, 0, XZ_scale);

        // DOTweenのシーケンスを作成
        Sequence sequence = DOTween.Sequence();

        // 共通アニメーション: Y軸を中心とした回転
        float rotationTarget = isPositiveRotation ? 360f : -360f;
        sequence.Append(DOTween.To(() => cubeMaterial.GetFloat("_RotationAngle"), x => cubeMaterial.SetFloat("_RotationAngle", x), rotationTarget, animationDuration));

        if (pattern == AnimationPattern.Pattern1_XZShrink)
        {
            // パターン1: Yスケールを最初から最大に設定
            cubeInstance.transform.localScale = new Vector3(XZ_scale, yGrowOffset, XZ_scale);

            // XZスケールを0まで縮小するアニメーション
            sequence.Join(DOTween.To(() => cubeMaterial.GetFloat("_XZScale"), x => cubeMaterial.SetFloat("_XZScale", x), 0, animationDuration));

            // オブジェクトのY座標を補正し、底面が地面に合うようにする
            Vector3 newPos = cubeInstance.transform.position;
            newPos.y = startY + (yGrowOffset * 0.5f);
            cubeInstance.transform.position = newPos;
        }
        else if (pattern == AnimationPattern.Pattern2_YGrowAndXZShrink)
        {
            // パターン2: Yスケールを下から上に伸ばすアニメーション
            // XZスケールとYスケールを同時にアニメーション
            sequence.Join(DOTween.To(() => cubeMaterial.GetFloat("_XZScale"), x => cubeMaterial.SetFloat("_XZScale", x), 0, animationDuration));

            // Yスケールと位置を同時にアニメーションさせ、底面を地面に固定
            sequence.Join(DOTween.To(() => 0f, (yScale) => {
                // TransformのYスケールを更新
                cubeInstance.transform.localScale = new Vector3(XZ_scale, yScale, XZ_scale);
                // キューブの中心点をYスケールの半分だけ上に移動させる
                Vector3 newPos = cubeInstance.transform.position;
                newPos.y = startY + (yScale * 0.5f);
                cubeInstance.transform.position = newPos;
            }, yGrowOffset, animationDuration));
        }

        // 全てのアニメーションが完了したら、キューブを破棄
        sequence.OnComplete(() =>
        {
            Destroy(cubeInstance);
        });
    }
}