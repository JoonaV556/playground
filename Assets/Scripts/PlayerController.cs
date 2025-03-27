using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

namespace RigidBodyChracterController
{
    [RequireComponent(typeof(Rigidbody))]
    public class RBCharacterController : MonoBehaviour, IInputListener
    {
        // Usage:
        // - feed player input to the controller continuously - ideally each frame in Update()

        public Transform CameraPivot;
        public Transform OrientationRoot;

        [Header("Movement Speed")]
        public float WalkForce = 10f;
        public float SprintForce = 10f;
        public float JumpForce = 1000f;
        public float MoveForceMultiplierWhileInAir = 0.1f;

        [Tooltip("How fast the character crouches etc.")]
        [Range(0.1f, 10f)]
        public float CrouchSpeed = 0.5f;

        [Tooltip("Extra downward force applied while in air. Imrpoves jump feel.")]
        public float ExtraGravityWhileInAir = 10f;

        [Header("Drag")]
        public float DragWhileOnGround = 5f;
        public float DragWhileInAir = 0f;

        [Header("Look Settings")]
        public float LookSensitivityMultiplier = 1f;
        public float CameraVerticalRotationUpperLimit = 90f;
        public float CameraVerticalRotationLowerLimit = -90f;

        public float GroundCheckRayLength = 0.1f;

        public LayerMask GroundLayers;
        [Tooltip(" Layers that block leaning")]
        public LayerMask LeanObstacleLayers;
        public LayerMask UncrouchObstacleLayers;

        [Header("Crouch Settings")]
        public Transform BodyVisualTransform;
        public Transform HeadPivotTransform;
        public CapsuleCollider CapsuleCollider;
        public Vector2 CrouchScalesBodyVisual = new Vector2(1, 0.5f);
        public Vector2 CrouchHeightsCapsuleCollider = new Vector2(2f, 1f);
        public Vector2 CrouchCapsuleColliderYCenter = new Vector2(1f, 0.5f);
        public Vector2 CrouchHeadPivotYPosition = new Vector2(1.5f, 0.5f);

        [Header("Ground check pivots")]
        public Transform[] GroundCheckSpherePivotTransforms;
        public Transform[] RayGroundCheckPivotTransforms;

        private Rigidbody _rb;
        private Transform _capsuleColliderTransform;

        private ICharacterInput _input;

        private float _cameraRotVertical = 0;
        /// <summary>
        /// 0 = standing, 1 = crouching
        /// </summary>
        private float _crouchAlpha = 0f;

        private bool _sprinting = false;
        private bool _jumpPending = false;
        /// <summary>
        /// readonly, use IsGrounded() instead
        /// </summary>
        private bool _grounded = false;

        /// <summary>
        /// Transform, SphereRadius
        /// </summary>
        /// <returns></returns>
        private Dictionary<Transform, float> _groundCheckSpheres = new();

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
            _capsuleColliderTransform = CapsuleCollider.transform;
        }

        private void Start()
        {
            try
            {
                // get ground check sphere pivots
                _groundCheckSpheres = GetGroundCheckSpherePivots(GroundCheckSpherePivotTransforms);
            }
            catch (System.Exception e)
            {
                Debug.Log("Error while setting up ground check spheres");
                Debug.LogException(e);
            }
        }

