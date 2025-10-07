using UnityEngine;
using System.Collections.Generic;

public class CheckCore : MonoBehaviour
{
    public Vector4 origins;

    [SerializeField] public Vector3 pos;
    [SerializeField] GameObject showCornerPrefab;

    GameObject showCornerParentObj;

    // Update is called once per frame
    void Update()
    {
        origins = new Vector4(GameStatus.fieldOrigin.x - 2, GameStatus.fieldOrigin.z - 2, GameStatus.fieldOrigin.x + 2, GameStatus.fieldOrigin.z + 2);
        pos = transform.position;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.z = Mathf.RoundToInt(pos.z);
        pos.y = Mathf.RoundToInt(pos.y);
    }

    public void ShowCorner()
    {

        if (showCornerParentObj != null)
        {
            Destroy(showCornerParentObj);
            return;
        }

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

        Vector3[] toBeShowPos = new Vector3[3];

        GameObject[] placedObjs = GameObject.FindGameObjectsWithTag("Placed");

        if (pos.x == origins.x && pos.z == origins.y)       //Position0 (-2,-2)
        {
            toBeShowPos[0] = new Vector3(origins.x, pos.y, origins.w);
            toBeShowPos[1] = new Vector3(origins.z, pos.y, origins.y);
            toBeShowPos[2] = new Vector3(origins.z, pos.y, origins.w);
        }
        else if (pos.x == origins.x && pos.z == origins.w)       //Position1 (-2,2)
        {
            toBeShowPos[0] = new Vector3(origins.x, pos.y, origins.y);
            toBeShowPos[1] = new Vector3(origins.z, pos.y, origins.y);
            toBeShowPos[2] = new Vector3(origins.z, pos.y, origins.w);
        }
        else if (pos.x == origins.x && pos.z == origins.y)       //Position0 (2,2)
        {
            toBeShowPos[0] = new Vector3(origins.x, pos.y, origins.w);
            toBeShowPos[1] = new Vector3(origins.z, pos.y, origins.y);
            toBeShowPos[2] = new Vector3(origins.z, pos.y, origins.w);
        }
        else if (pos.x == origins.x && pos.z == origins.y)       //Position0 (2,-2)
        {
            toBeShowPos[0] = new Vector3(origins.x, pos.y, origins.w);
            toBeShowPos[1] = new Vector3(origins.z, pos.y, origins.y);
            toBeShowPos[2] = new Vector3(origins.z, pos.y, origins.w);
        }
        else return;
        foreach (GameObject obj in placedObjs)
        {
            if (obj.GetComponent<BlockFade>() == null) continue;
            for (int i = 0; i < toBeShowPos.Length; i++)
            {
                if (obj.GetComponent<BlockFade>().worldPos == toBeShowPos[i])
                {
                    if (obj.GetComponent<CheckCore>() == null) return;
                    toBeShowPos[i] = new Vector3(99999, 99999, 99999);
                    break;
                }
            }
        }
        showCornerParentObj = new GameObject($"showCorner_y: {pos.y}");
        showCornerParentObj.transform.position = pos;
        showCornerParentObj.tag = "ShowCorner";
        for (int i = 0; i < toBeShowPos.Length; i++)
        {
            if (toBeShowPos[i] != new Vector3(99999, 99999, 99999))
            {
                Instantiate(showCornerPrefab, toBeShowPos[i], Quaternion.identity, showCornerParentObj.transform);
            }
        }
    }

