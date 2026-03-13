using UnityEngine;
using System.Collections;

/// <summary>
/// Attach to the Teacher GameObject.
/// Normal mode: sweeping fan cone.
/// Alert mode (triggered externally at 50% suspicion): narrow rectangle
///   that locks onto and tracks the player until suspicion cools down.
/// </summary>
public class TeacherVisionCone : MonoBehaviour
{
    [Header("Cone Shape")]
    [Range(10f, 160f)]
    public float coneAngle = 110f;
    public float coneRange = 6f;
    public int   rayCount  = 30;

    [Header("Visual")]
    public Color coneColor  = new Color(1f, 0.6f, 0f, 0.35f);
    public Color alertColor = new Color(1f, 0.05f, 0.05f, 0.55f); // red in alert mode

    [Header("Rotation (normal sweep)")]
    public float sweepAngle    = 30f;
    public float sweepSpeed    = 20f;
    public float pauseDuration = 1f;

    [Header("Alert Rectangle")]
    public float alertWidth = 0.8f;  // half-width of the rectangle beam

    [Header("Classroom Bounds")]
    public Vector2 roomMin = new Vector2(-5.5f, -4.0f);
    public Vector2 roomMax = new Vector2( 6.0f,  4.5f);

    // ── internals ─────────────────────────────────────────────────────────────
    private Transform         _conePivot;
    private MeshFilter        _mf;
    private MeshRenderer      _mr;
    private PolygonCollider2D _col;
    private float             _startAngle;

    private bool      _alertMode = false;
    private bool      _distractedMode = false;
    private Transform _playerTransform;

    // ── lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        // ConePivot: collider + detection + visual all rotate together
        GameObject pivot = new GameObject("ConePivot");
        pivot.transform.SetParent(transform, false);
        _conePivot = pivot.transform;

        _col           = pivot.AddComponent<PolygonCollider2D>();
        _col.isTrigger = true;

        // Visual mesh (must exist before CameraDetection.Awake fires)
        GameObject coneVisual = new GameObject("VisionConeMesh");
        coneVisual.transform.SetParent(_conePivot, false);

        _mf = coneVisual.AddComponent<MeshFilter>();
        _mr = coneVisual.AddComponent<MeshRenderer>();

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = coneColor;
        _mr.material = mat;
        _mr.sortingOrder = 1;

        pivot.AddComponent<CameraDetection>();