        private void Update()
        {
            if (_input == null) return;
            _grounded = IsGrounded();
            UpdateDrag();
            UpdateSprint(_input.GetSprint(), _input.GetMove());
            RotatePlayer();
            UpdateCrouch(_input.GetCrouch());
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

        private Dictionary<Transform, float> GetGroundCheckSpherePivots(Transform[] gcsPivots)
        {
            Dictionary<Transform, float> gcs = new();
            foreach (var pivot in GroundCheckSpherePivotTransforms)
            {
                if (pivot.TryGetComponent(out SphereCollider c))
                {
                    // save pivot and radius
                    gcs.Add(
                        pivot,
                        c.radius
                        );
                    // remove collider
                    Destroy(c);
                }
            }
            return gcs;
        }

        /// <summary>
        /// if crouch input is true, character will attempt to crouch or stay crouched. if false character will attempt to stand back up
        /// </summary>
        /// <param name="CrouchInput">Whether or not character is attempting to enter crouch or stay in crouched</param>
        private void UpdateCrouch(bool CrouchInput)
        {
            // update crouch progress
            if (CrouchInput)
            {
                // enter crouch / stay crouched
                _crouchAlpha += Time.deltaTime * CrouchSpeed;

            }
            else
            {
                // exit crouch / stay standing
                float _crouchAlphaBeforeObstacleCheck = _crouchAlpha;
                float newCrouchAlpha = _crouchAlpha - Time.deltaTime * CrouchSpeed;

                // check if there is an obstacle above the player
                Vector3 headPivotPositionBeforeObstacleCheck = HeadPivotTransform.localPosition;
                // temporarily raise head pivot to check for obstacles
                HeadPivotTransform.localPosition = new Vector3(
                    HeadPivotTransform.localPosition.x,
                    Mathf.Lerp(
                        CrouchHeadPivotYPosition.x,
                        CrouchHeadPivotYPosition.y,
                        newCrouchAlpha
                    ),
                    HeadPivotTransform.localPosition.z
                    );

                bool isObstacleAbove = Physics.CheckSphere(
                    HeadPivotTransform.position,
                    0.5f,
                    LeanObstacleLayers
                );

                if (!isObstacleAbove)
                {
                    // if no obstacle above, allow uncrouch
                    _crouchAlpha = newCrouchAlpha;
                }
                else
                {
                    // if there is an obstacle above, prevent uncrouch
                    _crouchAlpha = _crouchAlphaBeforeObstacleCheck;
                    HeadPivotTransform.localPosition = headPivotPositionBeforeObstacleCheck;
                }
            }
            _crouchAlpha = Mathf.Clamp(_crouchAlpha, 0f, 1f);

            // finally do necessary adjustments to player object
            ApplyCrouchChanges(_crouchAlpha);
        }

        private void ApplyCrouchChanges(float crouchAlpha)
        {
            BodyVisualTransform.localScale = Vector3.Lerp(
                            new Vector3(BodyVisualTransform.localScale.x, CrouchScalesBodyVisual.x, BodyVisualTransform.localScale.z),
                            new Vector3(BodyVisualTransform.localScale.x, CrouchScalesBodyVisual.y, BodyVisualTransform.localScale.z),
                            crouchAlpha
                        );
            CapsuleCollider.height = Mathf.Lerp(
                CrouchHeightsCapsuleCollider.x,
                CrouchHeightsCapsuleCollider.y,
                crouchAlpha
            );
            CapsuleCollider.center = new Vector3(
                CapsuleCollider.center.x,
                Mathf.Lerp(
                    CrouchCapsuleColliderYCenter.x,
                    CrouchCapsuleColliderYCenter.y,
                    crouchAlpha
                ),
                CapsuleCollider.center.z
            );
            HeadPivotTransform.localPosition = new Vector3(
                HeadPivotTransform.localPosition.x,
                Mathf.Lerp(
                    CrouchHeadPivotYPosition.x,
                    CrouchHeadPivotYPosition.y,
                    crouchAlpha
                ),
                HeadPivotTransform.localPosition.z
            );
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
            bool spheresHit = false;
            bool raysHit = false;
            // check if we are grounded by doing multiple OverlapSphere checks below player capsule
            foreach (var (pivot, radius) in _groundCheckSpheres)
            {
                if (Physics.OverlapSphereNonAlloc(
                    pivot.position,
                    radius,
                    new Collider[1],
                    GroundLayers
                ) > 0)
                {
                    spheresHit = true;
                    break;
                }
            }

            Ray ray = new();
            ray.direction = Vector3.down;
            RaycastHit[] hits = new RaycastHit[1];

            // check if we are grounded by doing multiple Raycasts below player capsule
            foreach (var pivot in RayGroundCheckPivotTransforms)
            {
                ray.origin = pivot.position;
                if (Physics.RaycastNonAlloc(
                    ray,
                    hits,
                    GroundCheckRayLength,
                    GroundLayers
                ) > 0)
                {
                    raysHit = true;
                    break;
                }
            }

            return spheresHit || raysHit;
        }

        /// <summary>
        /// Checks if character is attempting to move forwards according to input vector
        /// </summary>
        /// <param name="MoveInput"></param>
        /// <returns></returns>
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
}
