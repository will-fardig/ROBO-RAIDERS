using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    // Starts the game by loading the gameplay scene.
    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // Opens the settings menu.
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // Returns to the main menu.
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // Exits the game.
    public void ExitGame()
    {
        Debug.Log("Exiting ROBO RAIDERS...");
        Application.Quit();
    }
}