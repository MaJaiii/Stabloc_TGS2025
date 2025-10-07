using TMPro;
using UnityEngine;

public class Dropdown_ColorRandomize : MonoBehaviour
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
        currentIndex = settings.isColorRandom ? 0 : 1;

        dropdown.value = currentIndex;
    }

    private void Update()
    {
        if (dropdown.value == currentIndex) return;
        switch (dropdown.value)
        {
            case 0:
                settings.isColorRandom = true;
                break;
            case 1:
                settings.isColorRandom = false;
                break;
        }
        currentIndex = dropdown.value;
        consoleText.color = Color.white;
        consoleText.text = "Color randomization ... is modify as " + settings.isColorRandom;
        CsvSettingsSaver.Save(settings);

    }

}
