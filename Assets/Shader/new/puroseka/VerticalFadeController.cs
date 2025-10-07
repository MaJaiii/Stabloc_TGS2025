using System.Collections;
using UnityEngine;
using DG.Tweening;

public class VerticalFadeController : MonoBehaviour
{
    // メンバー変数の宣言
    [Header("Dependencies")]
    [SerializeField] private BlockAction m_blockAction;
    [SerializeField] private MeshRenderer m_meshRenderer;
    [SerializeField] private Spawn_BlockAnimation m_spawnAnimation;

	[Header("Settings")]
    [SerializeField] private float m_initialFadeDuration = 0.5f; // 最初のフェードインにかかる時間
    [SerializeField] private float m_coreGlowDuration = 0.5f;    // コア取得時の発光アニメーション時間
    [SerializeField] private float m_coreGlowIntensity = 0.8f;   // コア取得時の最大不透明度
    [SerializeField] private float m_fadeIntensity = 0.8f;   // fadeの最大値
    [SerializeField] private float m_growthOffset = 2f;          // lastGroundLevelからの成長オフセット
    [SerializeField] private float m_animDelay = 0.1f;           // コア演出開始前の待機時間

    private Material m_material;
    private Vector3 m_initialScale;
    private bool m_isManuallyPositioned = false;
    private bool m_isFirstBlockPlaced = false;
    private float m_initialYPosition; // 最初のY座標を保存

    GameSettings settings;

	private void Start()
    {
        // MeshRendererコンポーネントを取得
        if (m_meshRenderer == null)
        {
            m_meshRenderer = GetComponent<MeshRenderer>();
        }

        // マテリアルのインスタンスを作成して操作
        m_material = new Material(m_meshRenderer.sharedMaterial);
        m_meshRenderer.material = m_material;

        // 初期設定
        m_initialScale = transform.localScale;
        m_initialYPosition = transform.position.y;

        // _OverallOpacityと_FadeRangeを初期値の0に設定し、最初は非表示にする
        m_material.SetFloat("_OverallOpacity", 0);
        m_material.SetFloat("_FadeRange", 0);
    }

    private void Update()
    {
        // コア演出が再生中でない場合、lastGroundLevelに合わせてスケールと位置を更新
        if (!m_isManuallyPositioned)
        {
            if (GameStatus.gameState == GAME_STATE.GAME_INGAME)
            {
                UpdateScaleAndPositionBasedOnGroundLevel();
            }
        }
		

	}

    /// <summary>
    /// lastGroundLevelに合わせてスケールと位置を更新する
    /// </summary>
    public void UpdateScaleAndPositionBasedOnGroundLevel()
    {
        float targetHeight = m_growthOffset;
        Vector3 newScale = transform.localScale;
        newScale.y = targetHeight;
        transform.localScale = newScale;

        Vector3 newPosition = transform.position;
        newPosition.y = m_blockAction.lastGroundLevel - (transform.localScale.y / 2f) + 0.5f;
        transform.position = newPosition;
    }

    /// <summary>
    /// 4つのコアが揃った時の演出
    /// </summary>
    public void OnFourCoresCollected()
    {
        if (m_blockAction == null) return;

        m_blockAction.isAnim = true;

        // まず位置とスケールを即時更新
        UpdateScaleAndPositionBasedOnGroundLevel();

        // コルーチンで「先に位置確定 → 少し待機 → 光るアニメーション」
        StartCoroutine(StartAnimWithDelay(m_animDelay));
    }

    private IEnumerator StartAnimWithDelay(float delay)
    {

        // 少し待機（lastGroundLevelの更新ラグを吸収するため）
        yield return new WaitForSeconds(delay);

		m_meshRenderer.enabled = true;

		// DOTweenアニメーション開始
		Sequence mySequence = DOTween.Sequence();
        mySequence.Append(m_material.DOFloat(m_coreGlowIntensity, "_OverallOpacity", m_coreGlowDuration / 2)
            .SetEase(Ease.OutSine));
        mySequence.Append(m_material.DOFloat(m_fadeIntensity, "_FadeRange", m_coreGlowDuration / 2)
            .SetEase(Ease.OutSine));
        mySequence.Append(m_material.DOFloat(0, "_OverallOpacity", m_coreGlowDuration / 2)
            .SetEase(Ease.InSine));
        mySequence.Append(m_material.DOFloat(0, "_FadeRange", m_coreGlowDuration / 2)
            .SetEase(Ease.InSine));

       

        mySequence.OnComplete(() =>
        {
            m_material.SetFloat("_OverallOpacity", 0);
            m_material.SetFloat("_FadeRange", 0);
            if (m_spawnAnimation != null)
            {
				m_spawnAnimation.ResetCoreAnimFlag();
            }
            if (m_blockAction.isAnim)
            {
                m_blockAction.isAnim = false;
                AudioController.Instance.PlaySFX(m_blockAction.groundSE, settings.masterVolume * 5);
            }

        });
    }
}
