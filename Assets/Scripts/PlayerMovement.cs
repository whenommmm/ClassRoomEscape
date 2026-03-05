using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;

    [Header("Seat Interaction")]
    public float seatRadius = 0.75f;   // how close player must be to sit

    // Public so other systems (e.g. detection) can read it
    public bool IsStanding { get; private set; } = true;

    private Rigidbody2D _rb;
    private Vector2      _input;
    private bool         _spaceWasPressed;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
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
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed)  x += 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)   x -= 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)     y += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed)   y -= 1f;

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
            return;
        }

        // Find nearest seat within seatRadius
        Seat[]   seats      = FindObjectsByType<Seat>(FindObjectsSortMode.None);
        Seat     nearest    = null;
        float    nearestDist = float.MaxValue;

        foreach (Seat seat in seats)
        {
            float dist = Vector2.Distance(transform.position, seat.transform.position);
            if (dist <= seatRadius && dist < nearestDist)
            {
                nearest     = seat;
                nearestDist = dist;
            }
        }

        if (nearest != null)
        {
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
