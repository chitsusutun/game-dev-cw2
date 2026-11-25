using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using RPGM.Core;
using RPGM.Gameplay;
using RPGM.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public GameObject speakerNameBox;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueBodyText;

    [Header("Buttons")]
    public Button primaryButton;
    public TextMeshProUGUI primaryButtonText;
    public Button secondaryButton;
    public TextMeshProUGUI secondaryButtonText;

    [Header("Typewriter & Audio")]
    public float typingSpeed = 0.025f;
    public AudioSource voiceAudioSource;
    public AudioClip blipClip;

    GameModel model;
    Coroutine typingCoroutine;
    bool isTyping;
    string currentFullText;
    Action<int> onChoiceCallback;
    bool isTwoButtonPrompt;

    void Awake()
    {
        Instance = this;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (primaryButton != null)
            primaryButton.onClick.AddListener(() => HandleButtonClick(0));

        if (secondaryButton != null)
            secondaryButton.onClick.AddListener(() => HandleButtonClick(1));
    }

    void Start()
    {
        model = Schedule.GetModel<GameModel>();
    }

    void Update()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf)
            return;

        // Allow Spacebar or Enter to finish typing or advance 1-button dialogue
        if (Keyboard.current != null &&
            (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
        {
            if (isTyping)
            {
                FinishTypingImmediately();
            }
            else if (!isTwoButtonPrompt)
            {
                HandleButtonClick(0);
            }
        }
    }

    // Shows a single line of dialogue with a speaker name and 1 button (e.g., "Next").
    public void ShowLine(string speakerName, string message, string buttonLabel = "Next", Action onNext = null)
    {
        isTwoButtonPrompt = false;
        onChoiceCallback = (_) => onNext?.Invoke();

        SetupPanel(speakerName, message);

        if (primaryButton != null)
        {
            primaryButton.gameObject.SetActive(true);
            if (primaryButtonText != null) primaryButtonText.text = buttonLabel;
        }

        if (secondaryButton != null)
            secondaryButton.gameObject.SetActive(false);
    }

    // Shows a 2-button choice prompt (e.g., "Yes" / "No") at the bottom of the screen.
    public void ShowChoice(string speakerName, string message, string yesLabel, string noLabel, Action<int> onSelected)
    {
        isTwoButtonPrompt = true;
        onChoiceCallback = onSelected;

        SetupPanel(speakerName, message);

        if (primaryButton != null)
        {
            primaryButton.gameObject.SetActive(true);
            if (primaryButtonText != null) primaryButtonText.text = yesLabel;
        }

        if (secondaryButton != null)
        {
            secondaryButton.gameObject.SetActive(true);
            if (secondaryButtonText != null) secondaryButtonText.text = noLabel;
        }
    }

    public void Hide()
    {
        StopTypingAndAudio();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (model != null && model.input != null)
            model.input.ChangeState(InputController.State.CharacterControl);
    }

    void SetupPanel(string speakerName, string message)
    {
        StopTypingAndAudio();

        if (model == null) model = Schedule.GetModel<GameModel>();
        if (model != null && model.input != null)
        {
            model.input.ChangeState(InputController.State.DialogControl);
            if (model.player != null)
            {
                model.player.nextMoveCommand = Vector3.zero;
                var rb = model.player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }

        dialoguePanel.SetActive(true);

        bool hasSpeaker = !string.IsNullOrEmpty(speakerName);
        if (speakerNameBox != null) speakerNameBox.SetActive(hasSpeaker);
        if (speakerNameText != null) speakerNameText.text = speakerName;

        currentFullText = message;
        typingCoroutine = StartCoroutine(TypeTextRoutine(message));
    }

    IEnumerator TypeTextRoutine(string message)
    {
        isTyping = true;
        dialogueBodyText.text = "";

        int charCount = 0;
        foreach (char c in message)
        {
            dialogueBodyText.text += c;
            charCount++;

            // Play a subtle blip every 3 characters (never overlaps)
            if (voiceAudioSource != null && blipClip != null && charCount % 3 == 0 && !char.IsWhiteSpace(c))
            {
                voiceAudioSource.pitch = UnityEngine.Random.Range(0.95f, 1.1f);
                voiceAudioSource.PlayOneShot(blipClip, 0.5f);
            }

            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    void FinishTypingImmediately()
    {
        StopTypingAndAudio();
        if (dialogueBodyText != null)
            dialogueBodyText.text = currentFullText;
    }

    void StopTypingAndAudio()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;

        if (voiceAudioSource != null)
            voiceAudioSource.Stop();
    }

    void HandleButtonClick(int index)
    {
        if (isTyping)
        {
            FinishTypingImmediately();
            return;
        }

        StopTypingAndAudio();
        var callback = onChoiceCallback;
        onChoiceCallback = null;
        callback?.Invoke(index);
    }
}