using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineAnimation : MonoBehaviour
{
	[SerializeField]
	GroundLevelMode Ground_Level_Mode = GroundLevelMode.FastGroundLevel;

	private enum GroundLevelMode
	{
		FastGroundLevel,
		UpdateGroundLevel
	}

	[SerializeField] private BlockAction m_blockAction;
    private float lasttimeGround ;

	public Material lineMaterial;
	public float lineWidth = 0.1f;

	public float group1Duration = 1.0f;
	public float group2Duration = 1.5f;
	public float group3Duration = 1.5f;
	public float group4Duration = 1.5f;

	public float clearDelay = 2.0f;

	[Header("Particles")]
	[SerializeField] private GameObject particlePrefab;
	// 生成したパーティクルを追跡するためのリスト
	private List<GameObject> particleInstances = new List<GameObject>();

	public int lineanimation_pattern = 0;


	private void Start()
	{
		if(Ground_Level_Mode == GroundLevelMode.FastGroundLevel)
		{
			lasttimeGround = -4.5f;
		}
		else
		{
			lasttimeGround = -3.5f;
		}
		
	}

	private struct LineData
	{
		public Vector3 start;
		public Vector3 end;
	}

	private List<LineData> linesToDraw = new List<LineData>();
	private List<GameObject> lineObjects = new List<GameObject>();
	private bool isAnimating = true;

	private void GenerateCubeLines()
	{
		Vector3[] fillVertex = m_blockAction.fillVertex;
		float lastPlacedCorePos_y = (float)m_blockAction.lastPlacedCorePos_y;
		float lastGroundLevel = (float)m_blockAction.GetHighestPoint().y;
		Vector3[] topPoints = new Vector3[4];
		Vector3[] bottomPoints = new Vector3[4];

		Vector3 startPos1 = new Vector3(fillVertex[0].x - 0.5f, lastPlacedCorePos_y, fillVertex[0].z - 0.5f);
		Vector3 startPos2 = new Vector3(fillVertex[1].x + 0.5f, lastPlacedCorePos_y, fillVertex[0].z - 0.5f);
		Vector3 startPos3 = new Vector3(fillVertex[1].x + 0.5f, lastPlacedCorePos_y, fillVertex[1].z + 0.5f);
		Vector3 startPos4 = new Vector3(fillVertex[0].x - 0.5f, lastPlacedCorePos_y, fillVertex[1].z + 0.5f);

		topPoints[0] = new Vector3(fillVertex[0].x - 0.5f, lastGroundLevel, fillVertex[0].z - 0.5f);
		topPoints[1] = new Vector3(fillVertex[1].x + 0.5f, lastGroundLevel, fillVertex[0].z - 0.5f);
		topPoints[2] = new Vector3(fillVertex[1].x + 0.5f, lastGroundLevel, fillVertex[1].z + 0.5f);
		topPoints[3] = new Vector3(fillVertex[0].x - 0.5f, lastGroundLevel, fillVertex[1].z + 0.5f);

		bottomPoints[0] = new Vector3(fillVertex[0].x - 0.5f, lasttimeGround, fillVertex[0].z - 0.5f);
		bottomPoints[1] = new Vector3(fillVertex[1].x + 0.5f, lasttimeGround, fillVertex[0].z - 0.5f);
		bottomPoints[2] = new Vector3(fillVertex[1].x + 0.5f, lasttimeGround, fillVertex[1].z + 0.5f);
		bottomPoints[3] = new Vector3(fillVertex[0].x - 0.5f, lasttimeGround, fillVertex[1].z + 0.5f);

		#region pattern1(-X,-Z)
		Vector3 topBase1 = new Vector3(startPos1.x, lastGroundLevel, startPos1.z);
		Vector3 bottomBase1 = new Vector3(startPos1.x, lasttimeGround, startPos1.z);
		AddLine(startPos1, topBase1);
		AddLine(startPos1, bottomBase1);
		AddLine(topPoints[0], topPoints[1]);
		AddLine(topPoints[0], topPoints[3]);
		AddLine(bottomPoints[0], bottomPoints[1]);
		AddLine(bottomPoints[0], bottomPoints[3]);
		AddLine(topPoints[1], topPoints[2]);
		AddLine(topPoints[3], topPoints[2]);
		AddLine(bottomPoints[1], bottomPoints[2]);
		AddLine(bottomPoints[3], bottomPoints[2]);
		Vector3 middleY1_1 = new Vector3(topPoints[1].x, lastPlacedCorePos_y, topPoints[1].z);
		Vector3 middleY3_1 = new Vector3(topPoints[3].x, lastPlacedCorePos_y, topPoints[3].z);
		AddLine(topPoints[1], middleY1_1);
		AddLine(bottomPoints[1], middleY1_1);
		AddLine(topPoints[3], middleY3_1);
		AddLine(bottomPoints[3], middleY3_1);
		Vector3 middleY2_1 = new Vector3(topPoints[2].x, lastPlacedCorePos_y, topPoints[2].z);
		AddLine(topPoints[2], middleY2_1);
		AddLine(bottomPoints[2], middleY2_1);
		#endregion

		#region pattern2(X,-Z)
		Vector3 topBase2 = new Vector3(startPos2.x, lastGroundLevel, startPos2.z);
		Vector3 bottomBase2 = new Vector3(startPos2.x, lasttimeGround, startPos2.z);
		AddLine(startPos2, topBase2);
		AddLine(startPos2, bottomBase2);
		AddLine(topPoints[1], topPoints[2]);
		AddLine(topPoints[1], topPoints[0]);
		AddLine(bottomPoints[1], bottomPoints[2]);
		AddLine(bottomPoints[1], bottomPoints[0]);
		AddLine(topPoints[2], topPoints[3]);
		AddLine(topPoints[0], topPoints[3]);
		AddLine(bottomPoints[2], bottomPoints[3]);
		AddLine(bottomPoints[0], bottomPoints[3]);
		Vector3 middleY1_2 = new Vector3(topPoints[2].x, lastPlacedCorePos_y, topPoints[2].z);
		Vector3 middleY3_2 = new Vector3(topPoints[0].x, lastPlacedCorePos_y, topPoints[0].z);
		AddLine(topPoints[2], middleY1_2);
		AddLine(bottomPoints[2], middleY1_2);
		AddLine(topPoints[0], middleY3_2);
		AddLine(bottomPoints[0], middleY3_2);
		Vector3 middleY2_2 = new Vector3(topPoints[3].x, lastPlacedCorePos_y, topPoints[3].z);
		AddLine(topPoints[3], middleY2_2);
		AddLine(bottomPoints[3], middleY2_2);
		#endregion

		#region pattern3(X,Z)
		Vector3 topBase3 = new Vector3(startPos3.x, lastGroundLevel, startPos3.z);
		Vector3 bottomBase3 = new Vector3(startPos3.x, lasttimeGround, startPos3.z);
		AddLine(startPos3, topBase3);
		AddLine(startPos3, bottomBase3);
		AddLine(topPoints[2], topPoints[3]);
		AddLine(topPoints[2], topPoints[1]);
		AddLine(bottomPoints[2], bottomPoints[3]);
		AddLine(bottomPoints[2], bottomPoints[1]);
		AddLine(topPoints[3], topPoints[0]);
		AddLine(topPoints[1], topPoints[0]);
		AddLine(bottomPoints[3], bottomPoints[0]);
		AddLine(bottomPoints[1], bottomPoints[0]);
		Vector3 middleY1_3 = new Vector3(topPoints[3].x, lastPlacedCorePos_y, topPoints[3].z);
		Vector3 middleY3_3 = new Vector3(topPoints[1].x, lastPlacedCorePos_y, topPoints[1].z);
		AddLine(topPoints[3], middleY1_3);
		AddLine(bottomPoints[3], middleY1_3);
		AddLine(topPoints[1], middleY3_3);
		AddLine(bottomPoints[1], middleY3_3);
		Vector3 middleY2_3 = new Vector3(topPoints[0].x, lastPlacedCorePos_y, topPoints[0].z);
		AddLine(topPoints[0], middleY2_3);
		AddLine(bottomPoints[0], middleY2_3);
		#endregion

		#region pattern4(-X,Z)
		Vector3 topBase4 = new Vector3(startPos4.x, lastGroundLevel, startPos4.z);
		Vector3 bottomBase4 = new Vector3(startPos4.x, lasttimeGround, startPos4.z);
		AddLine(startPos4, topBase4);
		AddLine(startPos4, bottomBase4);
		AddLine(topPoints[3], topPoints[0]);
		AddLine(topPoints[3], topPoints[2]);
		AddLine(bottomPoints[3], bottomPoints[0]);
		AddLine(bottomPoints[3], bottomPoints[2]);
		AddLine(topPoints[0], topPoints[1]);
		AddLine(topPoints[2], topPoints[1]);
		AddLine(bottomPoints[0], bottomPoints[1]);
		AddLine(bottomPoints[2], bottomPoints[1]);
		Vector3 middleY1_4 = new Vector3(topPoints[0].x, lastPlacedCorePos_y, topPoints[0].z);
		Vector3 middleY3_4 = new Vector3(topPoints[2].x, lastPlacedCorePos_y, topPoints[2].z);
		AddLine(topPoints[0], middleY1_4);
		AddLine(bottomPoints[0], middleY1_4);
		AddLine(topPoints[2], middleY3_4);
		AddLine(bottomPoints[2], middleY3_4);
		Vector3 middleY2_4 = new Vector3(topPoints[1].x, lastPlacedCorePos_y, topPoints[1].z);
		AddLine(topPoints[1], middleY2_4);
		AddLine(bottomPoints[1], middleY2_4);
		#endregion
	}

	private void AddLine(Vector3 start, Vector3 end)
	{
		LineData newLine = new LineData { start = start, end = end };
		linesToDraw.Add(newLine);

		GameObject lineObj = new GameObject("Line_" + lineObjects.Count);
		lineObj.transform.SetParent(this.transform);

		LineRenderer lr = lineObj.AddComponent<LineRenderer>();
		lr.useWorldSpace = false;
		lr.startWidth = lineWidth;
		lr.endWidth = lineWidth;

		if (lineMaterial != null)
		{
			lr.material = lineMaterial;
		}
		else
		{
			lr.material = new Material(Shader.Find("Sprites/Default"));
		}

		lr.SetPosition(0, start);
		lr.SetPosition(1, start);

		lineObjects.Add(lineObj);
	}

	public IEnumerator AnimateGroups()
	{
		isAnimating = true;

		// 既存のラインとパーティクルをすべて破棄してクリーンアップ
		foreach (var obj in lineObjects)
		{
			Destroy(obj);
		}
		lineObjects.Clear();
		linesToDraw.Clear();

		foreach (var particle in particleInstances)
		{
			Destroy(particle);
		}
		particleInstances.Clear();

		GenerateCubeLines();

		// 全てのパーティクルをアニメーション開始前に一度だけ生成
		if (particlePrefab != null)
		{
			for (int i = 0; i < linesToDraw.Count; i++)
			{
				GameObject particleInstance = Instantiate(particlePrefab, this.transform);
				particleInstance.SetActive(false); // 初期状態では非表示
				particleInstances.Add(particleInstance);
			}
		}

		switch (lineanimation_pattern)
		{
			case 1:
				yield return StartCoroutine(AnimateGroup(0, 2, group1Duration));
				yield return StartCoroutine(AnimateGroup(2, 6, group2Duration));
				yield return StartCoroutine(AnimateGroup(6, 14, group3Duration));
				yield return StartCoroutine(AnimateGroup(14, 16, group4Duration));
				break;
			case 2:
				yield return StartCoroutine(AnimateGroup(16, 18, group1Duration));
				yield return StartCoroutine(AnimateGroup(18, 22, group2Duration));
				yield return StartCoroutine(AnimateGroup(22, 30, group3Duration));
				yield return StartCoroutine(AnimateGroup(30, 32, group4Duration));
				break;
			case 3:
				yield return StartCoroutine(AnimateGroup(32, 34, group1Duration));
				yield return StartCoroutine(AnimateGroup(34, 38, group2Duration));
				yield return StartCoroutine(AnimateGroup(38, 46, group3Duration));
				yield return StartCoroutine(AnimateGroup(46, 48, group4Duration));
				break;
			case 4:
				yield return StartCoroutine(AnimateGroup(48, 50, group1Duration));
				yield return StartCoroutine(AnimateGroup(50, 54, group2Duration));
				yield return StartCoroutine(AnimateGroup(54, 62, group3Duration));
				yield return StartCoroutine(AnimateGroup(62, 64, group4Duration));
				break;
			default:
				yield return StartCoroutine(AnimateGroup(0, 2, group1Duration));
				yield return StartCoroutine(AnimateGroup(2, 6, group2Duration));
				yield return StartCoroutine(AnimateGroup(6, 14, group3Duration));
				yield return StartCoroutine(AnimateGroup(14, 16, group4Duration));
				break;
		}

        if (Ground_Level_Mode == GroundLevelMode.UpdateGroundLevel)
        {
            lasttimeGround = (float)m_blockAction.lastGroundLevel + 0.5f;
        }

        yield return new WaitForSeconds(clearDelay);

		// 最終的なクリーンアップ
		foreach (var obj in lineObjects)
		{
			Destroy(obj);
		}
		lineObjects.Clear();
		linesToDraw.Clear();

		foreach (var particle in particleInstances)
		{
			Destroy(particle);
		}
		particleInstances.Clear();

		isAnimating = false;
	}

	/// <summary>
	/// 指定された範囲のラインを同時にアニメーションさせるコルーチン
	/// </summary>
	private IEnumerator AnimateGroup(int startIndex, int endIndex, float duration)
	{
		float elapsedTime = 0;

		// このループではパーティクルは生成しない。既存のものを再利用する。
		for (int i = startIndex; i < endIndex; i++)
		{
			int particleIndex = i;
			if (particleIndex < particleInstances.Count)
			{
				GameObject particle = particleInstances[particleIndex];
				particle.SetActive(true);
				ParticleSystem ps = particle.GetComponent<ParticleSystem>();
				if (ps != null)
				{
					ps.Play();
				}
			}
		}

		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float progress = elapsedTime / duration;

			for (int i = startIndex; i < endIndex; i++)
			{
				LineRenderer lr = lineObjects[i].GetComponent<LineRenderer>();
				Vector3 start = linesToDraw[i].start;
				Vector3 end = linesToDraw[i].end;
				Vector3 animatedEnd = Vector3.Lerp(start, end, progress);
				lr.SetPosition(1, animatedEnd);

				int particleIndex = i;
				if (particleIndex < particleInstances.Count)
				{
					GameObject particle = particleInstances[particleIndex];
					particle.transform.position = animatedEnd;
				}
			}
			yield return null;
		}

		// アニメーション完了後、終点を確定しパーティクルを停止
		for (int i = startIndex; i < endIndex; i++)
		{
			LineRenderer lr = lineObjects[i].GetComponent<LineRenderer>();
			lr.SetPosition(1, linesToDraw[i].end);

			int particleIndex = i;
			if (particleIndex < particleInstances.Count)
			{
				ParticleSystem ps = particleInstances[particleIndex].GetComponent<ParticleSystem>();
				if (ps != null)
				{
					ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
					particleInstances[particleIndex].SetActive(false); // 次回に備えて非表示にする
				}
			}
		}
	}
}