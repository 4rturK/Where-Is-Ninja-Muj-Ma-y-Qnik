using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLikeCamera : MonoBehaviour
{
    private Camera camera;

    private void Awake()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = transform.position - camera.transform.position;

        direction.y = 0;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}
