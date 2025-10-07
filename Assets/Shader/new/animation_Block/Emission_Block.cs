using System.Collections;
using UnityEngine;

public class Emission_Block : MonoBehaviour
{
    // �e�v���Z�X�̎��Ԑݒ�
    [Header("�t�F�[�h�C���̎���")]
    [SerializeField]
    private float fadeTime = 2f;

    [Header("�ҋ@����")]
    [SerializeField]
    private float waitTime = 1f;

    [Header("�f�B�]���u�̎���")]
    [SerializeField]
    private float dissolveTime = 3f;

    private Renderer myRenderer;
    private Material myMaterial;

    // �V�F�[�_�[�v���p�e�B��ID���L���b�V��
    private int glowFalloffID;
    private int dissolveAmountID;

    private void Awake()
    {
        // �����_���[�ƃ}�e���A�����擾
        myRenderer = GetComponent<Renderer>();
        myMaterial = myRenderer.material; // .material�͐V�����C���X�^���X���쐬���܂�

        // �V�F�[�_�[�v���p�e�B��ID���擾���A�p�t�H�[�}���X�����コ����
        glowFalloffID = Shader.PropertyToID("_GlowFalloff");
        dissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    }

    private void Start()
    {
        // �t�F�[�h�A�E�g�ƃf�B�]���u�̃R���[�`�����J�n
        StartCoroutine(GlowDissolveSequence());
    }

    private IEnumerator GlowDissolveSequence()
    {
        // �X�e�b�v1: _GlowFalloff��50����0�փA�j���[�V����
        float startGlowFalloff = 1f;
        float endGlowFalloff = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            float newGlowFalloff = Mathf.Lerp(startGlowFalloff, endGlowFalloff, elapsedTime / fadeTime);
            myMaterial.SetFloat(glowFalloffID, newGlowFalloff);
            elapsedTime += Time.deltaTime;
            yield return null; // ���̃t���[���܂őҋ@
        }
        myMaterial.SetFloat(glowFalloffID, endGlowFalloff); // �ŏI�l���m��

        // �X�e�b�v2: �����ҋ@
        yield return new WaitForSeconds(waitTime);

        // �X�e�b�v3: _DissolveAmount��0����1�փA�j���[�V����
        float startDissolveAmount = 0f;
        float endDissolveAmount = 1f;
        elapsedTime = 0f;

        while (elapsedTime < dissolveTime)
        {
            float newDissolveAmount = Mathf.Lerp(startDissolveAmount, endDissolveAmount, elapsedTime / dissolveTime);
            myMaterial.SetFloat(dissolveAmountID, newDissolveAmount);
            elapsedTime += Time.deltaTime;
            yield return null; // ���̃t���[���܂őҋ@
        }
        myMaterial.SetFloat(dissolveAmountID, endDissolveAmount); // �ŏI�l���m��

        // �X�e�b�v4: �I�u�W�F�N�g��j��
        Destroy(gameObject);
    }
}