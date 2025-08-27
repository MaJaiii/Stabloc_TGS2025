using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class CameraToRawImage : MonoBehaviour
{
    public Camera sourceCamera;            // assign in inspector
    public int renderWidth = 0;            // 0 = match RawImage rect (canvas pixels)
    public int renderHeight = 0;
    public RenderTextureFormat format = RenderTextureFormat.Default;
    public int depthBuffer = 24;

    RawImage rawImage;
    RenderTexture rt;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        if (sourceCamera == null)
        {
            Debug.LogError("CameraToRawImage: sourceCamera not assigned.");
            enabled = false;
            return;
        }
        SetupRenderTexture();
    }

    void SetupRenderTexture()
    {
        // Determine size
        int w = renderWidth;
        int h = renderHeight;

        if (w <= 0 || h <= 0)
        {
            // Try to get size from the RawImage's RectTransform in canvas pixels
            var rtRect = rawImage.rectTransform;
            Canvas canvas = rawImage.canvas;
            if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
            {
                Vector2 size = RectTransformUtility.PixelAdjustRect(rtRect, canvas).size;
                w = Mathf.Max(1, Mathf.RoundToInt(size.x));
                h = Mathf.Max(1, Mathf.RoundToInt(size.y));
            }
            else
            {
                // fallback default
                w = 1024; h = 1024;
            }
        }

        // Create RenderTexture
        rt = new RenderTexture(w, h, depthBuffer, format)
        {
            name = "RT_CameraToRawImage_" + sourceCamera.name,
            useMipMap = false,
            autoGenerateMips = false,
        };
        rt.Create();

        // Assign
        sourceCamera.targetTexture = rt;
        rawImage.texture = rt;
    }

    // If you change RawImage size at runtime and want to recreate the RT:
    public void RecreateForSize(int width, int height)
    {
        renderWidth = width;
        renderHeight = height;
        Cleanup();
        SetupRenderTexture();
    }

    void OnDisable()
    {
        Cleanup();
    }

    void OnDestroy()
    {
        Cleanup();
    }

    void Cleanup()
    {
        if (sourceCamera != null && sourceCamera.targetTexture == rt)
            sourceCamera.targetTexture = null;

        if (rt != null)
        {
            rt.Release();
            Destroy(rt);
            rt = null;
        }
        if (rawImage != null)
            rawImage.texture = null;
    }
}
