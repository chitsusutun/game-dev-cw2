using System.Collections;
using UnityEngine;

public class WorkerPatrol : MonoBehaviour
{
    public float safeDuration = 3.5f;
    public float alertDuration = 2.0f;
    public bool isLookingAtPlayer { get; private set; }

    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        StartCoroutine(LookCycle());
    }

    IEnumerator LookCycle()
    {
        while (true)
        {
            // Facing wall (safe to pickpocket)
            isLookingAtPlayer = false;
            if (spriteRenderer != null) spriteRenderer.flipX = false;
            yield return new WaitForSeconds(safeDuration);

            // Turns around to check alley
            isLookingAtPlayer = true;
            if (spriteRenderer != null) spriteRenderer.flipX = true;
            yield return new WaitForSeconds(alertDuration);
        }
    }
}