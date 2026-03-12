using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;

    [Header("Seat Interaction")]
    public float seatRadius    = 0.20f;   // how close player must be to sit
    private float sitCooldown   = 5f;      // seconds before player can sit again after standing up

    [Header("Walk Bounds")]
    public Vector2 walkMin = new Vector2(-5.5f, -4.0f);
    private Vector2 walkMax = new Vector2( 6.0f,  4.5f);

    // Public so other systems (e.g. detection) can read it
    public bool IsStanding { get; private set; } = true;

    private Rigidbody2D _rb;
    private Vector2     _input;
    private bool        _spaceWasPressed;
    private float       _sitCooldownTimer = 0f;  // counts down after standing up

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void Start()
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
            Vector2 snapPos = nearest.transform.position;
            transform.position   = snapPos;
            _rb.position         = snapPos;   // sync rigidbody so physics doesn't drift
            _rb.linearVelocity   = Vector2.zero;
            SetStanding(false);
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

        // Tick down the sit cooldown
        if (_sitCooldownTimer > 0f)
            _sitCooldownTimer -= Time.deltaTime;

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

        // Clamp position and kill velocity on whichever axis hit the wall
        Vector2 pos = _rb.position;
        Vector2 vel = _rb.linearVelocity;

        if (pos.x < walkMin.x) { pos.x = walkMin.x; vel.x = Mathf.Max(0f, vel.x); }
        if (pos.x > walkMax.x) { pos.x = walkMax.x; vel.x = Mathf.Min(0f, vel.x); }
        if (pos.y < walkMin.y) { pos.y = walkMin.y; vel.y = Mathf.Max(0f, vel.y); }
        if (pos.y > walkMax.y) { pos.y = walkMax.y; vel.y = Mathf.Min(0f, vel.y); }

        _rb.position = pos;
        _rb.linearVelocity = vel;
    }

    void TryToggleSit()
    {
        if (!IsStanding)
        {
            // Always allow standing up — start the cooldown now
            _sitCooldownTimer = sitCooldown;
            SetStanding(true);
            return;
        }

        // Block sitting if cooldown is still active
        if (_sitCooldownTimer > 0f)
        {
            int secsLeft = Mathf.CeilToInt(_sitCooldownTimer);
            DialogueManager.Instance?.ShowDialogue(
                $"You have to wait {secsLeft} second{(secsLeft == 1 ? "" : "s")}\nbefore you can sit again.", "");
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

            // Check if player is within radius AND within acceptable Y range
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
            transform.position = nearest.transform.position;
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
