using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerManager : MonoBehaviour
{
    [Tooltip("Root or base of the tower (usually bottom block or empty parent")]
    [SerializeField]
    public Transform towerRoot;
    
    [Tooltip("Maximum allowed tilt in degrees before collapse")]
    [SerializeField]
    float maxTiltAngle = 10f;

    int blockCount;

    private void Start()
    {
        blockCount = 0;
        if (towerRoot == null) towerRoot = transform;
    }

    void Update()
    {
        Vector3 tilt = towerRoot.transform.rotation.eulerAngles;

        float xTilt = NormalizeAngle(tilt.x);
        float zTilt = NormalizeAngle(tilt.z);

        if (Mathf.Abs(xTilt) > maxTiltAngle || Mathf.Abs(zTilt) > maxTiltAngle) CollapseTower();
    }

    float NormalizeAngle(float angle)
    {
        while (angle > 360) angle -= 360;
        if (angle > 180) angle -= 360;
        return angle;
    }

    void CollapseTower()
    {
        foreach (var block in GameObject.FindGameObjectsWithTag("Placed"))
        {
            var rb = block.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
    }
}
