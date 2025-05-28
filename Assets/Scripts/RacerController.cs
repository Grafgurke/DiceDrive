using Unity.VisualScripting;
using UnityEngine;

public class RacerController : MonoBehaviour
{
    public int racerId;                  // Unique ID for this racer
    public string racerName = "Racer";   // Name of the racer
    public int currentPosition = 0;      // Current race position (1st, 2nd, etc.)
    public int currentWaypoint = 0;      // Current waypoint index
    public int currentLap = 0;           // Current lap number
    
    // Track racer progress 
    public int totalWaypointsPassed = 0; // Total number of waypoints passed (increases continuously)
    public float lastWaypointTime = 0f;  // Time when the last waypoint was passed
    
    public Transform lastWaypoint;       // Reference to the last waypoint passed
    private RoundTrigger roundTrigger;       // Reference to the round trigger (if any)

    // Called when a racer passes a waypoint

    public virtual void OnWaypointPassed(int waypointIndex)
    {
        // Record the time when this waypoint was passed (for tie-breaking)
        lastWaypointTime = Time.time;

        // Increment total waypoints counter
        totalWaypointsPassed++;
      //  Debug.Log($"{racerName} passed waypoint {waypointIndex}. Total waypoints passed: {totalWaypointsPassed}");

        // Update current waypoint
        currentWaypoint = waypointIndex;

        // Check for lap completion (when passing waypoint 0 after at least one waypoint)
        if (waypointIndex == 0 && totalWaypointsPassed > 1)
        {
            currentLap++;
            OnLapCompleted();
        }

        // Update position through the central manager
        if (RacePositionManager.Instance != null)
        {
            RacePositionManager.Instance.UpdateRacerProgress(this);
        }
    }
    
    // Calculate the racer's overall progress value (for position ranking)
    public int GetProgressValue(int totalWaypointsInTrack) 
    {
        // Formula: (Laps completed * total waypoints in track) + current waypoint index
        return (currentLap * totalWaypointsInTrack) + currentWaypoint;
    }

    // Called when a racer completes a lap
    public virtual void OnLapCompleted()
    {
      //  Debug.Log($"{racerName} completed lap {currentLap}");
        roundTrigger.UpdateFurthestLap(currentLap);
    }
    
    // Called when a racer's position changes
    public virtual void OnPositionChanged(int newPosition)
    {
        currentPosition = newPosition;
    }

    // Use this for initialization
    protected virtual void Start()
    {
        // Register with the race manager
        if (RacePositionManager.Instance != null)
        {
            RacePositionManager.Instance.RegisterRacer(this);
        }
        else
        {
            Debug.LogError("RacePositionManager not found! Make sure it exists in the scene.");
        }
        roundTrigger = FindFirstObjectByType<RoundTrigger>();
    }
}