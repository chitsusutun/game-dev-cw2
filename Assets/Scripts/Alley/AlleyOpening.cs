using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGM.Core;
using RPGM.Gameplay;
using RPGM.UI;

public class AlleyOpening : MonoBehaviour
{
    public static bool hasPlayed = false;

    [Header("Actors")]
    public Transform workerTransform;
    public Transform doorSpawnPoint;
    public Transform wallStopPoint;
    public float workerWalkSpeed = 4f;
    public WorkerPatrol workerPatrol;

    [Header("On-Screen Task UI")]
    public GameObject objectiveContainer;
    public TextMeshProUGUI objectiveText;

    GameModel model;
    bool advanceRequested;

    string[] conIntroLines = new string[]
    {
        "How I wish I could enter just one animalistocrat party.",
        "Listening to the lively music from the outside is enough to make a chicken run around like he's headless!",
        "Only bigger animals are allowed into these parties as VIPs.",
        "They don't even know how powerful I am!",
        "All I would have to do is find a way into this party, and then I could become what I've always known I am.",
        "An animalistocrat!"
    };

    void Awake()
    {
        SetObjectiveText("");

        // If the cutscene already played earlier, place the worker at the wall and skip hiding
        if (hasPlayed || GameTaskTracker.IsTaskCompleted("pickpocket_id"))
        {
            if (workerTransform != null)
            {
                if (wallStopPoint != null)
                {
                    workerTransform.position = new Vector3(
                        wallStopPoint.position.x,
                        wallStopPoint.position.y,
                        workerTransform.position.z
                    );
                }
                workerTransform.gameObject.SetActive(true);
                if (workerPatrol != null) workerPatrol.enabled = false;
            }
            return;
        }

        // First visit: hide the worker until their cutscene cue
        if (workerTransform != null)
        {
            workerTransform.gameObject.SetActive(false);
            if (workerPatrol != null) workerPatrol.enabled = false;
        }
    }

    IEnumerator Start()
    {
        // Wait one frame so DialogueManager.Awake() and GameModel finish initializing
        yield return null;

        model = Schedule.GetModel<GameModel>();

        // Skip the opening cutscene if the player is returning to the alley
        if (hasPlayed || GameTaskTracker.IsTaskCompleted("pickpocket_id"))
        {
            if (model != null && model.input != null)
                model.input.ChangeState(InputController.State.CharacterControl);

            yield break;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("[AlleyOpening] DialogueManager.Instance is null! Make sure DialogueManager is in the scene.");
            yield break;
        }

        hasPlayed = true;
        LockPlayerMovement();

        // 1. Con's opening monologue
        for (int i = 0; i < conIntroLines.Length; i++)
        {
            yield return ShowDialogueLine("Con", conIntroLines[i]);
        }

        // 2. Worker dashes out of the kitchen door and stops at wallStopPoint
        if (workerTransform != null && doorSpawnPoint != null && wallStopPoint != null)
        {
            LockPlayerMovement();
            workerTransform.position = doorSpawnPoint.position;
            workerTransform.gameObject.SetActive(true);

            Vector3 targetPos = new Vector3(
                wallStopPoint.position.x,
                wallStopPoint.position.y,
                workerTransform.position.z
            );

            while (Vector2.Distance(workerTransform.position, targetPos) > 0.05f)
            {
                LockPlayerMovement();
                workerTransform.position = Vector3.MoveTowards(
                    workerTransform.position,
                    targetPos,
                    workerWalkSpeed * Time.deltaTime
                );
                yield return null;
            }

            workerTransform.position = targetPos;
        }

        // 3. Worker complains at the wall
        yield return ShowDialogueLine("Worker", "I hate my job!");

        // 4. Con's thought
        yield return ShowDialogueLine(
            "Con (Thought)",
            "This animal is a worker at this party! I could use this to gain entry to the kitchen."
        );

        // 5. Close dialogue and restore player movement
        DialogueManager.Instance.Hide();

        if (model != null && model.input != null)
            model.input.ChangeState(InputController.State.CharacterControl);

        // Display bottom objective and register task (GameTaskTracker automatically updates TaskListManager)
        SetObjectiveText("Task: Approach the Worker and hold [E] while they aren't looking to pickpocket their ID.");
        GameTaskTracker.StartTask("pickpocket_id", "Pickpocket the Worker's ID by holding [E] while they aren't looking.");

        if (workerPatrol != null)
            workerPatrol.enabled = true;
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