using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private string currentText;

    public void ShowDialogue(NPCBase npc, int line)
    {
        if (npc == null)
        {
            currentText = string.Empty;
            return;
        }

        currentText = npc.GetDialogueLine(line);
    }

    public string GetCurrentText()
    {
        return currentText;
    }
}
