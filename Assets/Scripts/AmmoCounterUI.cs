using UnityEngine;
using TMPro;

public class AmmoCounterUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Spaceman player;
    [SerializeField] private TMP_Text ammoValueText;

    private int previousMagazine = -1;
    private int previousReserve = -1;

    private void Start()
    {
        // Find the player if no reference was assigned.
        if (player == null)
        {
            player = FindAnyObjectByType<Spaceman>();
        }

        // Display the initial ammunition values.
        UpdateAmmoDisplay();
    }

    private void Update()
    {
        if (player == null || ammoValueText == null)
        {
            return;
        }

        // Update the HUD only when ammunition changes.
        if (player.prime_wpn_mag != previousMagazine ||
            player.prime_wpn_reserve != previousReserve)
        {
            UpdateAmmoDisplay();
        }
    }

    private void UpdateAmmoDisplay()
    {
        if (player == null || ammoValueText == null)
        {
            return;
        }

        int magazine = player.prime_wpn_mag;
        int reserve = player.prime_wpn_reserve;

        ammoValueText.text = $"{magazine:00}/{reserve:00}";

        previousMagazine = magazine;
        previousReserve = reserve;
    }
}