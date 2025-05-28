using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class MissleBehaviour : MonoBehaviour
{
    public Vector3 target;
    private float speed = 50; // Speed of the missile
    Rigidbody rb; // Rigidbody component of the missile
    private CarController carController; // Reference to the CarController script
    bool isPlayer1Missile = false; // Flag to check if the missile is from Player 1
    public ParticleSystem flyingMissileEffect; // Particle effect for the missile
    public GameObject explosionEffect; // Explosion effect prefab
    private GameObject instantiatedExplosionEffect; // Reference to the instantiated explosion effect
    private bool destroyed = false; // Flag to check if the missile is destroyed
    public bool AIcar = false;
    public bool train = false;
    public float explosionForce = 5500; // Explosion force
    private AudioSource audioSource; // Audio source for the missile sound
    void Start()
    {
        if(!train)
        audioSource = GetComponent<AudioSource>();
        if (this.gameObject.layer == 7)
        {
            isPlayer1Missile = true;
        }
        else
        {
            isPlayer1Missile = false;
        }   
    }

    public void StartMissle()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        target = GetClosestCar(); // Get the closest
        Vector3 direction = (target - transform.position).normalized;

        transform.rotation = Quaternion.LookRotation(direction);

        rb.AddForce(transform.forward * speed, ForceMode.Impulse); // Add forward force to the missile
        flyingMissileEffect.Play(); // Start the flying missile effect
        Debug.Log("Missile started" + target);

    }
    private void LateUpdate()
    {
        target = GetClosestCar();
        // Rotate the missile to face the target
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    Vector3 GetClosestCar()
    {
        GameObject[] cars = GameObject.FindGameObjectsWithTag("AICar")
            .Concat(GameObject.FindGameObjectsWithTag("P1"))
            .Concat(GameObject.FindGameObjectsWithTag("P2"))
            .ToArray();

        List<GameObject> filteredCars = new List<GameObject>(cars);

        filteredCars.Remove(this.gameObject.transform.parent.parent.gameObject); 
        
        float closestDistance = Mathf.Infinity;
        Vector3 closestCar = Vector3.zero;
        foreach (GameObject car in filteredCars)
        {
            float distance = Vector3.Distance(transform.position, car.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCar = new Vector3(car.transform.position.x, car.transform.position.y + .35f, car.transform.position.z);
            }
        }
        return closestCar;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Vector3 explosionPosition =  new Vector3(collision.contacts[0].point.x, collision.contacts[0].point.y, collision.contacts[0].point.z);
        Vector3 explosionEffectPosition = new Vector3(collision.contacts[0].point.x, collision.contacts[0].point.y + .5f, collision.contacts[0].point.z);
        float explosionRadius = 15;
        if (!train && !destroyed)
        {
            audioSource.Play(); // Play the explosion sound
            instantiatedExplosionEffect = Instantiate(explosionEffect, explosionEffectPosition, Quaternion.identity); // Instantiate the explosion effect
        }

        // Find all nearby colliders
            Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            // Apply explosion force to all nearby rigidbodies except the missile itself
            if (rb != null && rb != this.rb)
            {
                hit.TryGetComponent<CarController>(out carController);
                if (carController != null)
                {
                ///  carController = hit.GetComponent<CarController>();
                    GameObject cameraLookAt = carController.cameraLookAt;
                    cameraLookAt.transform.localPosition = new Vector3(0,0,0);
                }
                
                rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, 2.25f, ForceMode.Impulse);
            }
        }
        if(!destroyed && !train)
        StartCoroutine(DestroyMissile());
    }

    private System.Collections.IEnumerator DestroyMissile()
    {
        destroyed = true; // Set the destroyed flag to true
        //this.gameObject.SetActive(false); // Deactivate the missile
        if(carController != null)
        {
            Debug.Log("Missile hit car: " + carController.gameObject.name);
            carController.hitByMissile = true; // Set the hitByMissile flag to true
        }
        yield return new WaitForSeconds(1f); // Wait for 1 second before destroying the missile
        Destroy(instantiatedExplosionEffect);
        Destroy(gameObject); // Destroy the missile
    }
}
