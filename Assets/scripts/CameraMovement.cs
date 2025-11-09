using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform LookAt;

    private void Start()
    {
        transform.rotation = Quaternion.Euler(60, 0, 0);
    }

    void Update()
    {
        if (LookAt == null) return;
        transform.position = LookAt.position + new Vector3(0, 12, -5);
    }
}
