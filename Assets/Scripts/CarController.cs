using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
public class CarController : MonoBehaviour
{
    [Header("Drift Settings")]
    public float driftSidewaysStiffness = 0.5f;
    public float driftForwardStiffness = 0.7f;
    public float normalSidewaysStiffness = 1.7f;
    public float normalForwardStiffness = 7f;
    public float driftYawTorque = 300f;

    [Header("Car Settings")]
    public float motorForce = 500f;
    public float brakeForce = 300f;
    public float maxSteerAngle = 30f;
    
    [Header("AWD Settings")]
    [Range(0, 1)] public float frontPowerDistribution = 0.4f;
    [Range(0, 1)] public float rearPowerDistribution = 0.6f;

    [Header("Boost Settings")]
    public float boostForce = 20000f;
    public float boostTorqueMultiplier = 3.0f;
    public float boostDuration = 1.5f;
    public float boostCooldown = 3.0f;

    [Header("Input Settings")]
    [SerializeField] private bool isPlayer1 = true;
    private PlayerInput playerInput;
    public InputAction moveAction; // Movement (WASD or Arrows)
    public InputAction brakeAction; // Brake input
    public InputAction powerUpAction; // Activate power-up input
    private Vector2 moveInput;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;

    [Header("Wheel Transforms")]
    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;
    public Transform centerOfMass;
    private Transform CenterOfMassOGTransform;
    private Rigidbody rb;
    public GameObject followTarget;
    public GameObject cameraLookAt;

    [Header("Power Up Settings")]
    public bool hitByMissile = false;
    public float speedPowerUpForceMultiplier = 15000f;
    public float speedPowerUpTorqueMultiplier = 2.5f;
    public GameObject missileProjectilePrefab;
    public Transform missileSpawnPoint;
    private float slipMultiplier = 1f;
    public GameObject speedPowerUpPrefab;
    public GameObject puddlePrefab;
    private GameObject instantiatedPuddle;
    private GameObject instantiatedMissile;
    private GameObject instantiatedSpeedPowerUp;
    public float powerUpCooldown = 0.5f;
    private List<PowerUp> storedPowerUps = new List<PowerUp>();
    private bool canUsePowerUp = true;
    private bool isPowerUpActive = false;
    private Coroutine activePowerUpCoroutine;
    [Header("Power Up Sounds")]
    public AudioClip powerUpPickupSound;
    public AudioClip missileCollisionSound;
    public AudioClip puddleCollisionSound;
    public AudioClip speedPowerUpSound;
    public AudioClip puddleSound;
    public AudioClip missleSound;
    public AudioClip missleCollectionSound;
    public AudioClip moneySound;
    public AudioSource powerUpAudioSource;

    // Input values
    private float horizontalInput;
    private float verticalInput;
    private float currentBrakeForce;
    private bool isBraking;

    // Boost variables
    private bool isBoosting = false;
    private bool canBoost = true;
    private float currentBoostTime = 0f;
    private float currentCooldownTime = 0f;
    float deltaTime = 0.0f;
    [Header("Stuck Detection")]
    private Vector3 lastPosition;
    private float stuckThreshold = 1.0f;
    public ResetPosition resetPosition;
    public bool wasResettet = false;



    

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        string actionMapName = isPlayer1 ? "Player1" : "Player2";
        playerInput.SwitchCurrentActionMap(actionMapName);

