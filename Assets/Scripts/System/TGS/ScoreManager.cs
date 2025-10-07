using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ScoreManager
{

    private static string filePath = Path.Combine(Application.persistentDataPath, "scores.csv");

    public static List<ScoreEntry> LoadScores()
    {
        List<ScoreEntry> scores = new List<ScoreEntry>();
        if (!File.Exists(filePath)) return scores;

        string[] lines = File.ReadAllLines(filePath);
        for (int i = 1; i < lines.Length; i++) // ヘッダーはスキップ
        {
            string[] v = lines[i].Split(',');
            if (v.Length >= 3)
            {
                int.TryParse(v[0], out int score);
                int.TryParse(v[1], out int height);
                string date = v[2];
                scores.Add(new ScoreEntry(score, height) { date = date });
            }
        }
        return scores;
    }

    public static void AddScore(int score, int height)
    {
        List<ScoreEntry> scores = LoadScores();
        scores.Add(new ScoreEntry(score, height));

        // 複合条件でソート
        scores.Sort((a, b) =>
        {
            int cmp = b.score.CompareTo(a.score); // スコア高い順
            if (cmp == 0) cmp = b.height.CompareTo(a.height); // 同スコアなら高い順
            return cmp;
        });

        // 上位10件だけ保持
        if (scores.Count > 10) scores = scores.GetRange(0, 10);

        // CSVに保存
        List<string> lines = new List<string> { "Score,Height,Date" };
        foreach (var s in scores)
        {
            lines.Add($"{s.score},{s.height},{s.date}");
        }
        File.WriteAllLines(filePath, lines.ToArray());
    }
}

[System.Serializable]
public class ScoreEntry
{
    public int score;     // スコア
    public int height;    // 高度
    public string date;   // 日付

    public ScoreEntry(int score, int height)
    {
        this.score = score;
        this.height = height;
        this.date = System.DateTime.Now.ToString("MM/dd HH:mm");
    }
}

