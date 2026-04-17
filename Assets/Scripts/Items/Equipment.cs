using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Equipment", menuName = "RPG/Item/Equipment")]
public class Equipment : ScriptableObject
{
    public string ItemId;
    public string DisplayName;
    public EquipmentSlot Slot;
    public ItemRarity Rarity;
    public int Damage;
    public int Armor;
    public int BonusHP;
    public int BonusMana;
    public int BonusStrength;
    public int BonusDexterity;
    public int BonusIntelligence;
    public int BonusConstitution;
    public float BonusCritChance;
    public float BonusMoveSpeed;

    public static List<Equipment> BuildRuntimeCatalog()
    {
        var list = new List<Equipment>();
        AddWeapon(list, "iron_sword", "Iron Sword", ItemRarity.Common, 10);
        AddWeapon(list, "steel_sword", "Steel Sword", ItemRarity.Uncommon, 20);
        AddWeapon(list, "gold_sword", "Gold Sword", ItemRarity.Rare, 35);
        AddWeapon(list, "dragon_sword", "Dragon Sword", ItemRarity.Epic, 60);
        AddWeapon(list, "legendary_sword", "Legendary Sword", ItemRarity.Legendary, 100);

        AddArmorTier(list, EquipmentSlot.Helmet, "Helmet", new[] { 2, 4, 8, 15, 30 });
        AddArmorTier(list, EquipmentSlot.Chest, "Armor", new[] { 5, 10, 20, 40, 80 });
        AddArmorTier(list, EquipmentSlot.Legs, "Legs", new[] { 4, 8, 16, 32, 64 });
        AddArmorTier(list, EquipmentSlot.Boots, "Boots", new[] { 2, 3, 6, 10, 18 }, new[] { 0f, 0f, 0.1f, 0.2f, 0.3f });
        AddArmorTier(list, EquipmentSlot.Hands, "Gloves", new[] { 1, 2, 4, 8, 12 });

        list.Add(CreateAccessory("ring_strength", "Ring of Strength", ItemRarity.Rare, bonusStr: 5));
        list.Add(CreateAccessory("ring_agility", "Ring of Agility", ItemRarity.Rare, bonusDex: 5));
        list.Add(CreateAccessory("ring_intelligence", "Ring of Intelligence", ItemRarity.Rare, bonusInt: 5));
        list.Add(CreateAccessory("ring_constitution", "Ring of Constitution", ItemRarity.Epic, bonusCon: 5, bonusHP: 10));
        list.Add(CreateAccessory("ring_luck", "Ring of Luck", ItemRarity.Epic, bonusCrit: 0.10f));
        list.Add(CreateAccessory("amulet_protection", "Amulet of Protection", ItemRarity.Rare, armor: 10));
        list.Add(CreateAccessory("amulet_vitality", "Amulet of Vitality", ItemRarity.Epic, bonusHP: 50));
        list.Add(CreateAccessory("belt_strength", "Belt of Strength", ItemRarity.Epic, damage: 10));
        list.Add(CreateAccessory("cape_wisdom", "Cape of Wisdom", ItemRarity.Rare, bonusMana: 20));
        list.Add(CreateAccessory("boots_speed", "Boots of Speed", ItemRarity.Epic, bonusMoveSpeed: 0.5f));

        return list;
    }

    private static void AddWeapon(List<Equipment> list, string id, string name, ItemRarity rarity, int damage)
    {
        var equipment = CreateInstance<Equipment>();
        equipment.ItemId = id;
        equipment.DisplayName = name;
        equipment.Slot = EquipmentSlot.Weapon;
        equipment.Rarity = rarity;
        equipment.Damage = damage;
        list.Add(equipment);
    }

    private static void AddArmorTier(List<Equipment> list, EquipmentSlot slot, string nameSuffix, int[] armorValues, float[] speedValues = null)
    {
        var tiers = new[] { "Iron", "Steel", "Gold", "Dragon", "Legendary" };
        var rarities = new[] { ItemRarity.Common, ItemRarity.Uncommon, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary };
        for (var i = 0; i < tiers.Length; i++)
        {
            var equipment = CreateInstance<Equipment>();
            equipment.ItemId = $"{tiers[i].ToLower()}_{slot.ToString().ToLower()}";
            equipment.DisplayName = $"{tiers[i]} {nameSuffix}";
            equipment.Slot = slot;
            equipment.Rarity = rarities[i];
            equipment.Armor = armorValues[i];
            equipment.BonusMoveSpeed = speedValues == null ? 0f : speedValues[i];
            list.Add(equipment);
        }
    }

    private static Equipment CreateAccessory(
        string id,
        string name,
        ItemRarity rarity,
        int damage = 0,
        int armor = 0,
        int bonusHP = 0,
        int bonusMana = 0,
        int bonusStr = 0,
        int bonusDex = 0,
        int bonusInt = 0,
        int bonusCon = 0,
        float bonusCrit = 0f,
        float bonusMoveSpeed = 0f)
    {
        var equipment = CreateInstance<Equipment>();
        equipment.ItemId = id;
        equipment.DisplayName = name;
        equipment.Slot = EquipmentSlot.Accessory;
        equipment.Rarity = rarity;
        equipment.Damage = damage;
        equipment.Armor = armor;
        equipment.BonusHP = bonusHP;
        equipment.BonusMana = bonusMana;
        equipment.BonusStrength = bonusStr;
        equipment.BonusDexterity = bonusDex;
        equipment.BonusIntelligence = bonusInt;
        equipment.BonusConstitution = bonusCon;
        equipment.BonusCritChance = bonusCrit;
        equipment.BonusMoveSpeed = bonusMoveSpeed;
        return equipment;
    }
}
