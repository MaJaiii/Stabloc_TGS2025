// BlockGlowController.cs
using UnityEngine;

public class BlockGlowController : MonoBehaviour
{
    // C#���璼�ڃI�t�Z�b�g��ݒ肷�邽�߂̃p�u���b�N���\�b�h
    public void SetBlinkOffset(float offset)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            // renderer.sharedMaterial����V�����}�e���A���C���X�^���X���쐬
            // renderer.material�Ɋ��蓖��
            renderer.material = new Material(renderer.sharedMaterial);


            // �I�t�Z�b�g���}�e���A���̃C���X�^���X�ɐݒ�
            renderer.material.SetFloat("_BlinkOffset", offset);
        }
    }
}