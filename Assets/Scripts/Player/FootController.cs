using UnityEngine;
using UnityEngine.PlayerLoop;

public class FootController
{
    // Foot state
    private FootState footState;
    public FootState FootState { get { return footState; } }
    public bool justForwardPressed;
    public bool forwardPressed;
    public bool justForwardReleased;
    public bool nextForwardPressed;
    public bool justBackPressed;
    public bool backPressed;
    public bool justBackReleased;
    public bool nextBackPressed;
    public bool globalPressed;
    public float stepSpeed = 0f;
    // Obstacle related
    public bool legOnObstacle;
    public int legObstacleCounter = 0;
    public Vector3 obstacleOnLegContactPoint = Vector3.zero;
    public Vector3 closestFootColPointToObstacle = Vector3.zero;
    public  float signedObsForwardDirFromTranform;
    public  float signedObsForwardDirFromTarget;
    public  float signedObsRightDir; // Positive means obstacle is on the right side of the leg
    
    // Other foot state
    private FootController otherFootController;

    // Direction enum
    public enum Direction { Forward, Backward };

    //Global parameters
    private Transform playerTransform;
    private AnimationCurve speedStepCurve;
    private AnimationCurve heightFootCurve;
    private NoiseEmitter noiseEmitter;
    private LayerMask obstacleMask;
    private LayerMask groundMask;
    // Constant parameters
    private float maxStepLength;
    private float maxStepHeight;
    private float maxStepSpeed;
    private float speedFootToGround;
    private float toleranceDistanceForStep;
    private float distDiffFromOldOriginePos;
    public RaycastHit hitForCorrection;

    public FootController(Transform playerTransform, Transform IKTarget, AnimationCurve speedStepCurve, AnimationCurve heightFootCurve, NoiseEmitter noiseEmitter, LayerMask obstacleMask,
                        LayerMask groundMask, float maxStepLength, float maxStepHeight, float maxStepSpeed, float toleranceDistanceForStep, float speedFootToGround)
    {
        this.playerTransform = playerTransform;
        this.speedStepCurve = speedStepCurve;
        this.heightFootCurve = heightFootCurve;
        this.noiseEmitter = noiseEmitter;
        this.obstacleMask = obstacleMask;
        this.groundMask = groundMask;
        this.maxStepLength = maxStepLength;
        this.maxStepHeight = maxStepHeight;
        this.maxStepSpeed = maxStepSpeed;
        this.toleranceDistanceForStep = toleranceDistanceForStep;
        this.speedFootToGround = speedFootToGround;
        distDiffFromOldOriginePos = 0;
        // Initialize foot state
        footState = new FootState(IKTarget);
    }

    public void Initialization()
    {
        footState.currentMaxLen = maxStepLength;
        if (Physics.Raycast(footState.IKTarget.position, Vector3.down, out RaycastHit hit))
        {
            footState.IKTarget.position = hit.point + Vector3.up * 0.1f;
            footState.IKTargetOrigin = footState.IKTarget.position;
        }

        // Initialize desiredPos to actual state les desired world positions à l'état actuel
        footState.desiredPos = footState.IKTarget.position;
    }

    public void SetOtherFootState(FootController otherFootController)
    {
        this.otherFootController = otherFootController;
    }

    private void InputAssignement(out bool justPressed, out bool pressed, out bool justReleased, out bool nextPressed, Direction direction)
    {
        if (direction == Direction.Forward)
        {
            justPressed = justForwardPressed;
            pressed = forwardPressed;
            justReleased = justForwardReleased;
            nextPressed = nextForwardPressed;
        }
        else // Backward
        {
            justPressed = justBackPressed;
            pressed = backPressed;
            justReleased = justBackReleased;
            nextPressed = nextBackPressed;
        }
    }

