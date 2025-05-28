using System.Collections;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("P1") || other.gameObject.CompareTag("P2"))
        {
            ResetPositionOfCar(other.gameObject);
        }
        if(other.gameObject.CompareTag("AICar"))
        {
            AICarController aiCarController = other.gameObject.GetComponent<AICarController>();
            other.transform.position = aiCarController.lastWaypoint.position;
            other.transform.rotation = aiCarController.lastWaypoint.rotation;
            other.gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            other.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }

    public void ResetPositionOfCar(GameObject car)
    {
        CarController carController = car.GetComponent<CarController>();
        PlayerRacerController playerRacerController = car.GetComponent<PlayerRacerController>();
        car.transform.position = playerRacerController.lastWaypoint.position;
        car.transform.rotation = playerRacerController.lastWaypoint.rotation;
        car.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        car.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        carController.wasResettet = true;
        carController.ResetWheels();
    }


}
