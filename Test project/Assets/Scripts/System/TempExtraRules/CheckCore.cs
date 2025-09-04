using UnityEngine;

public class CheckCore : MonoBehaviour
{
    public Vector4 origins;

    [SerializeField] public Vector3 pos;


    // Update is called once per frame
    void Update()
    {
        origins = new Vector4(GameStatus.fieldOrigin.x - 2, GameStatus.fieldOrigin.z - 2, GameStatus.fieldOrigin.x + 2, GameStatus.fieldOrigin.z + 2);
        pos = transform.position;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.z = Mathf.RoundToInt(pos.z);
        pos.y = Mathf.RoundToInt(pos.y);
    }

    void ShowCorner()
    {
        pos = transform.position;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.z = Mathf.RoundToInt(pos.z);
        pos.y = Mathf.RoundToInt(pos.y);

        Ray rayFromThisToRight = new Ray(pos, Vector3.right);       //x+
        Ray rayFromThisToLeft = new Ray(pos, Vector3.left);         //x-
        Ray rayFromThisToUp = new Ray(pos, Vector3.up);             //y+
        Ray rayFromThisToDown = new Ray(pos, Vector3.down);         //y-
        Ray rayFromThisToForward = new Ray(pos, Vector3.forward);   //z+
        Ray rayFromThisToBack = new Ray(pos, Vector3.back);         //z-

        bool[] isShow = {true, true, true};

        if (pos.x == origins.x && pos.z == origins.y)       //Position0 (-2,-2)
        {
            RaycastHit[] hit0 = Physics.RaycastAll(rayFromThisToForward, 6);
            foreach (RaycastHit hit in hit0)
            {
                if (hit.collider.GetComponent<BlockFade>()?.worldPos.z != origins.w) continue;      //Check if the hit object is block
                if (hit.collider.GetComponent<CheckCore>() == null) return;                         //Check if there is already a non-core block
                isShow[0] = false;
            }
            hit0 = Physics.RaycastAll(rayFromThisToRight, 6);
            foreach (RaycastHit hit in hit0)
            {
                if (hit.collider.GetComponent<BlockFade>()?.worldPos.x != origins.z) continue;      //Check if the hit object is block
                if (hit.collider.GetComponent<CheckCore>() == null) return;                         //Check if there is already a non-core block
                isShow[1] = false;
            }
            Vector3 tempPos = pos;
            tempPos.x = origins.z;
            hit0 = Physics.RaycastAll(tempPos, Vector3.forward, 6);
            foreach (RaycastHit hit in hit0)
            {
                if (hit.collider.GetComponent<BlockFade>()?.worldPos.x != origins.z) continue;      //Check if the hit object is block
                if (hit.collider.GetComponent<CheckCore>() == null) return;                         //Check if there is already a non-core block
                isShow[2] = false;
            }
        }
    }

