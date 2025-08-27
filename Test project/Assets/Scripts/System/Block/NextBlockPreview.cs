using UnityEngine;
using UnityEngine.UI;

public class NextBlockPreview : MonoBehaviour
{
    [SerializeField]
    BlockAction blockAction;
    [SerializeField]
    Transform[] previewBlocksPositions;
    [SerializeField]
    Image[] previewUI;
    [SerializeField]
    Image[] previewTextFrame;
    [SerializeField]
    GameObject cubePrefab;

    int[] nowBlockIndex = new int[3] {-1, -1, -1};
    GameObject[] previewBlocks;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (blockAction == null) Destroy(this);
        previewBlocks = new GameObject[3];
    }

    // Update is called once per frame
    void Update()
    {
        if (blockAction.blockHistory[2] != nowBlockIndex[2])
        {
            for (int i = 0; i < nowBlockIndex.Length; i++)
            {
                nowBlockIndex[i] = blockAction.blockHistory[i];
            }
        }
    }

    public void GeneratePreviewBlock(int order, int blockType)
    {
        if (previewBlocks[order] != null) Destroy(previewBlocks[order]);
        previewBlocks[order] = new GameObject($"Preview Block ({order})");
        previewBlocks[order].transform.parent = previewBlocksPositions[order];

        Vector3 maxEdge = Vector3.zero;
        Vector3 minEdge = Vector3.zero;
        foreach (Vector3 offset in PentacubeShapes.Shapes[(Block3DType)blockType])
        {
            GameObject obj = Instantiate(cubePrefab, previewBlocksPositions[order].position + offset, Quaternion.identity, previewBlocks[order].transform);

            if (offset.x > maxEdge.x) maxEdge.x = offset.x;
            else if (offset.x < minEdge.x) minEdge.x = offset.x;

            if (offset.z > maxEdge.z) maxEdge.z = offset.z;
            else if (offset.z < minEdge.z) minEdge.z = offset.z;
            obj.GetComponent<BoxCollider>().enabled = false;
            obj.GetComponent<MeshRenderer>().material.color = blockAction.colorHistory[order];
        }
        previewUI[order].color = blockAction.colorHistory[order];
        if (order >= 1) previewTextFrame[order - 1].color = blockAction.colorHistory[order];
        previewBlocks[order].transform.position = -(maxEdge + minEdge) / 2;
    }
}
