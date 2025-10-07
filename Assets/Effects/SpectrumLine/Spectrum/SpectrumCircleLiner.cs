using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SpectrumCircleLiner: MonoBehaviour
{
	public enum SpectrumMode
	{
		AudioSourceOnly,
		AudioListenerAll
	}

	[Header("設定")]
	[SerializeField] private SpectrumMode mode = SpectrumMode.AudioSourceOnly;
	[SerializeField] private AudioSource targetAudioSource;
	[SerializeField, Range(1, 5000)] private float ampGain = 300;
	[SerializeField] private FFTWindow fftWindow = FFTWindow.BlackmanHarris;
	[SerializeField] private float baseRadius = 5f; // 円の基本半径

	private const int RESOLUTION = 1024;
	private LineRenderer lineRenderer;
	private readonly Vector3[] positions = new Vector3[RESOLUTION];
	private float[] spectrum = new float[RESOLUTION];

	void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.useWorldSpace = false; // ローカル座標で描画
	}

	void Start()
	{
		lineRenderer.positionCount = RESOLUTION;

		if (mode == SpectrumMode.AudioSourceOnly && targetAudioSource != null && !targetAudioSource.isPlaying)
		{
			targetAudioSource.loop = true;
			targetAudioSource.Play();
		}
	}

	void Update()
	{
		DrawSpectrum();
	}

	private void DrawSpectrum()
	{
		// スペクトラム取得
		if (mode == SpectrumMode.AudioSourceOnly)
		{
			if (targetAudioSource == null || !targetAudioSource.isPlaying) return;
			targetAudioSource.GetSpectrumData(spectrum, 0, fftWindow);
		}
		else
		{
			AudioListener.GetSpectrumData(spectrum, 0, fftWindow);
		}

		// ローカル座標で円形に配置
		for (int i = 0; i < RESOLUTION; i++)
		{
			float angle = (i / (float)RESOLUTION) * Mathf.PI * 2f;
			float radius = baseRadius + spectrum[i] * ampGain;
			float x = Mathf.Cos(angle) * radius;
			float y = Mathf.Sin(angle) * radius;
			positions[i] = new Vector3(x, y, 0);
		}

		lineRenderer.SetPositions(positions);
	}
}