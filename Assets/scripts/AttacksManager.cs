using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MiniJSON;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;

public class AttackData
{
    public string type;    // "basic", "continuous", "instance"
    public float cooldown; // sekundy
}

public static class SimpleJsonParser
{
    public static Dictionary<string, AttackData> Parse(string json)
    {
        var dict = new Dictionary<string, AttackData>();
        var raw = Json.Deserialize(json) as Dictionary<string, object>;
        if (raw == null) return dict;

        foreach (var kvp in raw)
        {
            var sub = kvp.Value as Dictionary<string, object>;
            if (sub == null) continue;

            float cooldownValue = 0f;
            if (sub.TryGetValue("cooldown", out var val))
            {
                switch (val)
                {
                    case double d: cooldownValue = (float)d; break;
                    case long l: cooldownValue = l; break;
                    case string s:
                        s = s.Replace(',', '.'); // <- kluczowy krok!
                        float.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out cooldownValue);
                        break;
                }
            }

            dict[kvp.Key] = new AttackData
            {
                type = sub.ContainsKey("type") ? sub["type"].ToString() : "basic",
                cooldown = cooldownValue
            };

        }

        return dict;
    }

}

public class BasicAttack
{
    protected ParticleSystem particle;
    protected KeyCode keyCode;
    protected Animator animator;
    protected AttacksManager manager;
    protected string attackName;
    protected float cooldown;

    public BasicAttack(ParticleSystem particle, KeyCode key, Animator animator, AttacksManager manager, string name, float cooldown)
    {
        this.particle = particle;
        this.keyCode = key;
        this.animator = animator;
        this.manager = manager;
        this.attackName = name;
        this.cooldown = cooldown;

    }

    public virtual void onUpdateFunc()
    {
        if (manager.attackCooldowns[keyCode] > 0f) return;

        if (Input.GetKeyDown(keyCode))
        {
            manager.attackCooldowns[keyCode] = cooldown;
            animator.SetTrigger("Melee1");
            particle?.Play();

        }
    }

    public virtual void Trigger()
    {
        if (manager.attackCooldowns[keyCode] > 0f) return;

        manager.attackCooldowns[keyCode] = cooldown;
        animator.SetTrigger("Melee1");
        particle?.Play();
    }
}

public class ContinuousAttack : BasicAttack
{
    public ContinuousAttack(ParticleSystem particle, KeyCode key, Animator animator, AttacksManager manager, string name, float cooldown)
        : base(particle, key, animator, manager, name, cooldown) { }

    public override void onUpdateFunc()
    {
        if(Input.GetKeyUp(keyCode))
        {
            manager.attackCooldowns[keyCode] = cooldown;
            animator.SetBool("CastingContinuousSpell1", false);
            particle.Stop();
        }

        if (manager.attackCooldowns[keyCode] > 0f) return;

        if (Input.GetKey(keyCode))
        {
            animator.SetBool("CastingContinuousSpell1", true);
            particle?.Play();
        }
        else
        {
            if (particle?.isPlaying == true)
            {
                animator.SetBool("CastingContinuousSpell1", false);
                particle.Stop();
            }
        }
    }

    public void StartCast()
    {
        animator.SetBool("CastingContinuousSpell1", true);
        particle?.Play();
    }

    public void StopCast(bool applyCooldown)
    {
        animator.SetBool("CastingContinuousSpell1", false);
        particle?.Stop();

        if (applyCooldown)
            manager.attackCooldowns[keyCode] = cooldown;
    }

    public override void Trigger()
    {
        if (manager.attackCooldowns[keyCode] > 0f) return;

        manager.TriggerContinuousVoice(this);
    }
}

public class RangedAttack : BasicAttack
{
    private GameObject projectilePrefab;
    private Transform firePoint;
    private float shotPower;

    public RangedAttack(GameObject projectile, Transform firePoint, KeyCode key, Animator animator, AttacksManager manager, string name, float cooldown, float shotPower)
        : base(null, key, animator, manager, name, cooldown)
    {
        this.projectilePrefab = projectile;
        this.firePoint = firePoint;
        this.shotPower = shotPower;
    }

    public override void onUpdateFunc()
    {
        if (manager.attackCooldowns[keyCode] > 0f) return;

        if (Input.GetKeyDown(keyCode))
        {
            manager.attackCooldowns[keyCode] = cooldown;
            animator.SetTrigger("CrossbowShoot");

            if (projectilePrefab != null && firePoint != null)
            {
                GameObject arrow = UnityEngine.Object.Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                Rigidbody rb = arrow.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddRelativeForce(Vector3.forward * shotPower, ForceMode.Impulse);
            }
        }
    }

