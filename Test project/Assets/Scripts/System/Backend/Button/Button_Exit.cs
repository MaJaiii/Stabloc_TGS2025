using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Button_Exit : MonoBehaviour
{
    Button button;
    [SerializeField]
    TextMeshProUGUI consoleText;
    [SerializeField]
    string sceneName;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        PlayerPrefs.SetFloat("hiScore", InGameSetting.hiScore);
        PlayerPrefs.SetInt("masterVolume", InGameSetting.masterVolume);
        PlayerPrefs.SetInt("timeLimitation", InGameSetting.timeLimitation);
        PlayerPrefs.SetInt("isColorRandomize", InGameSetting.isColorRandomize ? 1 : 0);
        PlayerPrefs.SetInt("coreFrequency[0]", InGameSetting.coreFrequency[0]);
        PlayerPrefs.SetInt("coreFrequency[1]", InGameSetting.coreFrequency[1]);
        PlayerPrefs.SetInt("isCursorVisible", InGameSetting.isCursorVisible ? 1 : 0);
        PlayerPrefs.Save();

        SceneManager.LoadScene(sceneName);
    }
}