    public Vector3[] CheckFill()
    {
        pos = transform.position;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.z = Mathf.RoundToInt(pos.z);
        pos.y = Mathf.RoundToInt(pos.y);

        Vector3[] result = new Vector3[0];


        Ray rayFromThisToRight = new Ray(pos, Vector3.right);       //x+
        Ray rayFromThisToLeft = new Ray(pos, Vector3.left);         //x-
        Ray rayFromThisToUp = new Ray(pos, Vector3.up);             //y+
        Ray rayFromThisToDown = new Ray(pos, Vector3.down);         //y-
        Ray rayFromThisToForward = new Ray(pos, Vector3.forward);   //z+
        Ray rayFromThisToBack = new Ray(pos, Vector3.back);         //z-

        
        if (pos.x == origins.x && pos.z == origins.y)     //Position 0(-2,-2)
        {
            //Horizontal
            RaycastHit[] hits0 = Physics.RaycastAll(rayFromThisToForward, 6, 1 << 8);
            foreach (RaycastHit hit0 in hits0)
            {
                CheckCore hit0CC = hit0.transform.GetComponentInChildren<CheckCore>();
                if (Mathf.Abs(Vector3.Magnitude(pos - hit0CC.pos) - 4) < .5f)
                {
                    RaycastHit[] hits1 = Physics.RaycastAll(rayFromThisToRight, 6, 1 << 8);
                    foreach (RaycastHit hit1 in hits1)
                    {
                        CheckCore hit1CC = hit1.transform.GetComponentInChildren<CheckCore>();
                        if (Mathf.Abs(Vector3.Magnitude(pos - hit1CC.pos) - 4) < .5f)
                        {
                            RaycastHit[] hits2 = Physics.RaycastAll(hit1CC.pos, Vector3.forward, 6, 1 << 8);
                            foreach (RaycastHit hit2 in hits2)
                            {
                                CheckCore hit2CC = hit2.transform.GetComponentInChildren<CheckCore>();
                                if (Mathf.Abs(Vector3.Magnitude(hit1CC.pos - hit2CC.pos) - 4) < .5f)
                                {
                                    result = new Vector3[4] { pos, hit0CC.pos, hit1CC.pos, hit2CC.pos };
                                    for (int i = 0; i < result.Length; i++) result[i].y = Mathf.Max(transform.position.y, hit0CC.transform.position.y, hit1CC.transform.position.y, hit2CC.transform.position.y);
                                    return result;
                                }
                            }
                        }
                    }
                }
            }
        }
        else if (pos.x == origins.x && pos.z == origins.w)     //Position 1(-2,2)
        {
            //Horizontal
            RaycastHit[] hits0 = Physics.RaycastAll(rayFromThisToBack, 6, 1 << 8);
            foreach (RaycastHit hit0 in hits0)
            {
                CheckCore hit0CC = hit0.transform.GetComponentInChildren<CheckCore>();
                if (Mathf.Abs(Vector3.Magnitude(pos - hit0CC.pos) - 4) < .5f)
                {
                    RaycastHit[] hits1 = Physics.RaycastAll(rayFromThisToRight, 6, 1 << 8);
                    foreach (RaycastHit hit1 in hits1)
                    {
                        CheckCore hit1CC = hit1.transform.GetComponentInChildren<CheckCore>();
                        if (Mathf.Abs(Vector3.Magnitude(pos - hit1CC.pos) - 4) < .5f)
                        {
                            RaycastHit[] hits2 = Physics.RaycastAll(hit1CC.pos, Vector3.back, 6, 1 << 8);
                            foreach (RaycastHit hit2 in hits2)
                            {
                                CheckCore hit2CC = hit2.transform.GetComponentInChildren<CheckCore>();
                                if (Mathf.Abs(Vector3.Magnitude(hit1CC.pos - hit2CC.pos) - 4) < .5f)
                                {
                                    result = new Vector3[4] { pos, hit0CC.pos, hit1CC.pos, hit2CC.pos };
                                    for (int i = 0; i < result.Length; i++) result[i].y = Mathf.Max(transform.position.y, hit0CC.transform.position.y, hit1CC.transform.position.y, hit2CC.transform.position.y);
                                    return result;
                                }
                            }
                        }
                    }
                }
            }
        }
        else if (pos.x == origins.z && pos.z == origins.w)     //Position 2(2,2)
        {
            //Horizontal
            RaycastHit[] hits0 = Physics.RaycastAll(rayFromThisToBack, 6, 1 << 8);
            foreach (RaycastHit hit0 in hits0)
            {
                CheckCore hit0CC = hit0.transform.GetComponentInChildren<CheckCore>();
                if (Mathf.Abs(Vector3.Magnitude(pos - hit0CC.pos) - 4) < .5f)
                {
                    RaycastHit[] hits1 = Physics.RaycastAll(rayFromThisToLeft, 6, 1 << 8);
                    foreach (RaycastHit hit1 in hits1)
                    {
                        CheckCore hit1CC = hit1.transform.GetComponentInChildren<CheckCore>();
                        if (Mathf.Abs(Vector3.Magnitude(pos - hit1CC.pos) - 4) < .5f)
                        {
                            RaycastHit[] hits2 = Physics.RaycastAll(hit1CC.pos, Vector3.back, 6, 1 << 8);
                            foreach (RaycastHit hit2 in hits2)
                            {
                                CheckCore hit2CC = hit2.transform.GetComponentInChildren<CheckCore>();
                                if (Mathf.Abs(Vector3.Magnitude(hit1CC.pos - hit2CC.pos) - 4) < .5f)
                                {
                                    result = new Vector3[4] { pos, hit0CC.pos, hit1CC.pos, hit2CC.pos };
                                    for (int i = 0; i < result.Length; i++) result[i].y = Mathf.Max(transform.position.y, hit0CC.transform.position.y, hit1CC.transform.position.y, hit2CC.transform.position.y);
                                    return result;
                                }
                            }
                        }
                    }
                }
            }
        }
        else if (pos.x == origins.z && pos.z == origins.y)     //Position 3(2,-2)
        {
            //Horizontal
            RaycastHit[] hits0 = Physics.RaycastAll(rayFromThisToForward, 6, 1 << 8);
            foreach (RaycastHit hit0 in hits0)
            {
                CheckCore hit0CC = hit0.transform.GetComponentInChildren<CheckCore>();
                if (hit0CC == null) continue;
                if (Mathf.Abs(Vector3.Magnitude(pos - hit0CC.pos) - 4) < .5f)
                {
                    RaycastHit[] hits1 = Physics.RaycastAll(rayFromThisToLeft, 6, 1 << 8);
                    foreach (RaycastHit hit1 in hits1)
                    {
                        CheckCore hit1CC = hit1.transform.GetComponentInChildren<CheckCore>();
                        if (hit1CC == null) continue;
                        if (Mathf.Abs(Vector3.Magnitude(pos - hit1CC.pos) - 4) < .5f)
                        {
                            RaycastHit[] hits2 = Physics.RaycastAll(hit1CC.pos, Vector3.forward, 6, 1 << 8);
                            foreach (RaycastHit hit2 in hits2)
                            {
                                CheckCore hit2CC = hit2.transform.GetComponentInChildren<CheckCore>();
                                if (hit2CC == null) continue;
                                if (Mathf.Abs(Vector3.Magnitude(hit1CC.pos - hit2CC.pos) - 4) < .5f)
                                {
                                    result = new Vector3[4] { pos, hit0CC.pos, hit1CC.pos, hit2CC.pos };
                                    for (int i = 0; i < result.Length; i++) result[i].y = Mathf.Max(transform.position.y, hit0CC.transform.position.y, hit1CC.transform.position.y, hit2CC.transform.position.y);
                                    return result;
                                }
                            }
                        }
                    }
                }
            }
        }
        return result;
    }
}
