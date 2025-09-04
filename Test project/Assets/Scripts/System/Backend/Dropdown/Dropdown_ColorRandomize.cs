using TMPro;
using UnityEngine;

public class Dropdown_ColorRandomize : MonoBehaviour
{
    TMP_Dropdown dropdown;
    int currentIndex;

    [SerializeField]
    TextMeshProUGUI consoleText;

    private void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        if (InGameSetting.isColorRandomize) currentIndex = 0;
        else currentIndex = 1;

        dropdown.value = currentIndex;
    }

    private void Update()
    {
        if (dropdown.value == currentIndex) return;
        switch (dropdown.value)
        {
            case 0:
                InGameSetting.isColorRandomize = true;
                break;
            case 1:
                InGameSetting.isColorRandomize = false;
                break;
        }
        currentIndex = dropdown.value;
        consoleText.color = Color.white;
        consoleText.text = "Color randomization ... is modify as " + InGameSetting.isColorRandomize;
    }

}
