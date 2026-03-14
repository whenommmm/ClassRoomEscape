using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Attach to the Teacher prefab.
/// Every X seconds, randomly selects a row (First, Middle, or Last) and makes it dangerous for 5 seconds.
/// </summary>
public class TeacherInspection : MonoBehaviour
{
    [Header("Inspection Settings")]
    public float    warningDuration    = 1.5f; // Seconds the row glows orange before danger

    private RowZone[] _allRows;
    private PlayerMovement _player;
    private TeacherVisionCone _teacherVision;
    private bool _isInspecting = false;
    public bool IsInspecting => _isInspecting;

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerMovement>();
        _teacherVision = GetComponent<TeacherVisionCone>();
        // Find all row zones in the scene once at startup
        _allRows = FindObjectsByType<RowZone>(FindObjectsSortMode.None);
    }

    public void TriggerInspection()
    {
        if (_isInspecting) return;
        StartCoroutine(InspectionRoutine());
    }

    private IEnumerator InspectionRoutine()
    {
        if (_allRows == null || _allRows.Length == 0 || _player == null)
            yield break;
       

        _isInspecting = true;
        if (_teacherVision != null) _teacherVision.SetDistracted(true);

        // Find the closest row to the player
        RowZone targetRow = null;
        float closestDist = float.MaxValue;

        foreach (var row in _allRows)
        {
            // Measure distance to row center
            float dist = Vector2.Distance(_player.transform.position, row.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                targetRow   = row;
            }
        }

        if (targetRow == null)
        {
            _isInspecting = false;
            if (_teacherVision != null) _teacherVision.SetDistracted(false);
            yield break;
        }

        // Announce via Dialogue using the universal row name
        DialogueManager.Instance?.ShowDialogue("That row, sit properly and keep quiet!", "Teacher");

        // 5. Warning Phase (Orange Glow)
        targetRow.SetState(isWarning: true, isDangerous: false);
        yield return new WaitForSeconds(warningDuration);

        // 6. Danger Phase (Red Glow - penalizes sitting players)
        // This phase continues INDEFINITELY as long as the player is inside.
        // It only ends if the player steps out and stays out for 2 continuous seconds.
        targetRow.SetState(isWarning: false, isDangerous: true);

        float escapeTimer = 0f;

        while (true)
        {
            if (!targetRow.PlayerIsInside)
            {
                escapeTimer += Time.deltaTime;
                if (escapeTimer >= 2f)
                {
                    // Player successfully stayed out for 2 continuous seconds!
                    break;
                }
            }
            else
            {
                // Player stepped back in (or never left), reset their escape progress
                escapeTimer = 0f;
            }

            yield return null;
        }

        // 7. Cooldown / Reset
        targetRow.SetState(isWarning: false, isDangerous: false);

        if (_teacherVision != null) _teacherVision.SetDistracted(false);
        _isInspecting = false;
    }
}
