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
        SceneManager.LoadScene(sceneName);
    }
}
