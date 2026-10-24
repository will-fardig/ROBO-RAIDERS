using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Spaceman player;

    [Header("UI References")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TMP_Text healthValueText;

    private void Start()
    {
        UpdateHealthBar();
    }

    private void Update()
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (player == null)
        {
            return;
        }

        float currentHealth = player.Lives;
        float maxHealth = player.MaxLives;

        float healthPercentage = 0f;

        if (maxHealth > 0)
        {
            healthPercentage = Mathf.Clamp01(
                currentHealth / maxHealth
            );
        }

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercentage;
        }

        if (healthValueText != null)
        {
            healthValueText.text =
                $"{Mathf.Max(0, player.Lives)}/{player.MaxLives} HP";
        }
    }
}