using UnityEngine;

public class InfiniteBackgroundScroll : MonoBehaviour
{
    [SerializeField] private RectTransform backgroundA;
    [SerializeField] private RectTransform backgroundB;

    [SerializeField] private float scrollSpeed = 30f;
    [SerializeField] private float overlap = 2f;

    private float backgroundWidth;
    private float scrollOffset;

    void Start()
    {
        backgroundWidth = backgroundA.rect.width;
        scrollOffset = 0f;
    }

    void Update()
    {
        float tileWidth = backgroundWidth - overlap;

        if (tileWidth <= 0f)
            return;

        scrollOffset = Mathf.Repeat(
            scrollOffset + scrollSpeed * Time.deltaTime,
            tileWidth * 2f
        );

        float positionA = -scrollOffset;

        if (positionA <= -tileWidth)
        {
            positionA += tileWidth * 2f;
        }

        float positionB = positionA + tileWidth;

        if (positionB <= -tileWidth)
        {
            positionB += tileWidth * 2f;
        }

        backgroundA.anchoredPosition =
            new Vector2(positionA, 0f);

        backgroundB.anchoredPosition =
            new Vector2(positionB, 0f);
    }
}