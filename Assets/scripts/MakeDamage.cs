using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeDamage : MonoBehaviour
{
    public float damage = 7;
    public string attackedTag = "Enemy";

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log(other.ToString());
        if (other.CompareTag(attackedTag))
        {

            EnemyLife hitted = other.GetComponent<EnemyLife>();
            Debug.Log(hitted.ToString());
            hitted.takeDamage(damage);
        }
    }
}
