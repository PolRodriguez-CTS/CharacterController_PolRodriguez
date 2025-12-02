using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class JaviScriptController : MonoBehaviour
{
    //Componentes
    private CharacterController _controller;
    private Animator _animator;
    //Inputs 
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _lookAction;
    private InputAction _aimAction;
    private InputAction _throwAction;
    [SerializeField] private Vector2 _lookInput;
    private InputAction _grabAction;
    private Vector2 _moveInput;
    //Variables
    [SerializeField] private float _jumpHeight = 2;
    [SerializeField] private float _movementSpeed = 5;
    [SerializeField] private float _smoothTime = 0.2f;
    private float _turnSmoothVelocity;
    [SerializeField] private float _pushForce = 10;
    [SerializeField] private float _throwForce = 20;
    //Gravedad 
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private Vector3 _playerGravity;
    //GroundSensor
    [SerializeField] private Transform _sensor;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _sensorRadius;
    //Camera
    private Transform _mainCamera;
    //Pillar cosillas y tal
    [SerializeField] private Transform _hands;
    [SerializeField] private Transform _grabedObject;
    [SerializeField] private Vector3 _handsSize = new Vector3(0.5f, 0.5f, 0.5f);


    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _moveAction = InputSystem.actions["Move"];
        _jumpAction = InputSystem.actions["Jump"];
        _lookAction = InputSystem.actions["Look"];
        _aimAction = InputSystem.actions["Aim"];
        _grabAction = InputSystem.actions["Interact"];
        _throwAction = InputSystem.actions["Throw"];
        _mainCamera = Camera.main.transform;
        _animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
    
    }
    

    void Update()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();
        _lookInput = _lookAction.ReadValue<Vector2>();

        //MovimientoCutre();
        //MovimientoHades();

        Gravity();

        if (_aimAction.IsInProgress())
        {
            AimMovment();
        }
        else
        {
            Movment();
        }

        if (_jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            Jump();
        }

       

        if (_aimAction.WasPerformedThisFrame())
        {
            Attack();
        }

        if (_grabAction.WasPressedThisFrame())
        {
            GrabObject();
        }
        if (_throwAction.WasPerformedThisFrame())
        {
            Throw();
        }
        RayTest();
    }

    void Attack()
    {
        //Como tienes herencia en un script de enemy general puedes simplemente llamara al script "Padre" y no necesitas 20 if
        Ray ray = Camera.main.ScreenPointToRay(_lookInput);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            IDamageable damageable = hit.transform.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(5);
            }
        }
    }

    [SerializeField] float _speedChangeRate = 10;

    float _speed;

    float _animationSpeed;
    
    bool isSprinting;

    float _sprintSpeed = 8;

    float targetAngle;

    void Movment()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);

        //float targetSpeed;

       /* if(isSprinting)
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
        }*/
        float targetSpeed = _movementSpeed;

        float currentSpeed = new Vector3(_controller.velocity.x, 0 , _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;

        if(currentSpeed < targetSpeed - speedOffset || currentSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentSpeed, targetSpeed * direction.magnitude, Time.deltaTime * _speedChangeRate);
            
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationSpeed = Mathf.Lerp(_animationSpeed, targetSpeed,Time.deltaTime * _speedChangeRate);

        if(_animationSpeed < 0.1f)
        {
            _animationSpeed = 0;
        }

        _animator.SetFloat("Speed", _animationSpeed);


        //_animator.SetFloat("Vertical", direction.magnitude);
        //_animator.SetFloat("Horizontal", 0);

        /*if (direction != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _smoothTime);
            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            _controller.Move(moveDirection.normalized * _movementSpeed * Time.deltaTime);
        }*/
        if (direction != Vector3.zero)
        {
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _smoothTime);
            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
            
        }
        Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
        _controller.Move(_speed * Time.deltaTime * moveDirection.normalized + _playerGravity * Time.deltaTime);
    }

    void AimMovment()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);
        _animator.SetFloat("Horizontal", _moveInput.x);
        _animator.SetFloat("Vertical", _moveInput.y);
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _mainCamera.eulerAngles.y, ref _turnSmoothVelocity, _smoothTime);
        transform.rotation = Quaternion.Euler(0, smoothAngle, 0);

        if (direction != Vector3.zero)
        {
            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            //_controller.Move(moveDirection.normalized * _movementSpeed * Time.deltaTime);
        }
    }

    /*void MovimientoHades()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);

        Ray ray = Camera.main.ScreenPointToRay(_lookInput);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Vector3 playerForward = hit.point - transform.position;
            Debug.Log(hit.transform.name);
            playerForward.y = 0;
            transform.forward = playerForward;
        }

        if (direction != Vector3.zero)
        {
            _controller.Move(direction.normalized * _movementSpeed * Time.deltaTime);
        }
    }*/

    /*void MovimientoCutre()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);

        if (direction != Vector3.zero)
        {
            //Rad2Deg --> transforma angulos radianes a normales de toda la vida de 360
            //El Atan2 te devuelve el angulo  en el que estas pulsando, si pulsas arriba pues 0, si pulsas abajo 180, si pulsas derecha 90 y asi.
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            //Hace que la transicion cuando giras sea mas smooth
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _smoothTime);
            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
            //El direction.normalized hace que el vector tenga de limite 1, para que no se mueva mas rapido en digonal que en linea recta.
            _controller.Move(direction.normalized * _movementSpeed * Time.deltaTime);
        }

    }*/

    void Jump()
    {
       // _animator.SetBool("IsJumping", true);
        _animator.SetBool("Jump", true);
        _playerGravity.y = Mathf.Sqrt(_jumpHeight * -2 * _gravity);
       // _controller.Move(_playerGravity * Time.deltaTime);
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
        /*if (!IsGrounded())
        {
            //Añade la fuerza de gravedad a la y del personaje
            _playerGravity.y += _gravity * Time.deltaTime;
        }
        else if (IsGrounded() && _playerGravity.y < 0)
        {
            _playerGravity.y = _gravity;
            _animator.SetBool("IsJumping", false);
        }*/
        //Aplica la gravedad al personaje
       // _controller.Move(_playerGravity * Time.deltaTime);
    }

    /*bool IsGrounded()
    {
        return Physics.CheckSphere(_sensor.position, _sensorRadius, _groundLayer);
    }*/

    bool IsGrounded()
    {
        if (Physics.Raycast(_sensor.position, -transform.up, _sensorRadius, _groundLayer))
        {
            Debug.DrawRay(_sensor.position, -transform.up * _sensorRadius, Color.green);
            return true;
        }
        else
        {
            Debug.DrawRay(_sensor.position, -transform.up * _sensorRadius, Color.red);
            return false;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_sensor.position, _sensorRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_hands.position, _handsSize);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.transform.gameObject.tag == "Empujable")
        {
            Rigidbody rBody = hit.collider.attachedRigidbody;
            //Rigidbody rBody = hit.transform.GetComponent<Rigidbody>();
            //es buena idea comprobar si esta vacio el rbody, para que no salten errores y se joda el juego.
            if(rBody == null || rBody.isKinematic)
            {
                return;
            }
            //hit.moveDirection te da la direccion en la que iba el controller antes de dar el golpe
            Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            rBody.linearVelocity = pushDirection * _pushForce / rBody.mass;
        }
    }

    void GrabObject()
    {
        if (_grabedObject == null)
        {
            Collider[] objectsToGrab = Physics.OverlapBox(_hands.position, _handsSize);
            foreach (Collider item in objectsToGrab)
            {
                IGrabeable grabeableObject = item.GetComponent<IGrabeable>();
                if (grabeableObject != null)
                {
                    _grabedObject = item.transform;
                    _grabedObject.SetParent(_hands);
                    _grabedObject.position = _hands.position;
                    _grabedObject.rotation = _hands.rotation;
                    _grabedObject.GetComponent<Rigidbody>().isKinematic = true;

                    return;
                }
            }
        }
        else
        {
            _grabedObject.SetParent(null);
            _grabedObject.GetComponent<Rigidbody>().isKinematic = false;
            _grabedObject = null;
        }

    }

    void Throw()
    {
        if (_grabedObject == null)
        {
            return;
        }

        Rigidbody grabedBody = _grabedObject.GetComponent<Rigidbody>();
        _grabedObject.SetParent(null);
        grabedBody.isKinematic = false;
        grabedBody.AddForce(_mainCamera.transform.forward * _throwForce, ForceMode.Impulse);
        _grabedObject = null;
    }
    
    void RayTest()
    {
        //Raycast simple
        //Dispara el rayo y si choca con algo ejecuta lo de dentro del if, pero no puedes sacar info ni nada complejo.
        if (Physics.Raycast(transform.position, transform.forward, 5))
        {
            Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
            Debug.Log("Muelto");
        }
        else
        {
            Debug.Log("Eres muy malo");
            Debug.DrawRay(transform.position, transform.forward * 5, Color.red);
        }

        //Raycast "Avanzado"
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.forward, out hit, 5))
        {
            Debug.Log(hit.transform.name);
            Debug.Log(hit.transform.position);
            Debug.Log(hit.transform.gameObject.layer);
            Debug.Log(hit.transform.tag);

            /*if(hit.transform.tag == "Empujable")
            {
                Box box = hit.transform.GetComponent<Box>();
                if(box != null)
                {
                    Debug.Log("Tengo hambre");
                }
            }*/

            IDamageable damageable = hit.transform.GetComponent<IDamageable>();
            if(damageable != null)
            {
                damageable.TakeDamage(5);
            }
        }
    }
    
}
