using UnityEngine;
using UnityEngine.UI;

public class ObjectPreviewController : MonoBehaviour
{
    [Header("References")]
    public Camera renderCamera;       // Secondary camera, ONLY for UI render
    public RawImage rawImage;         // UI RawImage to display the block
    public RenderTexture renderTex;   // Assigned RenderTexture asset
    public Transform pentacubeRoot;   // Parent object of the 5 cubes
    public Camera mainCamera;         // Reference to the main game camera

    [Header("View Settings")]
    [Tooltip("Camera offset from pentacube root")]
    public Vector3 cameraOffset = new Vector3(0, 2, -6);

    [Tooltip("Fixed tilt (X,Z) applied to render camera")]
    public Vector2 fixedTilt = new Vector2(20, 0);
    // X = look down tilt, Z = roll (rarely needed)

    void LateUpdate()
    {
        if (!renderCamera || !rawImage || !renderTex || !pentacubeRoot || !mainCamera)
            return;

        // Ensure render target & texture binding
        if (renderCamera.targetTexture != renderTex)
            renderCamera.targetTexture = renderTex;
        if (rawImage.texture != renderTex)
            rawImage.texture = renderTex;

        // Position render camera relative to the pentacube
        renderCamera.transform.position = pentacubeRoot.position + cameraOffset;

        // Extract only Y rotation from main camera
        float mainY = mainCamera.transform.eulerAngles.y;

        // Build final rotation (use inspector tilt for X/Z, mainCamera Y)
        Quaternion finalRot = Quaternion.Euler(fixedTilt.x, mainY, fixedTilt.y);

        renderCamera.transform.rotation = finalRot;
    }
}
