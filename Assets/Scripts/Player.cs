using System;
using MadKnight.Enums;
using MadKnight.ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;

namespace MadKnight
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
    public class Player : MonoBehaviour
    {
        private enum PlayerState
        {
            Idle,
            Walking,
            Crouching,
            Jumping,
            Airborne,
            WallSlide,
            WallBounce,
            WallClimb,
        }

        [SerializeField] private PlayerStatsSO _stats;

        [SerializeField] private Collider2D _headCollider;
        [SerializeField] private Transform _ceilCheck;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private Transform _wallCheckRight;
        [SerializeField] private Transform _wallCheckLeft;

        private Rigidbody2D _rb;
        private Animator _anim;
        private SpriteRenderer _sr;

        private PlayerState _state;
        private PlayerState _prevState;

        private float _horizontalAxis;
        private float _verticalAxis;
        private int _direction;
        private int _jumpRemaining;
        private bool _hasJumped;
        private bool _hasWallBounced;
        private bool _isOnCeil;
        private bool _isOnFloor;
        private bool _isOnWallRight;
        private bool _isOnWallLeft;

        private event UnityAction ResetGScale;

        private void OnDestroy()
        {
            ResetGScale = null;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _anim = GetComponent<Animator>();
            _sr = GetComponent<SpriteRenderer>();

            _state = PlayerState.Idle;
            _jumpRemaining = _stats.MaxJumpCount;

            var gScale = _rb.gravityScale;
            ResetGScale += () => _rb.gravityScale = gScale;
        }

        private void Update()
        {
            _horizontalAxis = Input.GetAxis("Horizontal");
            _verticalAxis = Input.GetAxis("Vertical");

            _anim.SetFloat(
                    nameof(PlayerAnimationEnum.FHorizontalVelocity),
                    _rb.linearVelocityX
            );
            _anim.SetFloat(
                    nameof(PlayerAnimationEnum.FVerticalVelocity),
                    _rb.linearVelocityY
            );
            _anim.SetBool(
                    nameof(PlayerAnimationEnum.BIsOnFloor),
                    _isOnFloor
            );
            _anim.SetBool(
                    nameof(PlayerAnimationEnum.BIsCrawling),
                    _state == PlayerState.Crouching
            );
            _anim.SetBool(
                    nameof(PlayerAnimationEnum.BIsClimbing),
                    _state == PlayerState.WallClimb
            );

            if (_direction > 0 && _sr.flipX)
            {
                _sr.flipX = false;
            }
            else if (_direction < 0 && !_sr.flipX)
            {
                _sr.flipX = true;
            }

            HandleStateTransition();
        }

        private void HandleStateTransition()
        {
            var initState = _state;

            bool isJumpClicked,
                isWallBounceClicked,
                isCrouchClicked;

            var jumpAvailable = _jumpRemaining > 0;
            var isFacingWall = _direction == 1 && _isOnWallRight ||
                _direction == -1 && _isOnWallLeft;

            isJumpClicked =
            isWallBounceClicked =
                Input.GetButtonDown("Jump");
            isCrouchClicked = Input.GetButton("Fire1");

            switch (_state)
            {
                case PlayerState.Idle:
                    {
                        if (_horizontalAxis != 0)
                        {
                            _state = PlayerState.Walking;
                        }
                        else if (isFacingWall && _verticalAxis != 0)
                        {
                            _state = PlayerState.WallClimb;
                        }
                        else if (_isOnFloor && isCrouchClicked)
                        {
                            _state = PlayerState.Crouching;
                        }
                        else if (isJumpClicked && jumpAvailable)
                        {
                            _state = PlayerState.Jumping;
                        }
                        else if (!_isOnFloor)
                        {
                            _state = PlayerState.Airborne;
                        }
                    }
                    break;
                case PlayerState.Walking:
                    {
                        if (_rb.linearVelocityX == 0)
                        {
                            _state = PlayerState.Idle;
                        }
                        else if (isFacingWall && _verticalAxis != 0)
                        {
                            _state = PlayerState.WallClimb;
                        }
                        else if (_isOnFloor && isCrouchClicked)
                        {
                            _state = PlayerState.Crouching;
                        }
                        else if (isJumpClicked && jumpAvailable)
                        {
                            _state = PlayerState.Jumping;
                        }
                        else if (!_isOnFloor)
                        {
                            _state = PlayerState.Airborne;
                        }
                    }
                    break;
                case PlayerState.Jumping:
                    {
                        if (!_isOnFloor && _hasJumped)
                        {
                            _state = PlayerState.Airborne;
                            _hasJumped = false;
                            _anim.SetTrigger(nameof(PlayerAnimationEnum.TJump));
                        }
                    }
                    break;
                case PlayerState.Airborne:
                    {
                        if (isJumpClicked && jumpAvailable)
                        {
                            _state = PlayerState.Jumping;
                        }
                        else if (isFacingWall && _verticalAxis != 0)
                        {
                            _state = PlayerState.WallClimb;
                        }
                        else if (_isOnFloor)
                        {
                            if (_rb.linearVelocityX == 0)
                            {
                                _state = PlayerState.Idle;
                            }
                            else
                            {
                                _state = PlayerState.Walking;
                            }

                            _jumpRemaining = _stats.MaxJumpCount;
                        }
                    }
                    break;
                case PlayerState.Crouching:
                    {
                        if (!_isOnCeil)
                        {
                            if (!_isOnFloor)
                            {
                                _state = PlayerState.Airborne;
                            }
                            else if (isJumpClicked && jumpAvailable)
                            {
                                _state = PlayerState.Jumping;
                            }
                            else if (isFacingWall && _verticalAxis != 0)
                            {
                                _state = PlayerState.WallClimb;
                            }
                            else if (!isCrouchClicked)
                            {
                                if (_rb.linearVelocityX == 0)
                                {
                                    _state = PlayerState.Idle;
                                }
                                else
                                {
                                    _state = PlayerState.Walking;
                                }
                            }
                        }

                        if (_state == PlayerState.Crouching)
                        {
                            _headCollider.enabled = false;
                        }
                        else
                        {
                            _headCollider.enabled = true;
                        }
                    }
                    break;
                case PlayerState.WallClimb:
                    {
                        if (_rb.gravityScale != 0)
                        {
                            _rb.gravityScale = 0;
                        }

                        if (_isOnFloor && _horizontalAxis != 0)
                        {
                            _state = PlayerState.Walking;
                        }
                        else if (!_isOnWallLeft && !_isOnWallRight && !_isOnFloor)
                        {
                            _state = PlayerState.Airborne;
                        }

                        if (_state != PlayerState.WallClimb)
                        {
                            ResetGScale.Invoke();
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (initState != _state)
            {
                _prevState = initState;
            }
        }

        private void FixedUpdate()
        {
            _isOnFloor = Physics2D.OverlapCircle(
                    _groundCheck.position,
                    0.2f,
                    LayerMask.GetMask(nameof(LayerMaskEnum.Ground))
            );
            _isOnCeil = Physics2D.OverlapCircle(
                    _ceilCheck.position,
                    0.2f,
                    LayerMask.GetMask(nameof(LayerMaskEnum.Ground))
            );
            _isOnWallRight = Physics2D.OverlapCircle(
                    _wallCheckRight.position,
                    0.2f,
                    LayerMask.GetMask(nameof(LayerMaskEnum.Ground))
            );
            _isOnWallLeft = Physics2D.OverlapCircle(
                    _wallCheckLeft.position,
                    0.2f,
                    LayerMask.GetMask(nameof(LayerMaskEnum.Ground))
            );

            HandleState();
        }

        private void HandleState()
        {
            var movementSmoothing = 0.1f;
            var wallBounceYForce = _stats.JumpForce * 1.2f;

            var horizontalXVelocity = _stats.Speed * _horizontalAxis;
            var verticalYVelocity = _stats.Speed * _stats.WallClimbSpeedMultiplier * _verticalAxis;

            switch (_state)
            {
                case PlayerState.Jumping:
                    {
                        if (!_hasJumped)
                        {
                            _rb.linearVelocity = new Vector2(_rb.linearVelocityX, 0);
                            _rb.AddForceY(_stats.JumpForce, ForceMode2D.Impulse);
                            _jumpRemaining--;
                            _hasJumped = true;
                        }
                    }
                    break;
                case PlayerState.Airborne:
                case PlayerState.Walking:
                case PlayerState.Crouching:
                    {
                        if (_state == PlayerState.Crouching)
                        {
                            horizontalXVelocity *= _stats.CrouchSpeedMultiplier;
                        }

                        if (horizontalXVelocity > 0 && _direction != 1)
                        {
                            _direction = 1;
                        }
                        else if (horizontalXVelocity < 0 && _direction != -1)
                        {
                            _direction = -1;
                        }


                        _rb.linearVelocity = new Vector2(
                            Mathf.Lerp(
                                _rb.linearVelocityX,
                                horizontalXVelocity,
                                movementSmoothing
                            ),
                            _rb.linearVelocityY
                        );
                    }
                    break;
                case PlayerState.WallClimb:
                    {
                        _rb.linearVelocity = new Vector2(
                            _rb.linearVelocityX,
                            verticalYVelocity
                        );
                    }
                    break;
                case PlayerState.Idle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LateUpdate()
        {
            if (Camera.main)
            {
                Camera.main.transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y,
                    Camera.main.transform.position.z
                );
            }
        }

        #region Save And Load
        public void Save(ref NPlayerSaveData data)
        {
            data.Position = transform.position;
            Debug.Log($"[Player] Saving position: {data.Position}");
        }
        public void Load(NPlayerSaveData data)
        {
            Debug.Log($"[Player] Loading position: {data.Position} (current: {transform.position})");

            // Set position trực tiếp
            transform.position = data.Position;

            // Nếu có Rigidbody2D, reset velocity và set position qua Rigidbody
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.position = data.Position;
                Debug.Log($"[Player] Reset Rigidbody velocity and position");
            }

            Debug.Log($"[Player] Position after load: {transform.position}");
        }
        #endregion
    }
}

/// <summary>
/// Simple Player Save Data for NSaveSystem
/// </summary>
[Serializable]
public struct NPlayerSaveData
{
    public Vector3 Position;
}
