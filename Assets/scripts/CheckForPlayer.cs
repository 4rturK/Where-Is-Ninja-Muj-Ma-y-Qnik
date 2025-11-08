using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForPlayer : MonoBehaviour
{
    [Header("Ustawienia sto¿ka")]
    public float viewDistance = 10f;
    public float viewAngle = 45f;
    public LayerMask layerMask;

    public float viewDistanceEveryDir = 5f;

    [Header("Wynik")]
    public bool isTargetVisible;
    public GameObject targetGameObject;

    public Color coneColor = Color.green;
    public Color detectedColor = Color.red;

    

    void Update()
    {
        isTargetVisible = CheckForTargetInCone();
    }

    bool CheckForTargetInCone()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance, layerMask);

        foreach (var hit in hits)
        {
            Vector3 dirToTarget = (hit.transform.position - transform.position);
            float dist = dirToTarget.magnitude;
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget.normalized);

            if (angleToTarget < viewAngle || dist < viewDistanceEveryDir)
            {
                targetGameObject = hit.gameObject;
                return true;
            }
        }

        return false;
    }
}
