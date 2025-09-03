// BlockGlowController.cs
using UnityEngine;

public class BlockGlowController : MonoBehaviour
{
    // C#から直接オフセットを設定するためのパブリックメソッド
    public void SetBlinkOffset(float offset)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            // renderer.sharedMaterialから新しいマテリアルインスタンスを作成
            // renderer.materialに割り当て
            renderer.material = new Material(renderer.sharedMaterial);


            // オフセットをマテリアルのインスタンスに設定
            renderer.material.SetFloat("_BlinkOffset", offset);
        }
    }
}