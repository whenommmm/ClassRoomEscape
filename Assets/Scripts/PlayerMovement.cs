using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    private float moveSpeed = 1.5f;

    [Header("Seat Interaction")]
    public float seatRadius = 0.20f;   // how close player must be to sit
    public float sitCooldown = 8f;     // Cannot sit back down immediately

    // Public so other systems (e.g. detection) can read it
    public bool IsStanding { get; private set; } = true;

    private Rigidbody2D _rb;
    private Vector2 _input;
    private bool _spaceWasPressed;
    private float _lastStandTime = -99f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        SnapToNearestSeat();
    }

    void SnapToNearestSeat()
    {
        Seat[] seats = FindObjectsByType<Seat>(FindObjectsSortMode.None);
        Seat nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Seat seat in seats)
        {
            float dist = Vector2.Distance(transform.position, seat.transform.position);
            if (dist < nearestDist)
            {
                nearest = seat;
                nearestDist = dist;
            }
        }

        if (nearest != null)
        {
            transform.position = nearest.transform.position;
            IsStanding = false;
        }
    }

    // Public so other systems (e.g. animation) can read movement direction
    public Vector2 InputDirection => _input;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // --- Sit / Stand toggle on Space (one press per tap) ---
        bool spacePressedNow = kb.spaceKey.isPressed;
        if (spacePressedNow && !_spaceWasPressed)
        {
            TryToggleSit();
        }
        _spaceWasPressed = spacePressedNow;

        // --- Movement (locked while sitting) ---
        if (!IsStanding)
        {
            _input = Vector2.zero;
            return;
        }

        float x = 0f, y = 0f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;

        _input = new Vector2(x, y);
    }


    void FixedUpdate()
    {
        _rb.linearVelocity = _input.normalized * moveSpeed;
    }

    void TryToggleSit()
    {
        if (!IsStanding)
        {
            // Always allow standing up
            SetStanding(true);
            _lastStandTime = Time.time;
            return;
        }

        // Check if player is actively under investigation so we bypass the cooldown
        bool isUnderAttack = false;
        TeacherInspection ti = FindFirstObjectByType<TeacherInspection>();
        if (ti != null && ti.IsInspecting) isUnderAttack = true;
        if (SuspicionMeter.Instance != null && SuspicionMeter.Instance.IsAlertActive) isUnderAttack = true;

        // Check 8-second cooldown ONLY if not under attack
        if (!isUnderAttack && Time.time - _lastStandTime < sitCooldown)
        {
            Debug.Log("[Player] Still on sitting cooldown.");
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowDialogue("You can't sit back down just yet!", "Narrator");
            }
            return;
        }

        // Find nearest seat within seatRadius
        Seat[] seats = FindObjectsByType<Seat>(FindObjectsSortMode.None);
        Seat nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Seat seat in seats)
        {
            float dist = Vector2.Distance(transform.position, seat.transform.position);
            Debug.Log($"[Player] Seat found at distance {dist}. Radius: {seatRadius}");
            
            // Check if player is within radius AND within acceptable Y range (player Y < seat Y with some tolerance)
            float yDifference = seat.transform.position.y - transform.position.y;
            if (dist <= seatRadius && dist < nearestDist && yDifference > -0.5f)
            {
                nearest = seat;
                nearestDist = dist;
            }
        }

        if (nearest != null)
        {
            Debug.Log($"[Player] Sitting! Distance was {nearestDist}, radius is {seatRadius}");
            transform.position = nearest.transform.position; // snap to seat
            SetStanding(false);
        }
        else
        {
            Debug.Log("[Player] No seat nearby to sit on.");
        }
    }

    void SetStanding(bool standing)
    {
        IsStanding = standing;
    }
}
