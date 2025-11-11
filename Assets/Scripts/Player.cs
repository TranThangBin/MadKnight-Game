using System;
using MadKnight.Enums;
using MadKnight.ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MadKnight
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
    public class Player : MonoBehaviour
    {
        private enum PlayerState
        {
            GroundWork,
            Crouching,
            Jumping,
            Airborne,
            WallBounce,
            WallClimb,
            ClimbOver,
            Dead,
        }

        [SerializeField] private PlayerStatsSO _stats;

        [SerializeField] private Collider2D _headCollider;
        [SerializeField] private Transform _ceilCheck;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private Transform _wallCheckRight;
        [SerializeField] private Transform _wallCheckLeft;
        [SerializeField] private Transform _ledgeCheckRight;
        [SerializeField] private Transform _ledgeCheckLeft;
        [SerializeField] private Vector2 _ledgeClimbOffset;
        [SerializeField] private float _climbOverAnimationTime;
        [SerializeField] private float _deadAnimationTime;

        private Rigidbody2D _rb;
        private Animator _anim;
        private SpriteRenderer _sr;

        private PlayerState _state;

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
        private bool _isOnLedgeRight;
        private bool _isOnLedgeLeft;
        private float _climbOverTimer;
        private bool _climbOverFinished;
        private bool _airControl;
        private float _maxClimbTimer;
        private float _deadTimer;

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

            _state = PlayerState.GroundWork;
            _jumpRemaining = _stats.MaxJumpCount;
            _climbOverTimer = _climbOverAnimationTime;

            const int restartDelay = 1;
            _deadTimer = _deadAnimationTime + restartDelay;

            var gScale = _rb.gravityScale;
            ResetGScale += () => _rb.gravityScale = gScale;
        }

        private void Update()
        {
            _horizontalAxis = Input.GetAxis("Horizontal");
            _verticalAxis = Input.GetAxis("Vertical");

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
            _anim.SetBool(
                    nameof(PlayerAnimationEnum.BIsClimbingOver),
                    _state == PlayerState.ClimbOver
            );
            _anim.SetBool(
                    nameof(PlayerAnimationEnum.BIsDead),
                    _state == PlayerState.Dead
            );

            var initState = _state;

            bool isJumpClicked,
                isWallBounceClicked,
                isCrouchClicked;

            var jumpAvailable = _jumpRemaining > 0;
            var isOnWall = _isOnWallRight || _isOnWallLeft;
            var isOnLedge = _isOnLedgeRight || _isOnLedgeLeft;

            isJumpClicked =
            isWallBounceClicked =
                Input.GetButtonDown("Jump");
            isCrouchClicked = Input.GetButton("Fire1");

            switch (_state)
            {
                case PlayerState.GroundWork:
                    {
                        _airControl = true;
                        _maxClimbTimer = _stats.MaxClimbTime;
                        _jumpRemaining = _stats.MaxJumpCount;

                        if (isOnWall && _maxClimbTimer > 0 && _verticalAxis != 0)
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
                        else if (isOnLedge)
                        {
                            _state = PlayerState.ClimbOver;
                        }
                    }
                    break;
                case PlayerState.Jumping:
                    {
                        if (!_isOnFloor && _hasJumped)
                        {
                            _state = PlayerState.Airborne;
                            _hasJumped = false;
                            _airControl = true;
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
                        else if (isOnWall && _maxClimbTimer > 0 && _verticalAxis != 0)
                        {
                            _state = PlayerState.WallClimb;
                        }
                        else if (_isOnFloor)
                        {
                            _state = PlayerState.GroundWork;
                        }
                        else if (isOnLedge)
                        {
                            _state = PlayerState.ClimbOver;
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
                            else if (isOnWall && _maxClimbTimer > 0 && _verticalAxis != 0)
                            {
                                _state = PlayerState.WallClimb;
                            }
                            else if (!isCrouchClicked)
                            {
                                _state = PlayerState.GroundWork;
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
                        _rb.gravityScale = 0;
                        _jumpRemaining = _stats.MaxJumpCount - 1;
                        if (_verticalAxis != 0)
                        {
                            _maxClimbTimer -= Time.deltaTime;
                        }

                        if (_isOnFloor && _horizontalAxis != 0)
                        {
                            _state = PlayerState.GroundWork;
                        }
                        else if (!_isOnWallLeft && !_isOnWallRight && !_isOnFloor)
                        {
                            _state = PlayerState.Airborne;
                        }
                        else if (isOnLedge)
                        {
                            _state = PlayerState.ClimbOver;
                        }
                        else if (isWallBounceClicked)
                        {
                            _state = PlayerState.WallBounce;
                        }
                        else if (_maxClimbTimer <= 0)
                        {
                            _state = PlayerState.Airborne;
                        }

                        if (_state != PlayerState.WallClimb)
                        {
                            ResetGScale.Invoke();
                        }
                    }
                    break;
                case PlayerState.ClimbOver:
                    {
                        _rb.bodyType = RigidbodyType2D.Kinematic;
                        _rb.linearVelocity = Vector2.zero;

                        _climbOverTimer -= Time.deltaTime;
                        if (_climbOverFinished && !isOnLedge)
                        {
                            _climbOverTimer = _climbOverAnimationTime;
                            _climbOverFinished = false;
                            _state = PlayerState.GroundWork;
                            _rb.bodyType = RigidbodyType2D.Dynamic;
                        }
                    }
                    break;
                case PlayerState.WallBounce:
                    {
                        if (_hasWallBounced && !isOnWall)
                        {
                            _state = PlayerState.Airborne;
                            _maxClimbTimer = _stats.MaxClimbTime;
                            _airControl = false;
                            _hasWallBounced = false;
                            _anim.SetTrigger(nameof(PlayerAnimationEnum.TJump));
                        }
                    }
                    break;
                case PlayerState.Dead:
                    {
                        _deadTimer -= Time.deltaTime;
                        if (_deadTimer <= 0)
                        {
                            var scene = SceneManager.GetActiveScene();
                            SceneManager.LoadScene(scene.name);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FixedUpdate()
        {
            _isOnFloor = Physics2D.OverlapCircle(
                    _groundCheck.position,
                    0.2f,
                    LayerMask.GetMask(nameof(LayerMaskEnum.Ground), nameof(LayerMaskEnum.Wall))
            );
            _isOnCeil = Physics2D.OverlapCircle(
                    _ceilCheck.position,
                    0.2f,
                    LayerMask.GetMask(nameof(LayerMaskEnum.Ground), nameof(LayerMaskEnum.Wall))
            );

            if (_direction == 1)
            {
                _isOnWallLeft = false;
                _isOnLedgeLeft = false;
                _isOnWallRight = Physics2D.OverlapCircle(
                        _wallCheckRight.position,
                        0.2f,
                        LayerMask.GetMask(nameof(LayerMaskEnum.Wall))
                );

                _isOnLedgeRight = !Physics2D.OverlapCircle(
                        new Vector2(
                            _ledgeCheckRight.position.x,
                            _ledgeCheckRight.position.y + 0.3f
                        ),
                        0.2f,
                        LayerMask.GetMask(nameof(LayerMaskEnum.Ground), nameof(LayerMaskEnum.Wall))
                ) && Physics2D.OverlapCircle(
                        _ledgeCheckRight.position,
                        0.2f,
                        LayerMask.GetMask(nameof(LayerMaskEnum.Ground), nameof(LayerMaskEnum.Wall))
                );
            }
            else if (_direction == -1)
            {
                _isOnWallRight = false;
                _isOnLedgeRight = false;
                _isOnWallLeft = Physics2D.OverlapCircle(
                        _wallCheckLeft.position,
                        0.2f,
                        LayerMask.GetMask(nameof(LayerMaskEnum.Wall))
                );

                _isOnLedgeLeft = !Physics2D.OverlapCircle(
                        new Vector2(
                            _ledgeCheckLeft.position.x,
                            _ledgeCheckLeft.position.y + 0.3f
                        ),
                        0.2f,
                        LayerMask.GetMask(nameof(LayerMaskEnum.Ground), nameof(LayerMaskEnum.Wall))
                ) && Physics2D.OverlapCircle(
                        _ledgeCheckLeft.position,
                        0.2f,
                        LayerMask.GetMask(nameof(LayerMaskEnum.Ground), nameof(LayerMaskEnum.Wall))
                );
            }

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
                case PlayerState.GroundWork:
                case PlayerState.Crouching:
                    {
                        if (_state == PlayerState.Crouching)
                        {
                            horizontalXVelocity *= _stats.CrouchSpeedMultiplier;
                        }

                        if (_airControl)
                        {
                            if (horizontalXVelocity > 0 && _direction != 1)
                            {
                                _direction = 1;
                            }
                            else if (horizontalXVelocity < 0 && _direction != -1)
                            {
                                _direction = -1;
                            }
                        }

                        if (horizontalXVelocity != 0 && _state == PlayerState.Airborne && _airControl)
                        {
                            _rb.linearVelocity = new Vector2(
                                Mathf.Lerp(
                                    _rb.linearVelocityX,
                                    horizontalXVelocity,
                                    movementSmoothing
                                ),
                                _rb.linearVelocityY
                            );
                        }

                        if (horizontalXVelocity != 0 &&
                            (
                                _state != PlayerState.Airborne ||
                                _state == PlayerState.Airborne && _airControl
                            ))
                        {
                            _rb.linearVelocity = new Vector2(
                                Mathf.Lerp(
                                    _rb.linearVelocityX,
                                    horizontalXVelocity,
                                    movementSmoothing
                                ),
                                _rb.linearVelocityY
                            );
                        }
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
                case PlayerState.ClimbOver:
                    {
                        if (!_climbOverFinished && _climbOverTimer <= 0)
                        {
                            _climbOverFinished = true;
                            transform.position = new Vector2(
                                    transform.position.x + _ledgeClimbOffset.x * _direction,
                                    transform.position.y + _ledgeClimbOffset.y
                            );
                        }
                    }
                    break;
                case PlayerState.WallBounce:
                    {
                        if (!_hasWallBounced)
                        {
                            _rb.linearVelocity = Vector2.zero;
                            _rb.AddForce(new Vector2(
                                        _stats.WallBounceForce * -_direction,
                                        wallBounceYForce
                            ), ForceMode2D.Impulse);
                            _direction *= -1;
                            _hasWallBounced = true;
                        }
                    }
                    break;
                case PlayerState.Dead:
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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (LayerMask.NameToLayer(nameof(LayerMaskEnum.Kill)) == collision.gameObject.layer)
            {
                _state = PlayerState.Dead;
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
