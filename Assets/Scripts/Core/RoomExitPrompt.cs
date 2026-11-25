using UnityEngine;
using UnityEngine.SceneManagement;
using RPGM.Core;
using RPGM.Gameplay;

[RequireComponent(typeof(BoxCollider2D))]
public class RoomExitPrompt : MonoBehaviour
{
    [Header("Regular Door Settings")]
    public string doorSpeakerName = "Door";
    public string nextSceneName = "Kitchen";
    public Transform sameSceneDestination;
    [TextArea(2, 3)]
    public string regularPromptMessage = "Do you want to go through this door?";
    public string yesButtonText = "Yes";
    public string noButtonText = "No";

    [Header("Task Requirement (Leave Blank for Normal Doors)")]
    [Tooltip("Task ID that must be completed before this door unlocks (e.g. 'pickpocket_id').")]
    public string requiredTaskId = "";
    public string lockedSpeakerName = "Con";
    [TextArea(2, 3)]
    public string lockedMessage = "The kitchen backdoor is locked. I need the worker's ID key card first.";
    public string lockedButtonText = "OK";

    [Header("Task Completed Upon Entering (Optional)")]
    [Tooltip("Task ID to mark complete when the player chooses Yes (e.g. 'enter_kitchen').")]
    public string completeTaskIdOnEnter = "";

    GameModel model;
    bool isPromptOpen;

    void Start()
    {
        model = Schedule.GetModel<GameModel>();
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPromptOpen || model == null || DialogueManager.Instance == null)
            return;

        if (other.GetComponentInParent<CharacterController2D>() == model.player)
        {
            isPromptOpen = true;

            // Check if a task is required and whether it has been completed yet
            bool isUnlocked = string.IsNullOrEmpty(requiredTaskId) || GameTaskTracker.IsTaskCompleted(requiredTaskId);

            if (!isUnlocked)
            {
                // Run the locked / needs-ID prompt
                DialogueManager.Instance.ShowLine(
                    lockedSpeakerName,
                    lockedMessage,
                    lockedButtonText,
                    () =>
                    {
                        DialogueManager.Instance.Hide();
                        Invoke(nameof(ResetTrigger), 0.5f);
                    }
                );
                return;
            }

            // Run the regular door prompt
            DialogueManager.Instance.ShowChoice(
                doorSpeakerName,
                regularPromptMessage,
                yesButtonText,
                noButtonText,
                HandleRegularDoorChoice
            );
        }
    }

    void HandleRegularDoorChoice(int index)
    {
        DialogueManager.Instance.Hide();

        // index 0 = Yes
        if (index == 0)
        {
            if (!string.IsNullOrEmpty(completeTaskIdOnEnter))
                GameTaskTracker.CompleteTask(completeTaskIdOnEnter);

            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else if (sameSceneDestination != null && model != null && model.player != null)
            {
                model.player.transform.position = sameSceneDestination.position;
            }
        }

        Invoke(nameof(ResetTrigger), 0.5f);
    }

    void ResetTrigger() => isPromptOpen = false;
}