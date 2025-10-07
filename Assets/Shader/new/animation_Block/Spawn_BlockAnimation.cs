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

	// �����o�[�ϐ��̐錾
	[Header("Dependencies")]
    [SerializeField] private BlockAction m_blockAction;
    [SerializeField] private BlockAnimation blockAnimation;
    [SerializeField] private LineAnimation m_lineAnimation;
    private GameSettings settings;

    // �A���O���b�g�u���b�N�p�̕ϐ�
    [Header("Spawn Prefab Settings")]
    [SerializeField] private GameObject m_prefabToSpawn;
    [SerializeField] private float m_spawnDelay = 0.1f;
    [SerializeField] private float m_startSpawnDelay = 0.5f;

    // �A�������u���b�N�p�̕ϐ�
    [Header("Emission Prefab Settings")]
    [SerializeField] private GameObject m_emissionPrefabToSpawn;
    [SerializeField] private float m_emissionSpawnDelay = 0.1f;
    [SerializeField] private float m_emissionStartSpawnDelay = 0.2f; // SpawnPrefabsWithDelay���x�点�邽�߂̃f�B���C

    // ��
    [Header("Shaders")]
	[SerializeField] VerticalFadeController[] m_verticalFadeController;
	private bool m_hasPlayedCoreAnim = false; // ���o���Đ����ꂽ���ǂ����̃t���O
	private bool m_isSequenceRunning = false; // ���C���A�j���[�V�����I���t���O
    private bool m_chainAvnimation = false; // Chain�A�j���[�W�����I���t���O

	private bool m_isFirstBlockPlaced = false;
    private float m_initialYPosition;
    private float m_lastGroundLevelCache;

    [Header("Audio")]
    [SerializeField] AudioClip m_lineAnimation_SE;
    [SerializeField] AudioClip m_chainAnimation_SE;
    [SerializeField] AudioClip m_emissionBlockAnimation_SE;
    [SerializeField] float SE_volume = 0.01f;
    [SerializeField] float SE_chainAnimation_limit = 8;
    [SerializeField] float SE_volume_adjustment = 5; // SE�̉��ʒ���
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
        // BlockAnimation�̃C�x���g�Ƀ��\�b�h��o�^
        //blockAnimation.OnDissolveStart += OnDissolveAnimationStart;
    }

    private void OnDestroy()
    {
        // �X�N���v�g���j�������Ƃ��ɃC�x���g�̓o�^������
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

			// �n�ʂ̍������ς�����Ƃ��A���V�[�P���X�����s���łȂ��ꍇ
			if (m_blockAction.lastGroundLevel != m_lastGroundLevelCache && !m_isSequenceRunning)
			{
				// �R�A�A�C�����[�V�����̍Đ�
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
    /// �v���n�u��x���𔺂��Đ�������R���[�`��
    /// </summary>
    private IEnumerator SpawnPrefabsWithDelay()
    {
        // �����J�n�^�C�~���O�𒲐����邽�߂̃f�B���C
        yield return new WaitForSeconds(30 / AudioController.Instance.bpm);
        

        float currentY = m_blockAction.lastPlacedCorePos_y + 1f;
        float Spawnpos = m_blockAction.lastPlacedCorePos_y - 1f;

        float Count_SE = 0;
        


        // lastGroundLevel����-4�܂�1�����������Ȃ���v���n�u�𐶐�
        while (Spawnpos >= m_lastGround || currentY <= m_blockAction.GetHighestPoint().y)
        {
            Vector3 downspawnPosition = new Vector3(m_blockAction.origin.x, Spawnpos, m_blockAction.origin.z);
			Vector3 upspawnPosition = new Vector3(m_blockAction.origin.x, currentY, m_blockAction.origin.z);

			// �v���n�u�̐���
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

            // SE�̍Đ����W�b�N
            // ���̃��[�v���Ō�̃u���b�N�����ɂȂ邩���`�F�b�N
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

            // ���̐����܂ł̃f�B���C
            yield return new WaitForSeconds(m_spawnDelay);
            currentY++;
			Spawnpos--;
            Count_SE++;
        }
    }

    /// <summary>
    /// Emission_Block�v���n�u�𐶐�����V�����R���[�`��
    /// </summary>
    private IEnumerator SpawnEmissionPrefabsWithDelay()
    {

		yield return new WaitForSeconds(m_emissionStartSpawnDelay);
        AudioController.Instance.PlaySFX(m_emissionBlockAnimation_SE, settings.effectVolume * (4 + SE_volume_adjustment));

        // �� �����J�n�ʒu�� lastPlacedCorePos_y �ɐݒ�
        float currentY = m_blockAction.lastPlacedCorePos_y;
        float Spawnpos = m_blockAction.lastPlacedCorePos_y;

        while (Spawnpos >= m_lastGround || currentY <= m_blockAction.GetHighestPoint().y)
        {
            Vector3 downspawnPosition = new Vector3(m_blockAction.origin.x, Spawnpos, m_blockAction.origin.z);
            Vector3 upspawnPosition = new Vector3(m_blockAction.origin.x, currentY, m_blockAction.origin.z);

            if (m_emissionPrefabToSpawn != null)
            {
                // �� �ŏ��̃��[�v��lastPlacedCorePos_y�̏㉺�𐶐�
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
    /// �A�j���[�V�����̎��s�V�[�P���X���Ǘ�����R���[�`��
    /// </summary>
    private IEnumerator LineHandleAnimationSequence()
	{
		m_isSequenceRunning = true; // �V�[�P���X�J�n�t���O�𗧂Ă�

		// ���C���A�j���[�V�������J�n���A������҂�
		if (m_lineAnimation != null && !m_chainAvnimation)
		{
            // SE�̍Đ�
            AudioController.Instance.PlaySFX(m_lineAnimation_SE, settings.effectVolume * (4 + SE_volume_adjustment));
            // �A�j���[�V�����Đ�
            StartCoroutine(m_lineAnimation.AnimateGroups());
            // ���C���A�j���\�V��������������܂őҋ@
            yield return new WaitForSeconds(0.8f);
		}

		
		//StartCoroutine(SpawnPrefabsWithDelay());

        // �� �ʏ�̃v���n�u�����̌�ɁA�V�����R���[�`�����J�n
        StartCoroutine(SpawnEmissionPrefabsWithDelay());

        Play_SE = true;
        // ���C���A�j���[�V����������Ƀv���n�u�����R���[�`�����J�n
        yield return StartCoroutine(SpawnPrefabsWithDelay());

        m_isSequenceRunning = false; // �V�[�P���X�I���t���O�����Z�b�g
		m_chainAvnimation = true; // �V�[�P���X�J�n�t���O�𗧂Ă�
        
		// ��
		// ���o���܂��Đ�����Ă��Ȃ��ꍇ�ɂ̂݌Ăяo��
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

		m_chainAvnimation = false; // �V�[�P���X�I���t���O�����Z�b�g
            

    }

	/// <summary>
	/// �A�j���[�V�����ƃ��b�V�������_���[�̕\���E��\���𐧌䂷��R���[�`��
	/// </summary>
	private IEnumerator HandleAnimationSequence()
    {
        Vector3 newPosition = new Vector3(m_blockAction.origin.x, m_blockAction.lastGroundLevel, m_blockAction.origin.z);
        transform.position = newPosition;

        Vector3 newScale = transform.localScale;
        newScale.y = 1f;
        transform.localScale = newScale;

        // 1. Ground�̃��b�V�������_���[���\���ɂ���
        //SetGroundMeshRenderer(false);

        // 2. BlockAnimation��AnimateGlow�R���[�`�����J�n���A�I������܂őҋ@����
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
        // �f�B�]���u�A�j���[�V�����J�n���Ƀ��b�V�������_���[��L���ɂ���
        //SetGroundMeshRenderer(true);
    }

    /// <summary>
    /// �w�肳�ꂽ�����ɂ���Ground�̃��b�V�������_���[�̕\��/��\����؂�ւ���
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

	// ��
	//�@�A�j���[�V�������Z�b�g
	public void ResetCoreAnimFlag()
	{
		m_hasPlayedCoreAnim = false;
	}

    private IEnumerator PlayShortClipPart(AudioClip clip, float duration)
    {
        // �ꎞ�I��AudioSource���쐬
        AudioSource tempAudioSource = gameObject.AddComponent<AudioSource>();
        tempAudioSource.clip = clip;
        tempAudioSource.volume = settings.effectVolume * (SE_volume + SE_volume_adjustment);
        tempAudioSource.Play();

        // �w�肵���Đ����Ԃ����ҋ@
        yield return new WaitForSeconds(duration);

        // �Đ����~���AAudioSource��j��
        tempAudioSource.Stop();
        Destroy(tempAudioSource);
    }
}