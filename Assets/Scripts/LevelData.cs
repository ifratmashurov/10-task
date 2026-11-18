using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelNumber = 1;
    public string nextScene = "Level2";
    public float timeLimit = 90f;          // секунд на уровень
    public bool requireAllApples = false;  // нужно ли собрать все яблоки до выхода
}