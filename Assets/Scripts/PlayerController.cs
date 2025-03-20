using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterController : MonoBehaviour, IInputListener
{
    public Transform CameraPivot;
    public Transform OrientationRoot;

    [Header("Movement Speed")]
    public float WalkForce = 10f;
    public float SprintForce = 10f;
    public float JumpForce = 1000f;
    public float MoveForceMultiplierWhileInAir = 0.1f;

    [Tooltip("Extra downward force applied while in air. Imrpoves jump feel.")]
    public float ExtraGravityWhileInAir = 10f;

    [Header("Drag")]
    public float DragWhileOnGround = 5f;
    public float DragWhileInAir = 0f;

    [Header("Look Settings")]
    public float LookSensitivityMultiplier = 1f;
    public float CameraVerticalRotationUpperLimit = 90f;
    public float CameraVerticalRotationLowerLimit = -90f;

    public LayerMask GroundLayers;

    private Rigidbody _rb;

    private ICharacterInput _input;

    private float _cameraRotVertical = 0;

    private bool _sprinting = false;
    private bool _jumpPending = false;
    /// <summary>
    /// readonly, use IsGrounded() instead
    /// </summary>
    private bool _grounded = false;

    public void FeedInput(ICharacterInput input)
    {
        _input = input;
        if (_input.GetJump() && !_jumpPending && IsGrounded())
        {
            _jumpPending = true;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.linearDamping = DragWhileOnGround;
    }

    private void Update()
    {
        if (_input == null) return;
        UpdateDrag();
        UpdateSprint(_input.GetSprint(), _input.GetMove());
        RotatePlayer();
        _grounded = IsGrounded();
    }

    private void FixedUpdate()
    {
        if (_input == null) return;

        MovePlayer(_input.GetMove(), GetMoveForce());

        // handle jumping
        HandleJumping();

        // apply extra gravity while in air
        if (!IsGrounded())
        {
            _rb.AddForce(Vector3.down.normalized * ExtraGravityWhileInAir);
        }
    }

    private void HandleJumping()
    {
        if (_jumpPending && IsGrounded())
        {
            _rb.linearDamping = DragWhileInAir;
            _rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            _jumpPending = false;
        }
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

    private void UpdateDrag()
    {
        _rb.linearDamping = IsGrounded() ? DragWhileOnGround : DragWhileInAir;
    }

    private bool IsGrounded()
    {
        return Physics.OverlapSphereNonAlloc(
            transform.position,
            0.02f,
            new Collider[1],
            GroundLayers
        ) > 0;
    }

    private bool MovingForwards(Vector2 MoveInput)
    {
        return MoveInput.y > 0.9f && MoveInput.x < 0.5f;
    }

    private float GetMoveForce()
    {
        float force = _sprinting ? SprintForce : WalkForce;
        if (!_grounded)
        {
            force *= MoveForceMultiplierWhileInAir;
        }
        return force;
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
            OrientationRoot.Rotate(Vector3.up, look.x * LookSensitivityMultiplier);
        }

        // rotate eyes vertically
        if (CameraPivot != null)
        {
            _cameraRotVertical -= look.y * LookSensitivityMultiplier;
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
