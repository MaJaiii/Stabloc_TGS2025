using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class NextBlockColor : MonoBehaviour
{
    Image image;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image = GetComponent<Image>();       
    }

    void Update()
    {
        if (image)
        {
            image.material.SetColor("_BaseColor", image.color);
        }
    }
}
