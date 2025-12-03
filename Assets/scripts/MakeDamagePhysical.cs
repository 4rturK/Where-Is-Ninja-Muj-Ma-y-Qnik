using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeDamagePhysical : MonoBehaviour
{
    public float damage = 7;
    public float pushForce = 5f;
    public string attackedTag = "Enemy";
    public Animator animator;

    public bool destroyOnHit = true;

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Kolizja z dobrym tagiem");
        if (collision.gameObject.CompareTag(attackedTag))
        {
            //Debug.Log("Kolizja z dobrym tagiem");
            LifeSystem hit = collision.gameObject.GetComponent<LifeSystem>();
            if (hit != null)
            {
                hit.takeDamage(damage);
            }

            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Kierunek od cz¹stki do przeciwnika
                Vector3 pushDir = (collision.transform.position - transform.position).normalized;
                rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
            }

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
            animator.SetBool("Idle", false);
            animator.SetBool("Running", false);
            animator.SetBool("Attack", true);
        }
    }
}
