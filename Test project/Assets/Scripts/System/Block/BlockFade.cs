using System.Collections;
using UnityEngine;

public class BlockFade : MonoBehaviour
{
    MeshRenderer meshRenderer;

    [SerializeField]
    MeshRenderer coreRenderer;
    [SerializeField]
    Vector3 worldPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        worldPos = this.transform.position;
    }

    public void StartChangeAlpha(float alpha)
    {
        StartCoroutine(ChangeAlpha(alpha));
    }

    IEnumerator ChangeAlpha(float targetValue)
    {
        Color color = meshRenderer.material.color;
        if (color.a > targetValue)
        {
            do
            {
                color.a -= .1f;
                meshRenderer.material.color = color;
                if (coreRenderer != null) coreRenderer.material.color = color;
                yield return new WaitForSeconds(0.005f);
            }
            while (color.a > targetValue);
        }
        else
        {
            color.a = 1;
            meshRenderer.material.color = color;
            if (coreRenderer != null) coreRenderer.material.color = color;
        }
    }
}
