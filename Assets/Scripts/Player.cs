using System;
using MadKnight.Enums;
using MadKnight.ScriptableObjects;
using UnityEngine;

namespace MadKnight
{
    [RequireComponent(typeof(Rigidbody2D))]
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
        }

        [SerializeField] private PlayerStatsSO _stats;

        [SerializeField] private Collider2D _headCollider;
        [SerializeField] private SpriteRenderer _srNormal;
        [SerializeField] private SpriteRenderer _srCrouch;
        [SerializeField] private Transform _ceilCheck;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private Transform _wallCheck;

        private Rigidbody2D _rb;

        private PlayerState _state;

        private float _horizontalVelocity;
        private int _jumpRemaining;
        private bool _hasJumped;
        private bool _isOnCeil;
        private bool _isOnFloor;
        private bool _isOnWall;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();

            _state = PlayerState.Idle;
            _jumpRemaining = _stats.MaxJumpCount;
        }

        private void Update()
        {
            _horizontalVelocity = _stats.Speed * Input.GetAxis("Horizontal");

            if (_horizontalVelocity > 0 && transform.localScale.x != 1)
            {
                transform.localScale = new Vector2(1, transform.localScale.y);
            }
            else if (_horizontalVelocity < 0 && transform.localScale.x != -1)
            {
                transform.localScale = new Vector2(-1, transform.localScale.y);
            }


            Camera.main.transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y,
                    Camera.main.transform.position.z
            );

            HandleStateTransition();
            HandleAnimation();
        }

        private void HandleStateTransition()
        {
            switch (_state)
            {
                case PlayerState.Idle:
                    {
                        if (_horizontalVelocity != 0)
                        {
                            _state = PlayerState.Walking;
                        }
                        else if (CrouchCondition())
                        {
                            _state = PlayerState.Crouching;
                        }
                        else if (JumpCondition())
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
                        if (_horizontalVelocity == 0)
                        {
                            _state = PlayerState.Idle;
                        }
                        else if (CrouchCondition())
                        {
                            _state = PlayerState.Crouching;
                        }
                        else if (JumpCondition())
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
                        }
                    }
                    break;
                case PlayerState.Airborne:
                    {
                        if (JumpCondition())
                        {
                            _state = PlayerState.Jumping;
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
                        else if (_isOnWall)
                        {
                            _state = PlayerState.WallSlide;
                        }
                    }
                    break;
                case PlayerState.Crouching:
                    if (!_isOnCeil && !CrouchCondition())
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
                    else if (!_isOnCeil && JumpCondition())
                    {
                        _state = PlayerState.Jumping;
                    }

                    if (_state == PlayerState.Crouching)
                    {
                        _headCollider.enabled = false;
                    }
                    else
                    {
                        _headCollider.enabled = true;
                    }

                    break;
                case PlayerState.WallSlide:
                    {
                        if (_isOnFloor)
                        {
                            _state = PlayerState.Idle;
                            _jumpRemaining = _stats.MaxJumpCount;
                        }
                        else if (!_isOnWall)
                        {
                            _state = PlayerState.Airborne;
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleAnimation()
        {
            switch (_state)
            {
                case PlayerState.Crouching:
                    _srNormal.gameObject.SetActive(false);
                    _srCrouch.gameObject.SetActive(true);
                    break;
                case PlayerState.Idle:
                case PlayerState.Walking:
                case PlayerState.Jumping:
                case PlayerState.Airborne:
                case PlayerState.WallSlide:
                    _srNormal.gameObject.SetActive(true);
                    _srCrouch.gameObject.SetActive(false);
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
                    LayerMask.GetMask(nameof(LayerMaskEnum.Ground))
            );
            _isOnCeil = Physics2D.OverlapCircle(
                    _ceilCheck.position,
                    0.2f,
                    LayerMask.GetMask(nameof(LayerMaskEnum.Ground))
            );
            _isOnWall = Physics2D.OverlapCircle(
                    _wallCheck.position,
                    0.2f,
                    LayerMask.GetMask(nameof(LayerMaskEnum.Ground))
            );

            HandleState();
        }

        private void HandleState()
        {
            switch (_state)
            {
                case PlayerState.Jumping:
                    {
                        if (!_hasJumped)
                        {
                            _rb.linearVelocity = new Vector2(_rb.linearVelocityX, 0);
                            _rb.AddForce(Vector2.up * _stats.JumpForce, ForceMode2D.Impulse);
                            _jumpRemaining--;
                            _hasJumped = true;
                        }
                    }
                    break;
                case PlayerState.Airborne:
                case PlayerState.Walking:
                    {
                        if (_horizontalVelocity != 0)
                        {
                            _rb.linearVelocity = new Vector2(
                                    _horizontalVelocity,
                                    _rb.linearVelocityY
                            );
                        }
                    }
                    break;
                case PlayerState.Crouching:
                    {
                        _rb.linearVelocity = new Vector2(
                                _horizontalVelocity * _stats.CrouchSpeedMultiplier,
                                _rb.linearVelocityY
                        );
                    }
                    break;
                case PlayerState.WallSlide:
                    {
                        _rb.linearVelocity = new Vector2(
                                0,
                                Mathf.Max(
                                    _rb.linearVelocityY,
                                    -_stats.WallSlideMaxSpeed
                                )
                        );
                    }
                    break;
                case PlayerState.Idle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool JumpCondition()
        {
            return _jumpRemaining > 0 && Input.GetButtonDown("Jump");
        }

        private bool CrouchCondition()
        {
            return _isOnFloor && Input.GetButton("Fire1");
        }
    }
}
