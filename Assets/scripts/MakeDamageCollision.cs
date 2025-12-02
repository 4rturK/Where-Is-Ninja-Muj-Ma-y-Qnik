using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeDamageCollision : MonoBehaviour
{
    public float damage = 7;
    public float pushForce = 5f;
    public string attackedTag = "Enemy";

    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag(attackedTag))
        {
            // Zadaj obra¿enia
            EnemyLife hitted = other.GetComponent<EnemyLife>();
            if (hitted != null)
            {
                hitted.takeDamage(damage);
            }

            // Spróbuj odepchn¹æ obiekt (jeœli ma Rigidbody)
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Kierunek od cz¹stki do przeciwnika
                Vector3 pushDir = (other.transform.position - transform.position).normalized;
                rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
            }
        }
    }
}
