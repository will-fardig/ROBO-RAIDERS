using UnityEngine;

public class SpacemanIdle : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float floatAmount = 8f;

    private RectTransform rectTransform;
    private Vector2 startingPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startingPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * floatSpeed) * floatAmount;

        rectTransform.anchoredPosition =
            startingPosition + new Vector2(0f, offset);
    }
}