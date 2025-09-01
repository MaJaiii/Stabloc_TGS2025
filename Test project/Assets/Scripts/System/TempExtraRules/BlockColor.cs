using UnityEngine;

public class BlockColor : MonoBehaviour
{
    public Color blockColor;
    public bool isCollapse = false;
    public float heightWhenSet = Mathf.NegativeInfinity;
    public BlockAction blockAction;
    public bool isGhost = true;
    public Vector3 startNor;
    public Vector3 originPos;

    [SerializeField]
    Vector3 nowNor;

    private void Update()
    {
        nowNor = transform.up;
        if (Vector3.Dot(startNor, nowNor) < .6f && !isGhost && (Mathf.Abs(transform.position.x - GameStatus.fieldOrigin.x) > 2.5f || Mathf.Abs(transform.position.z - GameStatus.fieldOrigin.z) > 2.5f))
        {
            isCollapse = true;
        }
        
    }
}
