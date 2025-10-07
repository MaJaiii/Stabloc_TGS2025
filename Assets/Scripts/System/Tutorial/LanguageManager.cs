using UnityEngine;
using TMPro;

[System.Serializable]
public class LanguageFont
{
    public Language language;
    public TMP_FontAsset font;
}

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }

    public Language CurrentLanguage = Language.English;

    [Header("Fonts per Language")]
    public LanguageFont[] fonts;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void SetLanguage(Language lang)
    {
        CurrentLanguage = lang;
        Debug.Log("Language changed to: " + lang);
    }

    public TMP_FontAsset GetFont(Language lang)
    {
        foreach (var lf in fonts)
        {
            if (lf.language == lang) return lf.font;
        }
        return null; // fallback
    }
}

public enum Language
{
    English,
    Japanese,
}

[System.Serializable]
public class LocalizedText
{
    public Language language;
    [TextArea(1, 4)] public string text;
}