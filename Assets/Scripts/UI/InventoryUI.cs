using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = FindObjectOfType<PlayerInventory>();
        }
    }

    public IReadOnlyList<PlayerInventory.InventorySlot> GetSlots()
    {
        return inventory == null ? new List<PlayerInventory.InventorySlot>() : inventory.Slots;
    }

    public void SortByRarity()
    {
        if (inventory != null)
        {
            inventory.SortByRarityThenName();
        }
    }
}
