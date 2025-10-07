using System.Collections;
using UnityEngine;

public class BlockFade : MonoBehaviour
{
    public Vector3Int worldPos;


    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        worldPos.x = Mathf.RoundToInt(pos.x); worldPos.y = Mathf.RoundToInt(pos.y); worldPos.z = Mathf.RoundToInt(pos.z);
    }
}
