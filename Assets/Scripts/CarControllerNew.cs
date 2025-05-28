using UnityEngine;

public class CarControllerNew: MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask drivable;
    [SerializeField] private Transform accelerationPoint;
    [SerializeField] private GameObject[] wheels;
    [SerializeField] private GameObject[] frontTireParents = new GameObject[2];
    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness;
    [SerializeField] private float damperStiffness;
    [SerializeField] private float restLength;
    [SerializeField] private float springTravel;
    [SerializeField] private float wheelRadius;

    private int[] wheelsGrounded = new int[4];
    private bool isGrounded = false;
    [Header("Input Settings")]
    private float moveInput;
    private float steerInput;
    [Header("Car Settings")]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float deacceleration = 10f;
    [SerializeField] private float steerStrength = 15f;
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float dragCoefficient = 1f;
    private Vector3 currecntCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0;
    [Header("Visuals")]
    [SerializeField] private float tireRotationSpeed = 3000f;
    [SerializeField] private float maxSteeringAngle = 30f;
    private void Start()
    {
        carRB = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocityRatio();
        Movement();
        Visuals();
    }
    private void Update()
    {
        GetPlayerInput();
    }

    private void Suspension()
    {
        for (int i = 0; i < rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxLength = restLength + springTravel;
            float maxDistance = restLength;
            if (Physics. Raycast (rayPoints[i].position, -rayPoints[i].up, out hit, maxLength + wheelRadius, drivable))
            {
                wheelsGrounded[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength) /springTravel;

                float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float damperForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;
                float netForce = springForce - damperForce;

                carRB.AddForceAtPosition(rayPoints[i].up * netForce, rayPoints[i].position);
                SetTirePosition(wheels[i], hit.point + rayPoints[i].up * wheelRadius, i);

                Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);
            }
            else
            {
                wheelsGrounded[i] = 0;
                SetTirePosition(wheels[i], rayPoints[i].position - rayPoints[i].up * maxDistance,i);

                Debug.DrawRay(rayPoints[i].position, -rayPoints[i].up * (maxLength + wheelRadius), Color.green);
            }
        }
    }
    private void GroundCheck()
    {
        int tempGroundedWheels = 0;
        for (int i = 0; i < wheelsGrounded.Length; i++)
        {
            tempGroundedWheels += wheelsGrounded[i];
        }
        if(tempGroundedWheels > 1)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
    private void GetPlayerInput()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }
    private void CalculateCarVelocityRatio()
    {
        currecntCarLocalVelocity = transform.InverseTransformDirection(carRB.linearVelocity);
        carVelocityRatio = currecntCarLocalVelocity.z / maxSpeed;
    }
    private void Movement()
    {
        if (isGrounded)
        {
            Acceleration();
            Deacceleration();
            Turn();
            SidewaysDrag();
        }
    }
    private void Acceleration()
    {
        carRB.AddForceAtPosition(acceleration * moveInput * transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }
    private void Deacceleration()
    {
        carRB.AddForceAtPosition(deacceleration * moveInput * -transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }
    private void Turn()
    {
        carRB.AddTorque(steerStrength * steerInput * turningCurve.Evaluate(carVelocityRatio) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }
    private void SidewaysDrag()
    {
        float currentSidewaysSpeed = currecntCarLocalVelocity.x;


        float dragMagnitude = -currentSidewaysSpeed * dragCoefficient;
        Vector3 dragForce = transform.right * dragMagnitude;
        carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
    }
    private void Visuals()
    {
        TireVisuals();
    }

    private void SetTirePosition(GameObject wheel, Vector3 targetPostion, int i)
    {
        if(i < 2)
        {
            
        }
        else
        {
            targetPostion.y = targetPostion.y + 0.04f;
        }
        wheel.transform.position = targetPostion;
    }
    private void TireVisuals()
    {
        float steeringAngle = maxSteeringAngle * steerInput;
        for (int i = 0; i < wheels.Length; i++)
        {
            if(i < 2)
            {
                wheels[i].transform.Rotate(Vector3.right, tireRotationSpeed * Time.deltaTime * carVelocityRatio, Space.Self);
                frontTireParents[i].transform.localEulerAngles = new Vector3(frontTireParents[i].transform.localEulerAngles.x, steeringAngle, frontTireParents[i].transform.localEulerAngles.z);
            }
            else
            {
                wheels[i].transform.Rotate(Vector3.right, tireRotationSpeed * Time.deltaTime * moveInput, Space.Self);
            }
        }
    }
}