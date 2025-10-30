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

public class BuildCast : BasicAttack 
{
    private GameObject gameobjectPrefab;
    public override void onUpdateFunc()
    {
        if (Input.GetKeyDown(keyCode))
        {
            animator.SetBool("CastingContinuousSpell1", true);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int groundMask = LayerMask.GetMask("Ground");
            if (Physics.Raycast(ray, out hit, 100f, groundMask))
            {
                Vector3 target = hit.point;
                UnityEngine.Object.Instantiate(gameobjectPrefab, target, animator.transform.rotation);
            }
        }
    }

    public BuildCast(KeyCode key, Animator animator, GameObject objectToInstance)
        : base(null, key, animator) { gameobjectPrefab = objectToInstance; }
}

public class AttacksManager : MonoBehaviour 
{
    private string[] continuousAttacks = { "Fire" };
    private string[] instanceActions = { "Build Wall" };

    [Header("Available Skills")]
    public List<ParticleSystem> availableAttacks = new List<ParticleSystem>();
    public List<GameObject> availableInstances = new List<GameObject>();

    [Header("Assigned Attacks")]
    public Object leftAttack;
    public Object rightAttack;
    public Object add1Attack;
    public Object add2Attack;

    BasicAttack leftClickAttack;
    BasicAttack rightClickAttack;
    BasicAttack additionalAttack1;
    BasicAttack additionalAttack2;

    public ManagerGame gameManager;

    public Animator animator;

    void Awake()
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
        leftClickAttack = CreateAttackForKey(leftAttack, KeyCode.Mouse0);
        rightClickAttack = CreateAttackForKey(rightAttack, KeyCode.Mouse1);
        additionalAttack1 = CreateAttackForKey(add1Attack, KeyCode.Mouse4);
        additionalAttack2 = CreateAttackForKey(add2Attack, KeyCode.Mouse3);
    }

    private BasicAttack CreateAttackForKey(UnityEngine.Object attackObject, KeyCode key)
    {
        if (attackObject == null)
            return new BasicAttack(null, key, animator);

        if (attackObject is GameObject go)
        {
            var ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                if (continuousAttacks.Contains(ps.name))
                    return new ContinuousAttack(ps, key, animator);
                else
                    return new BasicAttack(ps, key, animator);
            }

            if (instanceActions.Contains(go.name))
                return new BuildCast(key, animator, go);
        }

        if (attackObject is ParticleSystem directPs)
        {
            if (continuousAttacks.Contains(directPs.name))
                return new ContinuousAttack(directPs, key, animator);
            else
                return new BasicAttack(directPs, key, animator);
        }

        return new BasicAttack(null, key, animator);
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