using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider manaBar;
    [SerializeField] private Slider staminaBar;
    [SerializeField] private Slider xpBar;
    [SerializeField] private Text levelText;
    [SerializeField] private Text goldText;

    private PlayerStats _stats;

    private void Start()
    {
        _stats = FindObjectOfType<PlayerStats>();
    }

    private void Update()
    {
        if (_stats == null)
        {
            return;
        }

        SetSlider(hpBar, _stats.HP, _stats.MaxHP);
        SetSlider(manaBar, _stats.Mana, _stats.MaxMana);
        SetSlider(staminaBar, _stats.Stamina, _stats.MaxStamina);
        SetSlider(xpBar, _stats.Experience, _stats.ExperienceToNextLevel);

        if (levelText != null)
        {
            levelText.text = $"Lv {_stats.Level}";
        }

        if (goldText != null)
        {
            goldText.text = $"Gold: {_stats.Gold}";
        }
    }

    private static void SetSlider(Slider slider, float current, float max)
    {
        if (slider == null)
        {
            return;
        }

        slider.maxValue = Mathf.Max(1f, max);
        slider.value = Mathf.Clamp(current, 0f, slider.maxValue);
    }
}