    private void GetBackInput(Direction direction, bool justPressed, bool pressed, bool justReleased, bool nextPressed)
    {
        if (direction == Direction.Forward)
        {
            justForwardPressed = justPressed;
            forwardPressed = pressed;
            justForwardReleased = justReleased;
            nextForwardPressed = nextPressed;
        }
        else // Backward
        {
            justBackPressed = justPressed;
            backPressed = pressed;
            justBackReleased = justReleased;
            nextBackPressed = nextPressed;
        }
    }

    public void ProcessSteps()
    {
        ProcessStep(Direction.Forward);
        ProcessStep(Direction.Backward);
    }

    public void ProcessStep(Direction direction)
    {
        InputAssignement(out bool justPressed, out bool pressed, out bool justReleased, out bool nextPressed, direction);
        float directionSign = direction == Direction.Backward ? -1f : 1f;
        
        // === (justForwardPressed / backJustPressed) ===
        if (justPressed)
        {
            if (!CantStartStep())
            {
                justPressed = false;
                nextPressed = false;
            }
            else
            {
                distDiffFromOldOriginePos = 0;

                float signedMove = Vector3.Dot(
                    otherFootController.FootState.IKTarget.position - footState.IKTarget.position,
                    directionSign * playerTransform.forward);

                footState.currentMaxLen = maxStepLength + signedMove;

                if (footState.currentMaxLen < 0.1f)
                    pressed = false;
            }
        }
        // === Step in run ===
        else if (pressed)
        {
            stepSpeed = speedStepCurve.Evaluate(footState.currentStepLen) * maxStepSpeed;

            if (legOnObstacle)
            {
                float correction = IKTargetPositionCorrectionOnObstacle(
                    closestFootColPointToObstacle,
                    direction
                );

                if ((direction == Direction.Backward && signedObsForwardDirFromTranform <= 0) ||
                    (direction == Direction.Forward && signedObsForwardDirFromTarget >= 0))
                {
                    footState.currentStepLen += correction / footState.currentMaxLen;
                }
                else
                {
                    footState.currentStepLen += Time.deltaTime * stepSpeed / footState.currentMaxLen;
                }
            }
            else
            {
                footState.currentStepLen += Time.deltaTime * stepSpeed / footState.currentMaxLen;
            }

            footState.currentStepLen = Mathf.Clamp(footState.currentStepLen, 0, 1f);
            footState.currentHeight = heightFootCurve.Evaluate(footState.currentStepLen);

            bool otherDirPressed;
            if (direction == Direction.Backward){
                footState.currentHeight = Mathf.Clamp(footState.currentHeight, 0, 0.5f);
                otherDirPressed = forwardPressed;
            }
            else{
                otherDirPressed = backPressed;
            }

            if (otherFootController.globalPressed || otherDirPressed || footState.movingToOrigin || Mathf.Abs(footState.currentStepLen) >= 1)
            {
                footState.currentStepLen = 0;
                footState.movingToOrigin = true;
                nextPressed = false;
            }
            else
            {
                footState.onGround = false;
                // bodyYTarget = footState.currentHeight * maxStepHeight * YBodyMoveFactor; // For body movement when stepping
                footState.desiredPos =
                    footState.IKTargetOrigin
                    - directionSign * distDiffFromOldOriginePos * playerTransform.forward
                    + directionSign * footState.currentStepLen * footState.currentMaxLen * playerTransform.forward
                    + footState.currentHeight * maxStepHeight * playerTransform.up;
            }
        }
        // === Release ===
        else if (justReleased)
        {
            RaycastHit hit;
            footState.currentStepLen = 0;

            if (!footState.onGround && !legOnObstacle &&
                Physics.Raycast(footState.IKTarget.position, Vector3.down, out hit, 10f, groundMask))
            {
                footState.IKTargetOrigin = hit.point + Vector3.up * 0.1f;
            }

            footState.movingToOrigin = true;
        }
        GetBackInput(direction, justPressed, pressed, justReleased, nextPressed);
    }

