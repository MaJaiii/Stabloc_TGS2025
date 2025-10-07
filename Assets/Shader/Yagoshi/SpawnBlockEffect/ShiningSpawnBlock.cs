using UnityEngine;
using System.Collections;

public class ShiningSpawnBlock : MonoBehaviour
{
	[SerializeField] float maxShinningIntensity = 30.0f;    // �P������  
	[SerializeField] float strongerDuration = 0.4f;         // �����Ȃ鎞��
	[SerializeField] float weakerDuration = 0.2f;           // �キ�Ȃ鎞��
	[SerializeField] float smallerBlock = 0.2f;             // ���������鎞��

	float currentIntensity = 50.0f;  // ���݂̌��̋��� 
	float defaultIntensity = 10.0f;  // �ʏ�̌��̋���

	BlockColor blockColor;      // BlockColor�̃X�N���v�g
	Color shiningCoror;         // ���̐F
	MeshRenderer meshRenderer;
	Material material;

	void Start()
	{
		// Next�u���b�N�̎��͎��s���Ȃ�
		if (transform.root.gameObject.name != "ReleasePt")
		{
			transform.localScale = Vector3.zero;
			return;
		}

		// MeshRenderer��Material���擾����
		meshRenderer = GetComponent<MeshRenderer>();
		if (!meshRenderer)
		{
			Debug.LogError("MeshRenderer�̐ݒ肪�Ԉ���Ă��܂��B");
		}
		material = meshRenderer.materials[0];

		// �u���b�N�̐F���擾����        
		blockColor = transform.parent.parent.gameObject.GetComponent<BlockColor>();
		if (!blockColor)
		{
			Debug.LogError("�F���擾�ł��Ă��܂���B");
		}
		// �F��ݒ肷��
		shiningCoror = blockColor.blockColor;
		material.SetColor("_Color", shiningCoror);

		// ���̋�����ݒ肷��
		StartCoroutine(ShiningSetting());
	}

	// Update is called once per frame
	void Update()
	{
		// Next�u���b�N�̎��͎��s���Ȃ�
		if (transform.root.gameObject.name != "ReleasePt") { return; }

		// �X�P�[�����[���̎��폜����i�Đ��I���j        
		if (transform.localScale == Vector3.zero)
		{
			Destroy(gameObject);
		}

		// �}�e���A���Ɍ��̋�����ݒ肷��
		material.SetFloat("_BlockIntensity", currentIntensity);
	}

	// ���̋�����ݒ肷��
	IEnumerator ShiningSetting()
	{
		// ���������Ȃ�R���[�`��
		yield return ShiningBlock(defaultIntensity, maxShinningIntensity, strongerDuration);
		// �����キ�Ȃ�R���[�`��
		yield return ShiningBlock(maxShinningIntensity, defaultIntensity, weakerDuration);

		// �X�P�[�����[���ɂ���
		yield return transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, smallerBlock);
		// �X�P�[����␳
		transform.localScale = Vector3.zero;
	}

	// �u���b�N�����点��
	IEnumerator ShiningBlock(float startIntensity, float finishIntensity, float duration)
	{
		// �o�ߎ��Ԃ��J�E���g����ϐ�
		float elapsedTimer = 0.0f;

		//�ݒ莞�Ԍo�߂���܂ŌJ��Ԃ�
		while (elapsedTimer < duration)
		{
			// ���Ԃ��J�E���g
			elapsedTimer += Time.deltaTime;
			// �i�s�x�̌v�Z
			float time = Mathf.Clamp01(elapsedTimer / duration);

			// ���̋�����ύX
			currentIntensity = Mathf.SmoothStep(startIntensity, finishIntensity, time);

			yield return null;
		}
	}
}