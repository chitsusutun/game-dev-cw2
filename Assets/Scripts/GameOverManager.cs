using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using RPGM.Core;
using RPGM.Gameplay;
using RPGM.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI subtitleText;
    public Button homeButton;

    [Header("Scene Settings")]
    public string homeSceneName = "HomeScreen";

    GameModel model;
    bool isGameOver;

    void Awake()
    {
        Instance = this;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (homeButton != null)
            homeButton.onClick.AddListener(ReturnToHome);
    }

    void Start()
    {
        model = Schedule.GetModel<GameModel>();
    }

    /// Displays the Game Over panel with a specific reason subtitle and freezes gameplay.
    public void TriggerGameOver(string reasonSubtitle)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (subtitleText != null)
            subtitleText.text = reasonSubtitle;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Freeze all movement, timers, and animations
        Time.timeScale = 0f;

        // Lock player controls
        if (model == null) model = Schedule.GetModel<GameModel>();
        if (model != null && model.input != null)
            model.input.ChangeState(InputController.State.DialogControl);
    }

    public void ReturnToHome()
    {
        HomeScreenManager.ResetGameState();

        if (!string.IsNullOrEmpty(homeSceneName))
            SceneManager.LoadScene(homeSceneName);
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}