using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    private ParticleSystem particle;
    private bool isPlayed = false;
    private float lifeTimer = 0f;
    private float maxLifetime = 3f; // 再生されなかった場合の保険時間

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

        // 再生開始を検知
        if (!isPlayed && particle.isPlaying)
        {
            isPlayed = true;
        }

        if (isPlayed)
        {
            // 再生が完全に終わったら削除
            if (!particle.IsAlive(true))
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // 一度も再生されなかった場合はタイマーで削除
            lifeTimer += Time.deltaTime;
            if (lifeTimer > maxLifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}