using UnityEngine;
using UnityEngine.Rendering;

public class SpectrumLiner : MonoBehaviour
{
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    private AudioSource audiosource;

    [SerializeField] private int visible = 128; // �`�搔
    [SerializeField] private float waveLenght;  // �g�`�̍L��
    [SerializeField] private float yLength;     // �g�`�̍���

    private const int FFT_RESOLUTION = 128; // FFT�̃T���v����
    private float[] spectrum = null;        // �X�y�N�g�����z��
    private Vector3[] points = null;        // �`��|�C���g�z��


    void Start()
    {
        // �z��p���\�b�h
        Prepare();
    }
    /// <summary>
    /// �X�y�N�g�����ƕ`��|�C���g�̔z�����郁�\�b�h
    /// </summary>
    public void Prepare()
    {
        // �X�y�N�g�����z��(FFT�̃T���v����)�����
        spectrum = new float[FFT_RESOLUTION];
        // �`��|�C���g�z��(�`�悵������)�����
        points = new Vector3[visible + 1];
    }

    // Update is called once per frame
    void Update()
    {
        // �`��p���\�b�h
        LineRender();
    }


    /// <summary>
    /// �X�y�N�g�����̔z��ɉ����ă��C����`�悷�郁�\�b�h
    /// </summary>
    private void LineRender()
    {
        // �X�y�N�g�����̐��l�f�[�^���X�y�N�g�����z��ɓ����
        audiosource.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        //// �g�`�̍L������u�`��̃X�^�[�g�n�_�v�Ɓu�f�[�^�̊��o�v�����߂�
        var xStart = -waveLenght / 2;
        var xStep = waveLenght / spectrum.Length;

        var r = spectrum[0] * yLength;
        
        // var rad = Mathf.Deg2Rad;

        for (var i = 0; i < visible; i++)
        {
			// �X�y�N�g�����́u�f�[�^���l�v����f�J���g���W(x,y)�����߂�

			var y = spectrum[i] * yLength;
            var x = xStart + xStep * i;

            // �`�悷��|�C���g�̔z��Ɂu���߂��f�J���g���W�̈ʒu�v������
            points[i] = new Vector3(x, y, 0) + transform.position;

            if (points == null) return; 

            // ���C�������_���[�̃|�C���g����[�`��|�C���g�z��̐�]�ɍ��킹��
            // �e�|�C���g�Ɂu�|�C���g�̔z��̈ʒu�i���߂��ʒu���j�v��ݒ肷��
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);


        }
    }
}
