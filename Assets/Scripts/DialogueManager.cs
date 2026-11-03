
using UnityEngine;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Interface")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    private string[] currentLines;
    private int currentLine;
    private Action onDialogueEnd;

    public bool IsRunning { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!IsRunning)
            return;

        if (Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            NextLine();
        }
    }

    public void StartDialogue(string[] lines, Action onComplete = null)
    {
        if (IsRunning)
            return;

        if (lines == null || lines.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        currentLines = lines;
        currentLine = 0;
        onDialogueEnd = onComplete;

        IsRunning = true;
        dialoguePanel.SetActive(true);

        ShowLine();
    }

    private void ShowLine()
    {
        dialogueText.text = currentLines[currentLine];
    }

    private void NextLine()
    {
        currentLine++;

        if (currentLine >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine();
    }

    private void EndDialogue()
    {
        IsRunning = false;
        dialoguePanel.SetActive(false);

        Action callback = onDialogueEnd;
        onDialogueEnd = null;

        currentLines = null;
        callback?.Invoke();
    }
}
