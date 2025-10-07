using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Spawn_BlockAnimation : MonoBehaviour
{
	[SerializeField]
	GroundLevelMode Ground_Level_Mode = GroundLevelMode.FastGroundLevel;

	private enum GroundLevelMode
	{
		FastGroundLevel,
		UpdateGroundLevel
	}

    public float m_lastGround;

	// メンバー変数の宣言
	[Header("Dependencies")]
    [SerializeField] private BlockAction m_blockAction;
    [SerializeField] private BlockAnimation blockAnimation;
    [SerializeField] private LineAnimation m_lineAnimation;
    private GameSettings settings;

    // 連鎖グリットブロック用の変数
    [Header("Spawn Prefab Settings")]
    [SerializeField] private GameObject m_prefabToSpawn;
    [SerializeField] private float m_spawnDelay = 0.1f;
    [SerializeField] private float m_startSpawnDelay = 0.5f;

    // 連鎖発光ブロック用の変数
    [Header("Emission Prefab Settings")]
    [SerializeField] private GameObject m_emissionPrefabToSpawn;
    [SerializeField] private float m_emissionSpawnDelay = 0.1f;
    [SerializeField] private float m_emissionStartSpawnDelay = 0.2f; // SpawnPrefabsWithDelayより遅らせるためのディレイ

    // ★
    [Header("Shaders")]
	[SerializeField] VerticalFadeController[] m_verticalFadeController;
	private bool m_hasPlayedCoreAnim = false; // 演出が再生されたかどうかのフラグ
	private bool m_isSequenceRunning = false; // ラインアニメーション終了フラグ
    private bool m_chainAvnimation = false; // Chainアニメージョン終了フラグ

	private bool m_isFirstBlockPlaced = false;
    private float m_initialYPosition;
    private float m_lastGroundLevelCache;

    [Header("Audio")]
    [SerializeField] AudioClip m_lineAnimation_SE;
    [SerializeField] AudioClip m_chainAnimation_SE;
    [SerializeField] AudioClip m_emissionBlockAnimation_SE;
    [SerializeField] float SE_volume = 0.01f;
    [SerializeField] float SE_chainAnimation_limit = 8;
    [SerializeField] float SE_volume_adjustment = 5; // SEの音量調整
    bool Play_SE = true;

    private void Start()
    {
        if (Ground_Level_Mode == GroundLevelMode.FastGroundLevel)
        {
            m_lastGround = -4.0f;
        }
        else
        {
            m_lastGround = -3.0f;
        }

        settings = CsvSettingsLoader.Load();

        m_initialYPosition = transform.position.y;
        m_lastGroundLevelCache = -4f;

        if (m_blockAction == null)
        {
            Debug.LogError("BlockAction is not assigned to Spawn_BlockAnimation script.", this);
            return;
        }
        if (blockAnimation == null)
        {
            Debug.LogError("BlockAnimation is not assigned to Spawn_BlockAnimation script.", this);
        }
        // BlockAnimationのイベントにメソッドを登録
        //blockAnimation.OnDissolveStart += OnDissolveAnimationStart;
    }

    private void OnDestroy()
    {
        // スクリプトが破棄されるときにイベントの登録を解除
        if (blockAnimation != null)
        {
            //blockAnimation.OnDissolveStart -= OnDissolveAnimationStart;
        }
    }

    private void Update()
    {
        if (GameStatus.gameState == GAME_STATE.GAME_INGAME)
        {
            if (!m_isFirstBlockPlaced && m_blockAction.placedBlockCount > 0)
            {
                OnFirstBlockPlaced();
                m_isFirstBlockPlaced = true;
            }

			// 地面の高さが変わったとき、かつシーケンスが実行中でない場合
			if (m_blockAction.lastGroundLevel != m_lastGroundLevelCache && !m_isSequenceRunning)
			{
				// コアアインメーションの再生
	            //StartCoroutine(HandleAnimationSequence());

				StartCoroutine(LineHandleAnimationSequence());
				m_lastGroundLevelCache = m_blockAction.lastGroundLevel;

			}
		}
    }

    private void OnFirstBlockPlaced()
    {
        Vector3 initialPosition = new Vector3(m_blockAction.origin.x, m_initialYPosition, m_blockAction.origin.z);
        transform.position = initialPosition;
    }

    /// <summary>
    /// プレハブを遅延を伴って生成するコルーチン
    /// </summary>
    private IEnumerator SpawnPrefabsWithDelay()
    {
        // 処理開始タイミングを調整するためのディレイ
        yield return new WaitForSeconds(30 / AudioController.Instance.bpm);
        

        float currentY = m_blockAction.lastPlacedCorePos_y + 1f;
        float Spawnpos = m_blockAction.lastPlacedCorePos_y - 1f;

        float Count_SE = 0;
        


        // lastGroundLevelから-4まで1ずつ減少させながらプレハブを生成
        while (Spawnpos >= m_lastGround || currentY <= m_blockAction.GetHighestPoint().y)
        {
            Vector3 downspawnPosition = new Vector3(m_blockAction.origin.x, Spawnpos, m_blockAction.origin.z);
			Vector3 upspawnPosition = new Vector3(m_blockAction.origin.x, currentY, m_blockAction.origin.z);

			// プレハブの生成
			if (m_prefabToSpawn != null)
            {
                if(Spawnpos >= m_lastGround)
                {
					Instantiate(m_prefabToSpawn, downspawnPosition, Quaternion.identity);
				}
                if(currentY <= m_blockAction.GetHighestPoint().y)
                {
					Instantiate(m_prefabToSpawn, upspawnPosition, Quaternion.identity);
				}
				
			}

            // SEの再生ロジック
            // 次のループが最後のブロック生成になるかをチェック
            if (Play_SE)
            {
                bool isLastBlock = (Spawnpos - 1f < m_lastGround && currentY + 1f > m_blockAction.GetHighestPoint().y);
                bool isEighthBlock = (Count_SE == SE_chainAnimation_limit - 1);

                if (isLastBlock || isEighthBlock)
                {
                    AudioController.Instance.PlaySFX(m_chainAnimation_SE, settings.effectVolume * (8 + SE_volume_adjustment));
                    Play_SE = false;
                }
                else if (Count_SE < SE_chainAnimation_limit)
                {
                    StartCoroutine(PlayShortClipPart(m_chainAnimation_SE, 0.2f));
                }
            }

            // 次の生成までのディレイ
            yield return new WaitForSeconds(m_spawnDelay);
            currentY++;
			Spawnpos--;
            Count_SE++;
        }
    }

    /// <summary>
    /// Emission_Blockプレハブを生成する新しいコルーチン
    /// </summary>
    private IEnumerator SpawnEmissionPrefabsWithDelay()
    {

		yield return new WaitForSeconds(m_emissionStartSpawnDelay);
        AudioController.Instance.PlaySFX(m_emissionBlockAnimation_SE, settings.effectVolume * (4 + SE_volume_adjustment));

        // ★ 生成開始位置を lastPlacedCorePos_y に設定
        float currentY = m_blockAction.lastPlacedCorePos_y;
        float Spawnpos = m_blockAction.lastPlacedCorePos_y;

        while (Spawnpos >= m_lastGround || currentY <= m_blockAction.GetHighestPoint().y)
        {
            Vector3 downspawnPosition = new Vector3(m_blockAction.origin.x, Spawnpos, m_blockAction.origin.z);
            Vector3 upspawnPosition = new Vector3(m_blockAction.origin.x, currentY, m_blockAction.origin.z);

            if (m_emissionPrefabToSpawn != null)
            {
                // ★ 最初のループでlastPlacedCorePos_yの上下を生成
                if (currentY == m_blockAction.lastPlacedCorePos_y)
                {
                    Instantiate(m_emissionPrefabToSpawn, downspawnPosition, Quaternion.identity);
                    if (currentY != upspawnPosition.y)
                    {
                        Instantiate(m_emissionPrefabToSpawn, upspawnPosition, Quaternion.identity);
                    }
                }
                else
                {
                    if (Spawnpos >= m_lastGround)
                    {
                        Instantiate(m_emissionPrefabToSpawn, downspawnPosition, Quaternion.identity);
                    }
                    if (currentY <= m_blockAction.GetHighestPoint().y)
                    {
                        Instantiate(m_emissionPrefabToSpawn, upspawnPosition, Quaternion.identity);
                    }
                }
            }
            yield return new WaitForSeconds(m_emissionSpawnDelay);
            currentY++;
            Spawnpos--;
        }
        if (Ground_Level_Mode == GroundLevelMode.UpdateGroundLevel)
        {
            m_lastGround = m_blockAction.lastGroundLevel + 1f;
        }
    }

    /// <summary>
    /// アニメーションの実行シーケンスを管理するコルーチン
    /// </summary>
    private IEnumerator LineHandleAnimationSequence()
	{
		m_isSequenceRunning = true; // シーケンス開始フラグを立てる

		// ラインアニメーションを開始し、完了を待つ
		if (m_lineAnimation != null && !m_chainAvnimation)
		{
            // SEの再生
            AudioController.Instance.PlaySFX(m_lineAnimation_SE, settings.effectVolume * (4 + SE_volume_adjustment));
            // アニメーション再生
            StartCoroutine(m_lineAnimation.AnimateGroups());
            // ラインアニメ―ションが完了するまで待機
            yield return new WaitForSeconds(0.8f);
		}

		
		//StartCoroutine(SpawnPrefabsWithDelay());

        // ★ 通常のプレハブ生成の後に、新しいコルーチンを開始
        StartCoroutine(SpawnEmissionPrefabsWithDelay());

        Play_SE = true;
        // ラインアニメーション完了後にプレハブ生成コルーチンを開始
        yield return StartCoroutine(SpawnPrefabsWithDelay());

        m_isSequenceRunning = false; // シーケンス終了フラグをリセット
		m_chainAvnimation = true; // シーケンス開始フラグを立てる
        
		// ★
		// 演出がまだ再生されていない場合にのみ呼び出す
		//if (!m_hasPlayedCoreAnim)
		//{
		//	if (m_verticalFadeController != null)
		//	{
		//		m_verticalFadeController[0].OnFourCoresCollected();
		//		m_verticalFadeController[1].OnFourCoresCollected();
		//		m_hasPlayedCoreAnim = true;
		//	}
		//}
		// ------

		m_chainAvnimation = false; // シーケンス終了フラグをリセット
            

    }

	/// <summary>
	/// アニメーションとメッシュレンダラーの表示・非表示を制御するコルーチン
	/// </summary>
	private IEnumerator HandleAnimationSequence()
    {
        Vector3 newPosition = new Vector3(m_blockAction.origin.x, m_blockAction.lastGroundLevel, m_blockAction.origin.z);
        transform.position = newPosition;

        Vector3 newScale = transform.localScale;
        newScale.y = 1f;
        transform.localScale = newScale;

        // 1. Groundのメッシュレンダラーを非表示にする
        //SetGroundMeshRenderer(false);

        // 2. BlockAnimationのAnimateGlowコルーチンを開始し、終了するまで待機する
        if (blockAnimation != null && m_blockAction.lastGroundLevel != -4)
        {
            yield return StartCoroutine(blockAnimation.AnimateGlow());
        }
        else
        {
            //SetGroundMeshRenderer(true);
        }
    }

    private void OnDissolveAnimationStart()
    {
        // ディゾルブアニメーション開始時にメッシュレンダラーを有効にする
        //SetGroundMeshRenderer(true);
    }

    /// <summary>
    /// 指定された高さにあるGroundのメッシュレンダラーの表示/非表示を切り替える
    /// </summary>
    private void SetGroundMeshRenderer(bool isEnabled)
    {
        GameObject[] groundObjs = GameObject.FindGameObjectsWithTag("Ground");
        foreach (var groundObj in groundObjs)
        {
            if (Mathf.Abs(groundObj.transform.position.y - m_blockAction.lastGroundLevel) < 0.1f && m_blockAction.lastGroundLevel != -4)
            {
                MeshRenderer meshRenderer = groundObj.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = isEnabled;
                }
            }
        }
    }

	// ★
	//　アニメーションリセット
	public void ResetCoreAnimFlag()
	{
		m_hasPlayedCoreAnim = false;
	}

    private IEnumerator PlayShortClipPart(AudioClip clip, float duration)
    {
        // 一時的なAudioSourceを作成
        AudioSource tempAudioSource = gameObject.AddComponent<AudioSource>();
        tempAudioSource.clip = clip;
        tempAudioSource.volume = settings.effectVolume * (SE_volume + SE_volume_adjustment);
        tempAudioSource.Play();

        // 指定した再生時間だけ待機
        yield return new WaitForSeconds(duration);

        // 再生を停止し、AudioSourceを破棄
        tempAudioSource.Stop();
        Destroy(tempAudioSource);
    }
}