        BuildCone();
    }

    void Start()
    {
        _startAngle = _conePivot.eulerAngles.z;
        StartCoroutine(SweepRoutine());
    }

    void Update()
    {
        if (_distractedMode) return;

        if (_alertMode && _playerTransform != null)
        {
            // Rotate pivot to face player
            Vector2 dir = (_playerTransform.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.x, -dir.y) * Mathf.Rad2Deg;
            _conePivot.rotation = Quaternion.Euler(0f, 0f, angle);

            // Stretch rectangle to exactly reach the player.
            // InverseTransformPoint converts to _conePivot local space, so scale on
            // the teacher or its parents is automatically accounted for.
            Vector3 localPlayer = _conePivot.InverseTransformPoint(_playerTransform.position);
            float dist = localPlayer.magnitude + 0.15f;  // tiny overshoot so tip covers player
            BuildRectangle(dist);
        }
    }

    // ── public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by SuspicionMeter when suspicion crosses 50%.
    /// alert=true  → stop sweep, switch to red tracking rectangle.
    /// alert=false → resume sweep, restore fan cone.
    /// </summary>
    public void SetAlertMode(bool alert, Transform player = null)
    {
        if (_alertMode == alert) return;
        _alertMode = alert;

        if (alert)
        {
            _playerTransform = player;
            StopAllCoroutines();
            float dist = player != null
                ? Vector2.Distance(transform.position, player.position)
                : coneRange;
            BuildRectangle(dist);
            _mr.material.color = alertColor;
        }
        else
        {
            _playerTransform = null;
            BuildRectangle(coneRange); // reset to default range before switching back
            BuildCone();
            _mr.material.color = coneColor;
            _startAngle = _conePivot.eulerAngles.z;
            StartCoroutine(SweepRoutine());
        }
    }

    /// <summary>
    /// Forces the teacher to look straight down and stop tracking/sweeping
    /// during a row inspection event.
    /// </summary>
    public void SetDistracted(bool distracted)
    {
        if (_distractedMode == distracted) return;
        _distractedMode = distracted;

        if (distracted)
        {
            // Look straight ahead
            _conePivot.rotation = Quaternion.Euler(0, 0, 0);
            
            // Turn off tracker and sweep coroutines
            StopAllCoroutines();
            
            // Revert to normal wide cone for the inspection
            BuildCone();
            _mr.material.color = coneColor;
        }
        else
        {
            // Restore whatever mode she was in previously
            if (_alertMode)
            {
                float dist = _playerTransform != null 
                    ? Vector2.Distance(transform.position, _playerTransform.position)
                    : coneRange;
                BuildRectangle(dist);
                _mr.material.color = alertColor;
            }
            else
            {
                BuildCone();
                _mr.material.color = coneColor;
                _startAngle = _conePivot.eulerAngles.z;
                StartCoroutine(SweepRoutine());
            }
        }
    }

    // ── sweep (normal mode) ───────────────────────────────────────────────────
    IEnumerator SweepRoutine()
    {
        while (true)
        {
            yield return SweepTo(_startAngle + sweepAngle);
            yield return new WaitForSeconds(pauseDuration);
            yield return SweepTo(_startAngle - sweepAngle);
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    IEnumerator SweepTo(float targetAngle)
    {
        float delta = Mathf.DeltaAngle(_conePivot.eulerAngles.z, targetAngle);
        while (Mathf.Abs(delta) > 0.1f)
        {
            float step = Mathf.Sign(delta) * sweepSpeed * Time.deltaTime;
            if (Mathf.Abs(step) > Mathf.Abs(delta)) step = delta;
            _conePivot.rotation = Quaternion.Euler(0f, 0f, _conePivot.eulerAngles.z + step);
            delta = Mathf.DeltaAngle(_conePivot.eulerAngles.z, targetAngle);
            yield return null;
        }
        _conePivot.rotation = Quaternion.Euler(0f, 0f, targetAngle);
    }

    // ── mesh / collider builders ──────────────────────────────────────────────
    void BuildCone()
    {
        Mesh mesh = new Mesh();
        mesh.name = "VisionCone";

        Vector3[] verts = new Vector3[rayCount + 2];
        int[]     tris  = new int[rayCount * 3];
        Vector2[] poly  = new Vector2[rayCount + 2];

        verts[0] = Vector3.zero;
        poly[0]  = Vector2.zero;

        float halfAngle = coneAngle * 0.5f;
        float step      = coneAngle / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            float   angle = -halfAngle + step * i;
            Vector3 dir   = Quaternion.Euler(0, 0, angle) * Vector3.down;
            Vector3 pt    = dir * coneRange;
            verts[i + 1] = pt;
            poly[i + 1]  = new Vector2(pt.x, pt.y);
        }

        for (int i = 0; i < rayCount; i++)
        {
            tris[i * 3]     = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i + 2;
        }

        mesh.vertices  = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        _mf.mesh = mesh;
        _col.SetPath(0, poly);
    }

    void BuildRectangle(float length)
    {
        float w = alertWidth;
        float h = length;

        Mesh mesh = new Mesh();
        mesh.name = "AlertRect";

        Vector3[] verts = new Vector3[]
        {
            new Vector3(-w,  0f, 0f),
            new Vector3( w,  0f, 0f),
            new Vector3( w, -h,  0f),
            new Vector3(-w, -h,  0f)
        };

        int[] tris = new int[] { 0, 1, 2, 0, 2, 3 };

        mesh.vertices  = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        _mf.mesh = mesh;

        _col.SetPath(0, new Vector2[]
        {
            new Vector2(-w,  0f),
            new Vector2( w,  0f),
            new Vector2( w, -h),
            new Vector2(-w, -h)
        });
    }
}
