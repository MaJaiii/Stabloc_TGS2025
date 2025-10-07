using UnityEngine;

public class ColorTest : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    void Start()
    {
        // ���̃X�N���v�g���A�^�b�`���ꂽ�I�u�W�F�N�g��MeshRenderer�R���|�[�l���g���擾
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        // J�L�[�������ꂽ��A�}�e���A����_BackgroundColor��ΐF�ɐݒ�
        if (Input.GetKeyDown(KeyCode.J))
        {
            meshRenderer.material.SetColor("_BackgroundColor", Color.green);
            Debug.Log("Color changed to Green.");
        }

        // K�L�[�������ꂽ��A�}�e���A����_BackgroundColor��ԐF�ɐݒ�
        if (Input.GetKeyDown(KeyCode.K))
        {
            meshRenderer.material.SetColor("_BackgroundColor", Color.red);
            Debug.Log("Color changed to Red.");
        }
    }
}