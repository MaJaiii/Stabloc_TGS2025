using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldManager : MonoBehaviour
{

    TMP_InputField inputField;
    [SerializeField] string subject;
    [SerializeField] TextMeshProUGUI consoleText;

    GameSettings settings;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(OnInputFieldDeselected);

        settings = CsvSettingsLoader.Load();

        switch (subject)
        {
            case "masterVolume":
                inputField.text = settings.masterVolume.ToString();
                break;
            case "timeLimitation":
                inputField.text = settings.timeLimitation.ToString();
                break;
            case "coreFrequency[0]":
                inputField.text = settings.CoreFrom.ToString();
                break;
            case "coreFrequency[1]":
                inputField.text = settings.CoreGet.ToString();
                break;
            default:
                Debug.LogError($"{transform.parent.name} is not set to a valid subject!");
                break;
        }
    }

    
    void OnInputFieldDeselected(string value)
    {
        switch (subject)
        {
            case "masterVolume":
                int volume; 
                if (int.TryParse(value, out volume) && volume >= 0)
                {
                    settings.masterVolume = volume;
                    consoleText.color = Color.white;
                    consoleText.text = $"Master volume is now been set to {settings.masterVolume}.";
                }
                else
                {
                    consoleText.color = Color.red;
                    consoleText.text = "Error! Invaild input!";
                }
                break;
            case "timeLimitation":
                int time;
                if (int.TryParse(value, out time) && time >= 0)
                {
                    settings.timeLimitation = time;
                    consoleText.color = Color.white;
                    consoleText.text = $"Time limitation is now been set to {settings.timeLimitation}";
                }
                else
                {
                    consoleText.color = Color.red;
                    consoleText.text = "Error! Invaild input!";
                }
                break;
            case "coreFrequency[0]":
                int freq;
                if (int.TryParse(value, out freq) && freq >= 1)
                {
                    settings.CoreFrom = freq;
                    consoleText.color = Color.white;
                    consoleText.text = $"Core will now appear {settings.CoreGet} time(s) in {settings.CoreFrom} block(s)";
                }
                else
                {
                    consoleText.color = Color.red;
                    consoleText.text = "Error! Invaild input!";
                }
                break;
            case "coreFrequency[1]":
                int times;
                if (int.TryParse(value, out times) && times >= 0)
                {
                    settings.CoreGet = times;
                    consoleText.color = Color.white;
                    consoleText.text = $"Core will now appear {settings.CoreGet} time(s) in {settings.CoreFrom} block(s)";
                }
                else
                {
                    consoleText.color = Color.red;
                    consoleText.text = "Error! Invaild input!";
                }
                break;
            default:
                break;
        }
        CsvSettingsSaver.Save(settings);
    }
}
