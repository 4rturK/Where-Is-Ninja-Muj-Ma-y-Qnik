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
            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);

            if (angleToTarget < viewAngle)
            {
                targetGameObject = hit.gameObject;
                return true;
            }
        }

        return false;
    }
}
