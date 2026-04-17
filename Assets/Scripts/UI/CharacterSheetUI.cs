using UnityEngine;

public class CharacterSheetUI : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;

    private void Awake()
    {
        if (stats == null)
        {
            stats = FindObjectOfType<PlayerStats>();
        }
    }

    public string BuildSummary()
    {
        if (stats == null)
        {
            return "No player";
        }

        return $"STR {stats.Strength} DEX {stats.Dexterity} INT {stats.Intelligence} CON {stats.Constitution} LUCK {stats.Luck}\n" +
               $"HP {stats.HP}/{stats.MaxHP} Mana {stats.Mana}/{stats.MaxMana} Stamina {stats.Stamina}/{stats.MaxStamina}\n" +
               $"Damage {stats.Damage} Armor {stats.Armor} Crit {stats.CritChance:P0}";
    }
}
