using UnityEngine;
using UnityEngine.SceneManagement;


public class ExitGate : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null) return;

        Debug.Log("[Exit] Player escaped! Restarting");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }
}

