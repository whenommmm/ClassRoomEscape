using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Settings")]
    public Font customFont;
    public int totalLevels = 5;

    // --- Improved Classroom Palette ---
    private static readonly Color ColNavyBorder  = new Color(0.05f, 0.07f, 0.12f, 1f);
    private static readonly Color ColWoodFloor   = new Color(0.20f, 0.16f, 0.12f, 1f);

    private static readonly Color ColDeskSurface = new Color(0.82f, 0.75f, 0.62f, 1f);
    private static readonly Color ColDeskOutline = new Color(0.34f, 0.26f, 0.18f, 1f);

    private static readonly Color ColHighlight   = new Color(0.95f, 0.55f, 0.18f, 1f);

    private static readonly Color ColTextDark    = new Color(0.10f, 0.08f, 0.05f, 1f);
    private static readonly Color ColTextLight   = new Color(0.96f, 0.94f, 0.88f, 1f);

    private static readonly Color ColShadow      = new Color(0f, 0f, 0f, 0.55f);

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
        GameObject canvasGo = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGo.AddComponent<GraphicRaycaster>();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        GameObject bgNavy = CreateRect("Border", canvasGo.transform, Vector2.zero, ColNavyBorder, true);
        GameObject bgFloor = CreateRect("Floor", bgNavy.transform, new Vector2(-80, -80), ColWoodFloor, true);

        // MAIN MENU
        _mainMenuPanel = CreateRect("MainMenuPanel", bgFloor.transform, Vector2.zero, Color.clear, true);

        CreateText("TitleShadow", _mainMenuPanel.transform, "CLASS ESCAPE", 130, ColShadow, new Vector2(4, 296));
        CreateText("Title", _mainMenuPanel.transform, "CLASS ESCAPE", 130, ColTextLight, new Vector2(0, 300));

        CreateButton("Play", _mainMenuPanel.transform, "PLAY", new Vector2(400, 90), new Vector2(0, 0), OnPlayClicked);
        CreateButton("Quit", _mainMenuPanel.transform, "QUIT", new Vector2(400, 90), new Vector2(0, -140), OnQuitClicked);

        // LEVEL SELECT
        _levelSelectPanel = CreateRect("LevelSelectPanel", bgFloor.transform, Vector2.zero, Color.clear, true);

        CreateText("SelectShadow", _levelSelectPanel.transform, "SELECT LEVEL", 90, ColShadow, new Vector2(4, 346));
        CreateText("Select", _levelSelectPanel.transform, "SELECT LEVEL", 90, ColTextLight, new Vector2(0, 350));

        BuildLevelGrid(_levelSelectPanel.transform);

        CreateButton("Back", _levelSelectPanel.transform, "BACK", new Vector2(300, 80), new Vector2(0, -420), OnBackClicked);

        _mainMenuPanel.SetActive(true);
        _levelSelectPanel.SetActive(false);
    }

    private void BuildLevelGrid(Transform parent)
    {
        Vector2[] positions =
        {
            new Vector2(-320,80), new Vector2(0,80), new Vector2(320,80),
            new Vector2(-160,-180), new Vector2(160,-180)
        };

        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);
        int maxCompleted = highestUnlocked - 1;

        for (int i = 0; i < totalLevels; i++)
        {
            int levelNum = i + 1;
            bool unlocked = levelNum <= highestUnlocked;
            bool completed = levelNum <= maxCompleted;

            CreateLevelTile(parent, levelNum, positions[i], unlocked, completed);
        }
    }

    private void CreateLevelTile(Transform parent, int levelNum, Vector2 pos, bool unlocked, bool completed)
    {
        GameObject tile = new GameObject("Tile" + levelNum);
        tile.transform.SetParent(parent, false);

        RectTransform rt = tile.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(220,220);
        rt.anchoredPosition = pos;

        GameObject shadow = CreateRect("Shadow", tile.transform, new Vector2(220,220), ColShadow);
        shadow.GetComponent<RectTransform>().anchoredPosition = new Vector2(8,-8);

        GameObject btnGo = CreateButtonVisuals("Button", tile.transform, new Vector2(220,220), Vector2.zero, unlocked);

        Color textColor = unlocked ? ColTextDark : new Color(0.3f,0.25f,0.2f,1f);
        CreateText("Number", btnGo.transform, levelNum.ToString(), 90, textColor, Vector2.zero);

        if (!unlocked)
        {
            CreateRect("LockOverlay", btnGo.transform, new Vector2(-12,-12), new Color(0.02f,0.03f,0.05f,0.75f), true);
            CreateText("Locked", btnGo.transform, "[LOCKED]", 28, ColTextLight, new Vector2(0,-60));
        }
        else if (completed)
        {
            CreateRect("CompleteOverlay", btnGo.transform, new Vector2(-12,-12), new Color(0.35f,0.75f,0.35f,0.15f), true);
            CreateText("Check", btnGo.transform, "✓", 50, new Color(0.2f,0.6f,0.2f), new Vector2(65,-65));
        }

        if (unlocked)
        {
            Button btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(() => OnLevelSelected(levelNum));
            AddHoverEffect(btnGo, btnGo.GetComponent<Image>());
        }
    }

    private void OnPlayClicked()
    {
        _mainMenuPanel.SetActive(false);
        _levelSelectPanel.SetActive(true);
    }

    private void OnQuitClicked()
    {
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

    private void OnLevelSelected(int level)
    {
        // level 1 = build index 2, level 2 = build index 3, etc.
        SceneManager.LoadScene(level);
    }

    private GameObject CreateButton(string name, Transform parent, string text, Vector2 size, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject shadow = CreateRect(name+"Shadow", parent, size, ColShadow);
        shadow.GetComponent<RectTransform>().anchoredPosition = pos + new Vector2(6,-6);

        GameObject btnGo = CreateButtonVisuals(name, parent, size, pos, true);

        CreateText("Text", btnGo.transform, text, 48, ColTextDark, Vector2.zero);

        Button btn = btnGo.AddComponent<Button>();
        btn.onClick.AddListener(action);

        AddHoverEffect(btnGo, btnGo.GetComponent<Image>());

        return btnGo;
    }

    private GameObject CreateButtonVisuals(string name, Transform parent, Vector2 size, Vector2 pos, bool enabled)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent,false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        Image img = go.AddComponent<Image>();
        img.color = ColDeskOutline;

        Color innerColor = enabled ? ColDeskSurface : new Color(0.4f,0.35f,0.3f,1f);
        CreateRect("InnerSurface", go.transform, new Vector2(-12,-12), innerColor, true);

        return go;
    }

    private void AddHoverEffect(GameObject target, Image baseImage)
    {
        EventTrigger trigger = target.AddComponent<EventTrigger>();
        Image inner = target.transform.Find("InnerSurface").GetComponent<Image>();

        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((data) =>
        {
            inner.color = Color.Lerp(ColDeskSurface, ColHighlight, 0.25f);
            baseImage.color = ColHighlight;
        });
        trigger.triggers.Add(enter);

        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((data) =>
        {
            inner.color = ColDeskSurface;
            baseImage.color = ColDeskOutline;
        });
        trigger.triggers.Add(exit);
    }

    private GameObject CreateRect(string name, Transform parent, Vector2 sizeDelta, Color color, bool stretch=false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent,false);

        RectTransform rt = go.AddComponent<RectTransform>();

        if(stretch)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-sizeDelta.x/2,-sizeDelta.y/2);
            rt.offsetMax = new Vector2(sizeDelta.x/2,sizeDelta.y/2);
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
        go.transform.SetParent(parent,false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1000,300);
        rt.anchoredPosition = pos;

        Text t = go.AddComponent<Text>();
        t.text = text;
        t.font = customFont;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;

        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        return go;
    }
}