using Unity.VisualScripting;
using UnityEngine;

public enum FollowMethod
{
    Update,
    LateUpdate,
    FixedUpdate
}

public class CameraFollow : MonoBehaviour
{
    public Transform Target;

    public FollowMethod PositionFollowMethod = FollowMethod.Update;
    public FollowMethod RotationFollowMethod = FollowMethod.Update;

    public bool SmoothRotation = false;
    public float RotationAlpha = 0.95f;
    public float RotationMultiplier = 1f;

    public bool SmoothPosition = false;
    public float PositionAlpha = 0.95f;
    public float PositionMultiplier = 1f;

    private void Update()
    {
        if (PositionFollowMethod == FollowMethod.Update)
        {
            FollowPosition();
        }
        if (RotationFollowMethod == FollowMethod.Update)
        {
            FollowRotation();
        }
    }
    private void LateUpdate()
    {
        if (PositionFollowMethod == FollowMethod.LateUpdate)
        {
            FollowPosition();
        }
        if (RotationFollowMethod == FollowMethod.LateUpdate)
        {
            FollowRotation();
        }
    }
    private void FixedUpdate()
    {
        if (PositionFollowMethod == FollowMethod.FixedUpdate)
        {
            FollowPosition();
        }
        if (RotationFollowMethod == FollowMethod.FixedUpdate)
        {
            FollowRotation();
        }
    }

    private void FollowRotation()
    {
        if (Target == null)
        {
            return;
        }

        var smoothRot = Quaternion.Slerp(
            transform.rotation,
            Target.rotation,
            RotationAlpha * Time.deltaTime * RotationMultiplier
            );

        if (SmoothRotation)
        {
            transform.rotation = smoothRot;
        }
        else
        {
            transform.rotation = Target.rotation;
        }
    }

    private void FollowPosition()
    {
        var smoothPos = Vector3.Lerp(
           transform.position,
           Target.position,
           PositionAlpha * Time.deltaTime * PositionMultiplier
           );
        if (SmoothPosition)
        {
            transform.position = smoothPos;
        }
        else
        {
            transform.position = Target.position;
        }
    }
}
