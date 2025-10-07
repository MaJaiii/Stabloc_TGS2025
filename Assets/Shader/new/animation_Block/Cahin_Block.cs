using UnityEngine;
using System.Collections;

public class Cahin_Block : MonoBehaviour
{
    // �A�j���[�V�����̃p�����[�^
    [SerializeField] private float m_pulseDuration = 0.5f; // �ۓ���1���̒���
    [SerializeField] private float m_maxScale = 1.5f; // �g�厞�̍ő�X�P�[���l

    private Material m_material;

    private void Start()
    {
        // MeshRenderer����}�e���A���C���X�^���X���擾
        // ���̃C���X�^���X�͕�������邽�߁A���̃I�u�W�F�N�g�̃}�e���A���ɉe����^���܂���B
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            m_material = meshRenderer.material;
            // �A�j���[�V�����R���[�`�����J�n
            StartCoroutine(HeartbeatAnimation());
        }
        else
        {
            Debug.LogError("MeshRenderer not found on this GameObject.", this);
            Destroy(gameObject); // ���b�V�������_���[���Ȃ��ꍇ�͎��g��j��
        }
    }

    private IEnumerator HeartbeatAnimation()
    {
        // �ۓ��i�g��j
        float elapsedTime = 0f;
        while (elapsedTime < m_pulseDuration)
        {
            elapsedTime += Time.deltaTime;
            // 0����1�֌�������Ԓl
            float t = elapsedTime / m_pulseDuration;
            // �X���[�Y�ȉ����E�����̂��߂ɓ񎟊֐���K�p
            float scaleValue = Mathf.Lerp(1.0f, m_maxScale, t * t);
            m_material.SetFloat("_ObjectScale", scaleValue);
            yield return null;
        }

        // �k���i���̃T�C�Y�ɖ߂�j
        elapsedTime = 0f;
        while (elapsedTime < m_pulseDuration)
        {
            elapsedTime += Time.deltaTime;
            // 1����0�֌�������Ԓl
            float t = elapsedTime / m_pulseDuration;
            float scaleValue = Mathf.Lerp(m_maxScale, 1.0f, t);
            m_material.SetFloat("_ObjectScale", scaleValue);
            yield return null;
        }

        // �A�j���[�V�����I����A���g��j��
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // �V�[������I�u�W�F�N�g���j�������Ƃ��ɁA�}�e���A�����N���[���A�b�v����
        if (m_material != null)
        {
            Destroy(m_material);
        }
    }
}