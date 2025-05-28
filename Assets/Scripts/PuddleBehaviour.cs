using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class PuddleBehaviour : MonoBehaviour
{
    private float speed = 12; 
    Rigidbody rb; 
    private CarController carController; 
    public GameObject actualPuddlePrefab;
    public bool isPuddle = false; // Flag to check if the object is a puddle
    public void StartPuddle()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 10f;

        // Forward + upward direction
        Vector3 throwDirection = (-transform.forward + transform.up).normalized;

        transform.rotation = Quaternion.LookRotation(throwDirection);

        rb.AddForce(throwDirection * speed, ForceMode.Impulse);
    }

    
    void OnCollisionEnter(Collision collision)
    {
        if(!isPuddle && collision.gameObject.CompareTag("Road"))
        {
            Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y -.24f, transform.position.z);
            Instantiate(actualPuddlePrefab, spawnPosition, Quaternion.identity);
            Destroy(gameObject); // Destroy the puddle object
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("P1") || other.CompareTag("P2") && isPuddle)
        {
            carController = other.GetComponent<CarController>();
            StartCoroutine(carController.InPuddle());
            StartCoroutine(DestroyPuddle());
        }
    }
    private IEnumerator DestroyPuddle()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject); // Destroy the puddle object
    }

}
