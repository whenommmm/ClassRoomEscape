using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Pokémon-style typewriter dialogue box — redesigned with layered panels,
/// gold accent border, speaker badge, and a blinking ▼ advance indicator.
/// Call ShowDialogue("text") from anywhere.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public struct DialogueRequest
    {
        public string message;
        public string speaker;
    }

    [Header("Typewriter")]
    public float typeSpeed       = 0.035f;
    public float holdAfterFinish = 2.8f;

    // ── colour palette ────────────────────────────────────────────────────────
    static readonly Color ColBackground = new Color(0.06f, 0.06f, 0.14f, 0.97f);
    static readonly Color ColBorderOuter = new Color(0.85f, 0.70f, 0.20f, 1f);   // gold
    static readonly Color ColBorderInner = new Color(0.20f, 0.20f, 0.42f, 1f);   // deep violet
    static readonly Color ColNameBadge   = new Color(0.85f, 0.70f, 0.20f, 1f);   // gold
    static readonly Color ColNameText    = new Color(0.06f, 0.06f, 0.14f, 1f);   // dark
    static readonly Color ColBodyText    = new Color(0.95f, 0.95f, 1.00f, 1f);   // near-white

    // ── private refs ──────────────────────────────────────────────────────────
    private GameObject _root;
    private GameObject _badgeRoot;
    private Text       _badgeText;
    private Text       _bodyText;
    private Text       _arrow;
    private Coroutine  _routine;

    private Queue<DialogueRequest> _queue = new Queue<DialogueRequest>();

    // ── lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Build();
        _root.SetActive(false);
    }

    // ── public ────────────────────────────────────────────────────────────────
    public void ShowDialogue(string message, string speaker = "Teacher")
    {
        _queue.Enqueue(new DialogueRequest { message = message, speaker = speaker });

        // If not already running a dialogue, start the pump
        if (_routine == null)
        {
            _root.SetActive(true);
            _routine = StartCoroutine(PumpQueue());
        }
    }

    // ── coroutine ─────────────────────────────────────────────────────────────
    IEnumerator PumpQueue()
    {
        while (_queue.Count > 0)
        {
            DialogueRequest req = _queue.Dequeue();
            
            // Setup speaker badge
            bool hasSpeaker = !string.IsNullOrEmpty(req.speaker);
            _badgeRoot.SetActive(hasSpeaker);
            if (hasSpeaker) _badgeText.text = req.speaker;

            _bodyText.text = "";
            _arrow.enabled = false;

            // Typewriter effect
            foreach (char c in req.message)
            {
                _bodyText.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }

            // Blink the ▼ arrow while waiting for reading time
            _arrow.enabled = true;
            float timer = 0f;
            while (timer < holdAfterFinish)
            {
                timer += Time.deltaTime;
                _arrow.enabled = (Mathf.Sin(timer * 8f) > 0f);
                yield return null;
            }
        }

        // Queue empty, hide UI
        _root.SetActive(false);
        _routine = null;
    }

    // ── builder ───────────────────────────────────────────────────────────────
    void Build()
    {
        // Canvas
        GameObject cvGo = new GameObject("DialogueCanvas");
        cvGo.transform.SetParent(transform);
        Canvas cv = cvGo.AddComponent<Canvas>();
        cv.renderMode   = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 200;
        CanvasScaler cs = cvGo.AddComponent<CanvasScaler>();
        cs.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.matchWidthOrHeight  = 0.5f;
        cvGo.AddComponent<GraphicRaycaster>();

        // ── root (invisible, just holds children) ─────────────────────────────
        _root = new GameObject("DialogueRoot");
        _root.transform.SetParent(cvGo.transform, false);
        RectTransform rootRt = _root.AddComponent<RectTransform>();
        rootRt.anchorMin = new Vector2(0f, 0f);
        rootRt.anchorMax = new Vector2(1f, 0f);
        rootRt.pivot     = new Vector2(0.5f, 0f);
        rootRt.anchoredPosition = new Vector2(0f, 24f);
        rootRt.sizeDelta = new Vector2(-60f, 0f);
        // Make height auto-fit via layout
        _root.AddComponent<Image>().color = Color.clear; // needed for RectTransform to work as root

        // ── Layer 1 — gold outer border ───────────────────────────────────────
        GameObject outerBorder = Rect("OuterBorder", _root.transform,
            new Vector2(-4f, -4f), ColBorderOuter, height: 158f);

        // ── Layer 2 — violet inner border ─────────────────────────────────────
        GameObject innerBorder = Rect("InnerBorder", outerBorder.transform,
            new Vector2(-5f, -5f), ColBorderInner, height: 0f, stretch: true);

        // ── Layer 3 — dark background ─────────────────────────────────────────
        GameObject bg = Rect("Background", innerBorder.transform,
            new Vector2(-4f, -4f), ColBackground, height: 0f, stretch: true);

        // ── Speaker badge ─────────────────────────────────────────────────────
        _badgeRoot = new GameObject("SpeakerBadge");
        _badgeRoot.transform.SetParent(outerBorder.transform, false);
        RectTransform brt = _badgeRoot.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 1f);
        brt.anchorMax = new Vector2(0f, 1f);
        brt.pivot     = new Vector2(0f, 0f);
        brt.anchoredPosition = new Vector2(16f, 5f);
        brt.sizeDelta = new Vector2(130f, 34f);
        _badgeRoot.AddComponent<Image>().color = ColBorderOuter;

        // Badge text
        _badgeText = Child<Text>("BadgeText", _badgeRoot.transform);
        RectTransform bTxtRt = _badgeText.GetComponent<RectTransform>();
        bTxtRt.anchorMin = Vector2.zero;
        bTxtRt.anchorMax = Vector2.one;
        bTxtRt.offsetMin = new Vector2(10f, 4f);
        bTxtRt.offsetMax = new Vector2(-10f, -4f);
        _badgeText.text      = "Teacher";
        _badgeText.font      = Font();
        _badgeText.fontSize  = 20;
        _badgeText.fontStyle = FontStyle.Bold;
        _badgeText.color     = ColNameText;
        _badgeText.alignment = TextAnchor.MiddleLeft;

        // ── Body text ─────────────────────────────────────────────────────────
        Text body = Child<Text>("Body", bg.transform);
        RectTransform bodyRt = body.GetComponent<RectTransform>();
        bodyRt.anchorMin = Vector2.zero;
        bodyRt.anchorMax = Vector2.one;
        bodyRt.offsetMin = new Vector2(26f, 18f);
        bodyRt.offsetMax = new Vector2(-40f, -14f);
        body.font             = Font();
        body.fontSize         = 24;
        body.lineSpacing      = 1.3f;
        body.color            = ColBodyText;
        body.alignment        = TextAnchor.UpperLeft;
        body.supportRichText  = true;
        _bodyText = body;

        // ── Blinking ▼ arrow ──────────────────────────────────────────────────
        Text arrow = Child<Text>("Arrow", bg.transform);
        RectTransform arrowRt = arrow.GetComponent<RectTransform>();
        arrowRt.anchorMin = new Vector2(1f, 0f);
        arrowRt.anchorMax = new Vector2(1f, 0f);
        arrowRt.pivot     = new Vector2(1f, 0f);
        arrowRt.anchoredPosition = new Vector2(-16f, 14f);
        arrowRt.sizeDelta = new Vector2(30f, 30f);
        arrow.font      = Font();
        arrow.fontSize  = 22;
        arrow.text      = "▼";
        arrow.color     = ColBorderOuter;   // gold
        arrow.alignment = TextAnchor.MiddleCenter;
        arrow.enabled   = false;
        _arrow = arrow;
    }

    // ── helpers ───────────────────────────────────────────────────────────────
    /// Creates a full-width anchored rect with a fixed height (or stretches to parent).
    GameObject Rect(string name, Transform parent, Vector2 sizeDelta, Color color,
                    float height = 150f, bool stretch = false)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();

        if (stretch)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(-sizeDelta.x / 2f, -sizeDelta.y / 2f);
            rt.offsetMax = new Vector2( sizeDelta.x / 2f,  sizeDelta.y / 2f);
        }
        else
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot     = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(sizeDelta.x, height);
        }

        go.AddComponent<Image>().color = color;
        return go;
    }

    /// Creates a child GameObject with a component T and a RectTransform.
    T Child<T>(string name, Transform parent) where T : Component
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go.AddComponent<T>();
    }

    static UnityEngine.Font _font;
    static UnityEngine.Font Font()
    {
        if (_font == null)
            _font = Resources.GetBuiltinResource<UnityEngine.Font>("LegacyRuntime.ttf");
        return _font;
    }
}
