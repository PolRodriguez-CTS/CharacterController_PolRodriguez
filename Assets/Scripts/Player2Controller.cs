//using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player2Controller : MonoBehaviour
{
    //Componentes
    private CharacterController _controller;
    private Animator _animator;

    //Inputs
    private InputAction _moveAction;
    private Vector2 _moveInput;
    private InputAction _jumpAction;
    private InputAction _lookAction;
    private Vector2 _lookInput;

    [SerializeField] private float _movementSpeed = 5;
    [SerializeField] private float _jumpHeight = 2;
    [SerializeField] private float _smoothTime = 0.2f;
    //variable de referencia
    private float _turnSmoothVelocity;


    //Gravedad
    [SerializeField] private float _gravity = -10f;
    [SerializeField] private Vector3 _playerGravity;

    //Ground Sensor
    [SerializeField] private Transform _sensor;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] float _sensorRadius;

    private Transform _mainCamera;


    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        _moveAction = InputSystem.actions["Move"];
        _jumpAction = InputSystem.actions["Jump"];
        _lookAction = InputSystem.actions["Look"];

        _mainCamera = Camera.main.transform;
    }

    void Update()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();
        _lookInput = _lookAction.ReadValue<Vector2>();

        Gravity();

        Movement();

        if (_jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            Jump();
        }
    }

    void Attack()
    {
        Ray ray = Camera.main.ScreenPointToRay(_lookInput);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            IDamageable damageable = hit.transform.GetComponent<IDamageable>();
            
            if(damageable != null)
            {
                damageable.TakeDamage(5);
            }
        }
    }

    [SerializeField] float _speedChangeRate = 10;
    float speed;
    float _animationSpeed;
    bool isSprinting = false;
    float _sprintSpeed = 8;
    float targetAngle;


    void Movement()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);
        float targetSpeed;

        if (direction == Vector3.zero)
        {
            speed = 0;
            _animationSpeed = 0;
            _animator.SetFloat("Speed", 0);

            _controller.Move(_playerGravity * Time.deltaTime);
            return;
        }

        if(isSprinting)
        {
            targetSpeed = _sprintSpeed;
        }
        else
        {
            targetSpeed = _movementSpeed;
        }

        if(direction == Vector3.zero)
        {
            targetSpeed = 0;
        }

        float currentSpeed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;

        if(currentSpeed < targetSpeed - speedOffset || currentSpeed > targetSpeed + speedOffset)
        {
            speed = Mathf.Lerp(currentSpeed, targetSpeed, _speedChangeRate * Time.deltaTime);

            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = targetSpeed;
        }

        _animationSpeed = Mathf.Lerp(_animationSpeed, targetSpeed, _speedChangeRate * Time.deltaTime);

        if(_animationSpeed < 0.05f)
        {
            _animationSpeed = 0;
        }

        _animator.SetFloat("Speed", _animationSpeed);
        

        if (direction != Vector3.zero)
        {
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _smoothTime);

            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
        }

        Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

        _controller.Move(moveDirection.normalized * (speed * Time.deltaTime) + _playerGravity * Time.deltaTime);
    }

    void Jump()
    {
        _animator.SetBool("Jump", true);

        _playerGravity.y = Mathf.Sqrt(_jumpHeight * -2 * _gravity);
    }

    void Gravity()
    {
        _animator.SetBool("Grounded", IsGrounded());
        if(IsGrounded())
        {
            _animator.SetBool("Jump", false);
            _animator.SetBool("Fall", false);

            if(_playerGravity.y < 0)
            {
                _playerGravity.y = -2;
            }
        }
        
        else
        {
            _animator.SetBool("Fall", true);
            _playerGravity.y += _gravity * Time.deltaTime;
        }
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(_sensor.position, _sensorRadius, _groundLayer);
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_sensor.position, _sensorRadius);
    }
}