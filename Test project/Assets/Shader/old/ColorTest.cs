using UnityEngine;

public class ColorTest : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    void Start()
    {
        // このスクリプトがアタッチされたオブジェクトのMeshRendererコンポーネントを取得
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        // Jキーが押されたら、マテリアルの_BackgroundColorを緑色に設定
        if (Input.GetKeyDown(KeyCode.J))
        {
            meshRenderer.material.SetColor("_BackgroundColor", Color.green);
            Debug.Log("Color changed to Green.");
        }

        // Kキーが押されたら、マテリアルの_BackgroundColorを赤色に設定
        if (Input.GetKeyDown(KeyCode.K))
        {
            meshRenderer.material.SetColor("_BackgroundColor", Color.red);
            Debug.Log("Color changed to Red.");
        }
    }
}