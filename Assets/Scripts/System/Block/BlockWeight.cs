using UnityEngine;

public class BlockWeight : MonoBehaviour
{
    BlockColor blockColor;

    [SerializeField]
    GameObject core;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (blockColor == null)
        {
            blockColor = transform.parent.GetComponent<BlockColor>();
            return;
        }

        core.GetComponent<MeshRenderer>().material.color = blockColor.blockColor;
    }
}
