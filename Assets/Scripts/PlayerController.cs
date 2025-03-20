using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour, IInputListener
{
    public Transform CameraPivot;
    public Transform OrientationRoot;

    public float WalkForce = 10f;
    public float SprintForce = 10f;
    public float LookMultiplier = 1f;
    public float CameraVerticalRotationUpperLimit = 90f;
    public float CameraVerticalRotationLowerLimit = -90f;

    private Rigidbody _rb;

    private IPlayerInput _input;

    private float _cameraRotVertical = 0;

    private bool _sprinting = false;

    public void FeedInput(IPlayerInput input)
    {
        _input = input;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (_input == null) return;
        UpdateSprint(_input.GetSprint(), _input.GetMove());
        RotatePlayer();
    }

    private void FixedUpdate()
    {
        if (_input == null) return;

        MovePlayer(_input.GetMove(), GetMoveForce());
    }

    private void UpdateSprint(bool SprintInput, Vector2 MoveInput)
    {
        // log mmove vector
        Debug.Log(MoveInput);

        // handle sprinting 
        if (!_sprinting && SprintInput && MovingForwards(MoveInput))
        {
            _sprinting = true;
        }
        else if (_sprinting && (!SprintInput || !MovingForwards(MoveInput)))
        {
            _sprinting = false;
        }
    }

    private bool MovingForwards(Vector2 MoveInput)
    {
        return MoveInput.y > 0.9f && MoveInput.x < 0.5f;
    }

    private float GetMoveForce()
    {
        return _sprinting ? SprintForce : WalkForce;
    }

    private void MovePlayer(Vector2 MoveInput, float MoveForce)
    {
        if (MoveInput.magnitude > 0)
        {
            Vector3 moveDir = OrientationRoot.TransformVector(new Vector3(MoveInput.x, 0, MoveInput.y)).normalized;
            _rb.AddForce(moveDir * MoveForce, ForceMode.Force);
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
