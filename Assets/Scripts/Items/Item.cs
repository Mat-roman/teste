using System;
using UnityEngine;

public enum ItemType
{
    Equipment,
    Consumable,
    Material,
    Quest
}

public enum EquipmentSlot
{
    Helmet,
    Chest,
    Legs,
    Boots,
    Hands,
    Weapon,
    Shield,
    Accessory
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[Serializable]
public class Item
{
    public string Id;
    public string Name;
    public ItemType Type;
    public ItemRarity Rarity;
    public float Weight;
    public int MaxStack = 1;
    public Sprite Icon;
}
