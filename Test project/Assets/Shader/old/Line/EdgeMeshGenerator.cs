using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Transform))]
public class VoxelEdgeMesh : MonoBehaviour
{
    // エッジ描画用マテリアル
    [SerializeField] private Material m_edgeMaterial;

    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshRenderer;

    // 立方体の8つの頂点座標
    private static readonly Vector3[] cubeCorners = new Vector3[]
    {
        new Vector3(-0.5f,-0.5f,-0.5f),
        new Vector3(0.5f,-0.5f,-0.5f),
        new Vector3(0.5f,0.5f,-0.5f),
        new Vector3(-0.5f,0.5f,-0.5f),
        new Vector3(-0.5f,-0.5f,0.5f),
        new Vector3(0.5f,-0.5f,0.5f),
        new Vector3(0.5f,0.5f,0.5f),
        new Vector3(-0.5f,0.5f,0.5f)
    };

    // 立方体の12の辺（頂点インデックスのペア）
    private static readonly int[,] cubeEdges = new int[,]
    {
        {0,1},{1,2},{2,3},{3,0},
        {4,5},{5,6},{6,7},{7,4},
        {0,4},{1,5},{2,6},{3,7}
    };

    // 各面に属する辺のインデックス
    private static readonly int[][] faceEdges = new int[][]
    {
        new int[]{0,1,2,3},   // Back (-Z)
        new int[]{4,5,6,7},   // Front (+Z)
        new int[]{0,9,4,8},   // Bottom (-Y)
        new int[]{2,10,6,11}, // Top (+Y)
        new int[]{1,10,5,9},  // Right (+X)
        new int[]{3,11,7,8}   // Left (-X)
    };

    // 各面の法線方向
    private static readonly Vector3[] faceDirs = new Vector3[]
    {
        Vector3.back, Vector3.forward,
        Vector3.down, Vector3.up,
        Vector3.right, Vector3.left
    };

    void Start()
    {
        // UV連続化版のメッシュ生成関数を呼び出し
        GenerateEdgeMeshWithUV();
    }

    void GenerateEdgeMesh()
    {
        // 親オブジェクトの子を全て取得
        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            children[i] = transform.GetChild(i);

        // ボクセル位置をハッシュセットに格納
        HashSet<Vector3Int> cubePositions = new HashSet<Vector3Int>();
        foreach (var child in children)
            cubePositions.Add(Vector3Int.RoundToInt(child.position));

        // 重複する辺を格納するためのハッシュセット
        HashSet<string> edgeSet = new HashSet<string>();
        // メッシュ構築用リスト
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int idx = 0;

        foreach (var child in children)
        {
            Vector3Int cubePos = Vector3Int.RoundToInt(child.position);

            // 隣接するボクセルの存在チェック
            bool[] hideFace = new bool[6];
            for (int f = 0; f < 6; f++)
                hideFace[f] = cubePositions.Contains(cubePos + Vector3Int.RoundToInt(faceDirs[f]));

            // 各辺について処理
            for (int e = 0; e < cubeEdges.GetLength(0); e++)
            {
                // 辺が隠されるか判定
                bool hideEdge = false;
                for (int f = 0; f < 6; f++)
                    if (hideFace[f] && System.Array.IndexOf(faceEdges[f], e) != -1)
                        hideEdge = true;

                // 隠される辺はスキップ
                if (hideEdge) continue;

                // 頂点座標をワールド空間からローカル空間に変換
                Vector3 v0 = transform.InverseTransformPoint(
                    child.localToWorldMatrix.MultiplyPoint(Vector3.Scale(cubeCorners[cubeEdges[e, 0]], child.localScale)));
                Vector3 v1 = transform.InverseTransformPoint(
                    child.position + child.rotation * Vector3.Scale(cubeCorners[cubeEdges[e, 1]], child.localScale));

                // 辺の一意なキーを作成（頂点の順序に依存しない）
                string key = (v0.x < v1.x || (Mathf.Abs(v0.x - v1.x) < 0.001f && v0.y < v1.y) ||
                              (Mathf.Abs(v0.x - v1.x) < 0.001f && Mathf.Abs(v0.y - v1.y) < 0.001f && v0.z < v1.z)) ?
                              v0.ToString() + "_" + v1.ToString() : v1.ToString() + "_" + v0.ToString();

                // 未登録の辺のみ追加
                if (!edgeSet.Contains(key))
                {
                    edgeSet.Add(key);

                    // 頂点、インデックス、UVを追加
                    vertices.Add(v0);
                    vertices.Add(v1);

                    indices.Add(idx++);
                    indices.Add(idx++);

                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(1, 0));
                }
            }
        }

