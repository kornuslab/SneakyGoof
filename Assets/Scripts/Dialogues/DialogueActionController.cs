using System;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class DialogueActionController : MonoBehaviour
{
    public static DialogueActionController Instance { get; private set; }
    public WaitingActionType actionWaitingFor = WaitingActionType.None;
    [SerializeField] private List<Sprite> commandImages;
    public Action<WaitingActionType> actionApplied;

    private void Start()
    {
        Instance = this;
        if (Instance == null)
        {
            Instance = this;
        }else if (Instance != this)
        {
            Destroy(gameObject);
        }
        actionApplied += OnActionApplied;
    }

    public void ApplyLine(DialogueLine line)
    {
        UpdateInputHints(line.showInputs);
        SetWaitingAction(line.waitForAction);
    }


    private void UpdateInputHints(InputHint[] inputHints)
    {
        // Implementation to show input hints on the UI
    }

    private void OnActionApplied(WaitingActionType actionType)
    {
        DialogueController.Instance.NextLine(actionType);
    }

    private void SetWaitingAction(WaitingActionType actionType)
    {
        switch (actionType)
        {
            case WaitingActionType.None:
                actionWaitingFor = WaitingActionType.None;
                break;
            case WaitingActionType.MoveForward:
                actionWaitingFor = WaitingActionType.MoveForward;
                break;
            case WaitingActionType.MoveBackward:   
                actionWaitingFor = WaitingActionType.MoveBackward;
                break;
            case WaitingActionType.Rotate:
                actionWaitingFor = WaitingActionType.Rotate;
                break;
            case WaitingActionType.CameraMove:
                actionWaitingFor = WaitingActionType.CameraMove;
                break;
            case WaitingActionType.ChangeCameraMode:
                actionWaitingFor = WaitingActionType.ChangeCameraMode;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
        }
    }
}