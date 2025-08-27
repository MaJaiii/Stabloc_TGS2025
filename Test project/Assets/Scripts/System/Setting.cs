//TEMP FILE!!

using UnityEngine;

public class Setting : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        if (InGameSetting.swayLimit == 0) InGameSetting.swayLimit = 10;
        if (InGameSetting.masterVolume <= 0 || InGameSetting.masterVolume > 30) InGameSetting.masterVolume = 10;
    }

}

[System.Serializable]
public static class InGameSetting
{
    public static float hiScore;
    public static float swayLimit;
    public static bool isCursorVisible;
    public static int masterVolume;
}
