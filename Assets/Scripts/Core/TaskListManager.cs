using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using RPGM.Core;
using RPGM.Gameplay;
using RPGM.UI;

public class TaskListManager : MonoBehaviour
{
    public static TaskListManager Instance { get; private set; }

    [Header("Task List Canvas Menu")]
    public GameObject taskListPanel;
    public TextMeshProUGUI taskListBodyText;

    [Header("Update Notification Popup")]
    public GameObject updatePopupPanel;
    public TextMeshProUGUI updatePopupText;
    public float popupDuration = 4f;

    [Header("On-Screen Task UI")]
    public GameObject objectiveContainer;
    public TextMeshProUGUI objectiveText;

    GameModel model;
    bool isMenuOpen;
    InputController.State previousInputState = InputController.State.CharacterControl;
    Coroutine popupCoroutine;

    void Awake()
    {
        Instance = this;
        if (taskListPanel != null) taskListPanel.SetActive(false);
        if (updatePopupPanel != null) updatePopupPanel.SetActive(false);
    }

    void Start()
    {
        model = Schedule.GetModel<GameModel>();
        RefreshTaskText();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleTaskMenu();
        }
    }

    // Called by GameTaskTracker when a task is started or completed
    public void SyncFromTracker(bool showPopup = false)
    {
        RefreshTaskText();

        if (showPopup)
            ShowUpdatePopup("Task list updated! Press [Esc] to check your tasks.");
    }

    public void ToggleTaskMenu()
    {
        if (isMenuOpen)
            CloseTaskMenu();
        else
            OpenTaskMenu();
    }

    public void SetObjectiveText(string message)
    {
        bool hasText = !string.IsNullOrEmpty(message);

        if (objectiveContainer != null)
            objectiveContainer.SetActive(hasText);

        if (objectiveText == null) return;

        objectiveText.gameObject.SetActive(hasText);
        objectiveText.text = message;

        if (objectiveContainer == null)
        {
            GameObject container = objectiveText.transform.parent != null && objectiveText.transform.parent.GetComponent<Canvas>() == null
                ? objectiveText.transform.parent.gameObject
                : objectiveText.gameObject;

            container.SetActive(hasText);
        }
    }

    public void OpenTaskMenu()
    {
        if (isMenuOpen || taskListPanel == null) return;

        isMenuOpen = true;
        RefreshTaskText();
        taskListPanel.SetActive(true);

        // Freeze physics, coroutines, animations, and NPC patrols
        Time.timeScale = 0f;

        // Lock player movement/dialog inputs while the menu is open
        if (model != null && model.input != null)
        {
            previousInputState = model.input.state;
            model.input.ChangeState(InputController.State.DialogControl);
        }
    }

    public void CloseTaskMenu()
    {
        if (!isMenuOpen || taskListPanel == null) return;

        isMenuOpen = false;
        taskListPanel.SetActive(false);

        // Unfreeze the game
        Time.timeScale = 1f;

        // Restore the player's previous input state
        if (model != null && model.input != null)
        {
            model.input.ChangeState(previousInputState);
        }
    }

    void ShowUpdatePopup(string message)
    {
        if (updatePopupPanel == null) return;

        if (updatePopupText != null)
            updatePopupText.text = message;

        updatePopupPanel.SetActive(true);

        RectTransform panelRect = updatePopupPanel.GetComponent<RectTransform>();
        if (panelRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        if (popupCoroutine != null)
            StopCoroutine(popupCoroutine);

        popupCoroutine = StartCoroutine(PopupTimer());
    }

    IEnumerator PopupTimer()
    {
        updatePopupPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(popupDuration);
        updatePopupPanel.SetActive(false);
    }

    public void RefreshTaskText()
    {
        if (taskListBodyText == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int activeCount = 0;

        foreach (var record in GameTaskTracker.GetAllTasks())
        {
            // Only display incomplete tasks; completed tasks are automatically omitted
            if (!record.isCompleted)
            {
                sb.AppendLine($"• {record.description}");
                activeCount++;
            }
        }

        taskListBodyText.text = activeCount > 0 ? sb.ToString() : "No active tasks.";
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}