    private bool CantStartStep()
    {
        float otherDist = Vector3.Distance(footState.IKTarget.position, otherFootController.FootState.IKTarget.position);
        return otherFootController.globalPressed || !footState.onGround || otherDist > toleranceDistanceForStep;
    }

    private float IKTargetPositionCorrectionOnObstacle(Vector3 footPointOnObstacle, Direction dir)
    {
        float directionSign = dir == Direction.Backward ? -1f : 1f;
        float correction = 0;
        if (Physics.Raycast(footPointOnObstacle - 1 * directionSign * playerTransform.forward, directionSign * playerTransform.forward, out hitForCorrection, 3, obstacleMask))
        {
            correction = Vector3.Dot(hitForCorrection.point - footPointOnObstacle, directionSign * playerTransform.forward);
            Debug.DrawRay(footPointOnObstacle - 1 * directionSign * playerTransform.forward, directionSign * playerTransform.forward * 3, Color.green);
            if (correction  > 0)
            {
                correction = 0;
            }
        }

        if (Mathf.Abs(correction) < 0.05f)
        {
            correction = 0;
        }
        return correction;
    }

    public void FootMovingToOriginalPos()
    {
        if (footState.movingToOrigin)
        {
            float distL = Vector3.Distance(footState.IKTarget.position, footState.IKTargetOrigin);
            if (distL <= 0.05f)
            {
                // set desired world pos; actual transform will be applied in LateUpdate
                footState.desiredPos = footState.IKTargetOrigin;
                footState.movingToOrigin = false;
                footState.onGround = true;
                noiseEmitter.MakeNoise(1);
            }
            else
            {
                footState.desiredPos = Vector3.Lerp(footState.IKTarget.position, footState.IKTargetOrigin, Time.deltaTime * speedFootToGround);
            }
        }
    }

    public void SetIKTargetOrigin((Vector3 pos, float distDiff) data)
    {
        distDiffFromOldOriginePos += data.distDiff;
        footState.IKTargetOrigin = data.pos;
    }

    public void ObstacleDataUpdate()
    {
        if (legObstacleCounter > 0)
        {
            legOnObstacle = true;
        }
        else
        {
            legOnObstacle = false;
            legObstacleCounter = 0;
        }
    

        if (legOnObstacle)
        {
            signedObsForwardDirFromTranform = Vector3.Dot(obstacleOnLegContactPoint - playerTransform.position, playerTransform.forward);
            signedObsForwardDirFromTarget = Vector3.Dot(obstacleOnLegContactPoint - (footState.IKTarget.position + playerTransform.forward * 0.1f), playerTransform.forward);
            signedObsRightDir = Vector3.Dot(obstacleOnLegContactPoint - footState.IKTarget.position, playerTransform.right);
        }else
        {
            signedObsForwardDirFromTranform = 0;
            signedObsForwardDirFromTarget = 0;
            signedObsRightDir = 0;
        }
    }

    public void LateUpdatePositionSet()
    {
        footState.IKTarget.position = footState.desiredPos;
    }

    public void LateUpdateInputAssignation()
    {
        if (!nextForwardPressed)
        {
            forwardPressed = false;
            nextForwardPressed = true;
        }
        if (!nextBackPressed)
        {
            backPressed = false;
            nextBackPressed = true;
        }
    }

    
    public void BoolAssignation()
    {
        // Forward
        if (justForwardPressed)
        {
            justForwardPressed = false;
        }

        if (forwardPressed)
        {
            if (otherFootController.globalPressed) forwardPressed = false; // empêcher le maintien si l'autre pied est appuyé
        }

        if (justForwardReleased)
        {
            justForwardReleased = false;
        }

        // Backward
        if (justBackPressed)
        {
            justBackPressed = false;
        }

        if (backPressed)
        {
            if (otherFootController.globalPressed) backPressed = false; // empêcher le maintien si l'autre pied est appuyé
        }

        if (justBackReleased)
        {
            justBackReleased = false;
        }
    }
}
