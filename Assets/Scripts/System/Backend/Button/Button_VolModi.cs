using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Button_VolModi : MonoBehaviour
{
    Button button;
    AudioSource audioSource;
    [SerializeField]
    TextMeshProUGUI consoleText;
    [SerializeField]
    TMP_InputField inputField;

    GameSettings settings;



    private void Start()
    {
        settings = CsvSettingsLoader.Load();
        audioSource = GetComponent<AudioSource>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
        inputField.text = settings.masterVolume.ToString();
    }

    void OnButtonClicked()
    {
        int masterVolume = 0;
        if (int.TryParse(inputField.text, out masterVolume))
        {
            if (masterVolume > 0)
            {
                consoleText.color = Color.white;
                settings.masterVolume = masterVolume;
                consoleText.text = "Master Volume adjustment... Success! Master volume is now set to " + settings.masterVolume;

                audioSource.volume = 0.0005f * settings.masterVolume;
                audioSource.Play();
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
        CsvSettingsSaver.Save(settings);

    }
}
