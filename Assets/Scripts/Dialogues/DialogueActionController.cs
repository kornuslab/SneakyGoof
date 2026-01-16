using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[System.Serializable]
public class CommandSpriteEntry
{
    public string commandId;
    public GameObject sprite;
}

public class DialogueActionController : MonoBehaviour
{
    public static DialogueActionController Instance { get; private set; }
    public WaitingActionType actionWaitingFor = WaitingActionType.None;
    public WaitingActionType actionOnScreen = WaitingActionType.None;
    [SerializeField] private List<CommandSpriteEntry> commandImages;
    private Dictionary<string, GameObject> commandImageDict = new Dictionary<string, GameObject>();
    public Action<WaitingActionType> actionApplied;
    private List<GameObject> currentCommandImages = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
        if (Instance == null)
        {
            Instance = this;
        }else if (Instance != this)
        {
            Debug.LogWarning("Multiple instances of DialogueActionController detected. Destroying duplicate.");
            Destroy(gameObject);
        }
        actionApplied += OnActionApplied;


        foreach (var entry in commandImages)
        {
            if (!string.IsNullOrEmpty(entry.commandId) && entry.sprite != null)
            {
                commandImageDict[entry.commandId] = entry.sprite;
            }
        }

        GameManager.singleton.SetGameOnTutoMode();
    }

    public void ApplyLine(DialogueLine line)
    {
        ExecuteDialogueAction(line.dialogueAction);
        SetWaitingAction(line.waitForAction, ref actionWaitingFor);
    }


    private void ExecuteDialogueAction(DialogueAction action)
    {
        if (action == DialogueAction.EnableCameraMode)
        {
            GameManager.singleton.EnableCameraMode(true);
        }
        else if (action == DialogueAction.Show_Noise)
        {
            GameManager.singleton.EnableNoise(true);
        }else if (action == DialogueAction.FinishTutorial)
        {
            GameManager.singleton.SetGameOnTutoMode(false);
            gameObject.SetActive(false);
        }
    }

    private void OnActionApplied(WaitingActionType actionType)
    {
        DialogueController.Instance.NextLine(actionType);
    }

    public void ChangePassButton()
    {
        commandImageDict["SouthButtonPS"].SetActive(false);
        commandImageDict["SouthButtonXbox"].SetActive(false);
        commandImageDict["Enter"].SetActive(false);
        switch (GameManager.singleton.currentController)
        {
            case Controller.Xbox:
                commandImageDict["SouthButtonXbox"].SetActive(true);
                break;
            case Controller.Playstation:
                commandImageDict["SouthButtonPS"].SetActive(true);
                break;
            case Controller.Keyboard:
                commandImageDict["Enter"].SetActive(true);
                break;
        }

        SetWaitingAction(actionOnScreen, ref actionOnScreen);
    }


    private void SetWaitingAction(WaitingActionType actionType, ref WaitingActionType actionToChange)
    {
        switch (actionType)
        {
            case WaitingActionType.None:
                actionToChange = WaitingActionType.None;
                break;
            case WaitingActionType.MoveForward:
                SetMoveForwardCommandImage();
                actionToChange = WaitingActionType.MoveForward;
                break;
            case WaitingActionType.MoveBackward:
                SetMoveBackwardCommandImage();
                actionToChange = WaitingActionType.MoveBackward;
                break;
            case WaitingActionType.Rotate:
                SetRotateCommandImage();
                actionToChange = WaitingActionType.Rotate;
                break;
            case WaitingActionType.CameraMove:
                SetCameraMoveCommandImage();
                actionToChange = WaitingActionType.CameraMove;
                break;
            case WaitingActionType.ChangeCameraMode:
                SetCameraModeCommandImage();
                actionToChange = WaitingActionType.ChangeCameraMode;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
        }

        if (actionType != WaitingActionType.None)
        {
            actionOnScreen = actionType;
        }
    }

    public void SetActiveCommandImages(bool isActive)
    {
        foreach (var img in currentCommandImages)
        {
            img.SetActive(isActive);
        } 
        if (!isActive) currentCommandImages.Clear();
    }

    private void SetMoveForwardCommandImage()
    {
        if (GameManager.singleton.currentController == Controller.Xbox)
        {
            SetTwoCommandImageWithString("LB", "RB");
        }
        else if (GameManager.singleton.currentController == Controller.Playstation)
        {
            SetTwoCommandImageWithString("L1", "R1");
        }
        else
        {
            SetTwoCommandImageWithString("LeftArrow", "RightArrow");
        }
    }
    private void SetMoveBackwardCommandImage()
    {
        if (GameManager.singleton.currentController == Controller.Xbox)
        {
            SetTwoCommandImageWithString("LT", "RT");
        }
        else if (GameManager.singleton.currentController == Controller.Playstation)
        {
            SetTwoCommandImageWithString("L2", "R2");
        }
        else
        {
            SetTwoCommandImageWithString("LeftArrow", "RightArrow");
            SetOneCommandImageWithString("Ctrl", false, false);
        }
    }

    private void SetRotateCommandImage()
    {
        if (GameManager.singleton.currentController == Controller.Xbox)
        {
            SetOneCommandImageWithString("Joystick");
        }
        else if (GameManager.singleton.currentController == Controller.Playstation)
        {
            SetOneCommandImageWithString("Joystick");
        }
        else
        {
            SetTwoCommandImageWithString("A", "D");
        }
    }

     private void SetCameraMoveCommandImage()
    {
        if (GameManager.singleton.currentController == Controller.Xbox)
        {
            SetOneCommandImageWithString("Joystick");
        }
        else if (GameManager.singleton.currentController == Controller.Playstation)
        {
            SetOneCommandImageWithString("Joystick");
        }
        else
        {
            // Not implemented yet for keyboard
            // SetTwoCommandImageWithString("A", "D");
        }
    }

     private void SetCameraModeCommandImage()
    {
        if (GameManager.singleton.currentController == Controller.Xbox)
        {
            SetOneCommandImageWithString("NorthButtonXbox");
        }
        else if (GameManager.singleton.currentController == Controller.Playstation)
        {
            SetOneCommandImageWithString("NorthButtonPS4");
        }
        else
        {
            SetOneCommandImageWithString("Space");
        }
    }

    private void SetTwoCommandImageWithString(string leftInput, string rightInput, bool replaceOthers = true, bool anim = true)
    {
        SetActiveCommandImages(!replaceOthers);
        GameObject img = commandImageDict[leftInput];
        img.SetActive(true);
        GameObject img2 = commandImageDict[rightInput];
        img2.SetActive(true);
        currentCommandImages.Add(img);
        currentCommandImages.Add(img2);
        if (anim) StartCoroutine(ManageTwoButtonsAnimation(img, img2));
    }

    private void SetOneCommandImageWithString(string input, bool replaceOthers = true, bool anim = true)
    {
        SetActiveCommandImages(!replaceOthers);
        GameObject img = commandImageDict[input];
        img.SetActive(true);
        currentCommandImages.Add(img);
        if (anim) StartCoroutine(ManageOneButtonsAnimation(img));
    }

    private IEnumerator ManageTwoButtonsAnimation(GameObject img1, GameObject img2)
    {
        while(img1.activeSelf && img2.activeSelf)
        {
            img1.GetComponent<Animator>().SetTrigger("LaunchAnim");
            yield return new WaitForSeconds(1f);
            img2.GetComponent<Animator>().SetTrigger("LaunchAnim");
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator ManageOneButtonsAnimation(GameObject img1)
    {
        while(img1.activeSelf)
        {
            img1.GetComponent<Animator>().SetTrigger("LaunchAnim");
            yield return new WaitForSeconds(2f);
        }
    }
}