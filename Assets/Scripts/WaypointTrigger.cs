using UnityEngine;
using System.Collections.Generic;

public class WaypointTrigger : MonoBehaviour
{
    public int waypointIndex;    // The index of this waypoint in the track sequence
    
    // Optional: Timer to prevent multiple triggers in quick succession
    public float triggerCooldown = 1.0f;
    private Dictionary<int, float> lastTriggerTime = new Dictionary<int, float>();
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if any racer (player or AI) entered this waypoint
        // Use a common "Racer" tag for both human and AI racers
        if (other.CompareTag("P1") || other.CompareTag("P2") || other.CompareTag("AICar"))
        {
            // Get instance ID to track cooldowns per vehicle
            int objectId = other.gameObject.GetInstanceID();
            
            // Check if this object has triggered recently
            if (lastTriggerTime.ContainsKey(objectId))
            {
                if (Time.time - lastTriggerTime[objectId] < triggerCooldown)
                {
                    // Still in cooldown, ignore this trigger
                    return;
                }
            }
            
            // Update trigger time
            lastTriggerTime[objectId] = Time.time;
            
            // First try to get the new base RacerController
            RacerController racerController = other.GetComponent<RacerController>();

            racerController.lastWaypoint = transform;
            racerController.OnWaypointPassed(waypointIndex);
            WrongDirectionDetection wrongDirDetection = other.GetComponent<WrongDirectionDetection>();
            if (wrongDirDetection != null)
            {
                wrongDirDetection.SetCurrentWaypoint(waypointIndex);
                wrongDirDetection.CheckWrongDirection();
            }

        }
    }
}