    public override void Trigger()
    {
        if (manager.attackCooldowns[keyCode] > 0f) return;

        manager.attackCooldowns[keyCode] = cooldown;
        animator.SetTrigger("CrossbowShoot");

        if (projectilePrefab != null && firePoint != null)
        {
            GameObject arrow = UnityEngine.Object.Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            if (rb != null)
                rb.AddRelativeForce(Vector3.forward * shotPower, ForceMode.Impulse);
        }
    }
}


public class BuildCast : BasicAttack
{
    private GameObject prefab;

    public BuildCast(KeyCode key, Animator animator, GameObject prefab, AttacksManager manager, string name, float cooldown)
        : base(null, key, animator, manager, name, cooldown)
    {
        this.prefab = prefab;
    }

    public override void onUpdateFunc()
    {
        if (manager.attackCooldowns[keyCode] > 0f) return;

        if (Input.GetKeyDown(keyCode))
        {
            manager.attackCooldowns[keyCode] = cooldown;
            animator.SetTrigger("CastingSpell1");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
            {
                UnityEngine.Object.Instantiate(prefab, hit.point, animator.transform.rotation);
            }

            if (manager.allAttacks.TryGetValue(attackName, out var data))
                manager.attackCooldowns[keyCode] = data.cooldown;
        }
    }

    public override void Trigger()
    {
        if (manager.attackCooldowns[keyCode] > 0f) return;

        manager.attackCooldowns[keyCode] = cooldown;
        animator.SetTrigger("CastingSpell1");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            UnityEngine.Object.Instantiate(prefab, hit.point, animator.transform.rotation);
        }

        if (manager.allAttacks.TryGetValue(attackName, out var data))
            manager.attackCooldowns[keyCode] = data.cooldown;
    }
}

public class AttacksManager : MonoBehaviour 
{
    //private string[] continuousAttacks = { "Fire" };
    //private string[] instanceActions = { "Build Wall" };
    //private string[] basicAttacks = { "Basic Slash", "Double Slash", "BigSlash", "Freeze", "Magic Shield" };

    public enum WeaponMode
    {
        Magic,
        Crossbow
    }

    public enum AttackControlMode
    {
        Normal,
        Voice
    }

    [Header("Weapon Settings")]
    public WeaponMode currentWeapon = WeaponMode.Magic;
    public GameObject crossbowObject;

    public Dictionary<string, AttackData> allAttacks;

    [Header("Crossbow Settings")]
    public GameObject crossbowArrowPrefab;
    public Transform firePoint;
    public float reloadTime;
    public float arrowForce;

    private RangedAttack crossbowAttack;

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

    public Dictionary<KeyCode, float> attackCooldowns = new Dictionary<KeyCode, float>{
        { KeyCode.Mouse0, 0f },
        { KeyCode.Mouse1, 0f },
        { KeyCode.Mouse4, 0f },
        { KeyCode.Mouse3, 0f }
    };

    [Header("Attack Icons UI")]
    public Image leftAttackIcon;
    public Image rightAttackIcon;
    public Image add1AttackIcon;
    public Image add2AttackIcon;

    public TMP_Text cooldownTextLeft;
    public TMP_Text cooldownTextRight;
    public TMP_Text cooldownTextAdd1;
    public TMP_Text cooldownTextAdd2;

    public ManagerGame gameManager;
    public Animator animator;

    [Header("Attack Control Mode")]
    public AttackControlMode attackControlMode = AttackControlMode.Normal;
    public KeyCode toggleAttackModeKey = KeyCode.V;

    [Tooltip("D³ugoœæ 'pulsu' dla continuous w trybie Voice (sekundy, realtime)")]
    public float voiceContinuousDuration = 0.25f;

