using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dropdown_CursorVIsibility : MonoBehaviour
{
    TMP_Dropdown dropdown;
    int currentIndex;

    [SerializeField]
    TextMeshProUGUI consoleText;

    private void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        if (InGameSetting.isCursorVisible) currentIndex = 0;
        else currentIndex = 1;

        dropdown.value = currentIndex;
    }

    private void Update()
    {
        if (dropdown.value == currentIndex) return;
        switch (dropdown.value)
        {
            case 0:
                InGameSetting.isCursorVisible = true;
                break;
            case 1:
                InGameSetting.isCursorVisible = false;
                break;
        }
        currentIndex = dropdown.value;
        consoleText.color = Color.white;
        consoleText.text = "Cursor visibility ... is modify as " + InGameSetting.isCursorVisible;
    }
}
