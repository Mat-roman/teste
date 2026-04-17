using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private readonly Dictionary<string, QuestState> _states = new();
    private readonly Dictionary<string, int> _progress = new();
    private readonly List<Quest> _runtimeQuests = new();

    private PlayerStats _playerStats;

    public IReadOnlyList<Quest> RuntimeQuests => _runtimeQuests;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _playerStats = FindObjectOfType<PlayerStats>();
        BuildRuntimeQuestData();
    }

    public QuestState GetState(Quest quest)
    {
        if (quest == null)
        {
            return QuestState.NotStarted;
        }

        return _states.TryGetValue(quest.QuestId, out var state) ? state : QuestState.NotStarted;
    }

    public void StartQuest(Quest quest)
    {
        if (quest == null)
        {
            return;
        }

        _states[quest.QuestId] = QuestState.InProgress;
        _progress[quest.QuestId] = 0;
    }

    public void AddProgress(QuestType type, int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        foreach (var quest in _runtimeQuests)
        {
            if (quest == null || quest.Type != type || GetState(quest) != QuestState.InProgress)
            {
                continue;
            }

            _progress[quest.QuestId] += amount;
            if (_progress[quest.QuestId] >= quest.Objective.TargetCount)
            {
                CompleteQuest(quest);
            }

        }
    }

    private void CompleteQuest(Quest quest)
    {
        _states[quest.QuestId] = QuestState.Completed;
        if (_playerStats != null)
        {
            _playerStats.GainExperience(quest.RewardXP);
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.TryAutoSave();
        }

        if (quest.NextQuest != null)
        {
            StartQuest(quest.NextQuest);
        }
    }

    private void BuildRuntimeQuestData()
    {
        if (_runtimeQuests.Count > 0)
        {
            return;
        }

        AddQuest("goblin_1", "Investigate Attacks", QuestType.Dialogue, 1, 500, 0);
        AddQuest("goblin_2", "Defeat 10 Goblins", QuestType.Kill, 10, 1000, 0);
        AddQuest("goblin_3", "Recover Stolen Treasure", QuestType.Explore, 1, 2000, 1000);

        AddQuest("forest_1", "Speak with the Druid", QuestType.Dialogue, 1, 1000, 0);
        AddQuest("forest_2", "Collect 5 Blue Crystals", QuestType.Collect, 5, 1500, 0);
        AddQuest("forest_3", "Defeat the Orc Guardian", QuestType.Kill, 1, 3000, 0);
        AddQuest("forest_4", "Return Crystals to Druid", QuestType.Dialogue, 1, 2000, 0);

        AddQuest("dungeon_1", "Explore the Dungeon", QuestType.Explore, 1, 2000, 0);
        AddQuest("dungeon_2", "Defeat 5 Skeletons", QuestType.Kill, 5, 2500, 0);
        AddQuest("dungeon_3", "Find the Lost Artifact", QuestType.Explore, 1, 5000, 0);

        AddQuest("castle_1", "Gather Allies", QuestType.Dialogue, 3, 2000, 0);
        AddQuest("castle_2", "Prepare the Army", QuestType.Kill, 20, 3000, 0);
        AddQuest("castle_3", "Infiltrate the Castle", QuestType.Explore, 1, 2000, 0);
        AddQuest("castle_4", "Defeat the Senhor do Castelo", QuestType.Kill, 1, 10000, 0);
        AddQuest("castle_5", "Save the Queen", QuestType.Dialogue, 1, 20000, 0);

        if (_runtimeQuests.Count > 0)
        {
            StartQuest(_runtimeQuests[0]);
        }
    }

    private void AddQuest(string id, string name, QuestType type, int target, int rewardXp, int rewardGold)
    {
        var quest = ScriptableObject.CreateInstance<Quest>();
        quest.QuestId = id;
        quest.QuestName = name;
        quest.Type = type;
        quest.Objective = new QuestObjective { Description = name, TargetCount = target };
        quest.RewardXP = rewardXp;
        quest.RewardGold = rewardGold;
        _runtimeQuests.Add(quest);
    }
}
