/*old
using UnityEngine;

public class SpectrumScalerAll : MonoBehaviour
{
	[SerializeField] private FFTWindow fftWindow = FFTWindow.BlackmanHarris;
	[SerializeField, Range(1, 1000)] private float ampGain = 50f;
	[SerializeField] private int spectrumIndex = 10; // ���������������g����
	[SerializeField] private float smoothSpeed = 10f; // �X���[�W���O���x

	private const int RESOLUTION = 1024;
	private float[] spectrum = new float[RESOLUTION];
	private Vector3 baseScale;

	void Start()
	{
		baseScale = transform.localScale;
	}

	void Update()
	{
		// �V�[���S�̂̉����擾
		AudioListener.GetSpectrumData(spectrum, 0, fftWindow);

		// �w��C���f�b�N�X�̎��g���т̒l���擾
		float intensity = spectrum[spectrumIndex] * ampGain;

		// �X���[�W���O���ăX�P�[���ύX
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
	[SerializeField] private int spectrumIndex = 10; // ���������������g����
	[SerializeField] private float smoothSpeed = 10f; // �X���[�W���O���x
	[SerializeField] private float beatScaleBoost = 0.5f; // �r�[�g���̒ǉ��T�C�Y

	private const int RESOLUTION = 1024;
	private float[] spectrum = new float[RESOLUTION];
	private Vector3 baseScale;

	private void Start()
	{
		baseScale = transform.localScale;

		// AudioController��OnBeat�C�x���g�ɓo�^
		if (AudioController.Instance != null)
			AudioController.Instance.OnBeat += OnBeatReaction;
	}

	private void OnDestroy()
	{
		// �C�x���g�o�^����
		if (AudioController.Instance != null)
			AudioController.Instance.OnBeat -= OnBeatReaction;
	}

	private void Update()
	{
		//// �펞�X�y�N�g������́i�r�[�g���̋����Ɏg���j
		//AudioListener.GetSpectrumData(spectrum, 0, fftWindow);
	}

	// �r�[�g�������Ƃ��ɌĂ΂��
	private void OnBeatReaction(int beat)
	{
		// �w����g���т̋��x���擾
		float intensity = spectrum[spectrumIndex] * ampGain;

		// �r�[�g���ɃX�P�[������u�傫������
		float targetScale = baseScale.x + intensity + beatScaleBoost;

		// �X���[�Y�Ɋg��E�k��
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

		// ���̃T�C�Y�ɖ߂�
		t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * smoothSpeed;
			transform.localScale = Vector3.Lerp(end, baseScale, t);
			yield return null;
		}
	}
}