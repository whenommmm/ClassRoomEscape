using UnityEngine;

public enum RowType
{
    First,
    Middle,
    Last
}

/// <summary>
/// Attach to a trigger BoxCollider2D covering a row of desks.
/// A child SpriteRenderer should be assigned to act as the red "glow" warning.
/// </summary>
public class RowZone : MonoBehaviour
{
    [Header("Row Configuration")]
    public RowType rowType;
    public SpriteRenderer glowSprite;
    public Color warningColor = new Color(1f, 0.4f, 0f, 0.5f); // Orange for incoming warning
    public Color dangerColor = new Color(1f, 0f, 0f, 0.5f);    // Red for active danger

    public bool IsDangerous { get; private set; }
    private bool _isWarning = false;

    private void Start()
    {
        if (glowSprite != null)
        {
            glowSprite.enabled = false;
        }
    }

    /// <summary>
    /// Turns on the visual glow.
    /// Warning = orange (safe to sit, but getting ready)
    /// Danger  = red (sitting here increases suspicion)
    /// False   = hidden
    /// </summary>
    public void SetState(bool isWarning, bool isDangerous)
    {
        _isWarning = isWarning;
        IsDangerous = isDangerous;

        if (glowSprite != null)
        {
            if (!isWarning && !isDangerous)
            {
                glowSprite.enabled = false;
            }
            else
            {
                glowSprite.enabled = true;
                glowSprite.color = isDangerous ? dangerColor : warningColor;
            }
        }
    }

    private bool _playerIsInside = false;

    private void Update()
    {
        // If the player is inside the row and it's dangerous, force suspicion up regardless of sitting
        // Calling this in Update guarantees the flag is set every frame, avoiding physics sleep issues
        if (IsDangerous && _playerIsInside)
        {
            SuspicionMeter.Instance?.AddRowPenalty();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>() != null)
        {
            _playerIsInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>() != null)
        {
            _playerIsInside = false;
        }
    }
}
