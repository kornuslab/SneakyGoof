using UnityEngine;
public class FootState
{
    public float currentStepLen;
    public float currentMaxLen;
    public float currentHeight;
    public bool onGround;
    public bool movingToOrigin;
    public Transform IKTarget;
    public Vector3 IKTargetOrigin;
    public Vector3 desiredPos;

    public FootState(Transform IKTarget)
    {
        this.IKTarget = IKTarget;
        IKTargetOrigin = IKTarget.position;
        onGround = true;
    }
}
