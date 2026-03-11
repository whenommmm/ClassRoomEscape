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
    public float    inspectionInterval = 10f;  // Seconds between row picks
    public float    warningDuration    = 1.5f; // Seconds the row glows orange before danger
    public float    dangerDuration     = 5f;   // Seconds the row is dangerous (red)

    private RowZone[] _allRows;

    private void Start()
    {
        // Find all row zones in the scene once at startup
        _allRows = FindObjectsByType<RowZone>(FindObjectsSortMode.None);
        StartCoroutine(InspectionRoutine());
    }

    private IEnumerator InspectionRoutine()
    {
        if (_allRows == null || _allRows.Length == 0)
            yield break;

        while (true)
        {
            // Wait 10 seconds before the next inspection starts
            yield return new WaitForSeconds(inspectionInterval);

            // 1. Pick a RowType (0=First, 1=Middle, 2=Last)
            RowType pickedType = (RowType)Random.Range(0, 3);
            
            // 2. Filter all rows to just those matching the picked type
            List<RowZone> matchingRows = new List<RowZone>();
            foreach (var row in _allRows)
            {
                if (row.rowType == pickedType) matchingRows.Add(row);
            }

            RowZone targetRow;

            // 3. Pick one specific row from the matching group
            // Fallback: If the user didn't setup all 3 enums in the scene, just pick any random row so we don't skip entirely
            if (matchingRows.Count > 0)
            {
                targetRow = matchingRows[Random.Range(0, matchingRows.Count)];
            }
            else
            {
                targetRow = _allRows[Random.Range(0, _allRows.Length)];
                pickedType = targetRow.rowType; // Update the type so the dialogue matches
            }

            // 4. Announce via Dialogue
            string rowName = pickedType switch
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
    }
}
