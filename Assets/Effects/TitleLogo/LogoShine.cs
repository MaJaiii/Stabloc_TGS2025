using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LogoShine : MonoBehaviour
{
	[SerializeField] private List<Image> targetImages = new List<Image>();

	[SerializeField] private float edgeBase = 1f;
	[SerializeField] private float surfaceBase = 1f;
	[SerializeField] private float edgeBoost = 3f;
	[SerializeField] private float surfaceBoost = 2f;
	[SerializeField] private float decaySpeed = 2f;

	private float currentEdge;
	private float currentSurface;

	private List<Material> mats = new List<Material>();

	private void Start()
	{
		// 各 Image のマテリアルをインスタンス化して保持
		foreach (var img in targetImages)
		{
			if (img != null)
			{
				mats.Add(img.material); // material プロパティでインスタンス化
			}
		}

		currentEdge = edgeBase;
		currentSurface = surfaceBase;

		AudioController.Instance.OnBeat += HandleBeat;
	}

	private void OnDestroy()
	{
		if (AudioController.Instance != null)
			AudioController.Instance.OnBeat -= HandleBeat;
	}

	private void HandleBeat(int beatCount)
	{
		currentEdge = edgeBoost;
		currentSurface = surfaceBoost;
	}

	private void Update()
	{
		currentEdge = Mathf.MoveTowards(currentEdge, edgeBase, decaySpeed * Time.deltaTime);
		currentSurface = Mathf.MoveTowards(currentSurface, surfaceBase, decaySpeed * Time.deltaTime);

		foreach (var mat in mats)
		{
			if (mat != null)
			{
				mat.SetFloat("_EdgeShine", currentEdge);
				mat.SetFloat("_SurfaceShine", currentSurface);
			}
		}
	}
}