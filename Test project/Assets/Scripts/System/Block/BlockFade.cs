using System.Collections;
using UnityEngine;

public class BlockFade : MonoBehaviour
{
    MeshRenderer meshRenderer;

    [SerializeField]
    MeshRenderer coreRenderer;

    public Vector3Int worldPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        worldPos.x = Mathf.RoundToInt(pos.x); worldPos.y = Mathf.RoundToInt(pos.y); worldPos.z = Mathf.RoundToInt(pos.z);
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