        // メッシュを保持する新しいゲームオブジェクトを作成
        GameObject edgeObj = new GameObject("EdgeMesh");
        edgeObj.transform.SetParent(transform, false);
        // メッシュ表示に必要なコンポーネントを追加
        m_meshFilter = edgeObj.AddComponent<MeshFilter>();
        m_meshRenderer = edgeObj.AddComponent<MeshRenderer>();
        // マテリアルを適用
        m_meshRenderer.material = m_edgeMaterial;

        // メッシュを作成し、データを設定
        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        // トポロジーをLineに設定
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        mesh.SetUVs(0, uvs);

        // メッシュをMeshFilterに割り当て
        m_meshFilter.mesh = mesh;

        // 生成された辺の数をログに出力
        Debug.Log("EdgeMesh生成完了: " + edgeSet.Count + "本");
    }

    void GenerateEdgeMeshWithUV()
    {
        // 子ボクセルを全て取得
        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            children[i] = transform.GetChild(i);

        // ボクセル位置をハッシュセットに格納
        HashSet<Vector3Int> cubePositions = new HashSet<Vector3Int>();
        foreach (var child in children)
            cubePositions.Add(Vector3Int.RoundToInt(child.position));

        // 重複辺を避けるためのハッシュセット
        HashSet<string> edgeSet = new HashSet<string>();
        // 外側の辺のリスト
        List<Vector3[]> edgeList = new List<Vector3[]>();

        // 外周の辺を抽出
        foreach (var child in children)
        {
            Vector3Int cubePos = Vector3Int.RoundToInt(child.position);

            // 隣接ボクセルの存在チェック
            bool[] hideFace = new bool[6];
            for (int f = 0; f < 6; f++)
                hideFace[f] = cubePositions.Contains(cubePos + Vector3Int.RoundToInt(faceDirs[f]));

            for (int e = 0; e < cubeEdges.GetLength(0); e++)
            {
                // 辺が隠されるか判定
                bool hideEdge = false;
                for (int f = 0; f < 6; f++)
                    if (hideFace[f] && System.Array.IndexOf(faceEdges[f], e) != -1)
                        hideEdge = true;

                if (hideEdge) continue;

                // 頂点座標をローカル空間に変換
                Vector3 v0 = transform.InverseTransformPoint(
                    child.position + child.rotation * Vector3.Scale(cubeCorners[cubeEdges[e, 0]], child.localScale));
                Vector3 v1 = transform.InverseTransformPoint(
                    child.position + child.rotation * Vector3.Scale(cubeCorners[cubeEdges[e, 1]], child.localScale));

                // 辺の一意なキーを作成
                string key = (v0.x < v1.x || (Mathf.Abs(v0.x - v1.x) < 0.001f && v0.y < v1.y) ||
                              (Mathf.Abs(v0.x - v1.x) < 0.001f && Mathf.Abs(v0.y - v1.y) < 0.001f && v0.z < v1.z)) ?
                              v0.ToString() + "_" + v1.ToString() : v1.ToString() + "_" + v0.ToString();

                // 未登録の辺のみリストに追加
                if (!edgeSet.Contains(key))
                {
                    edgeSet.Add(key);
                    edgeList.Add(new Vector3[] { v0, v1 });
                }
            }
        }

        // 辺の総長を計算
        float totalLength = 0f;
        float[] edgeLengths = new float[edgeList.Count];
        for (int i = 0; i < edgeList.Count; i++)
        {
            edgeLengths[i] = Vector3.Distance(edgeList[i][0], edgeList[i][1]);
            totalLength += edgeLengths[i];
        }

        // メッシュ構築用リスト
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int idx = 0;
        float accumulated = 0f;

        // 線長に応じたUV値を設定
        foreach (var edge in edgeList)
        {
            Vector3 v0 = edge[0];
            Vector3 v1 = edge[1];
            // UV.xの開始値と終了値を計算
            float uv0 = accumulated / totalLength;
            float uv1 = (accumulated + Vector3.Distance(v0, v1)) / totalLength;
            // 累積長を更新
            accumulated += Vector3.Distance(v0, v1);

            // 頂点、インデックス、UVを追加
            vertices.Add(v0); vertices.Add(v1);
            indices.Add(idx++); indices.Add(idx++);
            uvs.Add(new Vector2(uv0, 0)); uvs.Add(new Vector2(uv1, 0));
        }

        // メッシュを保持するゲームオブジェクトを作成
        GameObject edgeObj = new GameObject("EdgeMesh");
        edgeObj.transform.SetParent(transform, false);
        // コンポーネントを追加
        m_meshFilter = edgeObj.AddComponent<MeshFilter>();
        m_meshRenderer = edgeObj.AddComponent<MeshRenderer>();
        // マテリアルを適用
        m_meshRenderer.material = m_edgeMaterial;

        // メッシュを作成し、データを設定
        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        mesh.SetUVs(0, uvs);
        m_meshFilter.mesh = mesh;

        // 生成ログ
        Debug.Log("EdgeMesh生成完了（UV連続化）: " + edgeSet.Count + "本");
    }
}