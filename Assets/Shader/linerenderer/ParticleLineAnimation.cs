// ParticleAnimator.cs
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ParticleLineAnimation : MonoBehaviour
{
    // �p�[�e�B�N���G�t�F�N�g�̃v���n�u
    [SerializeField] private GameObject particlePrefab;
    // �㉺�ړ��̃A�j���[�V�����ɂ����鎞��
    [SerializeField] private float cycleDuration = 0.5f;
    // �p�[�e�B�N�����㉺�Ɉړ������
    [SerializeField] private int cycleCount = 3;
    // �p�[�e�B�N�������B����ڕW�̍����i�I�t�Z�b�g�j
    [SerializeField] private float targetHeightOffset = 5.0f;

    // BlockAction.cs�ւ̎Q�Ƃ��C���X�y�N�^�[�Őݒ�
    [SerializeField] private BlockAction blockAction;

    // �X�|�[������ꏊ�̃I�t�Z�b�g(�Q�����S�A2.5�͊O��)
    [SerializeField] private float spawnoffset = 2.0f;

    // �A�j���[�V���������s����L�[
    public KeyCode animationKey = KeyCode.L;

    void Update()
    {
        // ��U�̊m�F�p
        if (Input.GetKeyDown(animationKey))
        {
            PlayAnimation();
        }
    }


    /// <summary>
    /// �A�j���[�V�������Đ�
    /// ���̃��\�b�h���Ăяo�������ň�A�̃A�j���[�V���������s
    /// </summary>
    public void PlayAnimation()
    {
        // �K�v�ȃR���|�[�l���g�ƃv���n�u���ݒ肳��Ă��邩���m�F
        if (blockAction == null || particlePrefab == null)
        {
            Debug.LogError("Required data is missing to start the animation.");
            return;
        }

        // blockAction����lastGroundLevel�̒l���擾
        float lastGroundLevel = blockAction.lastGroundLevel;

        // BlockAction��origin�t�B�[���h�𒼐ڎQ�Ƃ��Ďl���̍��W���v�Z
        float minX = blockAction.origin.x - spawnoffset; 
        float maxX = blockAction.origin.x + spawnoffset;
        float minZ = blockAction.origin.z - spawnoffset; 
        float maxZ = blockAction.origin.z + spawnoffset; 
        float y = lastGroundLevel;

        // 4�̊p�̍��W�𖾎��I�ɒ�`
        Vector3 corner1 = new Vector3(maxX, y, maxZ);
        Vector3 corner2 = new Vector3(minX, y, maxZ);
        Vector3 corner3 = new Vector3(maxX, y, minZ);
        Vector3 corner4 = new Vector3(minX, y, minZ);

        // �e�p��1���p�[�e�B�N���𐶐����A���ꂼ��̃A�j���[�V�������J�n
        AnimateSingleParticle(Instantiate(particlePrefab, corner1, Quaternion.identity), lastGroundLevel);
        AnimateSingleParticle(Instantiate(particlePrefab, corner2, Quaternion.identity), lastGroundLevel);
        AnimateSingleParticle(Instantiate(particlePrefab, corner3, Quaternion.identity), lastGroundLevel);
        AnimateSingleParticle(Instantiate(particlePrefab, corner4, Quaternion.identity), lastGroundLevel);
    }

    private void AnimateSingleParticle(GameObject particleInstance, float startY)
    {
        // �������ꂽ�p�[�e�B�N�����Ǘ����邽�߂̎q�I�u�W�F�N�g�ɂ���
        particleInstance.transform.SetParent(this.transform);

        // �A�j���[�V�����̃V�[�P���X���`
        Sequence sequence = DOTween.Sequence();

        // �ڕW�̍����ilastGroundLevel + �I�t�Z�b�g�j���v�Z
        float targetY = startY + targetHeightOffset;

        // �㉺�ړ��A�j���[�V���������[�v
        for (int i = 0; i < cycleCount; i++)
        {
            // �܂��ڕW�̍����܂ŏ㏸���A���ɊJ�nY���W�܂ŉ��~
            sequence.Append(particleInstance.transform.DOMoveY(targetY, cycleDuration / 2).SetEase(Ease.OutSine));
            sequence.Append(particleInstance.transform.DOMoveY(startY, cycleDuration / 2).SetEase(Ease.InSine));
        }

        // �S�ẴT�C�N��������������A�p�[�e�B�N���I�u�W�F�N�g��j��
        sequence.OnComplete(() => Destroy(particleInstance.gameObject));
    }
}