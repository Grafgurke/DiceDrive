using UnityEngine;
using UnityEditor;
public class RaceManagerSetup : MonoBehaviour
{
    // This script helps to easily set up the race manager with track information
    public GameObject[] waypoints;
    
    private void Awake()
    {
        // Make sure there is a RacePositionManager component on this GameObject
        RacePositionManager raceManager = GetComponent<RacePositionManager>();
        if (raceManager == null)
        {
            raceManager = gameObject.AddComponent<RacePositionManager>();
        }
        
        // Set the total number of waypoints
        raceManager.totalWaypoints = waypoints.Length;
    }
    
    // Utility function to help set up waypoint indices automatically
    public void SetupWaypointIndices()
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            WaypointTrigger trigger = waypoints[i].GetComponent<WaypointTrigger>();
            if (trigger != null)
            {
                trigger.waypointIndex = i;
                Debug.Log($"Set waypoint {waypoints[i].name} index to {i}");
            }
            else
            {
                Debug.LogWarning($"Waypoint {waypoints[i].name} does not have a WaypointTrigger component!");
            }
        }
    }
}

// Custom Editor to add a button to the component
#if UNITY_EDITOR


[CustomEditor(typeof(RaceManagerSetup))]
public class RaceManagerSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        RaceManagerSetup setup = (RaceManagerSetup)target;
        
        if (GUILayout.Button("Setup Waypoint Indices"))
        {
            setup.SetupWaypointIndices();
        }
    }
}
#endif