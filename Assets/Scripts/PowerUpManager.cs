using UnityEngine;
public class PowerUpManager : MonoBehaviour
{
    public PowerUpType powerUpType;
    public float powerUpDuration = 1.5f;
    public float powerUpStrength = 1.0f;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("P1") || other.CompareTag("P2"))
        {
            CarController carController = other.GetComponent<CarController>();
            carController.CollectPowerUp(powerUpType, powerUpDuration, powerUpStrength);
            //deactivate parent gameObject
            this.gameObject.transform.parent.gameObject.SetActive(false); // Disable the power-up object
        }
        if (other.CompareTag("AICar"))
        {
            AICarController aiCarController = other.GetComponent<AICarController>();
            aiCarController.CollectPowerUp(powerUpType, powerUpDuration, powerUpStrength);
            this.gameObject.transform.parent.gameObject.SetActive(false); // Disable the power-up object
        }
    }
}
