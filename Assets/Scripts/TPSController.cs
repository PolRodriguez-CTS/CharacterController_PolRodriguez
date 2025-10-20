using UnityEngine;
using UnityEngine.InputSystem;


public class TPSController : MonoBehaviour
{
    //Componentes
    private CharacterController _controller;
    private Animator _animator;

    //Inputs
    private InputAction _moveAction;
    private Vector2 _moveInput;
    private InputAction _jumpAction;
    private InputAction _aimAction;
    private InputAction _lookAction;
    [SerializeField] private Vector2 _lookInput;

    //Camara
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _cameraSensitivity = 20;
    [SerializeField] Transform _lookAtCamera;
    float xRotation;

    //Gravedad
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private Vector3 _playerGravity;

    //Movimiento
    [SerializeField] private float _movementSpeed = 5;
    [SerializeField] private float _jumpHeight = 2;

    //Ground Sensor
    [SerializeField] private Transform _sensor;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] float _sensorRadius;

    
    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        //_mainCamera = GetComponent

        _moveAction = InputSystem.actions["Move"];
        _lookAction = InputSystem.actions["Look"];
        _jumpAction = InputSystem.actions["Jump"];
        _aimAction = InputSystem.actions["Aim"];
    }

    void Update()
    { 
        Movement();
        _lookInput = _lookAction.ReadValue<Vector2>();
        _moveInput = _moveAction.ReadValue<Vector2>();
        
       

        if (_jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            Jump();
        }

        Gravity();
    }

    void Movement()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);

        float mouseX = _lookInput.x * _cameraSensitivity * Time.deltaTime;
        float mouseY = _lookInput.y * _cameraSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        _animator.SetFloat("Horizontal", _moveInput.x);
        _animator.SetFloat("Vertical", _moveInput.y);

        transform.Rotate(Vector3.up, mouseX);
        _lookAtCamera.localRotation = Quaternion.Euler(xRotation, 0, 0);
        //_lookAtCamera.Rotate(Vector3.right, mouseY);

        if(direction != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg /*+ _mainCamera.eulerAngles.y*/;
            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

            _controller.Move(moveDirection * _movementSpeed * Time.deltaTime);
        }
    }


    void Jump()
    {
        _animator.SetBool("IsJumping", true);

        _playerGravity.y = Mathf.Sqrt(_jumpHeight * -2 * _gravity);

        _controller.Move(_playerGravity * Time.deltaTime);
    }
    

    void Gravity()
    {
        if (!IsGrounded())
        {
            //Añade la gravedad
            _playerGravity.y += _gravity * Time.deltaTime;
        }

        else if (IsGrounded() && _playerGravity.y < 0)
        {
            _playerGravity.y = _gravity;
            _animator.SetBool("IsJumping", false);
        }

        //Aplica la gravedad
        _controller.Move(_playerGravity * Time.deltaTime);
        
    }

    //Sensor del suelo
    bool IsGrounded()
    {
        return Physics.CheckSphere(_sensor.position, _sensorRadius, _groundLayer);
    }

    //Para ver donde esta el sensor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(_sensor.position, _sensorRadius);
    }
}