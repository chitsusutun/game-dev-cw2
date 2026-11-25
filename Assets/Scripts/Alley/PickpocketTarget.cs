using UnityEngine;
using UnityEngine.InputSystem;
using RPGM.Core;
using RPGM.Gameplay;

[RequireComponent(typeof(CircleCollider2D))]
public class PickpocketTarget : MonoBehaviour
{
    public float holdDuration = 1.5f;
    public int maxAttempts = 3;

    public static bool hasKeyCard = false;

    WorkerPatrol patrol;
    GameModel model;
    bool playerInRange;
    float holdTimer;
    int failedAttempts;
    bool wasCaughtThisHold;

    void Start()
    {
        hasKeyCard = false;
        failedAttempts = 0;
        patrol = GetComponent<WorkerPatrol>();
        model = Schedule.GetModel<GameModel>();
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (model == null) model = Schedule.GetModel<GameModel>();

        if (model != null && other.GetComponentInParent<CharacterController2D>() == model.player)
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (model != null && other.GetComponentInParent<CharacterController2D>() == model.player)
        {
            playerInRange = false;
            holdTimer = 0f;
            wasCaughtThisHold = false;
        }
    }

    void Update()
    {
        if (hasKeyCard || !playerInRange || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.isPressed)
        {
            // Ignore further checks until the player lets go of [E] after getting caught
            if (wasCaughtThisHold)
                return;

            // Worker caught the player holding [E]
            if (patrol != null && patrol.isLookingAtPlayer)
            {
                wasCaughtThisHold = true;
                holdTimer = 0f;
                failedAttempts++;

                int remaining = maxAttempts - failedAttempts;

                if (remaining <= 0)
                {
                    TaskListManager.Instance?.SetObjectiveText("");
                    GameOverManager.Instance?.TriggerGameOver(
                        "You were caught trying to pickpocket the worker too many times!"
                    );
                }
                else
                {
                    TaskListManager.Instance?.SetObjectiveText(
                        $"Caught! Wait until the worker faces the wall. ({remaining} {(remaining == 1 ? "chance" : "chances")} left)"
                    );
                }
                return;
            }

            holdTimer += Time.deltaTime;
            int pct = Mathf.Clamp(Mathf.RoundToInt((holdTimer / holdDuration) * 100f), 0, 100);
            TaskListManager.Instance?.SetObjectiveText($"Pickpocketing ID... {pct}% (Keep holding [E])");

            if (holdTimer >= holdDuration)
            {
                hasKeyCard = true;
                TaskListManager.Instance?.SetObjectiveText("Got the Worker's ID! Walk up to the illuminated kitchen backdoor to enter.");

                GameTaskTracker.StartTask("enter_kitchen", "Use the Worker's ID key card to enter the illuminated kitchen backdoor.");
                GameTaskTracker.CompleteTask("pickpocket_id");
            }
        }
        else
        {
            // Player released [E], allowing a fresh attempt
            wasCaughtThisHold = false;

            if (holdTimer > 0f)
            {
                holdTimer = 0f;
                int remaining = maxAttempts - failedAttempts;
                TaskListManager.Instance?.SetObjectiveText(
                    $"Hold [E] while the worker isn't looking to steal their ID. ({remaining} {(remaining == 1 ? "chance" : "chances")} left)"
                );
            }
        }
    }
}