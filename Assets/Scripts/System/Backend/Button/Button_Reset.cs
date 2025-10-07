using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Button_Reset : MonoBehaviour
{
    Button button;
    [SerializeField]
    TextMeshProUGUI consoleText;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
        Cursor.visible = true;
    }

    void OnButtonClicked()
    {
        PlayerPrefs.SetFloat("highScore", 2);
        consoleText.color = Color.white;
        consoleText.text = "History highscore reset... Completed!";
    }
}
