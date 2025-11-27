



/// <summary>
/// Level types for categorization
/// </summary>
public enum LevelSceneType
{
    Normal,
    Story,
    Challenge,
    Bonus,
    Tutorial,
    Boss,
    Secret
}

/// <summary>
/// Serializable save data for a level
/// </summary>
[System.Serializable]
public class LevelSaveData
{
    public string levelName;
    public bool isUnlocked;
    public bool isCompleted;
    public int attemptsUsed;
    public float bestScore;
    public int timesCompleted;
}