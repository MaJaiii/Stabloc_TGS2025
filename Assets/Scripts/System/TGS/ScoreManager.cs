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
        for (int i = 1; i < lines.Length; i++) // �w�b�_�[�̓X�L�b�v
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

        // ���������Ń\�[�g
        scores.Sort((a, b) =>
        {
            int cmp = b.score.CompareTo(a.score); // �X�R�A������
            if (cmp == 0) cmp = b.height.CompareTo(a.height); // ���X�R�A�Ȃ獂����
            return cmp;
        });

        // ���10�������ێ�
        if (scores.Count > 10) scores = scores.GetRange(0, 10);

        // CSV�ɕۑ�
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
    public int score;     // �X�R�A
    public int height;    // ���x
    public string date;   // ���t

    public ScoreEntry(int score, int height)
    {
        this.score = score;
        this.height = height;
        this.date = System.DateTime.Now.ToString("MM/dd HH:mm");
    }
}

