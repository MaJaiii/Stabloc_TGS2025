//TEMP FILE!!

using UnityEngine;

public class Setting : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (InGameSetting.masterVolume <= 0) InGameSetting.masterVolume = 10;
        if (InGameSetting.coreFrequency[0] == 0) InGameSetting.coreFrequency[0] = 4;
        if (InGameSetting.coreFrequency[1] > InGameSetting.coreFrequency[0]) InGameSetting.coreFrequency[1] = 3;
    }

}

[System.Serializable]
public static class InGameSetting
{
    public static float hiScore;
    public static int masterVolume;
    public static int timeLimitation;
    public static bool isColorRandomize;
    public static int[] coreFrequency;      //[1] core(s) gained in [0] block(s)
    public static bool isCursorVisible;
}
