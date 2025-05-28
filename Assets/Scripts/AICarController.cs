using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class AICarController : RacerController
{
    [Header("Waypoint Navigation")]
    public Transform[] waypoints;
    private int currentWaypoint_nav = 0;
    [SerializeField] private float speed = 10f;
    public float turnSpeed = 5f;
    public float reachThreshold = 5f;

    [Header("Obstacle Avoidance")]
    public float obstacleDetectionDistance = 10f;
    public int rayCount = 7;  // Increased ray count for better coverage
    public float rayAngle = 60f;  // Total angle coverage
    public float avoidStrength = 15f;
    public float minAvoidanceDistance = 2f;  // Start maximum avoidance at this distance
    public LayerMask obstacleLayer;  // For filtering what's considered an obstacle

    [Header("Advanced Avoidance")]
    public float obstacleMemoryTime = 2f;  // How long to remember obstacles
    public float speedReductionFactor = 0.5f;  // How much to slow down when avoiding
    public bool debugDraw = true;
    private Rigidbody rb;
    private Vector3 avoidanceVector = Vector3.zero;
    private float currentSpeed;
    private Dictionary<Collider, float> recentObstacles = new Dictionary<Collider, float>();
    [Header("Power-Up Management")]
    public int hitChance = 50; // Chance to hit the target with a power-up
    private List<PowerUp> storedPowerUps = new List<PowerUp>();
    public float speedPowerUpForceMultiplier = 15000f;
    public float speedPowerUpTorqueMultiplier = 2.5f;
    public GameObject missileProjectilePrefab;
    public Transform missileSpawnPoint;
    private float slipMultiplier = 1f;
    public GameObject puddlePrefab;
    private GameObject instantiatedPuddle;
    private GameObject instantiatedMissile;
    public float powerUpCooldown = 0.5f;
    private bool canUsePowerUp = true;
    private bool isPowerUpActive = false;
    private Coroutine activePowerUpCoroutine;
    [Header("Position Display")]
    public bool showDebugInfo = true;
    private TextMesh positionTextMesh;
    public bool gameStarted = false;
    protected override void Start()
    {
        lastWaypoint = waypoints[0];
        rb = GetComponent<Rigidbody>();
        currentSpeed = speed;

        // Validate setup
        if (waypoints.Length == 0)
        {
            Debug.LogError("No waypoints assigned to AICarController");
            enabled = false;
        }
        base.Start();

        // Set a default name if none provided
        if (string.IsNullOrEmpty(racerName))
        {
            racerName = "AI Racer " + racerId;
        }

        // Create debug text mesh for position visualization if needed
        if (showDebugInfo)
        {
           // CreatePositionDisplay();
        }
    }

    void FixedUpdate()
    {
        if (!gameStarted) return;
        UpdateObstacleMemory();
        Vector3 avoidance = CalculateAvoidanceVector();
        Vector3 navigation = CalculateNavigationVector();

        // Combine avoidance and navigation forces
        Vector3 combinedDirection = Vector3.Lerp(navigation, avoidance, avoidance.magnitude);

        // Adjust speed based on obstacle proximity
        AdjustSpeed(avoidance.magnitude);

        // Apply movement
        rb.MovePosition(rb.position + transform.forward * currentSpeed * Time.fixedDeltaTime);

        // Apply rotation
        if (combinedDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(combinedDirection);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * turnSpeed));
        }

        // Check if reached waypoint
        Transform target = waypoints[currentWaypoint_nav];
        if (Vector3.Distance(transform.position, target.position) < reachThreshold)
        {
            currentWaypoint_nav = (currentWaypoint_nav + 1) % waypoints.Length;
            lastWaypoint = target;
            //randomize speed slightly when reaching a waypoint
            speed = speed + Random.Range(-1f, 3f);
            if (speed > 23f)
            {
                speed = 15f; // Cap speed to prevent it from getting too high
            }
            turnSpeed = turnSpeed + Random.Range(-0.5f, 1f);
            if (turnSpeed > 6f)
            {
                turnSpeed = 3f; // Ensure minimum turn speed
            }
        }
    }




    Vector3 CalculateNavigationVector()
    {
        Transform target = waypoints[currentWaypoint_nav];
        return (target.position - transform.position).normalized;
    }

    Vector3 CalculateAvoidanceVector()
    {
        Vector3 avoidanceDirection = Vector3.zero;
        bool obstacleDetected = false;
        float closestHitDistance = obstacleDetectionDistance;

        // Cast multiple rays in a fan pattern
        for (int i = 0; i < rayCount; i++)
        {
            // Calculate angle for this ray
            float angle = -rayAngle / 2f + rayAngle * i / (rayCount - 1);
            Vector3 rayDirection = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;

            // Cast ray
            RaycastHit hit;
            if (Physics.Raycast(transform.position, rayDirection, out hit, obstacleDetectionDistance, obstacleLayer))
            {
                if (!hit.collider.CompareTag("Waypoint"))
                {
                    //  Debug.Log($"Hit: {hit.collider.name} at distance: {hit.distance}");
                    obstacleDetected = true;

                    // Calculate avoidance strength based on distance
                    float weight = 1.0f - Mathf.Clamp01((hit.distance - minAvoidanceDistance) /
                                                       (obstacleDetectionDistance - minAvoidanceDistance));
                    weight = Mathf.Pow(weight, 2); // Exponential falloff for more responsive avoidance

                    // Calculate avoidance direction perpendicular to hit normal
                    Vector3 avoidDir = Vector3.Cross(hit.normal, Vector3.up);

                    // Make sure we're turning away from the obstacle
                    if (Vector3.Dot(avoidDir, transform.right) < 0)
                        avoidDir = -avoidDir;

                    // Weight by distance and angle importance
                    avoidanceDirection += avoidDir.normalized * weight * (1.0f + Mathf.Abs(angle) / rayAngle);

                    // Track closest obstacle
                    if (hit.distance < closestHitDistance)
                    {
                        closestHitDistance = hit.distance;
                    }

                    // Remember this obstacle
                    recentObstacles[hit.collider] = Time.time + obstacleMemoryTime;
                }
            }
        }

        // Check for obstacles we've recently detected but might not be in direct raycast
        foreach (var obstacle in recentObstacles)
        {
            if (obstacle.Key != null && obstacle.Value > Time.time)
            {
                Vector3 dirToObstacle = obstacle.Key.transform.position - transform.position;
                float distance = dirToObstacle.magnitude;

                // Only consider if within detection range
                if (distance < obstacleDetectionDistance)
                {
                    // Calculate direction perpendicular to obstacle direction
                    Vector3 avoidDir = Vector3.Cross(dirToObstacle.normalized, Vector3.up);

                    // Ensure consistent turning direction
                    if (Vector3.Dot(avoidDir, transform.right) < 0)
                        avoidDir = -avoidDir;

                    // Weight by memory recency and distance
                    float timeWeight = (obstacle.Value - Time.time) / obstacleMemoryTime;
                    float distWeight = 1.0f - Mathf.Clamp01(distance / obstacleDetectionDistance);

                    avoidanceDirection += avoidDir.normalized * distWeight * timeWeight * 0.5f;
                    obstacleDetected = true;
                }
            }
        }

        // Calculate final avoidance vector
        if (obstacleDetected)
        {
            // Normalize and apply strength based on closest obstacle
            avoidanceDirection = avoidanceDirection.normalized;
            float urgencyFactor = Mathf.Clamp01((obstacleDetectionDistance - closestHitDistance) / obstacleDetectionDistance);
            avoidanceDirection *= avoidStrength * urgencyFactor;

            // Smoothly blend with previous avoidance direction
            avoidanceVector = Vector3.Lerp(avoidanceVector, -avoidanceDirection, Time.fixedDeltaTime * 5f);
            return avoidanceVector;
        }

        // If no obstacles, gradually return to zero avoidance
        avoidanceVector = Vector3.Lerp(avoidanceVector, Vector3.zero, Time.fixedDeltaTime * 3f);
        return avoidanceVector;
    }

    void AdjustSpeed(float avoidanceMagnitude)
    {
        // Reduce speed when avoiding obstacles
        float targetSpeed = speed;
        if (avoidanceMagnitude > 0)
        {
            // Reduce speed proportional to avoidance strength
            targetSpeed = speed * (1f - (speedReductionFactor * Mathf.Clamp01(avoidanceMagnitude / avoidStrength)));
        }

        // Smoothly adjust speed
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.fixedDeltaTime * 3f);
    }

    void UpdateObstacleMemory()
    {
        // Remove expired obstacle memories
        List<Collider> expiredObstacles = new List<Collider>();
        foreach (var obstacle in recentObstacles)
        {
            if (obstacle.Value < Time.time || obstacle.Key == null)
            {
                expiredObstacles.Add(obstacle.Key);
            }
        }

        foreach (var expired in expiredObstacles)
        {
            recentObstacles.Remove(expired);
        }
    }

    void OnDrawGizmos()
    {
        if (!debugDraw || !Application.isPlaying) return;

        // Draw rays
        for (int i = 0; i < rayCount; i++)
        {
            float angle = -rayAngle / 2f + rayAngle * i / (rayCount - 1);
            Vector3 rayDirection = Quaternion.AngleAxis(angle, transform.up) * transform.forward;

            // Draw ray
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, rayDirection * obstacleDetectionDistance);
        }

        // Draw avoidance vector
        if (avoidanceVector.magnitude > 0.1f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, avoidanceVector.normalized * 3f);
        }

        // Draw path to current waypoint
        if (waypoints.Length > 0 && currentWaypoint_nav < waypoints.Length)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, waypoints[currentWaypoint_nav].position);
        }

        // Draw remembered obstacles
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Orange
        foreach (var obstacle in recentObstacles)
        {
            if (obstacle.Key != null)
            {
                Gizmos.DrawLine(transform.position + Vector3.up, obstacle.Key.transform.position);
            }
        }
    }
    #region Power-Up
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
                break;
            case PowerUpType.Puddle:
                instantiatedPuddle = Instantiate(puddlePrefab, missileSpawnPoint.position, Quaternion.identity, missileSpawnPoint);
                break;
            case PowerUpType.Missile:
                instantiatedMissile = Instantiate(missileProjectilePrefab, missileSpawnPoint.position, Quaternion.identity, missileSpawnPoint);
                break;
            case PowerUpType.Money:
                break;
            case PowerUpType.Random:
                InstantiateRandomPowerUp();
                break;
        }
        int randomIndex = Random.Range(0, 100);
        if (randomIndex < hitChance)
        {
            // Activate the power-up immediately
            float timeBeforeActivation = Random.Range(1f, 3f);
            StartCoroutine(ActivatePowerUpCoroutine(timeBeforeActivation));
        }

        // Play sound effect
        //if (powerUpPickupSound != null && audioSource != null)
        //    audioSource.PlayOneShot(powerUpPickupSound);

        // Update UI
        //UpdatePowerUpUI();

    }
    void InstantiateRandomPowerUp()
    {
        int randomIndex = Random.Range(0, 3);
        switch (randomIndex)
        {
            case 0:
                CollectPowerUp(PowerUpType.Puddle);
                break;
            case 1:
                CollectPowerUp(PowerUpType.Missile);
                break;
            case 2:
                CollectPowerUp(PowerUpType.Speed);
                break;
            default:
                Debug.LogWarning("Unknown power-up type: " + randomIndex);
                break;
        }
    }
    IEnumerator ActivatePowerUpCoroutine(float timeBeforeActivation)
    {
        yield return new WaitForSeconds(timeBeforeActivation);
        ActivatePowerUp();
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
    }


    void ActivatePowerUp()
    {
        if (storedPowerUps.Count > 0 && canUsePowerUp)
        {
            canUsePowerUp = false;

            PowerUp powerUp = storedPowerUps[0];
            storedPowerUps.RemoveAt(0);
            Debug.Log("Activating power-up: " + powerUp.type.ToString());

            switch (powerUp.type)
            {
                case PowerUpType.Speed:
                    if (activePowerUpCoroutine != null)
                        StopCoroutine(activePowerUpCoroutine);
                    activePowerUpCoroutine = StartCoroutine(SpeedPowerUpCoroutine(
                    speedPowerUpForceMultiplier * powerUp.strength,
                    speedPowerUpTorqueMultiplier * powerUp.strength,
                    powerUp.duration));
                    break;

                case PowerUpType.Puddle:
                    FirePuddle();
                    StartCoroutine(PowerUpCooldown(powerUpCooldown));
                    break;
                case PowerUpType.Missile:
                    FireMissile();
                    StartCoroutine(PowerUpCooldown(powerUpCooldown));
                    break;
                case PowerUpType.Money:
                    //JumpPowerUp();
                    StartCoroutine(PowerUpCooldown(powerUpCooldown));
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
        speed *= 3;
        yield return new WaitForSeconds(duration);

        isPowerUpActive = false;
        speed /= 2;
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
        Debug.Log("In puddle");
        slipMultiplier = 0.35f;
        yield return new WaitForSeconds(2f);

        slipMultiplier = 1f;
        // Debug.Log("Out of puddle" + normalForwardStiffness + " " + normalSidewaysStiffness);
    }

    IEnumerator PowerUpCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canUsePowerUp = true;
    }


    #endregion

    #region Position
        private void CreatePositionDisplay()
    {
        // Create a new GameObject as a child for the text display
        GameObject textObj = new GameObject("PositionDisplay");
        textObj.transform.parent = transform;
        textObj.transform.localPosition = new Vector3(0, 2.5f, 0); // Above the car
        textObj.transform.localRotation = Quaternion.identity;
        
        // Add a TextMesh component
        positionTextMesh = textObj.AddComponent<TextMesh>();
        positionTextMesh.fontSize = 24;
        positionTextMesh.alignment = TextAlignment.Center;
        positionTextMesh.anchor = TextAnchor.MiddleCenter;
        positionTextMesh.color = Color.yellow;
        positionTextMesh.text = "0";
        
        // Make the text face the camera
       // textObj.AddComponent<Billboard>();
    }
    
    public override void OnPositionChanged(int newPosition)
    {
        base.OnPositionChanged(newPosition);
        
        // Update the visual position display if it exists
        if (positionTextMesh != null)
        {
            string suffix = GetPositionSuffix(newPosition);
           // positionTextMesh.text = newPosition + suffix;
        }
    }
    
    private string GetPositionSuffix(int position)
    {
        if (position % 100 >= 11 && position % 100 <= 13)
        {
            return "th";
        }
        
        switch (position % 10)
        {
            case 1: return "st";
            case 2: return "nd";
            case 3: return "rd";
            default: return "th";
        }
    }
    
    #endregion

}


// Simple component to make text always face the camera
// public class Billboard : MonoBehaviour
// {
//     void Update()
//     {
//         if (Camera.main != null)
//         {
//             transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
//                              Camera.main.transform.rotation * Vector3.up);
//         }
//     }
// }