using System.IO;
using UnityEngine;

public static class SaveSystem
{
    const int TotalLevels = 3;

    static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");
    static string TmpPath  => Path.Combine(Application.persistentDataPath, "save.tmp");
    static SaveData _data;

    public static SaveData Data
    {
        get { if (_data == null) _data = Load(); return _data; }
    }

    public static void Save()
    {
        try
        {
            File.WriteAllText(TmpPath, JsonUtility.ToJson(Data, true));
            File.Copy(TmpPath, FilePath, true);
            File.Delete(TmpPath);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Save failed: " + e.Message);
        }
    }

    static SaveData Load()
    {
        if (!File.Exists(FilePath)) return new SaveData();
        try
        {
            var d = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
            if (d == null || d.unlockedLevels == null)
                return new SaveData();

            d.unlockedLevels.RemoveAll(l => l < 1 || l > TotalLevels);
            if (!d.unlockedLevels.Contains(1)) d.unlockedLevels.Add(1);
            d.musicVol = Mathf.Clamp01(d.musicVol);
            d.sfxVol = Mathf.Clamp01(d.sfxVol);
            return d;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Broken save: " + e.Message);
            return new SaveData();
        }
    }

    public static bool IsUnlocked(int levelNumber)
    {
        return Data.unlockedLevels.Contains(levelNumber);
    }

    public static void LevelCompleted(int levelNumber, int score)
    {
        Data.bestScore = Mathf.Max(Data.bestScore, score);
        int next = levelNumber + 1;
        if (next <= TotalLevels && !Data.unlockedLevels.Contains(next))
            Data.unlockedLevels.Add(next);
        Save();
    }

    public static void SetMusicVolume(float v)
    {
        Data.musicVol = Mathf.Clamp01(v);
        Save();
    }

    public static void SetSfxVolume(float v)
    {
        Data.sfxVol = Mathf.Clamp01(v);
        Save();
    }

    public static void ResetAll()
    {
        _data = new SaveData();
        Save();
    }
}