using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BeatLogoShine : MonoBehaviour
{
    public enum BeatEventType { OnBar, OnBeat }
    [SerializeField] private BeatEventType beatEventType = BeatEventType.OnBar;

    [SerializeField] private List<Image> targetImages = new List<Image>();

    [SerializeField] private float edgeBase = 1f;
    [SerializeField] private float surfaceBase = 1f;
    [SerializeField] private float edgeBoost = 3f;
    [SerializeField] private float surfaceBoost = 2f;
    [SerializeField] private float decaySpeed = 2f;

    private float currentEdge;
    private float currentSurface;

    private List<Material> mats = new List<Material>();

    private void Start()
    {
        foreach (var img in targetImages)
        {
            if (img != null)
            {
                mats.Add(img.material);
            }
        }

        currentEdge = edgeBase;
        currentSurface = surfaceBase;

        // イベント登録
        if (AudioController.Instance != null)
        {
            switch (beatEventType)
            {
                case BeatEventType.OnBar:
                    AudioController.Instance.OnBar += HandleBeat;
                    break;
                case BeatEventType.OnBeat:
                    AudioController.Instance.OnBeat += HandleBeat;
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        // イベント解除
        if (AudioController.Instance != null)
        {
            switch (beatEventType)
            {
                case BeatEventType.OnBar:
                    AudioController.Instance.OnBar -= HandleBeat;
                    break;
                case BeatEventType.OnBeat:
                    AudioController.Instance.OnBeat -= HandleBeat;
                    break;
            }
        }
    }

    private void HandleBeat(int beatCount)
    {
        currentEdge = edgeBoost;
        currentSurface = surfaceBoost;
    }

    private void Update()
    {
        currentEdge = Mathf.MoveTowards(currentEdge, edgeBase, decaySpeed * Time.deltaTime);
        currentSurface = Mathf.MoveTowards(currentSurface, surfaceBase, decaySpeed * Time.deltaTime);

        foreach (var mat in mats)
        {
            if (mat != null)
            {
                mat.SetFloat("_EdgeShine", currentEdge);
                mat.SetFloat("_SurfaceShine", currentSurface);
            }
        }
    }
}