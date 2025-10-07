/*old
using UnityEngine;

public class SpectrumScalerAll : MonoBehaviour
{
	[SerializeField] private FFTWindow fftWindow = FFTWindow.BlackmanHarris;
	[SerializeField, Range(1, 1000)] private float ampGain = 50f;
	[SerializeField] private int spectrumIndex = 10; // 反応させたい周波数帯
	[SerializeField] private float smoothSpeed = 10f; // スムージング速度

	private const int RESOLUTION = 1024;
	private float[] spectrum = new float[RESOLUTION];
	private Vector3 baseScale;

	void Start()
	{
		baseScale = transform.localScale;
	}

	void Update()
	{
		// シーン全体の音を取得
		AudioListener.GetSpectrumData(spectrum, 0, fftWindow);

		// 指定インデックスの周波数帯の値を取得
		float intensity = spectrum[spectrumIndex] * ampGain;

		// スムージングしてスケール変更
		AudioController.Instance
		float scaleValue = Mathf.Lerp(transform.localScale.x, baseScale.x + intensity, Time.deltaTime * smoothSpeed);
		transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
	}
}*/
using UnityEngine;

public class SpectrumScalerAll : MonoBehaviour
{
	[SerializeField] private FFTWindow fftWindow = FFTWindow.BlackmanHarris;
	[SerializeField, Range(1, 1000)] private float ampGain = 50f;
	[SerializeField] private int spectrumIndex = 10; // 反応させたい周波数帯
	[SerializeField] private float smoothSpeed = 10f; // スムージング速度
	[SerializeField] private float beatScaleBoost = 0.5f; // ビート時の追加サイズ

	private const int RESOLUTION = 1024;
	private float[] spectrum = new float[RESOLUTION];
	private Vector3 baseScale;

	private void Start()
	{
		baseScale = transform.localScale;

		// AudioControllerのOnBeatイベントに登録
		if (AudioController.Instance != null)
			AudioController.Instance.OnBeat += OnBeatReaction;
	}

	private void OnDestroy()
	{
		// イベント登録解除
		if (AudioController.Instance != null)
			AudioController.Instance.OnBeat -= OnBeatReaction;
	}

	private void Update()
	{
		//// 常時スペクトラム解析（ビート時の強調に使う）
		//AudioListener.GetSpectrumData(spectrum, 0, fftWindow);
	}

	// ビートが来たときに呼ばれる
	private void OnBeatReaction(int beat)
	{
		// 指定周波数帯の強度を取得
		float intensity = spectrum[spectrumIndex] * ampGain;

		// ビート時にスケールを一瞬大きくする
		float targetScale = baseScale.x + intensity + beatScaleBoost;

		// スムーズに拡大・縮小
		StopAllCoroutines();
		StartCoroutine(ScaleRoutine(targetScale));
	}

	private System.Collections.IEnumerator ScaleRoutine(float target)
	{
		float t = 0f;
		Vector3 start = transform.localScale;
		Vector3 end = new Vector3(target, target, target);

		while (t < 1f)
		{
			t += Time.deltaTime * smoothSpeed;
			transform.localScale = Vector3.Lerp(start, end, t);
			yield return null;
		}

		// 元のサイズに戻す
		t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * smoothSpeed;
			transform.localScale = Vector3.Lerp(end, baseScale, t);
			yield return null;
		}
	}
}