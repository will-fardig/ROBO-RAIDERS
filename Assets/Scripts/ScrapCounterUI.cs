using UnityEngine;
using TMPro;

public class ScrapCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scrapValueText;

    private void Start()
    {
        UpdateScrap(0);
    }

    public void UpdateScrap(int totalScrap)
    {
        if (scrapValueText != null)
        {
            scrapValueText.text = totalScrap.ToString();
        }
    }
}