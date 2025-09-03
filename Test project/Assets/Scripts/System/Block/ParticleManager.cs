using UnityEngine;

public class ParticleManager : MonoBehaviour
{

    ParticleSystem particle;
    bool isPlayed = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        particle = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (particle != null)
        {
            if (!isPlayed && particle.isPlaying) isPlayed = true;
            if (isPlayed && particle.isStopped) Destroy(gameObject);
        }
    }
}
