using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwayManager : MonoBehaviour
{
    [Tooltip("How much the tower sways (in degrees)")]
    [SerializeField]
    float baseSwayAmount = 2;
    [Tooltip("How fast the sway moves back and forth")]
    [SerializeField]
    float swaySpeed = 1;

    Quaternion initialRotation;

    private void Start()
    {
        initialRotation = transform.localRotation;
    }

    private void Update()
    {

        float swayAmount = (baseSwayAmount + BlockCount()) * .1f;
        float xSway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        float zSway = Mathf.Sin((Time.time + .5f) * swaySpeed) * swayAmount;

        transform.localRotation = initialRotation * Quaternion.Euler(xSway, 0, zSway);
    }

    int BlockCount()
    {
        int count = 0;
        foreach (var block in GameObject.FindGameObjectsWithTag("Placed"))
        {
            var rb = block.GetComponent<Rigidbody>();
            if (rb != null) count++;
        }
        return count;
    }
}
