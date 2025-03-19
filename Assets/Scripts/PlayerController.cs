using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour, IInputListener
{
    public Transform CameraPivot;
    public Transform OrientationRoot;

    public float WalkForce = 10f;
    public float LookMultiplier = 1f;
    public float CameraVerticalRotationUpperLimit = 90f;
    public float CameraVerticalRotationLowerLimit = -90f;

    private Rigidbody _rb;

    private IPlayerInput _input;

    private float _cameraRotVertical = 0;

    public void FeedInput(IPlayerInput input)
    {
        _input = input;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_input == null) return;

        MovePlayer();
    }

    private void Update()
    {
        if (_input == null) return;
        RotatePlayer();
    }

    private void MovePlayer()
    {
        // move player
        Vector2 move = _input.GetMove();
        if (move.magnitude > 0)
        {
            Vector3 moveDir = OrientationRoot.TransformVector(new Vector3(move.x, 0, move.y)).normalized;
            _rb.AddForce(moveDir * WalkForce, ForceMode.Force);
        }
    }

    private void RotatePlayer()
    {
        // rotate player horizontally
        Vector2 look = _input.GetLook();
        if (look.magnitude > 0)
        {
            OrientationRoot.Rotate(Vector3.up, look.x * LookMultiplier);
        }

        // rotate eyes vertically
        if (CameraPivot != null)
        {
            _cameraRotVertical -= look.y * LookMultiplier;
            var newCameraRot = new Vector3(
                Mathf.Clamp(
                    _cameraRotVertical,
                    CameraVerticalRotationLowerLimit,
                    CameraVerticalRotationUpperLimit
                ),
                0f,
                0f
            );
            CameraPivot.localRotation = Quaternion.Euler(newCameraRot);
        }
    }
}
