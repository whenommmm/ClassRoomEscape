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
    public float    dangerDuration     = 5f;   // Seconds the row is dangerous (red)

    private RowZone[] _allRows;
    private PlayerMovement _player;
    private bool _isInspecting = false;

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerMovement>();
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

            if (targetRow == null) continue;

            // Announce via Dialogue using the closest row's type
            string rowName = targetRow.rowType switch
            {
                RowType.First  => "Front row",
                RowType.Middle => "Middle row",
                RowType.Last   => "Back row",
                _              => "That row"
            };
            DialogueManager.Instance?.ShowDialogue($"{rowName}, sit properly and keep quiet!", "Teacher");

            // 5. Warning Phase (Orange Glow)
            targetRow.SetState(isWarning: true, isDangerous: false);
            yield return new WaitForSeconds(warningDuration);

            // 6. Danger Phase (Red Glow - penalizes sitting players)
            targetRow.SetState(isWarning: false, isDangerous: true);
            yield return new WaitForSeconds(dangerDuration);

            // 7. Cooldown / Reset
            targetRow.SetState(isWarning: false, isDangerous: false);
        }
        else
        {
            // Safety break if it somehow failed
            yield return null;
        }

        _isInspecting = false;
    }
}