    void Awake()
    {
        LoadAttackConfigs();
        ApplySelectedAttacks();
        crossbowObject.SetActive(false);
        crossbowAttack = new RangedAttack(
            crossbowArrowPrefab,
            firePoint,
            KeyCode.Mouse0,
            animator,
            this,
            "Crossbow",
            reloadTime,
            arrowForce
        );
    }
    void Update()
    {
        if (!gameManager.gameLoop) return;

        //if (Input.GetKeyDown(toggleAttackModeKey))
        //{
        //    attackControlMode = (attackControlMode == AttackControlMode.Normal)
        //        ? AttackControlMode.Voice
        //        : AttackControlMode.Normal;

        //    Debug.Log("Attack mode: " + attackControlMode);
        //}

        RotateToMouse();

        if (attackControlMode == AttackControlMode.Normal)
        {
            if (currentWeapon == WeaponMode.Magic)
            {
                leftClickAttack.onUpdateFunc();
                rightClickAttack.onUpdateFunc();
                additionalAttack1.onUpdateFunc();
                additionalAttack2.onUpdateFunc();
            }
            else if (currentWeapon == WeaponMode.Crossbow)
            {
                crossbowAttack?.onUpdateFunc();
            }
        }

        foreach (var key in attackCooldowns.Keys.ToList())
        {
            if (attackCooldowns[key] > 0f)
                attackCooldowns[key] -= Time.deltaTime;

            string waitingTime = attackCooldowns[key] <= 0 ? "" : (attackCooldowns[key]).ToString("0.#");
            switch (key)
            {
                case KeyCode.Mouse0: cooldownTextLeft.SetText(waitingTime); break;
                case KeyCode.Mouse1: cooldownTextRight.SetText(waitingTime); break;
                case KeyCode.Mouse4: cooldownTextAdd1.SetText(waitingTime); break;
                case KeyCode.Mouse3: cooldownTextAdd2.SetText(waitingTime); break;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchWeapon(WeaponMode.Magic);
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchWeapon(WeaponMode.Crossbow);
    }

    private Sprite LoadAttackIcon(string attackName)
    {

        Sprite icon = Resources.Load<Sprite>($"AttackIcons/{attackName}");
        
        return icon;
    }

    public void ApplySelectedAttacks()
    {
        leftClickAttack = CreateAttackForKey(leftAttack, KeyCode.Mouse0);
        rightClickAttack = CreateAttackForKey(rightAttack, KeyCode.Mouse1);
        additionalAttack1 = CreateAttackForKey(add1Attack, KeyCode.Mouse4);
        additionalAttack2 = CreateAttackForKey(add2Attack, KeyCode.Mouse3);

        UpdateAttackIcons();
    }

    private void UpdateAttackIcons()
    {
        if (leftAttack != null && leftAttackIcon != null)
            leftAttackIcon.sprite = LoadAttackIcon(leftAttack.name);

        if (rightAttack != null && rightAttackIcon != null)
            rightAttackIcon.sprite = LoadAttackIcon(rightAttack.name);

        if (add1Attack != null && add1AttackIcon != null)
            add1AttackIcon.sprite = LoadAttackIcon(add1Attack.name);

        if (add2Attack != null && add2AttackIcon != null)
            add2AttackIcon.sprite = LoadAttackIcon(add2Attack.name);
    }

    private void LoadAttackConfigs()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Config/attacks");
        if (jsonFile == null)
        {
            allAttacks = new Dictionary<string, AttackData>();
            return;
        }

        allAttacks = SimpleJsonParser.Parse(jsonFile.text);
    }

    private BasicAttack CreateAttackForKey(UnityEngine.Object attackObject, KeyCode key)
    {
        if (attackObject == null)
            return new BasicAttack(null, key, animator, this, null, 0);

        string attackName = attackObject.name;
        if (!allAttacks.TryGetValue(attackName, out AttackData data))
        {
            return new BasicAttack(GetParticleIfExists(attackObject), key, animator, this, attackName, 0);
        }

        switch (data.type.ToLower())
        {
            case "continuous":
                return new ContinuousAttack(GetParticleIfExists(attackObject), key, animator, this, attackName, data.cooldown);

            case "instance":
                return new BuildCast(key, animator, GetGameObjectIfExists(attackObject), this, attackName, data.cooldown);

            default:
                return new BasicAttack(GetParticleIfExists(attackObject), key, animator, this, attackName, data.cooldown);
        }
    }

    private ParticleSystem GetParticleIfExists(Object obj)
    {
        if (obj is ParticleSystem ps) return ps;
        if (obj is GameObject go) return go.GetComponent<ParticleSystem>();
        return null;
    }

    private GameObject GetGameObjectIfExists(Object obj)
    {
        if (obj is GameObject go) return go;
        if (obj is ParticleSystem ps) return ps.gameObject;
        return null;
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

    public void SwitchWeapon(WeaponMode newMode)
    {
        if (currentWeapon == newMode) return;

        currentWeapon = newMode;

        switch (currentWeapon)
        {
            case WeaponMode.Magic:
                if (crossbowObject != null)
                    crossbowObject.SetActive(false);
                animator.SetBool("CrossbowEquipped", false);
                break;

            case WeaponMode.Crossbow:
                if (crossbowObject != null)
                    crossbowObject.SetActive(true);
                animator.SetBool("CrossbowEquipped", true);
                break;
        }
    }

    public void TriggerPrimaryAttackVoice()
    {
        if (!gameManager.gameLoop) return;
        if (attackControlMode != AttackControlMode.Voice) return;

        if (currentWeapon == WeaponMode.Magic)
            leftClickAttack?.Trigger();
        else if (currentWeapon == WeaponMode.Crossbow)
            crossbowAttack?.Trigger();
    }

    // wywo³ywane przez ContinuousAttack.Trigger()
    public void TriggerContinuousVoice(ContinuousAttack atk)
    {
        StartCoroutine(ContinuousVoicePulse(atk));
    }

    private IEnumerator ContinuousVoicePulse(ContinuousAttack atk)
    {
        atk.StartCast();
        yield return new WaitForSecondsRealtime(voiceContinuousDuration);
        atk.StopCast(true);
    }
}