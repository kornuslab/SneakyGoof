using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }
    [SerializeField] private DialogueView dialogueView;
    private DialogueFile currentDialogue;
    private int currentLineIndex;
    public List<string> dialogueFileNames;
    private int dialogueIndex = 0;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        
        if (dialogueFileNames.Count > 0)
            StartDialogue(dialogueFileNames[0]);
    }

    public void StartDialogue(string dialogueFileName)
    {
        currentDialogue = DialogueLoader.Load(dialogueFileName);
        currentLineIndex = 0;

        ShowCurrentLine();
    }

    public void NextDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex >= dialogueFileNames.Count)
            return;
        StartDialogue(dialogueFileNames[dialogueIndex]);
    }

    public void NextLine(WaitingActionType actionApplied = WaitingActionType.None)
    {
        if (DialogueActionController.Instance.actionWaitingFor != actionApplied)
        {
            return;
        }else if (dialogueView.IsTyping())
        {
            dialogueView.SkipTyping();
            return;
        } 
        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            return;
        }

        ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        var line = currentDialogue.lines[currentLineIndex];
        string fullText = string.Join("\n", line.text);
        dialogueView.Show(line.speaker, fullText);
        DialogueActionController.Instance.ApplyLine(line);

        // tutorialContext.ApplyLine(line);
    }
}