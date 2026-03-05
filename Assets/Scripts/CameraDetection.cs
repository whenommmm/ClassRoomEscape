using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to the detector object (VisionCone child for cameras, Teacher for teacher).
/// For cameras: colors the SpriteRenderer on this same object.
/// For teacher: colors the VisionConeMesh child's MeshRenderer instead.
/// </summary>
public class CameraDetection : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionTime = 1f;
    public Color alertColor    = new Color(1f, 0f, 0f, 0.4f); // semi-transparent red


    private float          _exposureTimer;
    private PlayerMovement _trackedPlayer;

    private SpriteRenderer _sr;
    private MeshRenderer   _mr;
    private Color          _defaultColor;

    private void Awake()
    {
        // Teacher: find MeshRenderer anywhere in children (works through ConePivot nesting)
        _mr = GetComponentInChildren<MeshRenderer>();
        if (_mr != null)
        {
            _defaultColor = _mr.material.color;
            return;
        }

        // Camera: color the SpriteRenderer on this same object
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _defaultColor = _sr.color;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;
        _trackedPlayer = player;
        _exposureTimer = 0f;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_trackedPlayer == null) return;

        if (!_trackedPlayer.IsStanding)
        {
            _exposureTimer = 0f;
            SetConeColor(_defaultColor);
            return;
        }

        Color alert = alertColor;
        alert.a = _defaultColor.a;   // keep same opacity as the cone's normal color
        SetConeColor(alert);
        _exposureTimer += Time.deltaTime;

        if (_exposureTimer >= detectionTime)
        {
            Debug.Log("[Camera] Player busted!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null) return;
        _exposureTimer = 0f;
        _trackedPlayer = null;
        SetConeColor(_defaultColor);
    }

    private void SetConeColor(Color c)
    {
        if (_mr != null) _mr.material.color = c;
        else if (_sr != null) _sr.color = c;
    }
}
