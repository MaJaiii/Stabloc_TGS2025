using UnityEngine;
using UnityEngine.UI;

public class CameraOrbit : MonoBehaviour
{
    [Header("References")]
    public Camera renderCamera;       // Secondary camera for UI render
    public RawImage rawImage;         // UI RawImage to display the block
    public RenderTexture renderTex;   // Assigned RenderTexture asset
    public Transform pentacubeRoot;   // Parent object of the 5 cubes
    public Camera mainCamera;         // Main game camera

    [Header("View Settings")]
    [Tooltip("Extra tilt/offset rotation applied on top of main camera orbit")]
    public Vector3 additionalRotation = new Vector3(20, 0, 0);

    [Tooltip("Scale of orbit offset compared to main camera (1 = exact same)")]
    public float orbitScale = 1f;

    void LateUpdate()
    {
        if (!renderCamera || !rawImage || !renderTex || !pentacubeRoot || !mainCamera)
            return;

        // Ensure render target & texture binding
        if (renderCamera.targetTexture != renderTex)
            renderCamera.targetTexture = renderTex;
        if (rawImage.texture != renderTex)
            rawImage.texture = renderTex;

        // Calculate offset between main camera and pentacube
        Vector3 mainOffset = mainCamera.transform.position - pentacubeRoot.position;

        // Scale offset if needed (lets you zoom preview independently)
        Vector3 orbitOffset = mainOffset * orbitScale;

        // Position render camera relative to current pentacube position
        renderCamera.transform.position = pentacubeRoot.position + orbitOffset;

        // Always look at the pentacube root
        renderCamera.transform.LookAt(pentacubeRoot);

        // Apply optional inspector-defined adjustment
        renderCamera.transform.rotation *= Quaternion.Euler(additionalRotation);
    }
}
