using System.Collections.Generic;
using UnityEngine;

public class GhostBlockPreview : MonoBehaviour
{
    public Transform pivotObj;                  // Active block
    public Transform ghostBlock;                // Visual prediction
    public LayerMask placementMask;             // e.g., "Default"
    public Material ghostBlockMaterial;         // Material of normal block
    public Material ghostWeightedBlockMaterial; // Material of weighted block

    public bool isActive = false;               // Activation of ghost system

    BlockAction blockAction;

    Color recentColor;

    #region Fill field
    [SerializeField] GameObject fillBlockPrefab;
    public GameObject fillGhostParent;
    public List<GameObject> tempClearObjs = new();
    #endregion


    private void Start()
    {
        blockAction = GetComponent<BlockAction>();
    }
    public void CreateGhost(GameObject block, Color color)
    {
        if (ghostBlock != null) Destroy(ghostBlock.gameObject);

        
        ghostBlock = Instantiate(block).transform;
        ghostBlock.transform.position = block.transform.position;
        ghostBlock.name = "GhostBlock";
        ghostBlock.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        DestroyImmediate(ghostBlock.GetComponent<Rigidbody>());
        DestroyImmediate(ghostBlock.GetComponent<HingeJoint>());
        ghostBlock.GetComponent<BlockColor>().isGhost = true ;

        foreach (Transform child in ghostBlock)
        {
            Destroy(child.GetChild(0).gameObject);
            DestroyImmediate(child.GetComponent<BoxCollider>());
            DestroyImmediate(child.GetComponent<Rigidbody>());

            var renderer = child.GetComponent<MeshRenderer>();
            if (renderer)
            {
                renderer.enabled = true;
                var col = color;
                Material mat;
                if (child.GetComponent<BlockWeight>() != null)
                {
                    mat = new Material(ghostWeightedBlockMaterial);
                    col.a = 1f;
                }
                else
                {
                    mat = new Material(ghostBlockMaterial);
                    col.a = .8f;
                }
                
                recentColor = color;
                mat.color = col;
                renderer.material = mat;
            }
        }
    }

    public void UpdateGhostPosition()
    {
        if (pivotObj == null || ghostBlock == null || !isActive) return;

        #region Fill initialize
        if (fillGhostParent != null) DestroyImmediate(fillGhostParent);
        //sameX.Clear();
        //sameXdist.Clear();
        //if (sameXpivot != null) DestroyImmediate(sameXpivot.gameObject);
        //sameY.Clear();
        //sameYdist.Clear();
        //if (sameYpivot != null) DestroyImmediate(sameYpivot.gameObject);
        //sameZ.Clear();
        //sameZdist.Clear();
        //if (sameZpivot != null) DestroyImmediate(sameZpivot.gameObject);
        #endregion


        float minDrop = Mathf.Infinity;
        Color tempCol = recentColor;
        foreach (Transform child in blockAction.pivotObj)
        {
            Vector3[] offsets = new Vector3[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            foreach (var offset in offsets)
            {
                Ray ray = new Ray(child.position + offset * .4f, Vector3.down);
                RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
                foreach (var hit in hits)
                {
                    if (hit.collider.gameObject == child.gameObject) continue;
                    if (!hit.collider.CompareTag("Placed") && !hit.collider.CompareTag("Ground") && !hit.collider.CompareTag("GameOver")) continue;
                    float drop = child.position.y - hit.point.y;
                    if (drop < minDrop)
                    {
                        minDrop = drop;
                    }
                }
            }

        }

        if (minDrop != Mathf.Infinity)
        {
            ghostBlock.position = pivotObj.position - new Vector3(0, Mathf.Floor(minDrop - .5f), 0);
            ghostBlock.rotation = pivotObj.rotation;
            foreach (Transform child in ghostBlock)
            {
                if (child.GetComponent<CheckCore>() != null)
                {
                    CheckCore checkCore = child.GetComponent<CheckCore>();
                    Vector3[] vertex = checkCore.CheckFill();
                    if (vertex.Length == 3 && vertex[2] != vertex[1] && (vertex[2].x != vertex[1].x || vertex[2].z != vertex[1].z))
                    {
                        fillGhostParent = new GameObject("fillGhostParent");
                        blockAction.fillVertex[0] = vertex[2]; //Min
                        blockAction.fillVertex[1] = vertex[1]; //Max
                        GameObject[] placedObj = GameObject.FindGameObjectsWithTag("Placed");
                        foreach (var obj in placedObj)
                        {
                            for (int i = 0; i < obj.transform.childCount; i++)
                            {
                                GameObject children = obj.transform.GetChild(i).gameObject;
                                if (children.transform.position.x > vertex[2].x - .1f && children.transform.position.x < vertex[1].x + .1f &&
                                    children.transform.position.y > vertex[2].y - .1f && children.transform.position.y < vertex[1].y + .1f &&
                                    children.transform.position.z > vertex[2].z - .1f && children.transform.position.z < vertex[1].z + .1f &&
                                    children.GetComponent<MeshRenderer>() != null)
                                {
                                    tempClearObjs.Add(children);
                                    children.GetComponent<MeshRenderer>().enabled = false;
                                }
                            }

                        }
                        for (float x = vertex[2].x; x <= vertex[1].x; x += 1)
                        {
                            for (float y = vertex[2].y; y <= vertex[1].y; y += 1)
                            {
                                for (float z = vertex[2].z; z <= vertex[1].z; z += 1)
                                {
                                    Instantiate(fillBlockPrefab, new Vector3(x, y, z), Quaternion.identity).transform.parent = fillGhostParent.transform;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < blockAction.fillVertex.Length; i++) blockAction.fillVertex[i] = Vector3.zero;
                        foreach (var obj in tempClearObjs) obj.GetComponent<MeshRenderer>().enabled = true;                      
                        tempCol.a = .9f;
                        // ’Ç‰Á=========================================================================
                        child.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", tempCol);
                        // =============================================================================
                    }
                }
                else
                {
                    tempCol = recentColor;
                    tempCol.a = .8f;

                    // ’Ç‰Á=========================================================================
                    //child.GetComponent<MeshRenderer>().material.color = tempCol;
                    child.GetComponent<MeshRenderer>().material.SetColor("_SurfaceColor", tempCol);
                    child.GetComponent<MeshRenderer>().material.SetColor("_EdgeColor", tempCol);
                    // =============================================================================
                }

            }
        }


    }

    private void Update()
    {
        if (!isActive)
        {
            if (ghostBlock != null) Destroy(ghostBlock.gameObject);
            if (fillGhostParent != null) Destroy(fillGhostParent.gameObject);
            foreach (var obj in tempClearObjs) obj.GetComponent<MeshRenderer>().enabled = true;
            tempClearObjs.Clear();
        }
    }
}
