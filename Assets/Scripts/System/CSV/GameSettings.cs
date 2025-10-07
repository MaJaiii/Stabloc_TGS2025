using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class GameSettings
{
    public bool isCursorHidden = true;
    public int masterVolume = 10;
    public bool isColorRandom = true;
    public int timeLimitation = 20;
    public int CoreFrom = 2;
    public int CoreGet = 1;
    public int effectVolume = 10;
}

public static class CsvSettingsSaver
{
    private static readonly string filePath = Path.Combine(Application.persistentDataPath, "settings.csv");

    public static void Save(GameSettings settings)
    {
        string[] lines =
        {
            "Cursor,Volume,Color,Time,CoreFrom,CoreGet,VolumeSE",
            $"{settings.isCursorHidden},{settings.masterVolume},{settings.isColorRandom},{settings.timeLimitation},{settings.CoreFrom},{settings.CoreGet},{settings.effectVolume}"
        };

        File.WriteAllLines(filePath, lines);
        Debug.Log("Settings saved to " + filePath);

#if UNITY_EDITOR
        EditorUtility.RevealInFinder(filePath); // エディタ上でフォルダを開く
#endif
    }
}

public static class CsvSettingsLoader
{
    private static readonly string filePath = Path.Combine(Application.persistentDataPath, "settings.csv");

    public static GameSettings Load()
    {
        GameSettings settings = new GameSettings(); // デフォルト値

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Settings file not found, creating default one.");
            CsvSettingsSaver.Save(settings);
            return settings;
        }

        string[] lines = File.ReadAllLines(filePath);

        if (lines.Length < 2) return settings;

        string[] values = lines[1].Split(',');

        if (values.Length >= 7)
        {
            bool.TryParse(values[0], out settings.isCursorHidden);
            int.TryParse(values[1], out settings.masterVolume);
            bool.TryParse(values[2], out settings.isColorRandom);
            int.TryParse(values[3], out settings.timeLimitation);
            int.TryParse(values[4], out settings.CoreFrom);
            int.TryParse(values[5], out settings.CoreGet);
            int.TryParse(values[6], out settings.effectVolume);
        }

        return settings;
    }
}
