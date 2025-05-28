using UnityEngine;

public class WrongDirectionDetection : MonoBehaviour
{
    public float checkInterval = 1f; // Time interval to check for wrong direction
    public float wrongDirectionThreshold = 90f;
    public float minSpeed = 5f; // Minimum speed to consider the car moving
    public TMPro.TextMeshProUGUI wrongDirectionText; // Reference to the UI text element
    private Rigidbody rb;
    private bool isWrongDirection = false;
    public Transform[] trackWaypoints; // Array of track waypoints
    private int currentWaypointIndex = 0; // Index of the current waypoint
    private float lastCheckTime = 0f; // Time of the last check
    private float wrongDirDetectionTime = 0f; // Time when wrong direction was detected
    public ResetPosition resetPosition; // Reference to the ResetPosition script

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (wrongDirectionText != null)
            wrongDirectionText.gameObject.SetActive(false); // Hide the text at the start
    }

    void Update()
    {
        if (Time.time - lastCheckTime >= checkInterval)
        {
            CheckWrongDirection();
            lastCheckTime = Time.time;
        }
        if(isWrongDirection)
        {
            wrongDirDetectionTime += Time.deltaTime;
            if(wrongDirDetectionTime >= 2f) // 2 seconds of wrong direction
            {
              
                wrongDirectionText.gameObject.SetActive(true); 
                wrongDirectionText.text = "Wrong Direction!";
                
            }
            if (wrongDirDetectionTime >= 4f) // 3 seconds of wrong direction
            {
                // Reset the car's position or take any other action
                resetPosition.ResetPositionOfCar(gameObject);
                wrongDirDetectionTime = 0f; // Reset the timer
            }
        }
        else
        {
            wrongDirDetectionTime = 0f; // Reset the timer if not in wrong direction
        }
    }

    public void CheckWrongDirection()
    {
        // Make sure we have waypoints and we're not on the last waypoint
        if (trackWaypoints == null || trackWaypoints.Length <= 1 || 
            currentWaypointIndex >= trackWaypoints.Length - 1)
            return;

        if (rb.linearVelocity.magnitude > minSpeed)
        {
            Vector3 directionToWaypoint = (trackWaypoints[currentWaypointIndex + 1].position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToWaypoint);

            if (angle > wrongDirectionThreshold)
            {
                isWrongDirection = true;
            }
            else
            {
                isWrongDirection = false;
                if (wrongDirectionText != null)
                    wrongDirectionText.gameObject.SetActive(false); // Hide the text when in correct direction
            }
        }
        else
        {
            // Hide the warning when car is not moving fast enough
            if (isWrongDirection && wrongDirectionText != null)
            {
                isWrongDirection = false;
                wrongDirectionText.gameObject.SetActive(false);
            }
        }
    }

    public void SetCurrentWaypoint(int index)
    {
        if (trackWaypoints != null && index >= 0 && index < trackWaypoints.Length)
        {
            currentWaypointIndex = index;
        }
    }

    // For debugging purposes
    public bool IsWrongDirection()
    {
        return isWrongDirection;
    }
}