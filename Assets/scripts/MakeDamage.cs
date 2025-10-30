using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeDamage : MonoBehaviour
{
    public float damage = 7;
    public string attackedTag = "Enemy";

    private ParticleSystem ps;
    private List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
    public LayerMask layer;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnParticleTrigger()
    {
        Debug.Log("aaaa");
        int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Outside, enter);

        for (int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle p = enter[i];
            //Debug.Log(layer);
            Collider[] hits = Physics.OverlapSphere(p.position, 0.1f, layer);
            foreach (var hit in hits)
            {
                Debug.Log(hit.gameObject.ToString());
                if (hit.CompareTag(attackedTag))
                {
                    EnemyLife hitted = hit.GetComponent<EnemyLife>();
                    if (hitted != null)
                        hitted.takeDamage(damage);
                }
            }
        }

        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Outside, enter);
    }
}