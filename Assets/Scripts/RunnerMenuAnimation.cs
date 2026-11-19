using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RunnerMenuAnimation : MonoBehaviour
{
    [SerializeField] private Sprite[] animationFrames;
    [SerializeField] private float framesPerSecond = 12f;

    private Image runnerImage;
    private float timer;
    private int currentFrame;

    void Awake()
    {
        runnerImage = GetComponent<Image>();

        if (animationFrames == null || animationFrames.Length == 0)
        {
            Debug.LogWarning("RunnerMenu: No animation frames assigned.");
            return;
        }

        currentFrame = 0;
        timer = 0f;

        if (animationFrames[0] != null)
        {
            runnerImage.sprite = animationFrames[0];
        }
    }

    void Update()
    {
        if (runnerImage == null ||
            animationFrames == null ||
            animationFrames.Length == 0 ||
            framesPerSecond <= 0f)
        {
            return;
        }

        timer += Time.deltaTime;

        float frameDuration = 1f / framesPerSecond;

        if (timer >= frameDuration)
        {
            timer -= frameDuration;

            currentFrame =
                (currentFrame + 1) % animationFrames.Length;

            if (animationFrames[currentFrame] != null)
            {
                runnerImage.sprite = animationFrames[currentFrame];
            }
            else
            {
                Debug.LogWarning(
                    "RunnerMenu: Animation frame " +
                    currentFrame + " is missing!"
                );
            }
        }
    }
}