using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DunderInator : MonoBehaviour
{
    public float seconds = 5f;

    private void Start()
    {
        Invoke(nameof(Autodestruction), seconds);
    }

    void Autodestruction()
    {
        Destroy(gameObject);
    }
}
