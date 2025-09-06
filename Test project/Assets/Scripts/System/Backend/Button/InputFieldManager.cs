using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldManager : MonoBehaviour
{

    TMP_InputField inputField;
    [SerializeField] string subject;
    [SerializeField] TextMeshProUGUI consoleText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(OnValueChange);

        switch (subject)
        {
            case "masterVolume":
                inputField.text = InGameSetting.masterVolume.ToString();
                break;
            case "timeLimitation":
                inputField.text = InGameSetting.timeLimitation.ToString();
                break;
            case "coreFrequency[0]":
                inputField.text = InGameSetting.coreFrequency[0].ToString();
                break;
            case "coreFrequency[1]":
                inputField.text = InGameSetting.coreFrequency[1].ToString();
                break;
            default:
                Debug.LogError($"{transform.parent.name} is not set to a valid subject!");
                break;
        }
    }

    
    void OnValueChange(string value)
    {
        switch (subject)
        {
            case "masterVolume":
                int volume; 
                if (int.TryParse(value, out volume) && volume >= 0)
                {
                    InGameSetting.masterVolume = volume;
                    consoleText.color = Color.white;
                    consoleText.text = $"Master volume is now been set to {InGameSetting.masterVolume}.";
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
                    InGameSetting.timeLimitation = time;
                    consoleText.color = Color.white;
                    consoleText.text = $"Time limitation is now been set to {InGameSetting.timeLimitation}";
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
                    InGameSetting.coreFrequency[0] = freq;
                    if (InGameSetting.coreFrequency[0] < InGameSetting.coreFrequency[1]) InGameSetting.coreFrequency[1] = freq;
                    consoleText.color = Color.white;
                    consoleText.text = $"Core will now appear {InGameSetting.coreFrequency[1]} time(s) in {InGameSetting.coreFrequency[0]} block(s)";
                }
                else
                {
                    consoleText.color = Color.red;
                    consoleText.text = "Error! Invaild input!";
                }
                break;
            case "coreFrequency[1]":
                int times;
                if (int.TryParse(value, out times) && times >= 1)
                {
                    InGameSetting.coreFrequency[1] = times;
                    if (InGameSetting.coreFrequency[0] < InGameSetting.coreFrequency[1]) InGameSetting.coreFrequency[0] = times;
                    consoleText.color = Color.white;
                    consoleText.text = $"Core will now appear {InGameSetting.coreFrequency[1]} time(s) in {InGameSetting.coreFrequency[0]} block(s)";
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
    }
}
