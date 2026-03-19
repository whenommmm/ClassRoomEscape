using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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

    // --- Modern Muted Colors ---
    private static readonly Color ColDeskSurface = new Color(0.85f, 0.78f, 0.65f, 1f); // Beige/Tan
    private static readonly Color ColHighlight   = new Color(0.98f, 0.60f, 0.25f, 1f); // Soft Orange
    private static readonly Color ColDisabled    = new Color(0.45f, 0.40f, 0.35f, 1f); // Muted Dark Brown
    private static readonly Color ColOutline     = new Color(0.20f, 0.16f, 0.12f, 1f); // Dark Wood Outline

    private void Start()
    {
        // 1. Format and Position Main Menu Buttons
        FormatButton(playButton, new Vector2(200, 50), new Vector2(0, 40));
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);

        FormatButton(quitButton, new Vector2(200, 50), new Vector2(0, -30));
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

        // 2. Format and Position Level Select Buttons
        // Refined Grid Layout: Tighter spacing, perfectly centered, smaller UI elements
        Vector2[] gridPositions =
        {
            new Vector2(-150, 100), new Vector2(0, 100), new Vector2(150, 100),  // Top row
            new Vector2(-75, -40), new Vector2(75, -40)                          // Bottom row
        };

        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);

        if (levelButtons != null)
        {
            for (int i = 0; i < levelButtons.Length && i < 5; i++)
            {
                int levelNum = i + 1;
                Button btn = levelButtons[i];

                if (btn != null)
                {
                    // Size is smaller (110x110) to prevent clipping and look minimal
                    FormatButton(btn, new Vector2(110, 110), gridPositions[i]);

                    // Add outline effect to tiles
                    Outline outline = btn.gameObject.GetComponent<Outline>();
                    if (outline == null) outline = btn.gameObject.AddComponent<Outline>();
                    outline.effectColor = ColOutline;
                    outline.effectDistance = new Vector2(2, -2);

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
        }

        FormatButton(backButton, new Vector2(180, 45), new Vector2(0, -150));
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
        cb.pressedColor = new Color(0.7f, 0.4f, 0.1f, 1f); // Darker orange when clicked
        cb.disabledColor = ColDisabled;
        cb.colorMultiplier = 1f;
        btn.colors = cb;

        // Apply Soft UI Drop Shadow programmatically
        Shadow shadow = btn.gameObject.GetComponent<Shadow>();
        if (shadow == null) shadow = btn.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.45f);
        shadow.effectDistance = new Vector2(4, -4);

        // Apply Hover / Click Animations (Scale effect)
        EventTrigger trigger = btn.GetComponent<EventTrigger>();
        if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        // Hover Up
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((data) => { if(btn.interactable) btn.transform.localScale = new Vector3(1.08f, 1.08f, 1f); });
        trigger.triggers.Add(enter);

        // Hover Leave
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((data) => { btn.transform.localScale = Vector3.one; });
        trigger.triggers.Add(exit);

        // Mouse Down (Click)
        EventTrigger.Entry down = new EventTrigger.Entry();
        down.eventID = EventTriggerType.PointerDown;
        down.callback.AddListener((data) => { if(btn.interactable) btn.transform.localScale = new Vector3(0.95f, 0.95f, 1f); });
        trigger.triggers.Add(down);

        // Mouse Up (Release Click)
        EventTrigger.Entry up = new EventTrigger.Entry();
        up.eventID = EventTriggerType.PointerUp;
        up.callback.AddListener((data) => { if(btn.interactable) btn.transform.localScale = new Vector3(1.08f, 1.08f, 1f); });
        trigger.triggers.Add(up);
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
