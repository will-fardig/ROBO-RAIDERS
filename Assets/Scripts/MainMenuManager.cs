using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Main Menu UI")]
    [SerializeField] private GameObject gameTitle;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject settingsButton;
    [SerializeField] private GameObject exitButton;

    [Header("Main Menu Characters and Effects")]
    [SerializeField] private GameObject spacemanMenu;
    [SerializeField] private GameObject runnerMenu;
    [SerializeField] private GameObject bulletMenu;
    [SerializeField] private GameObject muzzleFlash;

    private void Start()
    {
        // Show the main menu when the scene starts.
        ShowMainMenuUI(true);

        // Keep Settings hidden initially.
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // Show or hide the main menu interface.
    private void ShowMainMenuUI(bool show)
    {
        if (gameTitle != null)
            gameTitle.SetActive(show);

        if (startButton != null)
            startButton.SetActive(show);

        if (settingsButton != null)
            settingsButton.SetActive(show);

        if (exitButton != null)
            exitButton.SetActive(show);

        // Hide characters and combat effects in Settings.
        if (spacemanMenu != null)
            spacemanMenu.SetActive(show);

        if (runnerMenu != null)
            runnerMenu.SetActive(show);

        // Bullet and flash are controlled by the shooting script.
        // Only force them off when opening Settings.
        if (!show)
        {
            if (bulletMenu != null)
                bulletMenu.SetActive(false);

            if (muzzleFlash != null)
                muzzleFlash.SetActive(false);
        }
    }

    // Start the game.
    public void StartGame()
    {
        Debug.Log("Starting ROBO RAIDERS...");

        // Load the gameplay scene.
        SceneManager.LoadScene("SampleScene");
    }

    // Open Settings.
    public void OpenSettings()
    {
        Debug.Log("Opening Settings...");

        ShowMainMenuUI(false);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // Return to the main menu.
    public void CloseSettings()
    {
        Debug.Log("Returning to Main Menu...");

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        ShowMainMenuUI(true);
    }

    // Exit the game.
    public void ExitGame()
    {
        Debug.Log("Exiting ROBO RAIDERS...");

#if UNITY_EDITOR
        // Stop Play Mode when testing inside Unity.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Close the application in a standalone build.
        Application.Quit();
#endif
    }
}