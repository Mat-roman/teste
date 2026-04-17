using System;
using UnityEngine;

public enum QuestType
{
    Kill,
    Collect,
    Explore,
    Dialogue
}

public enum QuestState
{
    NotStarted,
    InProgress,
    Completed
}

[Serializable]
public class QuestObjective
{
    public string Description;
    public int TargetCount;
}

[CreateAssetMenu(fileName = "Quest", menuName = "RPG/Quest")]
public class Quest : ScriptableObject
{
    public string QuestId;
    public string QuestName;
    [TextArea] public string Description;
    public QuestType Type;
    public QuestObjective Objective;
    public int RewardXP;
    public int RewardGold;
    public Item RewardItem;
    public Quest NextQuest;
}
