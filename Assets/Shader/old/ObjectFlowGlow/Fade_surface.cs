using UnityEngine;
using DG.Tweening;

public class Fade_surface : MonoBehaviour
{
	// メンバー変数の宣言
	[Header("Dependencies")]
	[SerializeField] private BlockAction m_blockAction;
	[SerializeField] private MeshRenderer m_meshRenderer;
	[SerializeField] private GameOver m_gameover;

	[Header("Settings")]
	[SerializeField] private float m_objectHeightOffset = 2f;    // 地面からの高さのオフセット

	private Material m_material;
	private bool m_isFirstBlockPlaced = false;
	private float m_initialYPosition;
	private Vector3 m_initialScale;


	private void Start()
	{
		// MeshRendererコンポーネントを取得
		if (m_meshRenderer == null)
		{
			m_meshRenderer = GetComponent<MeshRenderer>();
		}

		

		// オブジェクトの初期Y座標と初期スケールを保存
		m_initialYPosition = transform.position.y;
		m_initialScale = transform.localScale;

		// 最初の高さをほぼ0に設定
		Vector3 currentScale = transform.localScale;
		currentScale.y = 0.001f;
		transform.localScale = currentScale;
	}

	private void Update()
	{
		// GameManagerのステータスで制御
		if (GameStatus.gameState == GAME_STATE.GAME_INGAME)
		{
			// 最初のブロックが置かれたら初期配置とフェードインを実行
			if (!m_isFirstBlockPlaced && m_blockAction.placedBlockCount > 0)
			{
				OnFirstBlockPlaced();
				m_isFirstBlockPlaced = true;
			}

			// lastGroundLevelに合わせてスケールと位置を更新
			UpdateScaleAndPositionBasedOnGroundLevel();
		}
		// ゲームオーバー状態のチェック
		if (m_gameover.isGameOver)
		{
			m_meshRenderer.enabled = false;

		}
	}

	/// <summary>
	/// 最初のブロックが置かれた際の処理
	/// </summary>
	private void OnFirstBlockPlaced()
	{
		// 最初のブロックのoriginからX,Z座標を取得し、Y座標は初期位置に設定
		Vector3 initialPosition = new Vector3(m_blockAction.origin.x, m_initialYPosition, m_blockAction.origin.z);
		transform.position = initialPosition;

		Vector3 currentScale = transform.localScale;
		currentScale.y = 1.01f;
		transform.localScale = currentScale;

		m_meshRenderer.enabled = true;
	}

	/// <summary>
	/// lastGroundLevelの値に基づいてオブジェクトのYスケールと位置を更新
	/// </summary>
	private void UpdateScaleAndPositionBasedOnGroundLevel()
	{
		if (m_blockAction.lastGroundLevel == -4) return;

		// 地面からの高さを計算
		float targetHeight = (m_blockAction.lastGroundLevel - m_initialYPosition) + m_objectHeightOffset;

		// スケール変更と位置補正
		Vector3 newScale = m_initialScale;
		newScale.y = targetHeight;
		transform.localScale = newScale;

		// スケール変更による底面のずれを補正し、Y座標を調整
		Vector3 newPosition = transform.position;
		newPosition.y = m_initialYPosition + (targetHeight - m_initialScale.y) / 2f;
		transform.position = newPosition;
	}
	

}

