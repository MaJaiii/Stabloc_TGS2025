using System.Collections;
using UnityEngine;
using DG.Tweening;

public class VerticalFadeController : MonoBehaviour
{
    // �����o�[�ϐ��̐錾
    [Header("Dependencies")]
    [SerializeField] private BlockAction m_blockAction;
    [SerializeField] private MeshRenderer m_meshRenderer;
    [SerializeField] private Spawn_BlockAnimation m_spawnAnimation;

	[Header("Settings")]
    [SerializeField] private float m_initialFadeDuration = 0.5f; // �ŏ��̃t�F�[�h�C���ɂ����鎞��
    [SerializeField] private float m_coreGlowDuration = 0.5f;    // �R�A�擾���̔����A�j���[�V��������
    [SerializeField] private float m_coreGlowIntensity = 0.8f;   // �R�A�擾���̍ő�s�����x
    [SerializeField] private float m_fadeIntensity = 0.8f;   // fade�̍ő�l
    [SerializeField] private float m_growthOffset = 2f;          // lastGroundLevel����̐����I�t�Z�b�g
    [SerializeField] private float m_animDelay = 0.1f;           // �R�A���o�J�n�O�̑ҋ@����

    private Material m_material;
    private Vector3 m_initialScale;
    private bool m_isManuallyPositioned = false;
    private bool m_isFirstBlockPlaced = false;
    private float m_initialYPosition; // �ŏ���Y���W��ۑ�

    GameSettings settings;

	private void Start()
    {
        // MeshRenderer�R���|�[�l���g���擾
        if (m_meshRenderer == null)
        {
            m_meshRenderer = GetComponent<MeshRenderer>();
        }

        // �}�e���A���̃C���X�^���X���쐬���đ���
        m_material = new Material(m_meshRenderer.sharedMaterial);
        m_meshRenderer.material = m_material;

        // �����ݒ�
        m_initialScale = transform.localScale;
        m_initialYPosition = transform.position.y;

        // _OverallOpacity��_FadeRange�������l��0�ɐݒ肵�A�ŏ��͔�\���ɂ���
        m_material.SetFloat("_OverallOpacity", 0);
        m_material.SetFloat("_FadeRange", 0);
    }

    private void Update()
    {
        // �R�A���o���Đ����łȂ��ꍇ�AlastGroundLevel�ɍ��킹�ăX�P�[���ƈʒu���X�V
        if (!m_isManuallyPositioned)
        {
            if (GameStatus.gameState == GAME_STATE.GAME_INGAME)
            {
                UpdateScaleAndPositionBasedOnGroundLevel();
            }
        }
		

	}

    /// <summary>
    /// lastGroundLevel�ɍ��킹�ăX�P�[���ƈʒu���X�V����
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
    /// 4�̃R�A�����������̉��o
    /// </summary>
    public void OnFourCoresCollected()
    {
        if (m_blockAction == null) return;

        m_blockAction.isAnim = true;

        // �܂��ʒu�ƃX�P�[���𑦎��X�V
        UpdateScaleAndPositionBasedOnGroundLevel();

        // �R���[�`���Łu��Ɉʒu�m�� �� �����ҋ@ �� ����A�j���[�V�����v
        StartCoroutine(StartAnimWithDelay(m_animDelay));
    }

    private IEnumerator StartAnimWithDelay(float delay)
    {

        // �����ҋ@�ilastGroundLevel�̍X�V���O���z�����邽�߁j
        yield return new WaitForSeconds(delay);

		m_meshRenderer.enabled = true;

		// DOTween�A�j���[�V�����J�n
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
