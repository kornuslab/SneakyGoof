using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(NoiseEmitter))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform LeftLegIKTarget;
    [SerializeField] private Transform RightLegIKTarget;
    private float bodyYTarget = 0;
    [SerializeField] private float bodyYOffset = 1f;
    [SerializeField] private AnimationCurve heightFootCurve;
    [SerializeField] private AnimationCurve speedStepCurve;
    [SerializeField] private float toleranceDistanceForStep = 0.1f;
    [SerializeField] private float speedFootToGround = 20;
    // Distance in meters
    [SerializeField] private float maxStepLength = 1;
    [SerializeField] private float maxStepHeight = 0.3f;
    [SerializeField] private float maxStepSpeed = 1;
    [SerializeField] private float minStepSpeed = 0.1f;
    [SerializeField] private float rotationSpeed = 180f; // deg/s, configuré dans l'inspector
    [SerializeField] private float bodyLerpBtwLegs = 0.3f;
    private float initialYBodyPos;
    public LayerMask obstacleMask;
    [SerializeField] private LayerMask groundMask;
    public enum Foot { Left, Right }
    // Pied Gauche
    public FootController leftFootController;
    // Pied Droit
    public FootController rightFootController;
    // Rotation
    public bool rightRotationInput {get; private set;} = false;
    public bool leftRotationInput {get; private set;} = false;
    // Keyboard modifier
    public bool keyboardModifier {get; private set;} = false;

    private NoiseEmitter noiseEmitter;

    [Header("Speed 20m Measurement")]
    [SerializeField] private float measurementDistance = 20f;
    [SerializeField] private bool debugSpeedMeasurement = true;
    private Vector3 lastPosForMeasurement;
    private float accumulatedDistanceForMeasurement = 0f;
    private float measurementStartTime = 0f;

    [Header("Error checking")]
    [SerializeField] private Transform leftFootTransform;
    [SerializeField] private Transform rightFootTransform;
    [SerializeField] private float maxAllowedFootToIKTargetDistance = 0.5f;

    void Awake()
    {
        noiseEmitter = GetComponent<NoiseEmitter>();
        leftFootController = new FootController(transform, LeftLegIKTarget, speedStepCurve, heightFootCurve, noiseEmitter, obstacleMask, groundMask,
                                            maxStepLength, maxStepHeight, maxStepSpeed, toleranceDistanceForStep, speedFootToGround);
        rightFootController = new FootController(transform, RightLegIKTarget, speedStepCurve, heightFootCurve, noiseEmitter, obstacleMask, groundMask,
                                            maxStepLength, maxStepHeight, maxStepSpeed, toleranceDistanceForStep, speedFootToGround);
        rightFootController.SetOtherFootState(leftFootController);
        leftFootController.SetOtherFootState(rightFootController);
        leftFootController.Initialization();
        rightFootController.Initialization();
        
        initialYBodyPos = transform.position.y;
        var keys = speedStepCurve.keys;
        keys[speedStepCurve.keys.Length - 1].value = minStepSpeed / maxStepSpeed; // ensure min speed is respected
        speedStepCurve.keys = keys;

        // Init measurement for 20m speed
        lastPosForMeasurement = transform.position;
        accumulatedDistanceForMeasurement = 0f;
        measurementStartTime = Time.time;
    }

    public void SetKeyboardModifier(bool value)
    {
        keyboardModifier = value;
    }

    /// <summary>
    /// Set Left Rotation Input
    /// </summary>
    public void SetLRI(bool value)
    {
        leftRotationInput = value;
    }
    /// <summary>
    /// Set Right Rotation Input
    /// </summary>
    public void SetRRI(bool value)
    {
        rightRotationInput = value;
    }

    public void InputsStart(Foot foot, bool isForward)
    {
        // BackLeft
        if (foot == Foot.Left && !isForward)
        {
            leftFootController.justBackPressed = true;
            leftFootController.backPressed = true;
            leftFootController.globalPressed = true;
        }
        // BackRight
        else if (foot == Foot.Right && !isForward)
        {
            rightFootController.justBackPressed = true;
            rightFootController.backPressed = true;
            rightFootController.globalPressed = true;
            
        }
        // ForwardLeft
        else if (foot == Foot.Left && isForward)
        {
            leftFootController.justForwardPressed = true;
            leftFootController.forwardPressed = true;
            leftFootController.globalPressed = true;
        }
        // ForwardRight
        else if (foot == Foot.Right && isForward)
        {
           rightFootController.justForwardPressed = true;
           rightFootController.forwardPressed = true;
           rightFootController.globalPressed = true;
        }
    }

    public void InputsCancel(Foot foot, bool isForward)
    {
        // BackLeft
        if (foot == Foot.Left && !isForward)
        {
            leftFootController.backPressed = false;
            leftFootController.justBackReleased = true;
            leftFootController.globalPressed = false;
        }
        // BackRight
        else if (foot == Foot.Right && !isForward)
        {
            rightFootController.backPressed = false;
            rightFootController.justBackReleased = true;
            rightFootController.globalPressed = false;
        }
        // ForwardLeft
        else if (foot == Foot.Left && isForward)
        {
            leftFootController.forwardPressed = false;
            leftFootController.justForwardReleased = true;
            leftFootController.globalPressed = false;
        }
        // ForwardRight
        else if (foot == Foot.Right && isForward)
        {
           rightFootController.forwardPressed = false;
           rightFootController.justForwardReleased = true;
           rightFootController.globalPressed = false;
        }
    }

    void Update()
    {
        leftFootController.ObstacleDataUpdate();
        rightFootController.ObstacleDataUpdate();

        Rotation();

        leftFootController.FootMovingToOriginalPos();
        rightFootController.FootMovingToOriginalPos();
        
        leftFootController.ProcessSteps();
        rightFootController.ProcessSteps();

        //Main Body
        MainBodyPositionUpdate();

        leftFootController.BoolAssignation();
        rightFootController.BoolAssignation();
    }
    
    // Appliquer les positions monde calculées aux transforms enfants après que le body ait bougé.
    // LateUpdate écrase l'effet de la parenté chaque frame.
    void LateUpdate()
    {
        leftFootController.LateUpdateInputAssignation();
        rightFootController.LateUpdateInputAssignation();

        leftFootController.LateUpdatePositionSet();
        rightFootController.LateUpdatePositionSet();

        CheckFeetToIKTargetDistance();
    }

    private void Rotation()
    {
        if (rightRotationInput || leftRotationInput)
        {
            bool leftOnGround = leftFootController.FootState.onGround;
            bool rightOnGround = rightFootController.FootState.onGround;
            float turnInput = 0;
            //Foot Placement 
            float turnSign = leftFootController.backPressed || rightFootController.backPressed ? -1 : 1;
            // footToTransformZ Sign depends on turnSign
            float footToTransformOnZ = rightFootController.FootState.onGround ? Mathf.Sign(Vector3.Dot(LeftLegIKTarget.position - transform.position, transform.forward * turnSign)) 
                                                     : Mathf.Sign(Vector3.Dot(RightLegIKTarget.position - transform.position, transform.forward * turnSign));
            
            // Obstacle check from the foot not on ground
            bool obstacleOnLeft = rightOnGround ? leftFootController.signedObsRightDir < 0 : rightFootController.signedObsRightDir < 0;
            bool obstacleOnRight = rightOnGround ? leftFootController.signedObsRightDir > 0 : rightFootController.signedObsRightDir > 0;

            if (rightRotationInput && !(obstacleOnLeft && footToTransformOnZ < 0)) turnInput += turnSign;
            if (leftRotationInput && !(obstacleOnRight && footToTransformOnZ < 0)) turnInput -= turnSign;

            // only rotate if one foot is on ground
            if (leftOnGround != rightOnGround)
            {
                // choose pivot: prefer the foot that is on ground while the other is not
                Vector3 pivot;
                if (leftOnGround) pivot = leftFootController.FootState.IKTarget.position;
                else  pivot = rightFootController.FootState.IKTarget.position;
                float angle = turnInput * rotationSpeed * Time.deltaTime;

                // rotate the player transform around pivot (world space)
                transform.RotateAround(pivot, Vector3.up, angle);

                // also rotate the stored origin positions so the baseline (origins) follows the rotation
                Vector3 leftNewPos = Quaternion.AngleAxis(angle, Vector3.up) * (leftFootController.FootState.IKTargetOrigin - pivot) + pivot;
                leftFootController.SetIKTargetOrigin(IKTargetPositionFinder(leftNewPos, Foot.Left));
                Vector3 rightNewPos = Quaternion.AngleAxis(angle, Vector3.up) * (rightFootController.FootState.IKTargetOrigin - pivot) + pivot;
                rightFootController.SetIKTargetOrigin(IKTargetPositionFinder(rightNewPos, Foot.Right));
            }
        }
    }

    private (Vector3,float) IKTargetPositionFinder(Vector3 oldPos, Foot foot)
    {
        if (Physics.OverlapSphere(oldPos, 0.1f, obstacleMask).Length == 0)
        {
            return (oldPos, 0);
        }
        // Find pos on ground under the foot
        RaycastHit raycastHit;
        if (foot == Foot.Left)
        {
            Physics.Raycast(LeftLegIKTarget.transform.position, Vector3.down, out  raycastHit, 10f, groundMask);
        }
        else
        {
            Physics.Raycast(RightLegIKTarget.transform.position, Vector3.down, out raycastHit, 10f, groundMask);
        }

        Vector3 groundPointUnderFoot = raycastHit.point;
        Vector3 rayDir = oldPos - groundPointUnderFoot;

        if (Physics.Raycast(groundPointUnderFoot, rayDir.normalized, out RaycastHit hit, rayDir.magnitude, obstacleMask))
        {
            float distDiffFromOldOriginePos = Vector3.Distance(oldPos, hit.point  - rayDir * 0.1f);
            return (hit.point  - rayDir * 0.1f, distDiffFromOldOriginePos);
        }
        else
        {
            return (oldPos, 0);
        }
    }

    private void MainBodyPositionUpdate()
    {
        Vector3 feetAverage;
        float bodyLerpSpeed;
        bool leftOnGround = leftFootController.FootState.onGround;
        bool rightOnGround = rightFootController.FootState.onGround;
        if (leftOnGround && !rightOnGround)
        {
            feetAverage = Vector3.Lerp(LeftLegIKTarget.position, RightLegIKTarget.position, bodyLerpBtwLegs);
            bodyLerpSpeed = 5f;
        }
        else if (!leftOnGround && rightOnGround)
        {
            feetAverage = Vector3.Lerp(RightLegIKTarget.position, LeftLegIKTarget.position, bodyLerpBtwLegs);
            bodyLerpSpeed = 5f;
        }
        else
        {
            feetAverage = (rightFootController.FootState.IKTargetOrigin + leftFootController.FootState.IKTargetOrigin) * 0.5f;
            bodyLerpSpeed = 5;
        }
        Vector3 targetPos = new Vector3(feetAverage.x, initialYBodyPos + bodyYTarget + bodyYOffset, feetAverage.z);
        // Lissage — ajuste le facteur si besoin
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * bodyLerpSpeed);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(leftFootController.FootState.IKTargetOrigin, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(rightFootController.FootState.IKTargetOrigin, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(leftFootController.hitForCorrection.point, 0.1f);
        Gizmos.DrawSphere(rightFootController.hitForCorrection.point, 0.1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(leftFootController.closestFootColPointToObstacle, 0.1f);
        Gizmos.DrawSphere(rightFootController.closestFootColPointToObstacle, 0.1f);
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawSphere(hitCorrection, 0.07f);
    }

    // Mesure la vitesse moyenne sur `measurementDistance` mètres et debug quand atteint.
    private void Update20mSpeedMeasurement()
    {
        Vector3 currentPos = transform.position;
        float step = Vector3.Distance(currentPos, lastPosForMeasurement);
        accumulatedDistanceForMeasurement += step;
        lastPosForMeasurement = currentPos;

        if (accumulatedDistanceForMeasurement >= measurementDistance)
        {
            float elapsed = Time.time - measurementStartTime;
            float speed = accumulatedDistanceForMeasurement / Mathf.Max(0.0001f, elapsed); // m/s
            if (debugSpeedMeasurement)
            {
                Debug.Log($"[MovementController] Avg speed over {accumulatedDistanceForMeasurement:F2} m = {speed:F2} m/s (time {elapsed:F2}s).");
            }
            // reset pour la prochaine mesure
            measurementStartTime = Time.time;
            accumulatedDistanceForMeasurement = 0f;
        }
    }

    // Check distance between feet and their IKTargets

    private void CheckFeetToIKTargetDistance()
    {
        float leftFootDist = Vector3.Distance(LeftLegIKTarget.position, leftFootTransform.position);
        float rightFootDist = Vector3.Distance(RightLegIKTarget.position, rightFootTransform.position);

        if (leftFootDist > maxAllowedFootToIKTargetDistance)
        {
            Debug.LogWarning($"[MovementController] Left foot is too far from IK Target: {leftFootDist:F2} m.");
        }

        if (rightFootDist > maxAllowedFootToIKTargetDistance)
        {
            Debug.LogWarning($"[MovementController] Right foot is too far from IK Target: {rightFootDist:F2} m.");
        }
    }
}