//using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
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
    private InputAction _aimAction;
    private InputAction _grabAction;
    private InputAction _throwAction;
    

    [SerializeField] private float _movementSpeed = 5;
    [SerializeField] private float _jumpHeight = 2;
    [SerializeField] private float _pushForce = 10;
    [SerializeField]private float _throwForce = 20; 
    [SerializeField] private float _smoothTime = 0.2f;
    //variable de referencia
    private float _turnSmoothVelocity;


    //Gravedad
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private Vector3 _playerGravity;

    //Ground Sensor
    [SerializeField] private Transform _sensor;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] float _sensorRadius;

    private Transform _mainCamera;

    //Coger objetos
    [SerializeField] private Transform _hands;
    [SerializeField] private Transform _grabbedObject;
    [SerializeField] private Vector3 _handSensorSize = new Vector3(1, 1, 1);

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        _moveAction = InputSystem.actions["Move"];
        _jumpAction = InputSystem.actions["Jump"];
        _lookAction = InputSystem.actions["Look"];
        _aimAction = InputSystem.actions["Aim"];
        _grabAction = InputSystem.actions["Interact"];
        _throwAction = InputSystem.actions["Throw"];

        //La cámara principal tiene que tener la tag de main camera
        _mainCamera = Camera.main.transform;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _moveInput = _moveAction.ReadValue<Vector2>();
        _lookInput = _lookAction.ReadValue<Vector2>();



        if (_aimAction.IsInProgress())
        {
            AimMovement();
        }
        else
        {
            Movement();
        }

        //MovimientoCutre();
        //Movimiento2();

        if (_jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            Jump();
        }

        Gravity();

        if (_aimAction.WasPerformedThisFrame())
        {
            Attack();
        }

        if (_grabAction.WasPerformedThisFrame())
        {
            GrabObject();
        }

        if (_throwAction.WasPerformedThisFrame())
        {
            Throw();
            RayTest();
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


    void Movement()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);

        _animator.SetFloat("Vertical", direction.magnitude);
        _animator.SetFloat("Horizontal", 0);
        if (direction != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _smoothTime);

            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
            
            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

            _controller.Move(moveDirection * _movementSpeed * Time.deltaTime);
        }
    }

    void AimMovement()
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

            _controller.Move(moveDirection * _movementSpeed * Time.deltaTime);
        }
    }

    void Movimiento2()
    {
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);

        //Raycast
        Ray ray = Camera.main.ScreenPointToRay(_lookInput);
        RaycastHit hit;

        //out lo que hace es sacar la información del impacto y almacenarla en la variable hit
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
    }

    void MovimientoCutre()
    {
        //_moveInput = _moveAction.ReadValue<Vector2>();

        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);

        if (direction != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _smoothTime);

            transform.rotation = Quaternion.Euler(0, smoothAngle, 0);

            _controller.Move(direction.normalized * _movementSpeed * Time.deltaTime);
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
    /*bool IsGrounded()
    {
        return Physics.CheckSphere(_sensor.position, _sensorRadius, _groundLayer);
    }*/
    
    bool IsGrounded()
    {
        if (Physics.Raycast(_sensor.position, -transform.up, _sensorRadius, _groundLayer))
        {
            Debug.DrawRay(_sensor.position, -transform.up * _sensorRadius, Color.red);
            return true;
        }
        else
        {
            Debug.DrawRay(_sensor.position, -transform.up * _sensorRadius, Color.green);
            return false;
        }
    }

    //Para ver donde esta el sensor
    void OnDrawGizmos()
    {
        //Ground sensor
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(_sensor.position, _sensorRadius);

        //Sensor manos
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(_hands.position, _handSensorSize);
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //con el oncontrollercolliderhit hay que acceder primero al transform a través de la variable local, y luego ya accedemos al gameObject entero)
        if (hit.transform.gameObject.tag == "Empujable")
        {
            Rigidbody _rBody = hit.collider.attachedRigidbody;
            //Rigidbody _rBody = hit.transform.GetComponent<Rigidbody2D>();

            if (_rBody == null || _rBody.isKinematic)
            {
                return;
            }

            //Vector3 _pushDirection = hit.moveDirection;

            Vector3 _pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            _rBody.linearVelocity = _pushDirection * _pushForce / _rBody.mass;
        }
    }

    void GrabObject()
    {
        if (_grabbedObject == null)
        {
            Collider[] objectsToGrab = Physics.OverlapBox(_hands.position, _handSensorSize);

            foreach (Collider item in objectsToGrab)
            {
                IGrabeable grabableObject = item.GetComponent<IGrabeable>();

                if (grabableObject != null)
                {
                    _grabbedObject = item.transform;
                    _grabbedObject.SetParent(_hands);
                    _grabbedObject.position = _hands.position;
                    _grabbedObject.rotation = _hands.rotation;
                    _grabbedObject.GetComponent<Rigidbody>().isKinematic = true;

                    return;
                }
            }
        }
        else
        {
            _grabbedObject.SetParent(null);
            _grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
            _grabbedObject = null;
        }
    }
    void Throw()
    {
        if (_grabbedObject == null)
        {
            return;
        }

        Rigidbody _grabbedBody = _grabbedObject.GetComponent<Rigidbody>();

        _grabbedObject.SetParent(null);
        _grabbedBody.isKinematic = false;
        _grabbedBody.AddForce(_mainCamera.transform.forward * _throwForce, ForceMode.Impulse);
        _grabbedObject = null;
    }

    void RayTest()
    {
        //Raycast simple
        //este tipo de raycast no srive demasiado porque no te devuelve la información
        //necesitamos pasarle la posición desde la que sale el rayo, la dirección y el tamaño
        if (Physics.Raycast(transform.position, transform.forward, 5))
        {
            Debug.Log("hit");
            //Con esto podemos hacer que se dibuje el rayo
            Debug.DrawRay(transform.position, transform.forward * 5, Color.red);
        }
        else
        {
            Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        }

        //Raycast "Avanzado"
        //Igual que el simple pero con la variable hit, lo que nos permite almacenar la información de lo que ha impactdo el rayo
        RaycastHit hit;
        //out hit almacena dentro de la variable la información de lo que ha chocado el rayo
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
                    Debug.Log("cosas");
                }
            }*/

            IDamageable damageable = hit.transform.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(5);
            }
            
            /*
            //acordarse de poner delta en lo del raton en el input system
            
            Ray ray = Camera.main.ScreenPointToRay(_lookInput);
            RaycastHit hit;

            //out lo que hace es sacar la información del impacto y almacenarla en la variable hit
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 playerForward = hit.point - transform.position;
                Debug.Log(hit.transform.name);
                playerForward.y = 0;
                transform.forward = playerForward;
            }
            */
        }
    }
}