using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BasicAttack
{
    protected ParticleSystem particle;
    protected KeyCode keyCode;
    protected Animator animator;

    public virtual void onUpdateFunc() 
    {
        if (Input.GetKeyDown(keyCode))
        {
            animator.SetTrigger("Melee1");

            particle.Play();
        }
    }

    public BasicAttack(ParticleSystem particle, KeyCode key, Animator animator)
    {
        this.particle = particle;
        this.keyCode = key;
        this.animator = animator;
    }
}



public class ContinuousAttack : BasicAttack
{
    //static readonly KeyCode[] breakKeys = new KeyCode[]
    //{
    //    KeyCode.W,
    //    KeyCode.A,
    //    KeyCode.S,
    //    KeyCode.D,
    //    KeyCode.Mouse0,
    //    KeyCode.Mouse1,
    //    KeyCode.Mouse3,
    //    KeyCode.Mouse4
    //};

    public override void onUpdateFunc()
    {
        if (Input.GetKey(keyCode))
        {
            animator.SetBool("CastingContinuousSpell1", true);
            particle.Play();
        }
        else
        {
            if (particle.isPlaying)
            {
                animator.SetBool("CastingContinuousSpell1", false);
                particle.Stop();
            }
        }
    }

    public ContinuousAttack(ParticleSystem particle, KeyCode key, Animator animator)
        : base(particle, key, animator) { }
}

public class AttacksManager : MonoBehaviour 
{
    private string[] continuousAttacks = { "Fire" };

    [Header("Available Particles")]
    public List<ParticleSystem> availableAttacks = new List<ParticleSystem>();

    [Header("Assigned Attacks")]
    public ParticleSystem leftAttackParticle;
    public ParticleSystem rightAttackParticle;
    public ParticleSystem add1AttackParticle;
    public ParticleSystem add2AttackParticle;

    BasicAttack leftClickAttack;
    BasicAttack rightClickAttack;
    BasicAttack additionalAttack1;
    BasicAttack additionalAttack2;

    public ManagerGame gameManager;

    public Animator animator;

    void Start()
    {

        ApplySelectedAttacks();
    }
    void Update()
    {
        if(gameManager.gameLoop)
        {
            RotateToMouse();

            leftClickAttack.onUpdateFunc();
            rightClickAttack.onUpdateFunc();
            additionalAttack1.onUpdateFunc();
            additionalAttack2.onUpdateFunc();

        }
        
    }

    public void ApplySelectedAttacks()
    {
        leftClickAttack = continuousAttacks.Contains(leftAttackParticle.name) ? new ContinuousAttack(leftAttackParticle, KeyCode.Mouse0, animator) : new BasicAttack(leftAttackParticle, KeyCode.Mouse0, animator);
        rightClickAttack = continuousAttacks.Contains(rightAttackParticle.name) ? new ContinuousAttack(rightAttackParticle, KeyCode.Mouse1, animator) : new BasicAttack(rightAttackParticle, KeyCode.Mouse1, animator);
        additionalAttack1 = continuousAttacks.Contains(add1AttackParticle.name) ? new ContinuousAttack(add1AttackParticle, KeyCode.Mouse4, animator) : new BasicAttack(add1AttackParticle, KeyCode.Mouse4, animator);
        additionalAttack2 = continuousAttacks.Contains(add2AttackParticle.name) ? new ContinuousAttack(add2AttackParticle, KeyCode.Mouse3, animator) : new BasicAttack(add2AttackParticle, KeyCode.Mouse3, animator);
    }

    void RotateToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            Vector3 target = hit.point;
            target.y = transform.position.y;

            Vector3 direction = (target - transform.position).normalized;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
