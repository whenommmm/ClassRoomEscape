using UnityEngine;

/// <summary>
/// Attach to the detector object (VisionCone child for cameras, Teacher for teacher).
/// Instead of catching the player instantly, it now feeds into the SuspicionMeter.
/// For cameras: colors the SpriteRenderer on this same object.
/// For teacher: colors the VisionConeMesh child's MeshRenderer instead.
/// </summary>
public class CameraDetection : MonoBehaviour
{
    [Header("Visual Alert")]
    public Color alertColor = new Color(1f, 0f, 0f, 0.4f); // semi-transparent red

    private PlayerMovement _trackedPlayer;
    private bool _isWatching = false;   // true while we're feeding the SuspicionMeter

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
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_trackedPlayer == null) return;

        if (!_trackedPlayer.IsStanding)
        {
            // Player sat down — stop watching
            StopWatching();
            return;
        }

        // Player is standing inside our cone → alert color + feed meter
        Color alert = alertColor;
        alert.a = _defaultColor.a;   // keep the cone's original opacity
        SetConeColor(alert);

        if (!_isWatching)
        {
            SuspicionMeter.Instance?.RegisterWatcher();
            _isWatching = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null) return;
        _trackedPlayer = null;
        StopWatching();
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private void StopWatching()
    {
        if (_isWatching)
        {
            SuspicionMeter.Instance?.UnregisterWatcher();
            _isWatching = false;
        }
        SetConeColor(_defaultColor);
    }

    private void SetConeColor(Color c)
    {
        if (_mr != null) _mr.material.color = c;
        else if (_sr != null) _sr.color = c;
    }
}
