using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;

    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button quitButton;

    [Header("Level Select Buttons")]
    public Button backButton;
    public Button[] levelButtons; // Array of 5 buttons for the levels

    private void Start()
    {
        // 1. Assign Main Menu Button Listeners
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

        // 2. Assign Level Select Button Listeners
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNum = i + 1;
            Button btn = levelButtons[i];

            if (btn != null)
            {
                // Lock or unlock the button based on progress
                if (levelNum <= highestUnlocked)
                {
                    btn.interactable = true;
                    btn.onClick.AddListener(() => OnLevelSelected(levelNum));
                }
                else
                {
                    btn.interactable = false; // Level is locked
                }
            }
        }

        // 3. Set Initial State
        ShowMainMenu();
    }

    // ── Interaction Logic ──

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }

    public void ShowLevelSelect()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
    }

    private void OnPlayClicked()
    {
        ShowLevelSelect();
    }

    private void OnQuitClicked()
    {
        Debug.Log("QUIT GAME");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnBackClicked()
    {
        ShowMainMenu();
    }

    private void OnLevelSelected(int levelNum)
    {
        // Level 1 = build index 2, Level 2 = build index 3, etc.
        int buildIndex = levelNum + 1;
        Debug.Log($"Loading Scene Build Index: {buildIndex} (Level {levelNum})");
        SceneManager.LoadScene(buildIndex);
    }
}