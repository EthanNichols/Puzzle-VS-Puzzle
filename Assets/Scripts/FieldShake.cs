using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldShake : MonoBehaviour
{

    public Vector3 originalPosition;
    public float shakeDuration;

    public float shakeAmount = 10f;

    public Vector3 shakePosition = Vector3.zero;
    public Vector3 shakeDirection = Vector3.zero;

    // Use this for initialization
    void Start()
    {
        originalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (shakeDuration > 0) Shake();
        else ReturnToPosition();
    }

    private void ReturnToPosition()
    {
        transform.localPosition = originalPosition;
        shakeDirection = Vector3.zero;
        shakePosition = Vector3.zero;
    }

    private void Shake()
    {
        shakeDuration -= Time.deltaTime;
        shakeDirection.Normalize();

        Debug.Log(shakePosition);

        if (shakeDirection != Vector3.zero && shakePosition != Vector3.zero)
        {
            transform.localPosition = originalPosition + (Vector3)Random.insideUnitCircle * shakeAmount;
        }
        if (shakePosition != Vector3.zero)
        {
            Debug.Log("hi");
            transform.localPosition = originalPosition + shakePosition;
        }
    }
}