        moveAction = playerInput.actions.FindAction($"{actionMapName}/Move");
        brakeAction = playerInput.actions.FindAction($"{actionMapName}/Brake");
        powerUpAction = playerInput.actions.FindAction($"{actionMapName}/PowerUp");
    }
    private void OnEnable()
    {
        moveAction.Enable();
        brakeAction.Enable();
        powerUpAction.Enable();
        
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
    }
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void Start()
    {
        
        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = centerOfMass.localPosition;
        CenterOfMassOGTransform = centerOfMass;

        StartCoroutine(WheelSettle());
        
        ValidatePowerDistribution();
        lastPosition = transform.position;
        StartCoroutine(CheckIfStuck());
    }
    
    private void ValidatePowerDistribution()
    {
        float total = frontPowerDistribution + rearPowerDistribution;
        if (Mathf.Abs(total - 1.0f) > 0.01f)
        {
            frontPowerDistribution /= total;
            rearPowerDistribution /= total;
            Debug.LogWarning("AWD power distribution values were adjusted to ensure total equals 100%");
        }
    }

    private IEnumerator WheelSettle()
    {
        yield return new WaitForFixedUpdate();
        frontLeftWheelCollider.enabled = false;
        yield return new WaitForFixedUpdate();
        frontLeftWheelCollider.enabled = true;
        yield return new WaitForSeconds(2);
    }
    
    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        ManageBoost();
        UpdateWheels();
        UpdateFollowTarget();
        CheckGroundedState();
        CheckForPowerUpActivation();
    }

    private void UpdateFollowTarget()
    {
        if (followTarget != null)
        {
            // Position the follow target at the car's position
            followTarget.transform.position = transform.position;

            // Get the car's velocity direction
            Vector3 velocityDirection = rb.linearVelocity;

            // Only rotate if the car is actually moving
            if (velocityDirection.magnitude > 0.1f)
            {
                Vector3 horizontalVelocity = new Vector3(velocityDirection.x, 0, velocityDirection.z);
                
                if (horizontalVelocity.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized);
                    
                    followTarget.transform.rotation = Quaternion.Slerp(
                        followTarget.transform.rotation,
                        targetRotation,
                        Time.deltaTime * 10f  // Adjust this value to change rotation speed
                    );
                }
            }
        }
    }

    private void GetInput()
    {
        horizontalInput = moveInput.x; // Horizontal input (left/right)
        verticalInput = moveInput.y;   // Vertical input (forward/backward)
        
        // Check if braking using the brake action
        isBraking = brakeAction != null && brakeAction != null && brakeAction.ReadValue<float>() > 0.5f;
        
        // Get the car's speed
        float speed = rb.linearVelocity.magnitude * 3.6f; // Convert to km/h
        
        if (speed < 35f && speed > 10f && canBoost && verticalInput > 0.5f)
        {
            ActivateBoost();
        }
    }

    private void ActivateBoost()
    {
        if (canBoost)
        {
            isBoosting = true;
            canBoost = false;
            currentBoostTime = 0f;
            
            // Apply immediate forward force for quick acceleration
            rb.AddForce(transform.forward * boostForce, ForceMode.Impulse);
        }
    }

    private void ManageBoost()
    {
        // Handle boost duration
        if (isBoosting)
        {
            currentBoostTime += Time.fixedDeltaTime;
            
            if (currentBoostTime >= boostDuration)
            {
                isBoosting = false;
                currentCooldownTime = 0f;
            }
        }
        // Handle cooldown
        else if (!canBoost)
        {
            currentCooldownTime += Time.fixedDeltaTime;
            
            if (currentCooldownTime >= boostCooldown)
            {
                canBoost = true;
            }
        }
    }

    private void HandleMotor()
    {
        // Early return if there's no input
        if (moveAction == null || moveAction == null)
            return;
            
        float baseTorque = verticalInput * motorForce;
        
        // Apply boost torque multiplier if boosting
        if (isBoosting && verticalInput > 0)
        {
            baseTorque *= boostTorqueMultiplier;
        }
        
        // Distribute torque according to AWD settings
        float frontTorque = baseTorque * frontPowerDistribution;
        float rearTorque = baseTorque * rearPowerDistribution;
        
        // Apply motor torque to wheels according to power distribution
        frontLeftWheelCollider.motorTorque = frontTorque;
        frontRightWheelCollider.motorTorque = frontTorque;
        rearLeftWheelCollider.motorTorque = rearTorque;
        rearRightWheelCollider.motorTorque = rearTorque;
        
        // Apply braking
        currentBrakeForce = isBraking ? brakeForce : 0f;
        
        ApplyBraking();
    }

    private void ApplyBraking()
    {
        if (isBraking)
        {
            currentBrakeForce = brakeForce;

            // Apply drift friction to rear wheels
            float driftForwardStiffnessChanged = driftForwardStiffness * slipMultiplier;
            float driftSidewaysStiffnessChanged = driftSidewaysStiffness * slipMultiplier;
            SetDriftFriction(rearLeftWheelCollider, driftForwardStiffnessChanged, driftSidewaysStiffnessChanged);
            SetDriftFriction(rearRightWheelCollider, driftForwardStiffnessChanged, driftSidewaysStiffnessChanged);
            
            // Add yaw torque to simulate oversteer/drift
            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            float direction = Mathf.Sign(localVelocity.x); // Left or right
            rb.AddTorque(transform.up * direction * driftYawTorque);
            
            // Apply brake differently for drifting (more on front, less on rear)
            frontLeftWheelCollider.brakeTorque = currentBrakeForce;
            frontRightWheelCollider.brakeTorque = currentBrakeForce;
            rearLeftWheelCollider.brakeTorque = 0f;
            rearRightWheelCollider.brakeTorque = 0f;
        }
        else
        {
            currentBrakeForce = 0f;

            // Restore normal friction
            float normalForwardStiffnessChanged = normalForwardStiffness * slipMultiplier;
            float normalSidewaysStiffnessChanged = normalSidewaysStiffness * slipMultiplier;
            SetDriftFriction(rearLeftWheelCollider, normalForwardStiffnessChanged, normalSidewaysStiffnessChanged);
            SetDriftFriction(rearRightWheelCollider, normalForwardStiffnessChanged, normalSidewaysStiffnessChanged);
            
            // Reset brake torque on all wheels
            frontLeftWheelCollider.brakeTorque = currentBrakeForce;
            frontRightWheelCollider.brakeTorque = currentBrakeForce;
            rearLeftWheelCollider.brakeTorque = currentBrakeForce;
            rearRightWheelCollider.brakeTorque = currentBrakeForce;
        }
    }

    private void SetDriftFriction(WheelCollider wheel, float forwardStiffness, float sidewaysStiffness)
    {
        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;

        forwardFriction.stiffness = forwardStiffness;
        sidewaysFriction.stiffness = sidewaysStiffness;

        wheel.forwardFriction = forwardFriction;
        wheel.sidewaysFriction = sidewaysFriction;
    }

    private void HandleSteering()
    {
        // Calculate steering angle based on input
        float steerAngle = horizontalInput * maxSteerAngle;
        
        // Apply steering angle to front wheels
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        // Get wheel position and rotation from the physics simulation
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);
        
        // Convert the world rotation to local rotation relative to the car
        Quaternion localRotation = Quaternion.Inverse(wheelTransform.parent.rotation) * rotation;
        
        // Apply the local rotation
        wheelTransform.localRotation = localRotation;
    }

    void CheckGroundedState()
    {
        // Check if the car is grounded by checking the wheel colliders
        bool isGrounded = frontLeftWheelCollider.isGrounded || frontRightWheelCollider.isGrounded ||
                          rearLeftWheelCollider.isGrounded || rearRightWheelCollider.isGrounded;
        
        // If not grounded, apply a downward force to simulate gravity
        if (!isGrounded)
        {
            rb.centerOfMass = new Vector3(0, -0.252f, -0.8f); // Lower center of mass
            rb.angularDamping = 1.5f;
        }
        else
        {
            if(wasResettet)
            {
                wasResettet = false;
                rb.centerOfMass = CenterOfMassOGTransform.localPosition;
                this.transform.rotation = Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);
            }
            rb.angularDamping = .25f;
            rb.centerOfMass = centerOfMass.localPosition;
            if(hitByMissile)
            {
                
                StartCoroutine(ResetCameraPosition());
            }
        }
    }
    public void ResetWheels()
    {
        // Reset the wheel colliders to their original state
        frontLeftWheelCollider.transform.localRotation = Quaternion.identity;
        frontRightWheelCollider.transform.localRotation = Quaternion.identity;
        rearLeftWheelCollider.transform.localRotation = Quaternion.identity;
        rearRightWheelCollider.transform.localRotation = Quaternion.identity;
        WheelCollider[] wheelColliders = GetComponentsInChildren<WheelCollider>();
        foreach (WheelCollider wheelCollider in wheelColliders)
        {
            wheelCollider.transform.localRotation = Quaternion.identity;
            wheelCollider.rotationSpeed = 0f;
            wheelCollider.brakeTorque = Mathf.Infinity;
            wheelCollider.motorTorque = 0f;
        }

        // Reset the center of mass
        rb.centerOfMass = CenterOfMassOGTransform.localPosition;
    }
    private IEnumerator CheckIfStuck()
    {
        yield return new WaitForSeconds(8f);
        while (true)
        {
            yield return new WaitForSeconds(4f);

            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved < stuckThreshold)
            {
                resetPosition.ResetPositionOfCar(gameObject);
            }
            lastPosition = transform.position;
        }
    }
    private IEnumerator ResetCameraPosition()
    {
        yield return new WaitForSeconds(2f);
        hitByMissile = false;
        cameraLookAt.transform.localPosition = new Vector3(0,0,4.57f);
    }
    #region PowerUps

    public void CollectPowerUp(PowerUpType type, float strength = 1.0f, float duration = 1.5f)
        {
            // Clear any existing power-up
            if (storedPowerUps.Count > 0)
            {
                // Clean up any instantiated objects from the previous power-up
                CleanupPowerUpObjects();
                
                // Clear the list
                storedPowerUps.Clear();
            }
            
            // Add the new power-up
            storedPowerUps.Add(new PowerUp(type, strength, duration));
            PowerUp powerUp = storedPowerUps[0];

        // Handle any instantiation needed for the new power-up
        switch (powerUp.type)
        {
            case PowerUpType.Speed:
                powerUpAudioSource.PlayOneShot(powerUpPickupSound);
                instantiatedSpeedPowerUp = Instantiate(speedPowerUpPrefab, missileSpawnPoint.position, Quaternion.Euler(90f, 90, 180), missileSpawnPoint);
                break;
            case PowerUpType.Puddle:
                powerUpAudioSource.PlayOneShot(powerUpPickupSound);
                instantiatedPuddle = Instantiate(puddlePrefab, missileSpawnPoint.position, Quaternion.identity, missileSpawnPoint);
                if (isPlayer1)
                {
                    instantiatedPuddle.layer = 7;
                }
                else
                {
                    instantiatedPuddle.layer = 8;
                }
                break;
            case PowerUpType.Missile:
                powerUpAudioSource.PlayOneShot(powerUpPickupSound);
                instantiatedMissile = Instantiate(missileProjectilePrefab, missileSpawnPoint.position, Quaternion.identity, missileSpawnPoint);
                if (isPlayer1)
                {
                    instantiatedMissile.layer = 7;
                }
                else
                {
                    instantiatedMissile.layer = 8;
                }
                break;
            case PowerUpType.Money:
                powerUpAudioSource.PlayOneShot(moneySound);
                MoneyManager moneyManager = GetComponent<MoneyManager>();
                moneyManager.AddMoney(200);
                break;
            case PowerUpType.Random:
                PowerUpType randomPowerUpType = (PowerUpType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(PowerUpType)).Length);
                CollectPowerUp(randomPowerUpType, powerUp.strength, powerUp.duration);
                StartCoroutine(PowerUpCooldown(powerUpCooldown));
                // Handle random power-up logic here
                break;
            }
            
           
            
            Debug.Log("Power-up collected: " + type.ToString() + " (replaced previous power-up)");
        }

    // New helper method to clean up instantiated power-up objects
    private void CleanupPowerUpObjects()
    {
        // Destroy any instantiated objects
        if (instantiatedPuddle != null)
        {
            Destroy(instantiatedPuddle);
            instantiatedPuddle = null;
        }

        if (instantiatedMissile != null)
        {
            Destroy(instantiatedMissile);
            instantiatedMissile = null;
        }
        if (instantiatedSpeedPowerUp != null)
        {
            Destroy(instantiatedSpeedPowerUp);
            instantiatedSpeedPowerUp = null;

        }
    }
        
        void CheckForPowerUpActivation()
        {
            // Check if we have a power-up and can use it
            if (storedPowerUps.Count > 0 && canUsePowerUp && !isPowerUpActive)
            {
                // Check if activation button is pressed
                if (powerUpAction.ReadValue<float>() > 0.5f)
                {
                    ActivatePowerUp();
                }
            }
        }
        
        void ActivatePowerUp()
        {
            if (storedPowerUps.Count > 0 && canUsePowerUp)
            {
                canUsePowerUp = false;
                
                // Get the power-up
                PowerUp powerUp = storedPowerUps[0];
                storedPowerUps.RemoveAt(0);
                Debug.Log("Activating power-up: " + powerUp.type.ToString());
                
                // Activate the power-up based on its type
                switch (powerUp.type)
                {
                    case PowerUpType.Speed:
                        powerUpAudioSource.PlayOneShot(speedPowerUpSound);
                        Destroy(instantiatedSpeedPowerUp);
                        if (activePowerUpCoroutine != null)
                        StopCoroutine(activePowerUpCoroutine);
                        activePowerUpCoroutine = StartCoroutine(SpeedPowerUpCoroutine(
                            speedPowerUpForceMultiplier * powerUp.strength, 
                            speedPowerUpTorqueMultiplier * powerUp.strength, 
                            powerUp.duration));
                        break;
                    case PowerUpType.Puddle:
                        powerUpAudioSource.PlayOneShot(puddleSound);
                        FirePuddle();
                        StartCoroutine(PowerUpCooldown(powerUpCooldown));
                        break;
                    case PowerUpType.Missile:
                        powerUpAudioSource.PlayOneShot(missleSound);
                        FireMissile();
                        StartCoroutine(PowerUpCooldown(powerUpCooldown));
                        break;
                    case PowerUpType.Money:
                        StartCoroutine(PowerUpCooldown(powerUpCooldown));
                        break;
                    case PowerUpType.Random:
                        break;
                    default:
                        Debug.LogWarning("Unknown power-up type: " + powerUp.type);
                        isPowerUpActive = false;
                        StartCoroutine(PowerUpCooldown(powerUpCooldown));
                        break;
                }
            }
        }
        
        // Rest of the methods remain the same
        IEnumerator SpeedPowerUpCoroutine(float speedMultiplier, float torqueMultiplier, float duration)
        {
            isPowerUpActive = true;
            Debug.Log("Speed power-up activated for " + duration + " seconds");
            
            // Apply immediate boost
            rb.AddForce(transform.forward * speedMultiplier, ForceMode.Impulse);
            
            // Increase motor torque for the duration
            float originalMotorForce = motorForce;
            motorForce *= torqueMultiplier;
            
            // Wait for duration
            yield return new WaitForSeconds(duration);
            
            // Reset values
            motorForce = originalMotorForce;
            
            Debug.Log("Speed power-up ended");
            isPowerUpActive = false;
            
            // Start cooldown
            StartCoroutine(PowerUpCooldown(powerUpCooldown));
        }
        
        void FireMissile()
        {
            Debug.Log("Missile power-up activated");
            
            MissleBehaviour missileBehaviour = instantiatedMissile.GetComponent<MissleBehaviour>();
            missileBehaviour.StartMissle();
        }
        
        void FirePuddle()
        {
            PuddleBehaviour puddleBehaviour = instantiatedPuddle.GetComponent<PuddleBehaviour>();
            puddleBehaviour.StartPuddle();
        }
        
        public IEnumerator InPuddle()
        {
            powerUpAudioSource.PlayOneShot(puddleCollisionSound);
            Debug.Log("In puddle");
            slipMultiplier = 0.35f;
            yield return new WaitForSeconds(2f);
            
            slipMultiplier = 1f;
            Debug.Log("Out of puddle" + normalForwardStiffness + " " + normalSidewaysStiffness);
        }

        IEnumerator PowerUpCooldown(float cooldown)
        {
            yield return new WaitForSeconds(cooldown);
            canUsePowerUp = true;
        }

        void StopPowerUp()
        {
            float baseTorque = verticalInput * motorForce;

            float frontTorque = baseTorque * frontPowerDistribution;
            float rearTorque = baseTorque * rearPowerDistribution;
            
            frontLeftWheelCollider.motorTorque = frontTorque;
            frontRightWheelCollider.motorTorque = frontTorque;
            rearLeftWheelCollider.motorTorque = rearTorque;
            rearRightWheelCollider.motorTorque = rearTorque;
        }
    #endregion

        
        // Optional: Call this when the object is destroyed to clean up
    private void OnDisable()
    {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;

        moveAction.Disable();
        brakeAction.Disable();
        powerUpAction.Disable();
    }
    }

public enum PowerUpType
{
    Speed,
    Puddle,
    Missile,
    Money,
    Random
}
    [System.Serializable]
    public class PowerUp
    {
        public PowerUpType type;
        public float duration = 1.5f;
        public float strength = 1.0f;
        public Sprite icon;
        
        public PowerUp(PowerUpType type, float strength = 1.0f, float duration = 1.5f)
        {
            this.type = type;
            this.strength = strength;
            this.duration = duration;
        }
    }