using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeSystem: MonoBehaviour
{
    public float maxhp = 100;
    public float hp = 100;
    public Image healthbarForeground;
    public Canvas healthbarCanvas;

    public bool isAlive()
    {
        return hp > 0;
    }

    public void takeDamage(float damage)
    {
        hp -= damage;
        healthbarCanvas?.gameObject.SetActive(true);
        healthbarForeground.fillAmount = hp/maxhp;
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
