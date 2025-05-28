using TMPro;
using UnityEngine;

public class CarUI : MonoBehaviour
{
    public Rigidbody carRigidbody;        // Assign the car's Rigidbody
    public TMP_Text speedText;
    public RectTransform speedIndicator;                // Assign a UI Text element in the inspector

    void Update()
    {
        UpdateSpeedometer();
    }

    void UpdateSpeedometer()
{
    float speed = carRigidbody.linearVelocity.magnitude * 3.6f; // Convert m/s to km/h
    speedText.text = Mathf.RoundToInt(speed).ToString();

    // Clamp speed between 0 and 100 to stay within expected range
    float clampedSpeed = Mathf.Clamp(speed, 0f, 100f);

    // Map speed (0–100) to angle (160–-87)
    float angle = Mathf.Lerp(160f, -87f, clampedSpeed / 100f);

    // Apply rotation around Z axis
    speedIndicator.localRotation = Quaternion.Euler(0f, 0f, angle);
}

}
