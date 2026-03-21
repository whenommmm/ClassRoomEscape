using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Place this script on a permanent manager object in your scene (or prefab)
/// and drag your custom Win and Loss UI Panels into the slots.
/// </summary>
public class ResultScreenManager : MonoBehaviour
{
    public static ResultScreenManager Instance { get; private set; }

    [Header("Drag & Drop Panels Here")]
    public GameObject winPanel;
    public GameObject lossPanel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        
        // Ensure they start deactivated
        if (winPanel != null) winPanel.SetActive(false);
        if (lossPanel != null) lossPanel.SetActive(false);
    }

    public void ShowResult(bool isWin, int nextSceneIndex = 0)
    {
        StartCoroutine(ResultRoutine(isWin, nextSceneIndex));
    }

    private IEnumerator ResultRoutine(bool isWin, int nextSceneIndex)
    {
        // Snap everything to a complete halt
        Time.timeScale = 0f;
        
        // Show the respective panel you dragged in
        if (isWin)
        {
            if (winPanel != null) winPanel.SetActive(true);
        }
        else
        {
            if (lossPanel != null) lossPanel.SetActive(true);
        }

        // Wait absolutely exactly 3 real-world seconds
        yield return new WaitForSecondsRealtime(3f);

        // Restore reality
        Time.timeScale = 1f;

        // Route the player. If they lost, boot to Main Menu (0) unless specified otherwise.
        if (isWin)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(0); // Fail returns to Level Select
        }
    }
}
