using UnityEngine;
using System.Collections;

public class ShiningSpawnBlock : MonoBehaviour
{
	[SerializeField] float maxShinningIntensity = 30.0f;    // 輝く強さ  
	[SerializeField] float strongerDuration = 0.4f;         // 強くなる時間
	[SerializeField] float weakerDuration = 0.2f;           // 弱くなる時間
	[SerializeField] float smallerBlock = 0.2f;             // 小さくする時間

	float currentIntensity = 50.0f;  // 現在の光の強さ 
	float defaultIntensity = 10.0f;  // 通常の光の強さ

	BlockColor blockColor;      // BlockColorのスクリプト
	Color shiningCoror;         // 光の色
	MeshRenderer meshRenderer;
	Material material;

	void Start()
	{
		// Nextブロックの時は実行しない
		if (transform.root.gameObject.name != "ReleasePt")
		{
			transform.localScale = Vector3.zero;
			return;
		}

		// MeshRendererとMaterialを取得する
		meshRenderer = GetComponent<MeshRenderer>();
		if (!meshRenderer)
		{
			Debug.LogError("MeshRendererの設定が間違っています。");
		}
		material = meshRenderer.materials[0];

		// ブロックの色を取得する        
		blockColor = transform.parent.parent.gameObject.GetComponent<BlockColor>();
		if (!blockColor)
		{
			Debug.LogError("色が取得できていません。");
		}
		// 色を設定する
		shiningCoror = blockColor.blockColor;
		material.SetColor("_Color", shiningCoror);

		// 光の強さを設定する
		StartCoroutine(ShiningSetting());
	}

	// Update is called once per frame
	void Update()
	{
		// Nextブロックの時は実行しない
		if (transform.root.gameObject.name != "ReleasePt") { return; }

		// スケールがゼロの時削除する（再生終了）        
		if (transform.localScale == Vector3.zero)
		{
			Destroy(gameObject);
		}

		// マテリアルに光の強さを設定する
		material.SetFloat("_BlockIntensity", currentIntensity);
	}

	// 光の強さを設定する
	IEnumerator ShiningSetting()
	{
		// 光が強くなるコルーチン
		yield return ShiningBlock(defaultIntensity, maxShinningIntensity, strongerDuration);
		// 光が弱くなるコルーチン
		yield return ShiningBlock(maxShinningIntensity, defaultIntensity, weakerDuration);

		// スケールをゼロにする
		yield return transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, smallerBlock);
		// スケールを補正
		transform.localScale = Vector3.zero;
	}

	// ブロックを光らせる
	IEnumerator ShiningBlock(float startIntensity, float finishIntensity, float duration)
	{
		// 経過時間をカウントする変数
		float elapsedTimer = 0.0f;

		//設定時間経過するまで繰り返し
		while (elapsedTimer < duration)
		{
			// 時間をカウント
			elapsedTimer += Time.deltaTime;
			// 進行度の計算
			float time = Mathf.Clamp01(elapsedTimer / duration);

			// 光の強さを変更
			currentIntensity = Mathf.SmoothStep(startIntensity, finishIntensity, time);

			yield return null;
		}
	}
}