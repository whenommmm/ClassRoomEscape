using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Automatically generated singleton that builds a 3-second fullscreen UI overlay 
/// when the player wins or loses, then bounces them back to the Main Menu.
/// </summary>
public class ResultScreenManager : MonoBehaviour
{
    public static ResultScreenManager Instance { get; private set; }

    private GameObject _panelRoot;
    private Image      _bgImage;
    private Text       _msgText;
    private Outline    _textOutline;

    // Neo-Brutalist Colors
    private static readonly Color ColWinBg  = new Color(0.96f, 0.94f, 0.90f, 0.98f); // Paper white
    private static readonly Color ColLossBg = new Color(0.12f, 0.12f, 0.16f, 0.98f); // Dark Ink
    private static readonly Color ColWinText = new Color(0.12f, 0.12f, 0.16f, 1f);   // Dark Ink
    private static readonly Color ColLossText = new Color(0.85f, 0.20f, 0.20f, 1f);  // Danger Red

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        _panelRoot.SetActive(false);
    }

    private void BuildUI()
    {
        // 1. Create a top-layer Canvas
        GameObject canvasGo = new GameObject("ResultCanvas");
        canvasGo.transform.SetParent(transform);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; 
        
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 2. Fullscreen Panel Background
        _panelRoot = new GameObject("ResultPanel");
        _panelRoot.transform.SetParent(canvasGo.transform, false);
        RectTransform prt = _panelRoot.AddComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;
        
        _bgImage = _panelRoot.AddComponent<Image>();
        _bgImage.sprite = null;

        // 3. Brutalist Message Text
        GameObject textGo = new GameObject("ResultMessage");
        textGo.transform.SetParent(_panelRoot.transform, false);
        RectTransform trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 0.5f);
        trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.pivot = new Vector2(0.5f, 0.5f);
        trt.sizeDelta = new Vector2(1600, 400);
        
        _msgText = textGo.AddComponent<Text>();
        _msgText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _msgText.fontSize = 80;
        _msgText.alignment = TextAnchor.MiddleCenter;
        _msgText.horizontalOverflow = HorizontalWrapMode.Wrap;

        _textOutline = textGo.AddComponent<Outline>();
        _textOutline.effectDistance = new Vector2(4, -4);
    }

    public void ShowResult(bool isWin)
    {
        StartCoroutine(ResultRoutine(isWin));
    }

    private IEnumerator ResultRoutine(bool isWin)
    {
        // Snap everything to a complete halt
        Time.timeScale = 0f;
        
        _panelRoot.SetActive(true);
        
        if (isWin)
        {
            _bgImage.color = ColWinBg;
            _msgText.color = ColWinText;
            _msgText.text = "Successfully escaped the class";
            _textOutline.effectColor = Color.clear; // Dark text doesn't need an outline on white bg
        }
        else
        {
            _bgImage.color = ColLossBg;
            _msgText.color = ColLossText;
            _msgText.text = "You got caught, Pink Slip on the way!";
            _textOutline.effectColor = ColWinText; // Dark ink outline on red text
        }

        // Wait absolutely exactly 3 real-world seconds
        yield return new WaitForSecondsRealtime(3f);

        // Restore reality and boot to Main Menu (Scene 0)
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
