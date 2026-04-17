using UnityEngine;

public class NPCBase : MonoBehaviour
{
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private Quest questToGive;
    [SerializeField] private bool isShopkeeper;

    private int _relationship;

    public string NPCName => npcName;
    public int Relationship => _relationship;

    public string GetDialogueLine(int index)
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            return "...";
        }

        var safe = Mathf.Clamp(index, 0, dialogueLines.Length - 1);
        return dialogueLines[safe];
    }

    public void Interact()
    {
        _relationship++;
        if (questToGive != null && QuestManager.Instance != null && QuestManager.Instance.GetState(questToGive) == QuestState.NotStarted)
        {
            QuestManager.Instance.StartQuest(questToGive);
        }

        if (isShopkeeper)
        {
            var shop = FindObjectOfType<ShopUI>();
            if (shop != null)
            {
                shop.OpenShop(this);
            }
        }
    }
}
