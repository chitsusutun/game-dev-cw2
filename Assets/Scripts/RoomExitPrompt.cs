using UnityEngine;
using UnityEngine.SceneManagement;
using RPGM.Core;
using RPGM.Gameplay;
using RPGM.UI;

[RequireComponent(typeof(BoxCollider2D))]
public class RoomExitPrompt : MonoBehaviour
{
    [Header("Prompt Text")]
    public string promptMessage = "Do you want to leave the room?";
    public string yesButtonText = "Yes";
    public string noButtonText = "No";

    [Header("Destination")]
    public string nextSceneName = "Kitchen";

    GameModel model;
    bool isPromptOpen;

    void Start()
    {
        model = Schedule.GetModel<GameModel>();
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPromptOpen || model == null)
            return;

        if (other.GetComponentInParent<CharacterController2D>() == model.player)
        {
            isPromptOpen = true;

            // Place dialog in the upper-center of the camera view
            Vector3 dialogPos = Camera.main != null
                ? Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.65f, 10f))
                : model.player.transform.position;

            // Open the 2-button dialog directly so ShowConversation doesn't override onButton
            model.dialog.Show(dialogPos, promptMessage, yesButtonText, noButtonText);
            model.dialog.onButton += HandleOptionSelected;
        }
    }

    void HandleOptionSelected(int index)
    {
        Debug.Log($"[RoomExit] Button clicked with index: {index}");
        model.dialog.Hide();

        // Restore player movement controls after clicking a button
        if (model.input != null)
            model.input.ChangeState(InputController.State.CharacterControl);

        // index 0 = Yes, index 1 = No
        if (index == 0)
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"[RoomExit] Loading scene: '{nextSceneName}'...");
                SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            Debug.Log("[RoomExit] Player chose No.");
        }

        Invoke(nameof(ResetTrigger), 0.5f);
    }

    void ResetTrigger() => isPromptOpen = false;
}