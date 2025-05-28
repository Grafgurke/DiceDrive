using System.Collections.Generic;
using UnityEngine;

public class RacePositionManager : MonoBehaviour
{
    // Singleton instance for global access
    public static RacePositionManager Instance { get; private set; }
    
    // List of all racers in the race (both human players and AI)
    private List<RacerController> racers = new List<RacerController>();
    
    // Total number of waypoints in the track
    public int totalWaypoints = 0;
    
    // Whether to show detailed position debugging
    public bool debugPositionCalculation = false;
    
    private void Awake()
    {
        // Setup singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Register a racer (player or AI) to the race
    public void RegisterRacer(RacerController racer)
    {
        if (!racers.Contains(racer))
        {
            racers.Add(racer);
            
            // Initially update all positions
            UpdateAllPositions();
        }
    }
    
    // Update racer progress when they hit a waypoint
    public void UpdateRacerProgress(RacerController racer)
    {
        // Update positions of all racers
        UpdateAllPositions();
    }
    
    // Calculate and update positions for all racers
    private void UpdateAllPositions()
    {
        if (totalWaypoints <= 0)
        {
            Debug.LogWarning("RacePositionManager: totalWaypoints is not set properly!");
            totalWaypoints = 1; // Prevent division by zero
        }
        
        // Sort racers by their progress
        List<RacerController> sortedRacers = new List<RacerController>(racers);
        
        // Log progress details if debugging is enabled
        if (debugPositionCalculation)
        {
            Debug.Log("=== Race Position Calculation ===");
            foreach (var racer in racers)
            {
                Debug.Log($"{racer.racerName}: Lap {racer.currentLap}, WP {racer.currentWaypoint}, " +
                          $"Total WPs {racer.totalWaypointsPassed}, Time {racer.lastWaypointTime}");
            }
        }
        
        // Sort racers by: 
        // 1. Overall progress value (laps * totalWaypoints + current waypoint)
        // 2. Total waypoints passed (as tiebreaker)
        // 3. Earlier checkpoint time (as secondary tiebreaker)
        sortedRacers.Sort((r1, r2) => {
            // Compare progress values
            int progress1 = r1.GetProgressValue(totalWaypoints);
            int progress2 = r2.GetProgressValue(totalWaypoints);
            
            int progressComparison = progress2.CompareTo(progress1);
            if (progressComparison != 0) return progressComparison;
            
            // If same progress value, compare total waypoints passed
            int waypointComparison = r2.totalWaypointsPassed.CompareTo(r1.totalWaypointsPassed);
            if (waypointComparison != 0) return waypointComparison;
            
            // If still tied, the one who hit the waypoint earlier gets priority
            return r1.lastWaypointTime.CompareTo(r2.lastWaypointTime);
        });
        
        // Assign positions to all racers
        for (int i = 0; i < sortedRacers.Count; i++)
        {
            int position = i + 1;
            sortedRacers[i].OnPositionChanged(position);
            
            if (debugPositionCalculation)
            {
                Debug.Log($"Position {position}: {sortedRacers[i].racerName}");
            }
        }
    }
}