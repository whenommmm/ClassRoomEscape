using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Place this script on a permanent manager object in your scene (or prefab)
/// and drag your custom Win and Loss UI Panels into the slots.
/// 
/// Note: Make sure your Continue/Retry buttons call OnContinueClicked() and OnRetryClicked()
/// </summary>
public class ResultScreenManager : MonoBehaviour
{
    public static ResultScreenManager Instance { get; private set; }

    [Header("Drag & Drop Panels Here")]
    public GameObject winPanel;
    public GameObject lossPanel;

    private int _nextSceneIndex = 0;

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
        _nextSceneIndex = nextSceneIndex;
        GameObject targetPanel = isWin ? winPanel : lossPanel;
        
        if (targetPanel != null)
        {
            StartCoroutine(FadeInPanel(targetPanel));
        }
    }

    private IEnumerator FadeInPanel(GameObject panel)
    {
        // Snap everything to a complete halt
        Time.timeScale = 0f;
        panel.SetActive(true);

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        float duration = 0.8f; // Fade in over 0.8 seconds
        float elapsed = 0f;
        
        // Use unscaledDeltaTime because timeScale is 0
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    // Link this to your "Continue" button OnClick() event in the Win Panel
    public void OnContinueClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_nextSceneIndex);
    }

    // Link this to your "Retry" button OnClick() event in the Loss Panel
    public void OnRetryClicked()
    {
        Time.timeScale = 1f;
        // Retry restarts the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
