using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestScenarioSelector : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private GameObject Car1;
    [SerializeField] private GameObject Car1LookAt;
    [SerializeField] private WheelCollider[] wheelColliders;
    [SerializeField] private GameObject Car2;
    [SerializeField] private GameObject Car2LookAt;
    [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
    [SerializeField] private int scenarioIndex = 0; // Default scenario index
    [SerializeField] private GameObject selectionPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SelectScenario(int scenarioIndex)
    {
        switch (scenarioIndex)
        {
            case 0:
                cinemachineCamera.Follow = Car1.transform;
                cinemachineCamera.LookAt = Car1LookAt.transform;
                descriptionText.text = "Scenario 1 use space to break/drift. Reload/restart with escape";
                Car1.SetActive(true);
                StartCoroutine(WaitSeconds(.5f)); // Wait for 2 seconds before activating the car
                break;
            case 1:
                cinemachineCamera.Follow = Car2.transform;
                cinemachineCamera.LookAt = Car2LookAt.transform;
                descriptionText.text = "Scenario 2 only controll with w a s d. Reload/restart with escape";
                Car2.SetActive(true);
                Car1.SetActive(false);
                break;
            default:
                Debug.LogError("Invalid scenario index: " + scenarioIndex);
                break;
        }
        selectionPanel.SetActive(false);
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(this.gameObject.scene.name);
        }
    }
    IEnumerator WaitSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        foreach (WheelCollider wheelCollider in wheelColliders)
        {
            wheelCollider.enabled = false;
        }
        yield return new WaitForSeconds(seconds);
        foreach (WheelCollider wheelCollider in wheelColliders)
        {
            wheelCollider.enabled = true;
        }
    }
}
