using UnityEngine;

[System.Serializable]
public class DialogueFile
{
    public string dialogueId;
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    public string[] text;

    public InputHint[] showInputs;

    public WaitingActionType waitForAction;

    public string triggerEventId;
}

[System.Serializable]
public class InputHint
{
    public string input;
    public string action;
}
[System.Serializable]
public enum WaitingActionType
{
    None,
    MoveForward,
    MoveBackward,
    Rotate,
    CameraMove,
    ChangeCameraMode
}

public static class DialogueLoader
{
    public static DialogueFile Load(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Dialogues/" + fileName);

        if (jsonFile == null)
        {
            Debug.LogError($"Dialogue JSON introuvable : {fileName}");
            return null;
        }

        return JsonUtility.FromJson<DialogueFile>(jsonFile.text);
    }
}
