using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationAngle = 15f;    // degrees each side from starting rotation
    public float sweepSpeed    = 25f;    // degrees per second
    public float pauseDuration = 0.7f;   // pause at each extreme (seconds)

    private float _startAngle;

    void Start()
    {
        _startAngle = transform.eulerAngles.z;
        StartCoroutine(SweepRoutine());
    }

    IEnumerator SweepRoutine()
    {
        while (true)
        {
            // Sweep to +angle
            yield return SweepTo(_startAngle + rotationAngle);
            yield return new WaitForSeconds(pauseDuration);

            // Sweep to -angle
            yield return SweepTo(_startAngle - rotationAngle);
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    IEnumerator SweepTo(float targetAngle)
    {
        float current = transform.eulerAngles.z;

        // Normalize to [-180, 180] range for correct direction
        float delta = Mathf.DeltaAngle(current, targetAngle);

        while (Mathf.Abs(delta) > 0.1f)
        {
            float step = Mathf.Sign(delta) * sweepSpeed * Time.deltaTime;
            if (Mathf.Abs(step) > Mathf.Abs(delta)) step = delta;

            transform.rotation = Quaternion.Euler(0f, 0f, transform.eulerAngles.z + step);

            current = transform.eulerAngles.z;
            delta = Mathf.DeltaAngle(current, targetAngle);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
    }
}

