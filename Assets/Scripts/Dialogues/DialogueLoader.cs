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
    public string speaker = "";
    public string[] text = new string[0];

    public WaitingActionType waitForAction;
    public DialogueAction dialogueAction = DialogueAction.None;
    public string triggerEventId;
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

[System.Serializable]
public enum DialogueAction
{
    None,
    EnableCameraMode,
    Show_Noise,
    FinishTutorial
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
