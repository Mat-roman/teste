using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Serializable]
    public class InventorySlot
    {
        public Item Item;
        public int Amount;
    }

    [SerializeField] private int maxSlots = 20;
    [SerializeField] private float maxWeight = 120f;

    private readonly List<InventorySlot> _slots = new();
    private readonly Dictionary<EquipmentSlot, Equipment> _equipped = new();

    public IReadOnlyList<InventorySlot> Slots => _slots;
    public float CurrentWeight => _slots.Sum(s => s.Item == null ? 0f : s.Item.Weight * s.Amount);

    private void Awake()
    {
        foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
        {
            _equipped[slot] = null;
        }
    }

    public bool AddItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        var projectedWeight = CurrentWeight + item.Weight * amount;
        if (projectedWeight > maxWeight)
        {
            return false;
        }

        var existing = _slots.FirstOrDefault(s => s.Item != null && s.Item.Id == item.Id && s.Amount < item.MaxStack);
        if (existing != null)
        {
            existing.Amount += amount;
            return true;
        }

        if (_slots.Count >= maxSlots)
        {
            return false;
        }

        _slots.Add(new InventorySlot { Item = item, Amount = amount });
        return true;
    }

    public bool DropItem(int index)
    {
        if (index < 0 || index >= _slots.Count)
        {
            return false;
        }

        _slots.RemoveAt(index);
        return true;
    }

    public bool Equip(Equipment equipment)
    {
        if (equipment == null)
        {
            return false;
        }

        _equipped[equipment.Slot] = equipment;
        return true;
    }

    public bool Unequip(EquipmentSlot slot)
    {
        if (!_equipped.ContainsKey(slot))
        {
            return false;
        }

        _equipped[slot] = null;
        return true;
    }

    public Equipment GetEquipped(EquipmentSlot slot)
    {
        return _equipped.TryGetValue(slot, out var item) ? item : null;
    }

    public IEnumerable<Equipment> GetEquippedItems()
    {
        return _equipped.Values.Where(v => v != null);
    }

    public IEnumerable<InventorySlot> GetByType(ItemType type)
    {
        return _slots.Where(s => s.Item != null && s.Item.Type == type);
    }

    public void SortByRarityThenName()
    {
        _slots.Sort((a, b) =>
        {
            if (a.Item == null && b.Item == null) return 0;
            if (a.Item == null) return 1;
            if (b.Item == null) return -1;
            var rarityCompare = b.Item.Rarity.CompareTo(a.Item.Rarity);
            return rarityCompare != 0 ? rarityCompare : string.Compare(a.Item.Name, b.Item.Name, StringComparison.Ordinal);
        });
    }

    public bool UseConsumable(Consumable consumable, PlayerStats stats)
    {
        if (consumable == null || stats == null)
        {
            return false;
        }

        consumable.Apply(stats);
        return true;
    }
}
