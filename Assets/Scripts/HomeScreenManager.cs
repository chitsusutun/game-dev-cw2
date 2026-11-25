using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeScreenManager : MonoBehaviour
{
    public string startingSceneName = "Alley";

    void Start()
    {
        // Ensure time is never frozen on the home screen
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        ResetGameState();
        SceneManager.LoadScene(startingSceneName);
    }

    public static void ResetGameState()
    {
        // Restore normal game speed
        Time.timeScale = 1f;

        // Reset all static progress and cutscene flags
        PickpocketTarget.hasKeyCard = false;
        AlleyOpening.hasPlayed = false;
        KitchenArrival.hasPlayed = false;

        // Destroy the persistent task tracker so a fresh one initializes in the new game
        if (GameTaskTracker.Instance != null)
        {
            Destroy(GameTaskTracker.Instance.gameObject);
        }
    }
}