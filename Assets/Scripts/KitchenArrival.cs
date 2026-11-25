using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGM.Core;
using RPGM.Gameplay;
using RPGM.UI;

public class KitchenArrival : MonoBehaviour
{
    GameModel model;
    bool advanceRequested;

    public static bool hasPlayed = false;


    string[] kitchenLines = new string[]
    {
        "I am greater than any animal alive!",
        "Now that I have entered the kitchen, I can enter this party.",
        "All I have to do is charm everyone, and then I can become the animalistocrat I always dreamt of being!"
    };

void Awake()
    {
        SetObjectiveText("");

        if (hasPlayed || GameTaskTracker.IsTaskCompleted("enter_kitchen"))
        {
            return;
        }
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.4f);

        model = Schedule.GetModel<GameModel>();

        if (hasPlayed || GameTaskTracker.IsTaskCompleted("enter_kitchen"))
        {
            if (model != null && model.input != null)
                model.input.ChangeState(InputController.State.CharacterControl);

            yield break;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("[KitchenArrival] DialogueManager.Instance is null! Make sure DialogueManager is in the scene.");
            yield break;
        }

        hasPlayed = true;
        LockPlayerMovement();

        // 1. Con's opening monologue
        for (int i = 0; i < kitchenLines.Length; i++)
        {
            yield return ShowDialogueLine("Con", kitchenLines[i]);
        }
        
        DialogueManager.Instance.Hide();

        GameTaskTracker.CompleteTask("enter_kitchen");

        if (model != null && model.input != null)
            model.input.ChangeState(InputController.State.CharacterControl);

        GameTaskTracker.StartTask("explore_kitchen", "Explore the kitchen and find a way to enter the party.");
        SetObjectiveText("Explore the kitchen and find a way to enter the party.");
    }
    void LockPlayerMovement()
    {
        if (model == null)
            model = Schedule.GetModel<GameModel>();

        if (model != null)
        {
            if (model.input != null)
                model.input.ChangeState(InputController.State.DialogControl);

            if (model.player != null)
            {
                model.player.nextMoveCommand = Vector3.zero;
                var rb = model.player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }
    }

    IEnumerator ShowDialogueLine(string speakerName, string text)
    {
        advanceRequested = false;
        DialogueManager.Instance.ShowLine(speakerName, text, "Next", () => advanceRequested = true);
        yield return new WaitUntil(() => advanceRequested);
    }

    public void SetObjectiveText(string message)
    {
        bool hasText = !string.IsNullOrEmpty(message);

        TaskListManager.Instance?.SetObjectiveText(message);

    }
}