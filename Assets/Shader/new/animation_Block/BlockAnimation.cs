using UnityEngine;
using System.Collections;

public class BlockAnimation : MonoBehaviour
{
	// アニメーションの時間を秒単位で設定
	[Header("AnimationTimer")]
	public float animationDuration = 2.0f;

	// アニメーション終了後の待機時間を設定
	[Header("EndAnimationDelay")]
	public float postAnimationDelay = 0.2f;

	// ディゾルブ表現の効果時間
	[Header("DissolveTimer")]
	public float DissolveDuration = 0.2f;

	// アニメーションさせる開始値と終了値
	[Header("GlowIntensity")]
	public float startGlowIntensity = 0.5f;
	public float endGlowIntensity = 2.0f;

	[Header("GlowFalloff")]
	public float startGlowFalloff = 0.5f;
	public float endGlowFalloff = 2.0f;

	// ディゾルブ
	private float startDissolveAmount = 0.0f;
	private float endDissolveAmount = 1.0f;

	// アニメーションを実行するキー
	public KeyCode animationKey = KeyCode.Space;

	// シェーダーのプロパティ名
	private const string glowIntensityPropertyName = "_GlowIntensity";
	private const string glowFalloffPropertyName = "_GlowFalloff";
	private const string dissolvePropertyName = "_DissolveAmount";

	// RendererとMaterialのインスタンスを格納する配列
	private Renderer[] targetRenderers;
	private Material[] targetMaterials;
	private bool isAnimating = false;

	// ディゾルブ開始時に呼び出すイベント
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
			Debug.LogError("Rendererが見つかりません。");
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
		// アニメーション開始時に全てのレンダラーを有効にする
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
		// グロウアニメーションのループ
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

		// コアブロックとコアのメッシュレンダラーだけ、このタイミングでオフにする
		for (int i = 0; i < 8; i++)
		{
			if (targetRenderers[i] != null)
			{
				targetRenderers[i].enabled = false;
			}
		}

		yield return new WaitForSeconds(postAnimationDelay);

		// イベントを呼び出し、ディゾルブアニメーションの開始を通知
		if (OnDissolveStart != null)
		{
			OnDissolveStart();
		}

		elapsedTime = 0f;

		// ディゾルブアニメーションのループ
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

		// アニメーション終了後に全てのレンダラーを無効にする
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