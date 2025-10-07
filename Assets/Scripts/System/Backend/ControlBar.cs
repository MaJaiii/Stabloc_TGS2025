using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlBar : MonoBehaviour
{
    [SerializeField]
    Image controlBarImage;

    [SerializeField]
    Sprite[] controlBarSprites;


    private void Update()
    {
        if (Gamepad.current == null)
        {
            controlBarImage.sprite = controlBarSprites[0];
        }
        else
        {
            controlBarImage.sprite = controlBarSprites[1];
        }
    }
}
