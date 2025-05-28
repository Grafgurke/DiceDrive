using UnityEngine;
using UnityEngine.Splines;

public class FollowSplineCamera : MonoBehaviour
{
    public SplineContainer splineContainer;
    public float speed = 1.0f;
    private float t = 0f;
    public GameObject train;

    void Update()
    {
        if (splineContainer == null)
            return;

        t += speed * Time.deltaTime;
        t %= 1f; // Loop the path

        // Evaluate the spline position and rotation
        var curve = splineContainer.Spline;
        var position = curve.EvaluatePosition(t);
        var tangent = curve.EvaluateTangent(t);
        var rotation = Quaternion.LookRotation(tangent);

        transform.position = position;
        transform.rotation = rotation;
        float distance = Vector3.Distance(position, new Vector3(143.31189f, 2.96524334f, -128.992569f));
        if (distance < 1f)
        {
            // Stop the train when close to the target position
            train.SetActive(true);
        }
 
    }
}
