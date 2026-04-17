using System.Collections.Generic;
using UnityEngine;

public class QuestLogUI : MonoBehaviour
{
    public List<Quest> GetActiveQuests()
    {
        var result = new List<Quest>();
        if (QuestManager.Instance == null)
        {
            return result;
        }

        foreach (var quest in QuestManager.Instance.RuntimeQuests)
        {
            if (QuestManager.Instance.GetState(quest) == QuestState.InProgress)
            {
                result.Add(quest);
            }
        }

        return result;
    }
}
