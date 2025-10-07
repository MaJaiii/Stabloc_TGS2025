// ParticleLineAnimation.cs

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BeaconLineAnimation : MonoBehaviour
{
    // �A�j���[�V�����̃p�^�[�����`����񋓌^
    public enum AnimationPattern
    {
        Pattern1_XZShrink,          // �p�^�[��1: XZ�����k������
        Pattern2_YGrowAndXZShrink   // �p�^�[��2: Y�����L�тȂ���XZ�����k������
    }

    [Header("Dependencies")]
    // BlockAction�X�N���v�g�ւ̎Q�ƁB�u���b�N�̔z�u���i�n�ʂ̍����Ȃǁj���擾���邽�߂Ɏg�p
    [SerializeField] private BlockAction blockAction;

    [Header("Animation Settings")]
    // �A�j���[�V�����Ɏg�p����L���[�u�̃v���n�u
    [SerializeField] private GameObject cubePrefab;
    // �C���X�y�N�^�[�Őݒ肷�錻�݂̃A�j���[�V�����p�^�[��
    [SerializeField] private AnimationPattern currentPattern = AnimationPattern.Pattern2_YGrowAndXZShrink;
    // �A�j���[�V�����S�̂̏��v����
    [SerializeField] private float animationDuration = 2.0f;
    // �L���[�u���X�|�[������ʒu�̃I�t�Z�b�g�i���S����̋����j
    [SerializeField] private float spawnoffset = 2.0f;
    // �X�^�[�g����XZ�̃X�P�[��(1���Ɖ�]���ɔ�яo��)
    [SerializeField] private float XZ_scale = 0.7f;

    [Header("Y Scale Settings")]
    // Y���̍ő�X�P�[���l�B�p�^�[��1�ł͍ŏ����炱�̒l�A�p�^�[��2�ł͂��̒l�܂ŐL�т�
    [SerializeField] private float yGrowOffset = 80.0f;

    [Header("Debug")]
    // �f�o�b�O�p�ɃA�j���[�V�������J�n���邽�߂̃L�[
    public KeyCode animationKey = KeyCode.L;

    void Update()
    {
        // �f�o�b�O�L�[�������ꂽ��A�j���[�V�������J�n����
        if (Input.GetKeyDown(animationKey))
        {
            PlayAnimation();
        }
    }

    /// <summary>
    /// �A�j���[�V�������Đ�����p�u���b�N���\�b�h
    /// ���̃��\�b�h���O������Ăяo�����ƂŁA�A�j���[�V�������J�n�ł���
    /// </summary>
    public void PlayAnimation()
    {
        if (blockAction == null || cubePrefab == null)
        {
            Debug.LogError("Required data is missing to start the animation.");
            return;
        }

        // 4�̊p�̊���W���`
        Vector3 origin = blockAction.origin;
        List<Vector2> cornersXZ = new List<Vector2>
        {
            new Vector2(origin.x + spawnoffset, origin.z + spawnoffset),
            new Vector2(origin.x - spawnoffset, origin.z + spawnoffset),
            new Vector2(origin.x + spawnoffset, origin.z - spawnoffset),
            new Vector2(origin.x - spawnoffset, origin.z - spawnoffset)
        };

        // �V�[�����̂��ׂẴR�A�I�u�W�F�N�g�𒼐ڌ������āA���̈ʒu���擾����
        CheckCore[] allCoresInScene = FindObjectsByType<CheckCore>(FindObjectsSortMode.None);

        // �A�j���[�V�����ɕK�v�ȏ����擾
        Color currentColor = blockAction.GetCurrentBlockColor();
        float lastGroundLevel = blockAction.lastGroundLevel + 0.5f;

        // �e�p�ɂ��āA�R�A�u���b�N�̑��݂��`�F�b�N
        for (int i = 0; i < cornersXZ.Count; i++)
        {
            Vector2 cornerXZ = cornersXZ[i];
            bool isCorePresentAtCorner = false;

            // �V�[�����̑S�R�A�����[�v���A���݂̊p�̈ʒu�ɊY������R�A�����邩�m�F
            foreach (CheckCore core in allCoresInScene)
            {
                // �^�O��"Placed"�̃R�A�݂̂�ΏۂƂ���
                if (core.CompareTag("Placed"))
                {
                    Vector2 corePosXZ = new Vector2(core.transform.position.x, core.transform.position.z);

                    // Vector2.Distance���g���Č덷�����e���Ȃ���ʒu���r
                    if (Vector2.Distance(cornerXZ, corePosXZ) < 0.1f)
                    {
                        isCorePresentAtCorner = true;
                        break;
                    }
                }
            }

            // �R�A�����̊p�ɑ��݂��Ȃ��ꍇ�ɂ̂݁A�A�j���[�V�������J�n
            if (!isCorePresentAtCorner)
            {
                Vector3 animationSpawnPos = new Vector3(cornerXZ.x, lastGroundLevel, cornerXZ.y);

                AnimateSingleCube(Instantiate(cubePrefab, animationSpawnPos, Quaternion.identity), currentPattern, true, lastGroundLevel, currentColor);
            }
        }
    }

    /// <summary>
    /// �P��̃L���[�u�ɑ΂��ăA�j���[�V������K�p���郁�\�b�h
    /// </summary>
    private void AnimateSingleCube(GameObject cubeInstance, AnimationPattern pattern, bool isPositiveRotation, float startY, Color color)
    {
        // �L���[�u�̐e�q�֌W��ݒ肵�A�Ǘ����₷������
        cubeInstance.transform.SetParent(this.transform);

        // �}�e���A�����擾���āA�V�F�[�_�[�v���p�e�B�𑀍�ł���悤�ɂ���
        Material cubeMaterial = cubeInstance.GetComponent<Renderer>().material;

        // _BeaconLineColor
        cubeMaterial.SetColor("_BeaconLineColor", color);

        // �����v���p�e�B��ݒ�
        cubeMaterial.SetFloat("_RotationAngle", 0);
        cubeMaterial.SetFloat("_XZScale", XZ_scale);

        // Transform�̃��[�J���X�P�[���𑀍�
        cubeInstance.transform.localScale = new Vector3(XZ_scale, 0, XZ_scale);

        // DOTween�̃V�[�P���X���쐬
        Sequence sequence = DOTween.Sequence();

        // ���ʃA�j���[�V����: Y���𒆐S�Ƃ�����]
        float rotationTarget = isPositiveRotation ? 360f : -360f;
        sequence.Append(DOTween.To(() => cubeMaterial.GetFloat("_RotationAngle"), x => cubeMaterial.SetFloat("_RotationAngle", x), rotationTarget, animationDuration));

        if (pattern == AnimationPattern.Pattern1_XZShrink)
        {
            // �p�^�[��1: Y�X�P�[�����ŏ�����ő�ɐݒ�
            cubeInstance.transform.localScale = new Vector3(XZ_scale, yGrowOffset, XZ_scale);

            // XZ�X�P�[����0�܂ŏk������A�j���[�V����
            sequence.Join(DOTween.To(() => cubeMaterial.GetFloat("_XZScale"), x => cubeMaterial.SetFloat("_XZScale", x), 0, animationDuration));

            // �I�u�W�F�N�g��Y���W��␳���A��ʂ��n�ʂɍ����悤�ɂ���
            Vector3 newPos = cubeInstance.transform.position;
            newPos.y = startY + (yGrowOffset * 0.5f);
            cubeInstance.transform.position = newPos;
        }
        else if (pattern == AnimationPattern.Pattern2_YGrowAndXZShrink)
        {
            // �p�^�[��2: Y�X�P�[�����������ɐL�΂��A�j���[�V����
            // XZ�X�P�[����Y�X�P�[���𓯎��ɃA�j���[�V����
            sequence.Join(DOTween.To(() => cubeMaterial.GetFloat("_XZScale"), x => cubeMaterial.SetFloat("_XZScale", x), 0, animationDuration));

            // Y�X�P�[���ƈʒu�𓯎��ɃA�j���[�V���������A��ʂ�n�ʂɌŒ�
            sequence.Join(DOTween.To(() => 0f, (yScale) => {
                // Transform��Y�X�P�[�����X�V
                cubeInstance.transform.localScale = new Vector3(XZ_scale, yScale, XZ_scale);
                // �L���[�u�̒��S�_��Y�X�P�[���̔���������Ɉړ�������
                Vector3 newPos = cubeInstance.transform.position;
                newPos.y = startY + (yScale * 0.5f);
                cubeInstance.transform.position = newPos;
            }, yGrowOffset, animationDuration));
        }

        // �S�ẴA�j���[�V����������������A�L���[�u��j��
        sequence.OnComplete(() =>
        {
            Destroy(cubeInstance);
        });
    }
}