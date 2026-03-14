using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Programmatically creates the Main Menu and Level Selection UI for "Class Escape".
/// This ensures exact, minimal pixel-art aesthetic with muted classroom colors
/// without requiring manual editor setup for complex canvases.
/// Just attach to an empty GameObject in your Main Menu scene and hit Play!
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("Leave empty to use Unity's default font, or drag a pixel font here (like 8-bit limit).")]
    public Font customFont;
    public int totalLevels = 5;

    // --- Muted Classroom Stealth Palette ---
    private static readonly Color ColNavyBorder  = new Color(0.08f, 0.10f, 0.16f, 1f); // Dark navy
    private static readonly Color ColWoodFloor   = new Color(0.18f, 0.15f, 0.12f, 1f); // Dimmed wood
    private static readonly Color ColDeskSurface = new Color(0.72f, 0.65f, 0.53f, 1f); // Beige desks
    private static readonly Color ColDeskOutline = new Color(0.40f, 0.33f, 0.25f, 1f); // Desk border
    private static readonly Color ColHighlight   = new Color(0.85f, 0.45f, 0.15f, 1f); // Subtle orange
    private static readonly Color ColTextDark    = new Color(0.12f, 0.10f, 0.08f, 1f); // Ink
    private static readonly Color ColTextLight   = new Color(0.92f, 0.88f, 0.82f, 1f); // Chalk/Paper
    private static readonly Color ColShadow      = new Color(0.04f, 0.04f, 0.06f, 0.6f); // Soft drop shadow

    private GameObject _mainMenuPanel;
    private GameObject _levelSelectPanel;

    private void Awake()
    {
        if (customFont == null)
            customFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        BuildUI();
    }

    private void BuildUI()
    {
        // 1. Setup Master Canvas
        GameObject canvasGo = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // 2. Setup Event System (REQUIRED for buttons to work!)
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();
        }

        // 3. Background Layers (Simulating Classroom Floor)
        GameObject bgNavy = CreateRect("NavyBorder", canvasGo.transform, Vector2.zero, ColNavyBorder, stretch: true);
        // Dimmed inner floor with a 40px padding creating the navy border
        GameObject bgFloor = CreateRect("WoodFloor", bgNavy.transform, new Vector2(-80f, -80f), ColWoodFloor, stretch: true);

        // ============================================
        // 3. MAIN MENU PANEL
        // ============================================
        _mainMenuPanel = CreateRect("MainMenuPanel", bgFloor.transform, Vector2.zero, Color.clear, stretch: true);
        
        // Title text + Shadow
        GameObject ts = CreateText("TitleShadow", _mainMenuPanel.transform, "CLASS ESCAPE", 130, ColShadow, new Vector2(6f, 294f));
        GameObject title = CreateText("Title", _mainMenuPanel.transform, "CLASS ESCAPE", 130, ColTextLight, new Vector2(0f, 300f));
        
        // Buttons
        CreateButton("Btn_Play", _mainMenuPanel.transform, "PLAY", new Vector2(400, 90), new Vector2(0, 0), OnPlayClicked);
        CreateButton("Btn_Quit", _mainMenuPanel.transform, "QUIT", new Vector2(400, 90), new Vector2(0, -140), OnQuitClicked);


        // ============================================
        // 4. LEVEL SELECT PANEL
        // ============================================
        _levelSelectPanel = CreateRect("LevelSelectPanel", bgFloor.transform, Vector2.zero, Color.clear, stretch: true);
        
        CreateText("LSTitleShadow", _levelSelectPanel.transform, "SELECT LEVEL", 90, ColShadow, new Vector2(6f, 346f));
        CreateText("LSTitle", _levelSelectPanel.transform, "SELECT LEVEL", 90, ColTextLight, new Vector2(0, 350f));

        BuildLevelGrid(_levelSelectPanel.transform);

        CreateButton("Btn_Back", _levelSelectPanel.transform, "BACK", new Vector2(300, 80), new Vector2(0, -420), OnBackClicked);

        // Initialize Panel States
        _mainMenuPanel.SetActive(true);
        _levelSelectPanel.SetActive(false);
    }

    private void BuildLevelGrid(Transform parent)
    {
        // 3 levels on top row, 2 centered below
        Vector2[] positions = new Vector2[]
        {
            new Vector2(-320, 80), new Vector2(0, 80), new Vector2(320, 80), // Row 1
            new Vector2(-160, -180), new Vector2(160, -180)                  // Row 2
        };

        // Determine unlocked state based on standard PlayerPrefs
        // For testing, defaults to unlocking Level 1 only.
        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);
        int maxCompleted = highestUnlocked - 1; 

        for (int i = 0; i < totalLevels; i++)
        {
            int levelNum = i + 1;
            bool isUnlocked = levelNum <= highestUnlocked;
            bool isCompleted = levelNum <= maxCompleted;

            Vector2 pos = (i < positions.Length) ? positions[i] : new Vector2(0, 0);

            CreateLevelTile(parent, levelNum, pos, isUnlocked, isCompleted);
        }
    }

    private void CreateLevelTile(Transform parent, int levelNum, Vector2 pos, bool isUnlocked, bool isCompleted)
    {
        GameObject tile = new GameObject($"Tile_{levelNum}");
        tile.transform.SetParent(parent, false);
        RectTransform rt = tile.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220, 220);
        rt.anchoredPosition = pos;

        // Tile Drop Shadow
        GameObject shadow = CreateRect("Shadow", tile.transform, new Vector2(220, 220), ColShadow);
        shadow.GetComponent<RectTransform>().anchoredPosition = new Vector2(8f, -8f);

        // Base Button Visuals
        GameObject btnGo = CreateButtonVisuals($"Btn_{levelNum}", tile.transform, new Vector2(220, 220), Vector2.zero, isUnlocked);
        
        // Level Number Text
        Color textColor = isUnlocked ? ColTextDark : new Color(0.3f, 0.25f, 0.2f, 1f);
        CreateText("NumberText", btnGo.transform, levelNum.ToString(), 90, textColor, Vector2.zero);

        if (!isUnlocked)
        {
            // Locked UI (Dark overlay + Text placeholder for lock)
            CreateRect("LockOverlay", btnGo.transform, new Vector2(-12f, -12f), new Color(0.04f, 0.06f, 0.08f, 0.6f), stretch: true);
            CreateText("LockText", btnGo.transform, "[LOCKED]", 28, new Color(0.8f, 0.7f, 0.6f, 0.8f), new Vector2(0, -60f));
        }
        else if (isCompleted)
        {
            // Completed UI (Subtle highlight + Checkmark)
            CreateRect("HighlightOverlay", btnGo.transform, new Vector2(-12f, -12f), new Color(0.4f, 0.9f, 0.3f, 0.08f), stretch: true);
            CreateText("CheckMark", btnGo.transform, "v", 50, new Color(0.2f, 0.6f, 0.2f, 0.8f), new Vector2(65f, -65f));
        }

        if (isUnlocked)
        {
            Button btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(() => OnLevelSelected(levelNum));
            AddHoverEffect(btnGo, btnGo.GetComponent<Image>());
        }
    }

    // ── Interaction Logic ──

    private void OnPlayClicked()
    {
        _mainMenuPanel.SetActive(false);
        _levelSelectPanel.SetActive(true);
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
        _levelSelectPanel.SetActive(false);
        _mainMenuPanel.SetActive(true);
    }

    private void OnLevelSelected(int levelNum)
    {
        Debug.Log($"Loading Scene: Level0{levelNum}");
        // Format to Level01, Level02 etc based on the screenshot
        SceneManager.LoadScene($"Level{levelNum:D2}");
    }

    // ── UI Factory Helpers ──

    private GameObject CreateButton(string name, Transform parent, string text, Vector2 size, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        // 1. Drop shadow
        GameObject shadow = CreateRect(name + "_Shadow", parent, size, ColShadow);
        shadow.GetComponent<RectTransform>().anchoredPosition = pos + new Vector2(6f, -6f);

        // 2. Button Visuals (Border + Inner Beige)
        GameObject btnGo = CreateButtonVisuals(name, parent, size, pos, true);
        
        // 3. Text
        CreateText("Text", btnGo.transform, text, 48, ColTextDark, Vector2.zero);

        // 4. Logic & Hover Effects
        Button btn = btnGo.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
        AddHoverEffect(btnGo, btnGo.GetComponent<Image>());

        return btnGo;
    }

    private GameObject CreateButtonVisuals(string name, Transform parent, Vector2 size, Vector2 pos, bool isEnabled)
    {
        GameObject btnGo = new GameObject(name);
        btnGo.transform.SetParent(parent, false);
        RectTransform rt = btnGo.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        // Outer acts as the darker desk border/outline
        Image img = btnGo.AddComponent<Image>();
        img.color = ColDeskOutline;

        // Inner acts as the flat beige desk surface (padded inwards by 6px -> sizeDelta -12)
        Color surfaceColor = isEnabled ? ColDeskSurface : new Color(0.40f, 0.35f, 0.30f, 1f);
        CreateRect("InnerSurface", btnGo.transform, new Vector2(-12f, -12f), surfaceColor, stretch: true);

        return btnGo;
    }

    private void AddHoverEffect(GameObject target, Image baseImage)
    {
        EventTrigger trigger = target.AddComponent<EventTrigger>();
        Image innerSurface = target.transform.Find("InnerSurface").GetComponent<Image>();

        // ON HOVER (Glowing subtle orange outline & brighter warmer inner surface)
        EventTrigger.Entry entryHover = new EventTrigger.Entry();
        entryHover.eventID = EventTriggerType.PointerEnter;
        entryHover.callback.AddListener((data) => {
            innerSurface.color = Color.Lerp(ColDeskSurface, Color.white, 0.4f); // Pop brightness
            baseImage.color = ColHighlight; 
        });
        trigger.triggers.Add(entryHover);

        // ON EXIT (Revert to desk colors)
        EventTrigger.Entry exitHover = new EventTrigger.Entry();
        exitHover.eventID = EventTriggerType.PointerExit;
        exitHover.callback.AddListener((data) => {
            innerSurface.color = ColDeskSurface;
            baseImage.color = ColDeskOutline; 
        });
        trigger.triggers.Add(exitHover);
    }

    private GameObject CreateRect(string name, Transform parent, Vector2 sizeDelta, Color color, bool stretch = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();

        if (stretch)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            // Negative sizeDelta creates an inset (padding).
            rt.offsetMin = new Vector2(-sizeDelta.x / 2f, -sizeDelta.y / 2f); // bottom left padding
            rt.offsetMax = new Vector2( sizeDelta.x / 2f,  sizeDelta.y / 2f); // top right padding
        }
        else
        {
            rt.sizeDelta = sizeDelta;
        }

        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    private GameObject CreateText(string name, Transform parent, string text, int fontSize, Color color, Vector2 pos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1000, 300); // generic large wrapper
        rt.anchoredPosition = pos;

        Text t = go.AddComponent<Text>();
        t.text = text;
        t.font = customFont;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        
        // Prevent layout overlapping pixel issues
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        return go;
    }
}
