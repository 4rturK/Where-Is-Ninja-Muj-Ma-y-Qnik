using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeSystem: MonoBehaviour
{
    public float hp = 100;

    public bool isAlive()
    {
        return hp > 0;
    }

    public void takeDamage(float damage)
    {
        hp -= damage;
    }
}


public class EnemyLife : LifeSystem
{
    void Update()
    {
        if (!isAlive())
        {
            Destroy(gameObject);
        }
    }
}
