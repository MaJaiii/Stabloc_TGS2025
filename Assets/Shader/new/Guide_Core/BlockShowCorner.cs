using UnityEngine;

public class BlockShowCorner : MonoBehaviour
{
	BlockColor blockColor;

	[SerializeField] GameObject block;
	[SerializeField] GameObject core;

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

		block.GetComponent<MeshRenderer>().material.color = blockColor.blockColor;
		core.GetComponent<MeshRenderer>().material.color = blockColor.blockColor;
	}
}
