using UnityEngine;
using System.Collections;

/// <summary>
/// Attach to the Teacher GameObject (which already has a SpriteRenderer).
/// The PolygonCollider2D and CameraDetection are placed on the ConePivot child
/// so they rotate with the visible cone — fixing detection mismatch.
/// </summary>
public class TeacherVisionCone : MonoBehaviour
{
    [Header("Cone Shape")]
    [Range(10f, 160f)]
    public float coneAngle = 110f;
    public float coneRange = 6f;
    public int   rayCount  = 30;

    [Header("Visual")]
    public Color coneColor = new Color(1f, 0.6f, 0f, 0.35f);

    [Header("Rotation")]
    public float sweepAngle    = 30f;
    public float sweepSpeed    = 20f;
    public float pauseDuration = 1f;

    private Transform         _conePivot;
    private MeshFilter        _mf;
    private MeshRenderer      _mr;
    private PolygonCollider2D _col;    // lives on ConePivot — rotates with cone
    private float             _startAngle;

    void Awake()
    {
        // ConePivot: collider + detection + visual all rotate together
        GameObject pivot = new GameObject("ConePivot");
        pivot.transform.SetParent(transform, false);
        _conePivot = pivot.transform;

        // PolygonCollider2D on ConePivot
        _col           = pivot.AddComponent<PolygonCollider2D>();
        _col.isTrigger = true;

        // VisionConeMesh FIRST — must exist before CameraDetection.Awake() fires
        GameObject coneVisual = new GameObject("VisionConeMesh");
        coneVisual.transform.SetParent(_conePivot, false);

        _mf = coneVisual.AddComponent<MeshFilter>();
        _mr = coneVisual.AddComponent<MeshRenderer>();

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = coneColor;
        _mr.material = mat;
        _mr.sortingOrder = 1;

        // CameraDetection AFTER MeshRenderer exists — Awake will find it correctly
        pivot.AddComponent<CameraDetection>();

        BuildCone();
    }

    void Start()
    {
        _startAngle = _conePivot.eulerAngles.z;
        StartCoroutine(SweepRoutine());
    }

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
}
