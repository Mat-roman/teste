using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [Serializable]
    private class SaveData
    {
        public int Level;
        public int Experience;
        public int Gold;
        public int HP;
        public int Mana;
        public int Stamina;
        public Vector3 Position;
        public int SceneIndex;
        public float TimePlayed;
        public List<string> CompletedQuests = new();
    }

    public static SaveManager Instance { get; private set; }

    private const int MaxSlots = 3;
    private float _timePlayed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        _timePlayed += Time.deltaTime;
    }

    public bool Save(int slot, PlayerStats stats)
    {
        if (slot < 1 || slot > MaxSlots || stats == null)
        {
            return false;
        }

        var data = new SaveData
        {
            Level = stats.Level,
            Experience = stats.Experience,
            Gold = stats.Gold,
            HP = stats.HP,
            Mana = stats.Mana,
            Stamina = stats.Stamina,
            Position = stats.transform.position,
            SceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex,
            TimePlayed = _timePlayed
        };

        PlayerPrefs.SetString(Key(slot), JsonUtility.ToJson(data));
        PlayerPrefs.Save();
        return true;
    }

    public bool Load(int slot, PlayerStats stats)
    {
        if (slot < 1 || slot > MaxSlots || stats == null || !PlayerPrefs.HasKey(Key(slot)))
        {
            return false;
        }

        var data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(Key(slot)));
        stats.SetStateForLoad(data.Level, data.Experience, data.HP, data.Mana, data.Stamina);
        stats.SetGold(data.Gold);
        stats.transform.position = data.Position;
        return true;
    }

    public bool Delete(int slot)
    {
        if (slot < 1 || slot > MaxSlots || !PlayerPrefs.HasKey(Key(slot)))
        {
            return false;
        }

        PlayerPrefs.DeleteKey(Key(slot));
        return true;
    }

    public void TryAutoSave()
    {
        var stats = FindObjectOfType<PlayerStats>();
        if (stats != null)
        {
            Save(1, stats);
        }
    }

    private static string Key(int slot)
    {
        return $"RPG_SAVE_{slot}";
    }
}
