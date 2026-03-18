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

    // --- Colors ---
    private static readonly Color ColDeskSurface = new Color(0.82f, 0.75f, 0.62f, 1f);
    private static readonly Color ColHighlight   = new Color(0.95f, 0.55f, 0.18f, 1f);
    private static readonly Color ColDisabled    = new Color(0.4f, 0.35f, 0.3f, 1f);

    private void Start()
    {
        // 1. Format and Position Main Menu Buttons
        FormatButton(playButton, new Vector2(400, 90), new Vector2(0, 0));
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);

        FormatButton(quitButton, new Vector2(400, 90), new Vector2(0, -140));
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

        // 2. Format and Position Level Select Buttons
        Vector2[] gridPositions =
        {
            new Vector2(-320, 80), new Vector2(0, 80), new Vector2(320, 80),
            new Vector2(-160, -180), new Vector2(160, -180)
        };

        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);

        for (int i = 0; i < levelButtons.Length && i < 5; i++)
        {
            int levelNum = i + 1;
            Button btn = levelButtons[i];

            if (btn != null)
            {
                // Format the size and position
                FormatButton(btn, new Vector2(220, 220), gridPositions[i]);

                // Lock or unlock
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

        FormatButton(backButton, new Vector2(300, 80), new Vector2(0, -420));
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

        // 3. Set Initial State
        ShowMainMenu();
    }

    private void FormatButton(Button btn, Vector2 size, Vector2 anchoredPos)
    {
        if (btn == null) return;
        
        RectTransform rt = btn.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
        }

        // Apply colors using Unity's built-in Button Color Tints
        ColorBlock cb = btn.colors;
        cb.normalColor = ColDeskSurface;
        cb.highlightedColor = ColHighlight;
        cb.pressedColor = new Color(0.7f, 0.4f, 0.1f, 1f);
        cb.disabledColor = ColDisabled;
        cb.colorMultiplier = 1f;
        btn.colors = cb;
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
        SceneManager.LoadScene(buildIndex);
    }
}
