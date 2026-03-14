using UnityEngine;
using UnityEngine.SceneManagement;


public class ExitGate : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null) return;

        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        // As per the setup, Level 1 has build index 2.
        int currentLevelNum = currentBuildIndex - 1; 

        // Update highest level unlocked
        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);
        if (currentLevelNum + 1 > highestUnlocked)
        {
            PlayerPrefs.SetInt("HighestLevelUnlocked", currentLevelNum + 1);
            PlayerPrefs.Save();
        }

        if (currentLevelNum >= 5)
        {
            Debug.Log("[Exit] All levels completed! Returning to Main Menu.");
            // Assuming Main Menu is named "MainMenu"
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.Log($"[Exit] Player escaped! Loading Level {currentLevelNum + 1}");
            SceneManager.LoadScene(currentBuildIndex + 1);
        }
    }
}

