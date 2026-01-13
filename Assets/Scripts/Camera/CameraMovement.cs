using System;
using System.Data.SqlTypes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform eye;
    private Vector3 offset;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float distanceBehindPlayer;
    [SerializeField] private float speedLerp = 5f;
    // Section Camera moves by player
    private PlayerInputActions input;
    private Vector2 inputVector;
    private Vector3 inputDir;
    private float deadzone = 0.2f;
    private bool eyeCameraDir = true;

    void Start()
    {
        input = player.GetComponent<PInputController>().input;
        input.Player.CameraMove.performed += ctx => 
        {
            inputVector = ctx.ReadValue<Vector2>();
            if (DialogueActionController.Instance != null)
                DialogueActionController.Instance.actionApplied?.Invoke(WaitingActionType.CameraMove);
        };
        input.Player.CameraMove.canceled += ctx => inputVector = Vector2.zero;
        input.Player.CameraMode.performed += ctx => eyeCameraDir = !eyeCameraDir;
        offset = transform.position - player.position;
        distanceBehindPlayer = offset.magnitude;
    }
    void Update()
    {
        if (inputVector.sqrMagnitude > deadzone * deadzone)
        {
            inputDir = new Vector3(inputVector.x, 0, inputVector.y);
        }
        else
        {
            inputDir = Vector3.zero;
        }
    }

    void LateUpdate()
    {
        Vector3 direction;
        if (eyeCameraDir)
        {
            direction = (eye.position - player.position).normalized;
        }
        else
        {
            direction = player.forward;
        }

        // float angleY = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
        float angleY = Mathf.Lerp(-90, 90, (inputDir.x + 1)/2);

        Quaternion horizontalRot = Quaternion.Euler(0f, angleY, 0f);
        direction = horizontalRot * direction;

        Vector3 targetPosition = player.position - direction * distanceBehindPlayer;

        targetPosition.y = transform.position.y;

        if (inputDir.sqrMagnitude > deadzone * deadzone)
        {
            Vector3 lookDirection = player.position - targetPosition;
            lookDirection.y = direction.y;
            targetRotation = Quaternion.LookRotation(lookDirection);
        }
        else
        {
            targetRotation = Quaternion.LookRotation(direction);
        }

        MoveAroundPlayer(targetPosition);
    }


    private void MoveAroundPlayer(Vector3 _targetPosition)
    {
        float orbitSpeed = speedLerp * 50;

        // Directions autour du joueur
        Vector3 currentDir = transform.position - player.position;
        Vector3 targetDir = _targetPosition - player.position;
        Vector3 currentDirFlat = new Vector3(currentDir.x, 0, currentDir.z);
        Vector3 targetDirFlat = new Vector3(targetDir.x, 0, targetDir.z);
        float angle = Vector3.SignedAngle(currentDirFlat, targetDirFlat, Vector3.up);
      
        if (Mathf.Abs(angle) > 2 && inputDir.sqrMagnitude > deadzone * deadzone)
        {
            float deltaAngle = Mathf.Clamp(angle, -orbitSpeed * Time.deltaTime, orbitSpeed * Time.deltaTime);

            transform.RotateAround(
                player.position,
                Vector3.up,
                deltaAngle
            );
            if (Mathf.Abs(currentDir.magnitude - distanceBehindPlayer) > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, player.position + currentDir, Time.deltaTime * speedLerp);
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * speedLerp);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speedLerp);
        }
    }
}