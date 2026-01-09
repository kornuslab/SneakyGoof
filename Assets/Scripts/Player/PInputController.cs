using Unity.VisualScripting;
using UnityEngine;

public class PInputController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    public PlayerInputActions input;

    private void InputsStart(PlayerController.Foot foot, bool isForward)
    {
        DialogueActionController.Instance.actionApplied?.Invoke(isForward ? WaitingActionType.MoveForward : WaitingActionType.MoveBackward);
        playerController.InputsStart(foot, isForward);
    }
    private void InputsCancel(PlayerController.Foot foot, bool isForward)
    {
        playerController.InputsCancel(foot, isForward);
    }

    private void Awake()
    {
        input = new PlayerInputActions();
    }

    void OnEnable()
    {
        input.Player.Enable();

        // Keyboard modifier
        input.Player.KeyboardModifier.started += ctx => playerController.SetKeyboardModifier(true);
        input.Player.KeyboardModifier.canceled += ctx => playerController.SetKeyboardModifier(false);

        input.Player.Pause.started += ctx => GameManager.singleton.OnPause();
    

        input.Player.PassDialogue.started += ctx => {
            if (DialogueActionController.Instance.actionWaitingFor != WaitingActionType.None)
                return;
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

        input.Player.Right.started += ctx => playerController.SetRRI(true);
        input.Player.Right.canceled += ctx =>  playerController.SetRRI(false); 
        input.Player.Left.started += ctx => playerController.SetLRI(true);
        input.Player.Left.canceled += ctx => playerController.SetLRI(false);
    }

    void OnDisable()
    {
        input.Player.Disable();
    }

}
