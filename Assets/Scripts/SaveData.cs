using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int version = 1;
    public int bestScore;
    public List<int> unlockedLevels = new List<int> { 1 };  // уровень 1 открыт сразу
    public float musicVol = 0.8f;
}