using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;

    [Header("Main Menu Elements")]
    public RectTransform titleText;
    public Button playButton;
    public Button quitButton;

    [Header("Level Select Elements")]
    public Button backButton;
    public Button[] levelButtons; // Array of 5 buttons for the levels

    // --- Neo-Brutalist Colors ---
    private static readonly Color ColPaperWhite  = new Color(0.96f, 0.94f, 0.90f, 1f); // Off-white cream
    private static readonly Color ColVibrantYell = new Color(1.00f, 0.85f, 0.20f, 1f); // Flashy yellow hover
    private static readonly Color ColPressedYell = new Color(0.90f, 0.70f, 0.10f, 1f); // Click state
    private static readonly Color ColDisabled    = new Color(0.60f, 0.58f, 0.55f, 1f); // Grayed out
    
    // Core structural color for heavy borders & shadows
    private static readonly Color ColHeavyInk    = new Color(0.12f, 0.12f, 0.16f, 1f); // Almost black navy ink

    private void Start()
    {
        // 0. Format Title (Heavy Outline and gentle tilt to make it pop)
        if (titleText != null)
        {
            titleText.anchorMin = new Vector2(0.5f, 0.5f);
            titleText.anchorMax = new Vector2(0.5f, 0.5f);
            titleText.pivot = new Vector2(0.5f, 0.5f);
            titleText.anchoredPosition = new Vector2(0, 180); // Brought up slightly from 140
            titleText.localRotation = Quaternion.Euler(0, 0, 8f); // 8 degrees is a much more natural sticker tilt
            titleText.localScale = new Vector3(1.6f, 1.6f, 1f); // Scale it up natively so it stands out against the giant buttons
            
            // For Text, Outline is infinitely more readable than Drop Shadow
            Shadow oldShadow = titleText.GetComponent<Shadow>();
            if (oldShadow != null) Destroy(oldShadow); // Strip the muddy shadow

            Outline titleOutline = titleText.GetComponent<Outline>();
            if (titleOutline == null) titleOutline = titleText.gameObject.AddComponent<Outline>();
            titleOutline.effectColor = ColHeavyInk;
            titleOutline.effectDistance = new Vector2(3, -3); // Sharp, readable border
        }

        // 1. Format and Position Main Menu Buttons (Chunkier, 240x70)
        FormatButton(playButton, new Vector2(240, 70), new Vector2(0, 20));
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);

        FormatButton(quitButton, new Vector2(240, 70), new Vector2(0, -70));
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

        // 2. Format and Position Level Select Buttons
        // Slightly tighter to look like a solid keypad block
        Vector2[] gridPositions =
        {
            new Vector2(-140, 90), new Vector2(0, 90), new Vector2(140, 90),  // Top row
            new Vector2(-70, -50), new Vector2(70, -50)                       // Bottom row
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
                    // Perfectly square, chunky tiles
                    FormatButton(btn, new Vector2(120, 120), gridPositions[i]);

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

        FormatButton(backButton, new Vector2(180, 50), new Vector2(0, -180));
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

        // 3. Set Initial State
        ShowMainMenu();
    }

    /// <summary>
    /// Applies a highly polished "Neo-Brutalist" style:
    /// - Strips the blurry default rounded Sprite to become a crisp, flat rectangle
    /// - Thick solid border (Outline)
    /// - Super thick solid block shadow (Shadow)
    /// - Physical "pushing the button down into the shadow" animation logic
    /// </summary>
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

        // 1. Remove the default rounded sprite to create perfectly sharp, flat brutalist corners
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = null; 
            img.color = Color.white;
        }

        // 2. Configure Unity's tint colors
        ColorBlock cb = btn.colors;
        cb.normalColor = ColPaperWhite;
        cb.highlightedColor = ColVibrantYell;
        cb.pressedColor = ColPressedYell;
        cb.disabledColor = ColDisabled;
        cb.colorMultiplier = 1f;
        btn.colors = cb;

        // 3. Thick solid dark border (Outline)
        Outline outline = btn.gameObject.GetComponent<Outline>();
        if (outline == null) outline = btn.gameObject.AddComponent<Outline>();
        outline.effectColor = ColHeavyInk;
        outline.effectDistance = new Vector2(4, -4); 

        // 4. Solid, very deep 3D block shadow (Shadow)
        Shadow shadow = btn.gameObject.GetComponents<Shadow>()[btn.gameObject.GetComponents<Shadow>().Length - 1]; 
        if (shadow == null || shadow is Outline) shadow = btn.gameObject.AddComponent<Shadow>();
        shadow.effectColor = ColHeavyInk;
        
        Vector2 normalShadowDist = new Vector2(8, -8);
        Vector2 pressedShadowDist = new Vector2(2, -2);
        shadow.effectDistance = normalShadowDist;

        // 5. Dynamic physical interaction physics (Squish into the board when clicked)
        EventTrigger trigger = btn.GetComponent<EventTrigger>();
        if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        // Hover scale
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((data) => { if(btn.interactable) rt.localScale = new Vector3(1.03f, 1.03f, 1f); });
        trigger.triggers.Add(enter);

        // Hover leave
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((data) => { rt.localScale = Vector3.one; rt.anchoredPosition = anchoredPos; shadow.effectDistance = normalShadowDist; });
        trigger.triggers.Add(exit);

        // Mouse Down (Physically press the button down and right, and shrink the shadow to simulate depth)
        EventTrigger.Entry down = new EventTrigger.Entry();
        down.eventID = EventTriggerType.PointerDown;
        down.callback.AddListener((data) => { 
            if(btn.interactable) {
                rt.localScale = Vector3.one; 
                rt.anchoredPosition = anchoredPos + new Vector2(6, -6); // shift button physically down into shadow gap
                shadow.effectDistance = pressedShadowDist;
            } 
        });
        trigger.triggers.Add(down);

        // Mouse Up (Spring back up)
        EventTrigger.Entry up = new EventTrigger.Entry();
        up.eventID = EventTriggerType.PointerUp;
        up.callback.AddListener((data) => { 
            if(btn.interactable) {
                rt.localScale = new Vector3(1.03f, 1.03f, 1f); 
                rt.anchoredPosition = anchoredPos; 
                shadow.effectDistance = normalShadowDist;
            } 
        });
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
        // Level 1 = build index 1, Level 2 = build index 2, etc.
        SceneManager.LoadScene(levelNum);
    }
}
