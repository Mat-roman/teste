using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private int level = 1;
    [SerializeField] private int maxLevel = 50;
    [SerializeField] private int experience;
    [SerializeField] private int experienceToNextLevel = 100;

    [Header("Primary Resources")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int hp = 100;
    [SerializeField] private int maxMana = 80;
    [SerializeField] private int mana = 80;
    [SerializeField] private int maxStamina = 100;
    [SerializeField] private int stamina = 100;

    [Header("Attributes")]
    [SerializeField] private int strength = 5;
    [SerializeField] private int dexterity = 5;
    [SerializeField] private int intelligence = 5;
    [SerializeField] private int constitution = 5;
    [SerializeField] private int luck = 5;

    private PlayerInventory _inventory;

    public event Action Changed;

    public int HP => hp;
    public int MaxHP => maxHP + GetEquipmentBonus(e => e.BonusHP);
    public int Mana => mana;
    public int MaxMana => maxMana + intelligence * 5 + GetEquipmentBonus(e => e.BonusMana);
    public int Stamina => stamina;
    public int MaxStamina => maxStamina;
    public int Experience => experience;
    public int ExperienceToNextLevel => experienceToNextLevel;
    public int Level => level;

    public int Strength => strength + GetEquipmentBonus(e => e.BonusStrength);
    public int Dexterity => dexterity + GetEquipmentBonus(e => e.BonusDexterity);
    public int Intelligence => intelligence + GetEquipmentBonus(e => e.BonusIntelligence);
    public int Constitution => constitution + GetEquipmentBonus(e => e.BonusConstitution);
    public int Luck => luck;

    public int Armor => Constitution + GetEquipmentBonus(e => e.Armor);
    public int Damage => Strength * 2 + GetEquipmentBonus(e => e.Damage);
    public float CritChance => Mathf.Clamp01(0.05f + Luck * 0.01f + GetEquipmentBonusFloat(e => e.BonusCritChance));

    private void Awake()
    {
        _inventory = GetComponent<PlayerInventory>();
    }

    public void GainExperience(int amount)
    {
        if (amount <= 0 || level >= maxLevel)
        {
            return;
        }

        experience += amount;
        while (level < maxLevel && experience >= experienceToNextLevel)
        {
            experience -= experienceToNextLevel;
            LevelUp();
        }

        Changed?.Invoke();
    }

    public void TakeDamage(int value)
    {
        var finalDamage = Mathf.Max(1, value - Armor / 3);
        hp = Mathf.Clamp(hp - finalDamage, 0, MaxHP);
        Changed?.Invoke();
    }

    public void Heal(int value)
    {
        hp = Mathf.Clamp(hp + Mathf.Max(0, value), 0, MaxHP);
        Changed?.Invoke();
    }

    public bool SpendMana(int value)
    {
        if (value < 0 || mana < value)
        {
            return false;
        }

        mana -= value;
        Changed?.Invoke();
        return true;
    }

    public void RestoreMana(int value)
    {
        mana = Mathf.Clamp(mana + Mathf.Max(0, value), 0, MaxMana);
        Changed?.Invoke();
    }

    public bool SpendStamina(int value)
    {
        if (value < 0 || stamina < value)
        {
            return false;
        }

        stamina -= value;
        Changed?.Invoke();
        return true;
    }

    public void RestoreStamina(int value)
    {
        stamina = Mathf.Clamp(stamina + Mathf.Max(0, value), 0, MaxStamina);
        Changed?.Invoke();
    }

    public int CalculateOutgoingDamage(int min, int max)
    {
        var baseValue = UnityEngine.Random.Range(min, max + 1) + Damage;
        var isCrit = UnityEngine.Random.value <= CritChance;
        return isCrit ? Mathf.RoundToInt(baseValue * 1.5f) : baseValue;
    }

    private void LevelUp()
    {
        level++;
        strength += 1;
        dexterity += 1;
        intelligence += 1;
        constitution += 1;
        luck += 1;
        maxHP += 10;
        maxMana += 5;
        maxStamina += 3;
        hp = MaxHP;
        mana = MaxMana;
        stamina = MaxStamina;
        experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.15f);
    }

    private int GetEquipmentBonus(Func<Equipment, int> selector)
    {
        if (_inventory == null)
        {
            return 0;
        }

        var total = 0;
        foreach (var item in _inventory.GetEquippedItems())
        {
            if (item != null)
            {
                total += selector(item);
            }
        }

        return total;
    }

    private float GetEquipmentBonusFloat(Func<Equipment, float> selector)
    {
        if (_inventory == null)
        {
            return 0f;
        }

        var total = 0f;
        foreach (var item in _inventory.GetEquippedItems())
        {
            if (item != null)
            {
                total += selector(item);
            }
        }

        return total;
    }
}
