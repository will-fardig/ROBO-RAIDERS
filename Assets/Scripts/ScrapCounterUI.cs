using UnityEngine;
using TMPro;

public class ScrapCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scrapValueText;

    private void Start()
    {
        // Show the player's real starting scrap instead of 0
        Spaceman spaceman = FindFirstObjectByType<Spaceman>();
        UpdateScrap(spaceman != null ? spaceman.Scrap : 0);
    }

    public void UpdateScrap(int totalScrap)
    {
        if (scrapValueText != null)
        {
            scrapValueText.text = totalScrap.ToString();
        }
    }
}