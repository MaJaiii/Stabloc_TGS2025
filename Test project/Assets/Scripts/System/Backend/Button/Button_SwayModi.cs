using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Button_SwayModi : MonoBehaviour
{
    Button button;
    [SerializeField]
    TextMeshProUGUI consoleText;
    [SerializeField]
    TMP_InputField inputField;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
        inputField.text = InGameSetting.swayLimit.ToString();
    }

    void OnButtonClicked()
    {
        float swayLimit = 0;
        if (float.TryParse(inputField.text, out swayLimit))
        {
            if (swayLimit > 0)
            {
                consoleText.color = Color.white;
                consoleText.text = "Sway modification... Sucess! Now sway limit is set to " + swayLimit;
                InGameSetting.swayLimit = swayLimit;
            }
            else
            {
                consoleText.color = Color.yellow;
                consoleText.text = "Error! Invaild value!";
            }
        }
        else
        {
            consoleText.color = Color.yellow;
            consoleText.text = "Error! No value detected!";
        }
    }
}
