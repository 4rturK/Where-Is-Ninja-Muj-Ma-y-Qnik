using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeDamagePhysical : MonoBehaviour
{
    public float damage = 7;
    public float pushForce = 5f;
    public string attackedTag = "Enemy";

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(attackedTag))
        {
            // Zadaj obra¿enia
            EnemyLife hitted = collision.gameObject.GetComponent<EnemyLife>();
            if (hitted != null)
            {
                hitted.takeDamage(damage);
            }

            // Spróbuj odepchn¹æ obiekt (jeœli ma Rigidbody)
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Kierunek od cz¹stki do przeciwnika
                Vector3 pushDir = (collision.transform.position - transform.position).normalized;
                rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
            }
        }
    }
}
