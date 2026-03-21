using UnityEngine;
using UnityEngine.SceneManagement;


public class ExitGate : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null) return;

        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        
        // Assuming Main Menu is build index 0, Level 1 is 1, Level 2 is 2, etc.
        int currentLevelNum = currentBuildIndex; 

        // Update highest level unlocked
        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);
        if (currentLevelNum + 1 > highestUnlocked)
        {
            PlayerPrefs.SetInt("HighestLevelUnlocked", currentLevelNum + 1);
            PlayerPrefs.Save();
        }

        if (currentLevelNum >= 5)
        {
            Debug.Log("[Exit] All levels completed! Resolving Game.");
        }
        else
        {
            Debug.Log($"[Exit] Player escaped! Unlocked Level {currentLevelNum + 1}");
        }

        int nextScene = (currentLevelNum >= 5) ? 0 : currentBuildIndex + 1;

        // Trigger generic 3-second result overlay for Win
        if (ResultScreenManager.Instance != null)
        {
            ResultScreenManager.Instance.ShowResult(true, nextScene);
        }
        else
        {
            // Fallback
            SceneManager.LoadScene(nextScene);
        }
    }
}

