using UnityEngine;

[CreateAssetMenu(fileName = "Consumable", menuName = "RPG/Item/Consumable")]
public class Consumable : ScriptableObject
{
    public string ItemId;
    public string DisplayName;
    public int HealAmount;
    public int ManaAmount;
    public int StaminaAmount;

    public void Apply(PlayerStats stats)
    {
        if (stats == null)
        {
            return;
        }

        if (HealAmount > 0)
        {
            stats.Heal(HealAmount);
        }

        if (ManaAmount > 0)
        {
            stats.RestoreMana(ManaAmount);
        }

        if (StaminaAmount > 0)
        {
            stats.RestoreStamina(StaminaAmount);
        }
    }
}
