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

    private Controls _controls;
    private float _throttle;
    private float _steer;

    private bool _engineStarted;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();

        _engine = GameObject.Find("Engine").GetComponent<AudioSource>();
        _engineStart = GameObject.Find("Start").GetComponent<AudioSource>();
        _engineStop = GameObject.Find("Stop").GetComponent<AudioSource>();

        _controls = new Controls();
        _controls.Engine.Enable();

        _controls.Engine.Trigger.performed += TriggerEngine;

        _controls.Car.Throttle.performed += context => _throttle = context.ReadValue<float>();
        _controls.Car.Throttle.canceled += context => _throttle = 0f;

        _controls.Car.Steer.performed += context => _steer = context.ReadValue<float>();
        _controls.Car.Steer.canceled += context => _steer = 0f;

        _controls.Car.Brake.performed += context => currentBrakeForce = brakeForce;
        _controls.Car.Brake.canceled += context => currentBrakeForce = 0f;
    }

    private void TriggerEngine(InputAction.CallbackContext ctx)
    {
        _engineStarted = !_engineStarted;

        if (_engineStarted)
        {
            _engineStart.Play();
            _engine.Play();
            _controls.Car.Enable();
        }
        else
        {
            _engine.Stop();
            _engineStop.Play();
            _controls.Car.Disable();
        }
    }

    private void FixedUpdate()
    {
        currentAcceleration = acceleration * _throttle;
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

        _engine.pitch = 1 + rb.velocity.magnitude / 10;
    }

    void UpdateWheel(WheelCollider col, Transform trans)
    {
        col.GetWorldPose(out Vector3 position, out Quaternion rotation);
        trans.SetPositionAndRotation(position, rotation);
    }
}
