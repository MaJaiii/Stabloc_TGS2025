using UnityEngine;
using DG.Tweening;

public class Fade_surface : MonoBehaviour
{
	// �����o�[�ϐ��̐錾
	[Header("Dependencies")]
	[SerializeField] private BlockAction m_blockAction;
	[SerializeField] private MeshRenderer m_meshRenderer;
	[SerializeField] private GameOver m_gameover;

	[Header("Settings")]
	[SerializeField] private float m_objectHeightOffset = 2f;    // �n�ʂ���̍����̃I�t�Z�b�g

	private Material m_material;
	private bool m_isFirstBlockPlaced = false;
	private float m_initialYPosition;
	private Vector3 m_initialScale;


	private void Start()
	{
		// MeshRenderer�R���|�[�l���g���擾
		if (m_meshRenderer == null)
		{
			m_meshRenderer = GetComponent<MeshRenderer>();
		}

		

		// �I�u�W�F�N�g�̏���Y���W�Ə����X�P�[����ۑ�
		m_initialYPosition = transform.position.y;
		m_initialScale = transform.localScale;

		// �ŏ��̍������ق�0�ɐݒ�
		Vector3 currentScale = transform.localScale;
		currentScale.y = 0.001f;
		transform.localScale = currentScale;
	}

	private void Update()
	{
		// GameManager�̃X�e�[�^�X�Ő���
		if (GameStatus.gameState == GAME_STATE.GAME_INGAME)
		{
			// �ŏ��̃u���b�N���u���ꂽ�珉���z�u�ƃt�F�[�h�C�������s
			if (!m_isFirstBlockPlaced && m_blockAction.placedBlockCount > 0)
			{
				OnFirstBlockPlaced();
				m_isFirstBlockPlaced = true;
			}

			// lastGroundLevel�ɍ��킹�ăX�P�[���ƈʒu���X�V
			UpdateScaleAndPositionBasedOnGroundLevel();
		}
		// �Q�[���I�[�o�[��Ԃ̃`�F�b�N
		if (m_gameover.isGameOver)
		{
			m_meshRenderer.enabled = false;

		}
	}

	/// <summary>
	/// �ŏ��̃u���b�N���u���ꂽ�ۂ̏���
	/// </summary>
	private void OnFirstBlockPlaced()
	{
		// �ŏ��̃u���b�N��origin����X,Z���W���擾���AY���W�͏����ʒu�ɐݒ�
		Vector3 initialPosition = new Vector3(m_blockAction.origin.x, m_initialYPosition, m_blockAction.origin.z);
		transform.position = initialPosition;

		Vector3 currentScale = transform.localScale;
		currentScale.y = 1.01f;
		transform.localScale = currentScale;

		m_meshRenderer.enabled = true;
	}

	/// <summary>
	/// lastGroundLevel�̒l�Ɋ�Â��ăI�u�W�F�N�g��Y�X�P�[���ƈʒu���X�V
	/// </summary>
	private void UpdateScaleAndPositionBasedOnGroundLevel()
	{
		if (m_blockAction.lastGroundLevel == -4) return;

		// �n�ʂ���̍������v�Z
		float targetHeight = (m_blockAction.lastGroundLevel - m_initialYPosition) + m_objectHeightOffset;

		// �X�P�[���ύX�ƈʒu�␳
		Vector3 newScale = m_initialScale;
		newScale.y = targetHeight;
		transform.localScale = newScale;

		// �X�P�[���ύX�ɂ���ʂ̂����␳���AY���W�𒲐�
		Vector3 newPosition = transform.position;
		newPosition.y = m_initialYPosition + (targetHeight - m_initialScale.y) / 2f;
		transform.position = newPosition;
	}
	

}