    public Vector3[] CheckFill()
    {
		

        pos = transform.position;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.z = Mathf.RoundToInt(pos.z);
        pos.y = Mathf.RoundToInt(pos.y);

		//Vector3[] result = new Vector3[0];
		List<Vector3> result = new List<Vector3>();
		Vector3[] cornerPos = new Vector3[4];
		cornerPos[0] = new Vector3(origins.x, pos.y, origins.w);
		cornerPos[1] = new Vector3(origins.z, pos.y, origins.y);
		cornerPos[2] = new Vector3(origins.z, pos.y, origins.w);
		cornerPos[3] = new Vector3(origins.x, pos.y, origins.y);
		bool[] isCorePlaced = new bool[4] {false, false, false, false};
		bool isCorner = false;
		for (int i = 0; i < cornerPos.Length; i++)
		{
			if (pos.x == cornerPos[i].x && pos.z == cornerPos[i].z)
			{
				isCorner = true; 
				isCorePlaced[i] = true;
				break;
			}
		}

		if (!isCorner)
		{
			Debug.Log("no corner");
			return new Vector3[0];
		}
		result.Add(pos);
		int cornerCount = 1;
		foreach (var obj in GameObject.FindGameObjectsWithTag("Placed"))
		{
			CheckCore core = obj.GetComponent<CheckCore>();
			if (core == null) continue;
			for (int i = 0; i < cornerPos.Length; i++)
			{
				if (core.pos.x == cornerPos[i].x && core.pos.z == cornerPos[i].z)
				{
					if (!isCorePlaced[i])
					{
						cornerCount++;
						isCorePlaced[i] = true;
					}
					result.Add(core.pos);
				}
			}
		}
		if (cornerCount != 4)
		{
			Debug.Log("Corner count: " + cornerCount);
			return new Vector3[0];
		}

		Vector3[] sol = new Vector3[3] ;
		sol[0] = pos;
		sol[1] = Vector3.negativeInfinity;
		sol[2] = Vector3.positiveInfinity;
		for (int i = 0; i <result.Count; i++)
		{
			sol[1] = new Vector3(Mathf.Max(sol[1].x, result[i].x), Mathf.Max(sol[1].y, result[i].y), Mathf.Max(sol[1].z, result[i].z));
			sol[2] = new Vector3(Mathf.Min(sol[2].x, result[i].x), Mathf.Min(sol[2].y, result[i].y), Mathf.Min(sol[2].z, result[i].z));
		}

		return sol;
		#region sameHeight
		//Ray rayFromThisToRight = new Ray(pos, Vector3.right);       //x+
		//Ray rayFromThisToLeft = new Ray(pos, Vector3.left);         //x-
		//Ray rayFromThisToUp = new Ray(pos, Vector3.up);             //y+
		//Ray rayFromThisToDown = new Ray(pos, Vector3.down);         //y-
		//Ray rayFromThisToForward = new Ray(pos, Vector3.forward);   //z+
		//Ray rayFromThisToBack = new Ray(pos, Vector3.back);         //z-


		//if (pos.x == origins.x && pos.z == origins.y)     //Position 0(-2,-2)
		//{
		//    //Horizontal
		//    RaycastHit[] hits0 = Physics.RaycastAll(rayFromThisToForward, 6, 1 << 8);
		//    foreach (RaycastHit hit0 in hits0)
		//    {
		//        CheckCore hit0CC = hit0.transform.GetComponentInChildren<CheckCore>();
		//        if (Mathf.Abs(Vector3.Magnitude(pos - hit0CC.pos) - 4) < .5f)
		//        {
		//            RaycastHit[] hits1 = Physics.RaycastAll(rayFromThisToRight, 6, 1 << 8);
		//            foreach (RaycastHit hit1 in hits1)
		//            {
		//                CheckCore hit1CC = hit1.transform.GetComponentInChildren<CheckCore>();
		//                if (Mathf.Abs(Vector3.Magnitude(pos - hit1CC.pos) - 4) < .5f)
		//                {
		//                    RaycastHit[] hits2 = Physics.RaycastAll(hit1CC.pos, Vector3.forward, 6, 1 << 8);
		//                    foreach (RaycastHit hit2 in hits2)
		//                    {
		//                        CheckCore hit2CC = hit2.transform.GetComponentInChildren<CheckCore>();
		//                        if (Mathf.Abs(Vector3.Magnitude(hit1CC.pos - hit2CC.pos) - 4) < .5f)
		//                        {
		//                            result = new Vector3[4] { pos, hit0CC.pos, hit1CC.pos, hit2CC.pos };
		//                            for (int i = 0; i < result.Length; i++) result[i].y = Mathf.Max(transform.position.y, hit0CC.transform.position.y, hit1CC.transform.position.y, hit2CC.transform.position.y);
		//                            return result;
		//                        }
		//                    }
		//                }
		//            }
		//        }
		//    }
		//}
		//else if (pos.x == origins.x && pos.z == origins.w)     //Position 1(-2,2)
		//{
		//    //Horizontal
		//    RaycastHit[] hits0 = Physics.RaycastAll(rayFromThisToBack, 6, 1 << 8);
		//    foreach (RaycastHit hit0 in hits0)
		//    {
		//        CheckCore hit0CC = hit0.transform.GetComponentInChildren<CheckCore>();
		//        if (Mathf.Abs(Vector3.Magnitude(pos - hit0CC.pos) - 4) < .5f)
		//        {
		//            RaycastHit[] hits1 = Physics.RaycastAll(rayFromThisToRight, 6, 1 << 8);
		//            foreach (RaycastHit hit1 in hits1)
		//            {
		//                CheckCore hit1CC = hit1.transform.GetComponentInChildren<CheckCore>();
		//                if (Mathf.Abs(Vector3.Magnitude(pos - hit1CC.pos) - 4) < .5f)
		//                {
		//                    RaycastHit[] hits2 = Physics.RaycastAll(hit1CC.pos, Vector3.back, 6, 1 << 8);
		//                    foreach (RaycastHit hit2 in hits2)
		//                    {
		//                        CheckCore hit2CC = hit2.transform.GetComponentInChildren<CheckCore>();
		//                        if (Mathf.Abs(Vector3.Magnitude(hit1CC.pos - hit2CC.pos) - 4) < .5f)
		//                        {
		//                            result = new Vector3[4] { pos, hit0CC.pos, hit1CC.pos, hit2CC.pos };
		//                            for (int i = 0; i < result.Length; i++) result[i].y = Mathf.Max(transform.position.y, hit0CC.transform.position.y, hit1CC.transform.position.y, hit2CC.transform.position.y);
		//                            return result;
		//                        }
		//                    }
		//                }
		//            }
		//        }
		//    }
		//}
		//else if (pos.x == origins.z && pos.z == origins.w)     //Position 2(2,2)
		//{
		//    //Horizontal
		//    RaycastHit[] hits0 = Physics.RaycastAll(rayFromThisToBack, 6, 1 << 8);
		//    foreach (RaycastHit hit0 in hits0)
		//    {
		//        CheckCore hit0CC = hit0.transform.GetComponentInChildren<CheckCore>();
		//        if (Mathf.Abs(Vector3.Magnitude(pos - hit0CC.pos) - 4) < .5f)
		//        {
		//            RaycastHit[] hits1 = Physics.RaycastAll(rayFromThisToLeft, 6, 1 << 8);
		//            foreach (RaycastHit hit1 in hits1)
		//            {
		//                CheckCore hit1CC = hit1.transform.GetComponentInChildren<CheckCore>();
		//                if (Mathf.Abs(Vector3.Magnitude(pos - hit1CC.pos) - 4) < .5f)
		//                {
		//                    RaycastHit[] hits2 = Physics.RaycastAll(hit1CC.pos, Vector3.back, 6, 1 << 8);
		//                    foreach (RaycastHit hit2 in hits2)
		//                    {
		//                        CheckCore hit2CC = hit2.transform.GetComponentInChildren<CheckCore>();
		//                        if (Mathf.Abs(Vector3.Magnitude(hit1CC.pos - hit2CC.pos) - 4) < .5f)
		//                        {
		//                            result = new Vector3[4] { pos, hit0CC.pos, hit1CC.pos, hit2CC.pos };
		//                            for (int i = 0; i < result.Length; i++) result[i].y = Mathf.Max(transform.position.y, hit0CC.transform.position.y, hit1CC.transform.position.y, hit2CC.transform.position.y);
		//                            return result;
		//                        }
		//                    }
		//                }
		//            }
		//        }
		//    }
		//}
		//else if (pos.x == origins.z && pos.z == origins.y)     //Position 3(2,-2)
		//{
		//    //Horizontal
		//    RaycastHit[] hits0 = Physics.RaycastAll(rayFromThisToForward, 6, 1 << 8);
		//    foreach (RaycastHit hit0 in hits0)
		//    {
		//        CheckCore hit0CC = hit0.transform.GetComponentInChildren<CheckCore>();
		//        if (hit0CC == null) continue;
		//        if (Mathf.Abs(Vector3.Magnitude(pos - hit0CC.pos) - 4) < .5f)
		//        {
		//            RaycastHit[] hits1 = Physics.RaycastAll(rayFromThisToLeft, 6, 1 << 8);
		//            foreach (RaycastHit hit1 in hits1)
		//            {
		//                CheckCore hit1CC = hit1.transform.GetComponentInChildren<CheckCore>();
		//                if (hit1CC == null) continue;
		//                if (Mathf.Abs(Vector3.Magnitude(pos - hit1CC.pos) - 4) < .5f)
		//                {
		//                    RaycastHit[] hits2 = Physics.RaycastAll(hit1CC.pos, Vector3.forward, 6, 1 << 8);
		//                    foreach (RaycastHit hit2 in hits2)
		//                    {
		//                        CheckCore hit2CC = hit2.transform.GetComponentInChildren<CheckCore>();
		//                        if (hit2CC == null) continue;
		//                        if (Mathf.Abs(Vector3.Magnitude(hit1CC.pos - hit2CC.pos) - 4) < .5f)
		//                        {
		//                            result = new Vector3[4] { pos, hit0CC.pos, hit1CC.pos, hit2CC.pos };
		//                            for (int i = 0; i < result.Length; i++) result[i].y = Mathf.Max(transform.position.y, hit0CC.transform.position.y, hit1CC.transform.position.y, hit2CC.transform.position.y);
		//                            return result;
		//                        }
		//                    }
		//                }
		//            }
		//        }
		//    }
		//}
		//return result;
		#endregion
	}
}
