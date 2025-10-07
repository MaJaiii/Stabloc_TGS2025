using UnityEngine;
using System.Collections;

public class BlockAnimation : MonoBehaviour
{
	// �A�j���[�V�����̎��Ԃ�b�P�ʂŐݒ�
	[Header("AnimationTimer")]
	public float animationDuration = 2.0f;

	// �A�j���[�V�����I����̑ҋ@���Ԃ�ݒ�
	[Header("EndAnimationDelay")]
	public float postAnimationDelay = 0.2f;

	// �f�B�]���u�\���̌��ʎ���
	[Header("DissolveTimer")]
	public float DissolveDuration = 0.2f;

	// �A�j���[�V����������J�n�l�ƏI���l
	[Header("GlowIntensity")]
	public float startGlowIntensity = 0.5f;
	public float endGlowIntensity = 2.0f;

	[Header("GlowFalloff")]
	public float startGlowFalloff = 0.5f;
	public float endGlowFalloff = 2.0f;

	// �f�B�]���u
	private float startDissolveAmount = 0.0f;
	private float endDissolveAmount = 1.0f;

	// �A�j���[�V���������s����L�[
	public KeyCode animationKey = KeyCode.Space;

	// �V�F�[�_�[�̃v���p�e�B��
	private const string glowIntensityPropertyName = "_GlowIntensity";
	private const string glowFalloffPropertyName = "_GlowFalloff";
	private const string dissolvePropertyName = "_DissolveAmount";

	// Renderer��Material�̃C���X�^���X���i�[����z��
	private Renderer[] targetRenderers;
	private Material[] targetMaterials;
	private bool isAnimating = false;

	// �f�B�]���u�J�n���ɌĂяo���C�x���g
	public System.Action OnDissolveStart;

	public AudioClip emissionSE;

	GameSettings settings;

	void Start()
	{

		settings = CsvSettingsLoader.Load();
		targetRenderers = GetComponentsInChildren<Renderer>();

		if (targetRenderers.Length > 0)
		{
			targetMaterials = new Material[targetRenderers.Length];
			for (int i = 0; i < targetRenderers.Length; i++)
			{
				targetMaterials[i] = targetRenderers[i].material;
				targetRenderers[i].enabled = false;
			}
		}
		else
		{
			Debug.LogError("Renderer��������܂���B");
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(animationKey) && !isAnimating)
		{
			StartCoroutine(AnimateGlow());
		}
	}

	public void StartAnimation()
	{
		StartCoroutine(AnimateGlow());
	}

	public IEnumerator AnimateGlow()
	{
		isAnimating = true;
		// �A�j���[�V�����J�n���ɑS�Ẵ����_���[��L���ɂ���
		foreach (Renderer rend in targetRenderers)
		{
			if (rend != null)
			{
				rend.enabled = true;
			}
		}
		foreach (Material mat in targetMaterials)
		{
			if (mat != null)
			{
				mat.SetFloat(dissolvePropertyName, 0);
			}
		}

		float elapsedTime = 0f;
		AudioController.Instance.PlaySFX(emissionSE, settings.masterVolume);
		// �O���E�A�j���[�V�����̃��[�v
		while (elapsedTime < animationDuration)
		{
			float currentGlowIntensity = Mathf.Lerp(startGlowIntensity, endGlowIntensity, elapsedTime / animationDuration);
			float currentGlowFalloff = Mathf.Lerp(startGlowFalloff, endGlowFalloff, elapsedTime / animationDuration);

			foreach (Material mat in targetMaterials)
			{
				if (mat != null)
				{
					mat.SetFloat(glowIntensityPropertyName, currentGlowIntensity);
					mat.SetFloat(glowFalloffPropertyName, currentGlowFalloff);
				}
			}

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		foreach (Material mat in targetMaterials)
		{
			if (mat != null)
			{
				mat.SetFloat(glowIntensityPropertyName, endGlowIntensity);
				mat.SetFloat(glowFalloffPropertyName, endGlowFalloff);
			}
		}

		// �R�A�u���b�N�ƃR�A�̃��b�V�������_���[�����A���̃^�C�~���O�ŃI�t�ɂ���
		for (int i = 0; i < 8; i++)
		{
			if (targetRenderers[i] != null)
			{
				targetRenderers[i].enabled = false;
			}
		}

		yield return new WaitForSeconds(postAnimationDelay);

		// �C�x���g���Ăяo���A�f�B�]���u�A�j���[�V�����̊J�n��ʒm
		if (OnDissolveStart != null)
		{
			OnDissolveStart();
		}

		elapsedTime = 0f;

		// �f�B�]���u�A�j���[�V�����̃��[�v
		while (elapsedTime < DissolveDuration)
		{
			float currentDissolveAmount = Mathf.Lerp(startDissolveAmount, endDissolveAmount, elapsedTime / DissolveDuration);

			foreach (Material mat in targetMaterials)
			{
				if (mat != null)
				{
					mat.SetFloat(dissolvePropertyName, currentDissolveAmount);
				}
			}

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		foreach (Material mat in targetMaterials)
		{
			if (mat != null)
			{
				mat.SetFloat(dissolvePropertyName, endDissolveAmount);
			}
		}

		// �A�j���[�V�����I����ɑS�Ẵ����_���[�𖳌��ɂ���
		foreach (Renderer rend in targetRenderers)
		{
			if (rend != null)
			{
				rend.enabled = false;
			}
		}

		isAnimating = false;
	}

	void OnDisable()
	{
		if (targetMaterials != null)
		{
			foreach (Material mat in targetMaterials)
			{
				if (mat != null)
				{
					Destroy(mat);
				}
			}
		}
	}
}