using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    [SerializeField] WheelCollider frontLeft;
    [SerializeField] WheelCollider frontRight;
    [SerializeField] WheelCollider rearLeft;
    [SerializeField] WheelCollider rearRight;

    [SerializeField] Transform frontLeftTransform;
    [SerializeField] Transform frontRightTransform;
    [SerializeField] Transform rearLeftTransform;
    [SerializeField] Transform rearRightTransform;

    public float acceleration = 500f;
    public float brakeForce = 300f;
    public float maxTurnAngle = 30f;

    private float currentAcceleration = 0f;
    private float currentBrakeForce = 0f;
    private float currentTurnAngle = 0f;

    private Rigidbody rb;
    
    private AudioSource _engine;
    private AudioSource _engineStart;
    private AudioSource _engineStop;
    AudioSource _horn;
    AudioSource _brakes;

    private Controls _controls;
    private float _throttle;
    private float _steer;

    private bool _engineRunning;
    Transform _spawn;
    ParticleSystem _smoke;
    GameObject _lights;

    private void Awake()
    {
        _smoke = GameObject.Find("Smoke").GetComponent<ParticleSystem>();
        /*GameObject go = GameObject.Find("Traffic_light_EU_red");
        while (go)
        {
            go.GetComponent<MeshRenderer>().enabled = false;
            go = GameObject.Find("Traffic_light_EU_red");
        }*/
        _lights = GameObject.Find("Lights");
        _lights.SetActive(false);

        _spawn = GameObject.Find("Spawn")?.transform;
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();

        _engine = GameObject.Find("Engine").GetComponent<AudioSource>();
        _engineStart = GameObject.Find("Start").GetComponent<AudioSource>();
        _engineStop = GameObject.Find("Stop").GetComponent<AudioSource>();
        _horn = GameObject.Find("Horn").GetComponent<AudioSource>();
        //_brakes = GameObject.Find("Brakes").GetComponent<AudioSource>();

        _controls = new Controls();
        _controls.Car.Enable();

        _controls.Car.ToggleEngine.performed += ToggleEngine;
        _controls.Car.Respawn.performed += Respawn;

        _controls.Car.Throttle.performed += context => _throttle = context.ReadValue<float>();
        _controls.Car.Throttle.canceled += context => _throttle = 0f;

        _controls.Car.Steer.performed += context => _steer = context.ReadValue<float>();
        _controls.Car.Steer.canceled += context => _steer = 0f;

        _controls.Car.Brake.performed += Brake;
        _controls.Car.Brake.canceled += context => currentBrakeForce = 0f;

        _controls.Car.Horn.performed += Horn;
        //_controls.Car.Horn.canceled += HornStop;
    }

    void Brake(InputAction.CallbackContext ctx)
    {
        currentBrakeForce = brakeForce;
        //_brakes.Play();
    }

    void Respawn(InputAction.CallbackContext ctx)
    {
        if (!_spawn) return;
        _spawn.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
        transform.SetPositionAndRotation(position, rotation);
    }

    void Horn(InputAction.CallbackContext ctx)
    {
        
        _horn.Play();
    }

    void HornStop(InputAction.CallbackContext ctx)
    {

        _horn.Stop();
    }

    private void ToggleEngine(InputAction.CallbackContext ctx)
    {
        _engineRunning = !_engineRunning;

        if (_engineRunning)
        {
            _lights.SetActive(true);
            _engineStart.Play();
            _engine.Play();
            _smoke.Play();
        }
        else
        {
            _lights.SetActive(false);
            _engine.Stop();
            _engineStop.Play();
            _smoke.Stop();
        }
    }

    private void FixedUpdate()
    {
        currentAcceleration = _engineRunning ? acceleration * _throttle : 0f;
        currentTurnAngle = maxTurnAngle * _steer;

        rearLeft.motorTorque = currentAcceleration;
        rearRight.motorTorque = currentAcceleration;
        

        frontRight.brakeTorque = currentBrakeForce;
        frontLeft.brakeTorque = currentBrakeForce;
        rearRight.brakeTorque = currentBrakeForce;
        rearLeft.brakeTorque = currentBrakeForce;


        frontLeft.steerAngle = currentTurnAngle;
        frontRight.steerAngle = currentTurnAngle;

        UpdateWheel(frontLeft, frontLeftTransform);
        UpdateWheel(frontRight, frontRightTransform);
        UpdateWheel(rearLeft, rearLeftTransform);
        UpdateWheel(rearRight, rearRightTransform);

        _engine.pitch = 1 + rb.velocity.magnitude / 5;
    }

    void UpdateWheel(WheelCollider col, Transform trans)
    {
        col.GetWorldPose(out Vector3 position, out Quaternion rotation);
        trans.SetPositionAndRotation(position, rotation);
    }
}
