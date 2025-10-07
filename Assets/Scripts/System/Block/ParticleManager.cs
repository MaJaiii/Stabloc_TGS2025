using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    private ParticleSystem particle;
    private bool isPlayed = false;
    private float lifeTimer = 0f;
    private float maxLifetime = 3f; // �Đ�����Ȃ������ꍇ�̕ی�����

    void Awake()
    {
        particle = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (particle == null)
        {
            Destroy(gameObject);
            return;
        }

        // �Đ��J�n�����m
        if (!isPlayed && particle.isPlaying)
        {
            isPlayed = true;
        }

        if (isPlayed)
        {
            // �Đ������S�ɏI�������폜
            if (!particle.IsAlive(true))
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // ��x���Đ�����Ȃ������ꍇ�̓^�C�}�[�ō폜
            lifeTimer += Time.deltaTime;
            if (lifeTimer > maxLifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}