using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton suspicion meter.
/// Renders a world-space bar above the player's head.
/// CameraDetection calls RegisterWatcher/UnregisterWatcher each frame.
/// </summary>
public class SuspicionMeter : MonoBehaviour
{
    public static SuspicionMeter Instance { get; private set; }

    private float watcherBonus  = 0.15f;  
    private float fillRate      = 0.04f;  
    private float drainRate     = 0.13f;  
    private float catchThreshold = 1f; 
    

    private Vector2 barOffset   = new Vector2(0f, 0.8f);
    private float   barWidth    = 1.4f;
    private float   barHeight   = 0.29f;
    private Color   bgColor     = new Color(0.15f, 0.15f, 0.15f, 0.85f);
    private Color   fillColor   = new Color(1f, 0.35f, 0f, 1f);
    private Color   dangerColor = new Color(1f, 0.05f, 0.05f, 1f);
    // ── private ──────────────────────────────────────────────────────────────
    private float              _suspicion;
    private int                _watchers;
    private PlayerMovement     _player;
    private TeacherVisionCone  _teacher;
    private bool               _alertActive    = false;
    private bool               _dialoguePlayed = false;   // fires only once ever
    private bool               _hasRowPenalty  = false;   // set by RowZone each frame
    private bool               _inspectionTriggered = false; // ensures row inspection triggers once per 70% threshold crossing
    private const float        AlertThreshold  = 0.4f;

    public float CurrentSuspicion => _suspicion;

    private Transform      _barRoot;
    private SpriteRenderer _bgSr;
    private SpriteRenderer _fill;

    // ── lifecycle ────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _player  = FindFirstObjectByType<PlayerMovement>();
        _teacher = FindFirstObjectByType<TeacherVisionCone>();
        if (_player != null)
            BuildBar(_player.transform);

        // Opening teacher dialogue — fires once 1 second after the scene loads
        Invoke(nameof(ShowOpeningDialogue), 1f);
    }

    void ShowOpeningDialogue()
    {
        DialogueManager.Instance?.ShowDialogue(
            "Okay Class, the Attendance is done.\nLet's try and design a bird today.");
    }

    void Update()
    {
        if (_player == null) return;

        if (_player.IsStanding || _hasRowPenalty)
        {
            // Always fills while standing; watchers add bonus speed
            // If seated but in a dangerous row, fills at base speed
            float bonus = _player.IsStanding ? (watcherBonus * _watchers) : 0f;
            _suspicion += (fillRate + bonus) * Time.deltaTime;
        }
        else
        {
            // Drain while safely seated
            _suspicion -= drainRate * Time.deltaTime;
        }

        // Reset the flag for the next frame's physics step
        _hasRowPenalty = false;

        _suspicion = Mathf.Clamp01(_suspicion);
        

        // Trigger teacher alert at 50% suspicion
        bool shouldAlert = _suspicion >= AlertThreshold;
        if (shouldAlert != _alertActive)
        {
            _alertActive = shouldAlert;
            _teacher?.SetAlertMode(_alertActive, _player.transform);

            // Dialogue — only the very first time ever
            if (_alertActive && !_dialoguePlayed)
            {
                _dialoguePlayed = true;
                DialogueManager.Instance?.ShowDialogue("Sit the FUCK down!");
            }
        }

        // Trigger Row Inspection at 70% suspicion
        bool shouldInspect = _suspicion >= 0.7f;
        if (shouldInspect && !_inspectionTriggered)
        {
            _inspectionTriggered = true;
            FindFirstObjectByType<TeacherInspection>()?.TriggerInspection();
        }
        else if (!shouldInspect)
        {
            _inspectionTriggered = false;
        }

        UpdateBar();

        if (_suspicion >= catchThreshold)
            OnCaught();
    }

    // ── public API ───────────────────────────────────────────────────────────
    public void RegisterWatcher()   => _watchers++;
    public void UnregisterWatcher() => _watchers = Mathf.Max(0, _watchers - 1);
    public void AddRowPenalty()     => _hasRowPenalty = true;

    // ── bar building ─────────────────────────────────────────────────────────
    void BuildBar(Transform parent)
    {
        // Inherit the player's sorting layer so we always render above game sprites
        SpriteRenderer playerSr = parent.GetComponent<SpriteRenderer>();
        string sortLayer = playerSr != null ? playerSr.sortingLayerName : "Default";
        int    baseOrder = playerSr != null ? playerSr.sortingOrder + 1 : 10;

        // Root follows the player
        _barRoot = new GameObject("SuspicionBar").transform;
        _barRoot.SetParent(parent);
        _barRoot.localPosition = barOffset;
        _barRoot.localScale    = Vector3.one;

        // Background — always visible
        GameObject bg = CreateQuad("BG", bgColor, sortLayer, baseOrder);
        bg.transform.SetParent(_barRoot, false);
        bg.transform.localScale = new Vector3(barWidth, barHeight, 1f);
        _bgSr = bg.GetComponent<SpriteRenderer>();

        // Fill (anchored left; we scale its x each frame)
        GameObject fillGo = CreateQuad("Fill", fillColor, sortLayer, baseOrder + 1);
        fillGo.transform.SetParent(_barRoot, false);
        _fill = fillGo.GetComponent<SpriteRenderer>();
        fillGo.transform.localPosition = new Vector3(-barWidth * 0.5f, 0f, 0f);
        fillGo.transform.localScale    = new Vector3(0f, barHeight * 0.85f, 1f);
    }

    void UpdateBar()
    {
        if (_fill == null) return;

        // Scale fill from left: width goes 0 → barWidth
        float w = barWidth * _suspicion;
        _fill.transform.localPosition = new Vector3(-barWidth * 0.5f + w * 0.5f, 0f, 0f);
        _fill.transform.localScale    = new Vector3(Mathf.Max(w, 0f), barHeight * 0.85f, 1f);

        // Lerp color from orange to danger-red as meter fills
        _fill.color = Color.Lerp(fillColor, dangerColor, _suspicion);

        // Background always on; hide fill when empty
        _fill.enabled = _suspicion > 0.001f;
    }

    // ── helpers ──────────────────────────────────────────────────────────────
    GameObject CreateQuad(string goName, Color color, string sortLayer, int sortOrder)
    {
        var go = new GameObject(goName);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite           = GetWhiteSprite();
        sr.color            = color;
        sr.sortingLayerName = sortLayer;
        sr.sortingOrder     = sortOrder;
        return go;
    }

    static Sprite _whiteSprite;
    static Sprite GetWhiteSprite()
    {
        if (_whiteSprite != null) return _whiteSprite;
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _whiteSprite;
    }

    void OnCaught()
    {
        Debug.Log("[SuspicionMeter] Player caught!");
        _suspicion = 0f;
        _watchers  = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
