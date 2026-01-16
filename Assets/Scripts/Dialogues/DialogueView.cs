using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject commandImagesPanel;

    [Header("Typing")]
    [SerializeField] private float charDelay = 0.03f;

    private Coroutine typingCoroutine;
    private string fullText;
    private bool isTyping;

    public void Show(string speakerName, string text)
    {
        panel.SetActive(true);
        speakerNameText.text = speakerName;

        fullText = text;
        dialogueText.text = "";

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        isTyping = true;

        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
    }

    public void SkipTyping()
    {
        if (!isTyping)
            return;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = fullText;
        isTyping = false;
    }

    public bool IsTyping()
    {
        return isTyping;
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    public void ShowCommandImages(bool show)
    {
        commandImagesPanel.SetActive(show);
    }
}
