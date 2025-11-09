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
    public Animator animator;

    public bool isAlive()
    {
        return hp > 0;
    }

    public void takeDamage(float damage)
    {
        hp -= damage;
        healthbarForeground.fillAmount = hp/maxhp;
        if (healthbarCanvas != null)
        {
            healthbarCanvas?.gameObject.SetActive(true);
        }
    }
}


public class EnemyLife : LifeSystem
{
    void Update()
    {
        if (!isAlive())
        {
            //Destroy(gameObject);
            animator.SetTrigger("Death1");
            healthbarCanvas.gameObject.SetActive(false);
        }
    }
}
