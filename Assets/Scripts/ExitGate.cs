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
            Debug.Log("[Exit] All levels completed! Returning to Main Menu.");
            SceneManager.LoadScene(0); // Main menu build index
        }
        else
        {
            Debug.Log($"[Exit] Player escaped! Loading Level {currentLevelNum + 1}");
            SceneManager.LoadScene(currentBuildIndex + 1);
        }
    }
}

