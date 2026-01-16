using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XInput;

public class PInputController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    public PlayerInputActions input;

    private void InputsStart(PlayerController.Foot foot, bool isForward)
    {
        if (DialogueActionController.Instance != null)
            DialogueActionController.Instance.actionApplied?.Invoke(isForward ? WaitingActionType.MoveForward : WaitingActionType.MoveBackward);
        playerController.InputsStart(foot, isForward);
    }
    private void InputsCancel(PlayerController.Foot foot, bool isForward)
    {
        playerController.InputsCancel(foot, isForward);
    }

    private void Rotate(bool isLeft, bool isStart)
    {
        if (DialogueActionController.Instance != null && isStart)
            DialogueActionController.Instance.actionApplied?.Invoke(WaitingActionType.Rotate);

        if (isLeft)
        {
            if (isStart)
                playerController.SetLRI(true);
            else
                playerController.SetLRI(false);
        }
        else
        {
            if (isStart)
                playerController.SetRRI(true);
            else
                playerController.SetRRI(false);
        }
        
    }

    private void Awake()
    {
        input = new PlayerInputActions();
    }


    void OnEnable()
    {
        input.Player.Enable();

        if (DialogueActionController.Instance != null) InputSystem.onAnyButtonPress.Call(OnAnyButtonPress);

        // Keyboard modifier
        input.Player.KeyboardModifier.started += ctx => playerController.SetKeyboardModifier(true);
        input.Player.KeyboardModifier.canceled += ctx => playerController.SetKeyboardModifier(false);

        input.Player.Pause.started += ctx => GameManager.singleton.OnPause();
    

        input.Player.PassDialogue.started += ctx => {
            if (DialogueActionController.Instance != null)
                DialogueController.Instance.NextLine();
        };

        // Gauche
        input.Player.LeftStep.started += ctx =>
        {
            if (playerController.keyboardModifier)
            {
                InputsStart(PlayerController.Foot.Left, false);
            }
            else
            {
                InputsStart(PlayerController.Foot.Left, true);
            }
        };
        input.Player.LeftStep.canceled += ctx =>
        {
            if (playerController.leftFootController.backPressed && !playerController.leftFootController.forwardPressed)
            {
                InputsCancel(PlayerController.Foot.Left, false);
            }
            else
            {
                InputsCancel(PlayerController.Foot.Left, true);
            }
        };  
        input.Player.LeftStepBackward.performed += ctx =>
        {
            InputsStart(PlayerController.Foot.Left, false);
        };
        input.Player.LeftStepBackward.canceled += ctx =>
        {
            InputsCancel(PlayerController.Foot.Left, false);
        };

        // Droite
        input.Player.RightStep.started += ctx =>
        {
            if (playerController.keyboardModifier)
            {
                InputsStart(PlayerController.Foot.Right, false);
            }
            else
            {
                InputsStart(PlayerController.Foot.Right, true);
            }
        };
        input.Player.RightStep.canceled += ctx =>
        {
            if (playerController.rightFootController.backPressed && !playerController.rightFootController.forwardPressed)
            {
                InputsCancel(PlayerController.Foot.Right, false);
            }
            else
            {
                InputsCancel(PlayerController.Foot.Right, true);
            }
        };
        input.Player.RightStepBackward.performed += ctx =>
        {
            InputsStart(PlayerController.Foot.Right, false);
        };
        input.Player.RightStepBackward.canceled += ctx =>
        {
            InputsCancel(PlayerController.Foot.Right, false);
        };


        input.Player.Right.started += ctx => Rotate(false, true);
        input.Player.Right.canceled += ctx => Rotate(false, false);
        input.Player.Left.started += ctx => Rotate(true, true);
        input.Player.Left.canceled += ctx => Rotate(true, false);
    }

    void OnDisable()
    {
        input.Player.Disable();
    }

    private void OnAnyButtonPress(InputControl control)
    {
        Controller controller = GameManager.singleton.currentController;
        switch (control.device)
        {
            case Keyboard:
                GameManager.singleton.currentController = Controller.Keyboard;
                break;
            case Mouse:
                GameManager.singleton.currentController = Controller.Keyboard;
                break;
            case DualShockGamepad:
                GameManager.singleton.currentController = Controller.Playstation;
                break;
            case XInputController:
                GameManager.singleton.currentController = Controller.Xbox;
                break;
        }

        if (controller != GameManager.singleton.currentController)
        {
            DialogueActionController.Instance.ChangePassButton();
        }

    }

}
