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

    GameSettings settings;

    private void Start()
    {
        settings = CsvSettingsLoader.Load();

        dropdown = GetComponent<TMP_Dropdown>();
        currentIndex = settings.isCursorHidden ? 1 : 0;

        dropdown.value = currentIndex;
    }

    private void Update()
    {
        if (dropdown.value == currentIndex) return;
        switch (dropdown.value)
        {
            case 0:
                settings.isCursorHidden = false;
                break;
            case 1:
                settings.isCursorHidden = true;
                break;
        }
        currentIndex = dropdown.value;
        consoleText.color = Color.white;
        consoleText.text = "Cursor visibility ... is modify as " + !settings.isCursorHidden;
        CsvSettingsSaver.Save(settings);
    }
}
