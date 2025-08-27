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



    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
        inputField.text = InGameSetting.masterVolume.ToString();
    }

    void OnButtonClicked()
    {
        int masterVolume = 0;
        if (int.TryParse(inputField.text, out masterVolume))
        {
            if (masterVolume > 0)
            {
                consoleText.color = Color.white;
                consoleText.text = "Master Volume adjustment... Success! Master volume is now set to " + masterVolume;
                InGameSetting.masterVolume = masterVolume;
                audioSource.volume = 0.0005f * InGameSetting.masterVolume;
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
    }
}
