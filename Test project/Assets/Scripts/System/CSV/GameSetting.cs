using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
public class GameSettings
{
    public Dictionary<string, string> settings = new Dictionary<string, string>();

    public void Set(string key, string value) 
    { 
        settings[key] = value; 
    }

    public string Get(string key, string defauleValue = "") 
    { 
        return settings.ContainsKey(key) ? settings[key] : defauleValue;
    }

    public float GetFloat(string key, float defauleValue = 0.0f)
    {
        return float.TryParse(Get(key), out float value) ? value : defauleValue;
    }

    public bool GetBool(string key, bool defauleValue = false)
    {
        return bool.TryParse(Get(key), out bool value) ? value : defauleValue;
    }

    public int GetInt(string key,  int defauleValue = 0)
    {
        return int.TryParse(Get(key), out int value) ? value : defauleValue ;
    }
}

public class GameSettingSaver : MonoBehaviour
{
    string filePath;
    public GameSettings gameSettings = new GameSettings();

    private void Start()
    {
        gameSettings.Set("Sway Limit", "10");
        filePath = Path.Combine(Application.persistentDataPath, "settings.csv");
    }

    void SaveSettings()
    {

        if (!File.Exists(filePath))
        {
            Debug.Log("Setting data not found. Creating default settings file...");
            CreateDefaultSettingsFile();
        }
        else
        {

        }
    }

    void CreateDefaultSettingsFile()
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Setting,Value");
            writer.WriteLine("Sway Limit,10");
            writer.WriteLine("Cursor Visible,false");
            writer.WriteLine("Highscore,2.0");
        }

        Debug.Log("Default settings CSV created at: " + filePath);
    }

    void LoadSettings()
    {

    }
}
