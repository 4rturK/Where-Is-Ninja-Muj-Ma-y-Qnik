using System.Linq;
using TMPro;
using UnityEngine;

public class AttackUIManager : MonoBehaviour
{
    public AttacksManager attacksManager;

    public TMP_Dropdown leftDropdown;
    public TMP_Dropdown rightDropdown;
    public TMP_Dropdown add1Dropdown;
    public TMP_Dropdown add2Dropdown;

    public GameObject attackSelectPanel;

    public ManagerGame gameManager;

    void Start()
    {
        var attackNames = attacksManager.availableAttacks.Select(a => a.name).ToList();

        SetupDropdown(leftDropdown, attackNames, attacksManager.leftAttackParticle);
        SetupDropdown(rightDropdown, attackNames, attacksManager.rightAttackParticle);
        SetupDropdown(add1Dropdown, attackNames, attacksManager.add1AttackParticle);
        SetupDropdown(add2Dropdown, attackNames, attacksManager.add2AttackParticle);

        leftDropdown.onValueChanged.AddListener(OnLeftChanged);
        rightDropdown.onValueChanged.AddListener(OnRightChanged);
        add1Dropdown.onValueChanged.AddListener(OnAdd1Changed);
        add2Dropdown.onValueChanged.AddListener(OnAdd2Changed);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = attackSelectPanel.active ? 1f: 0.01f;
            gameManager.gameLoop = attackSelectPanel.active;
            attackSelectPanel.SetActive(!attackSelectPanel.active);
        }
    }

    void SetupDropdown(TMP_Dropdown dropdown, System.Collections.Generic.List<string> names, ParticleSystem current)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(names);

        int index = current ? names.IndexOf(current.name) : 0;
        dropdown.value = Mathf.Clamp(index, 0, names.Count - 1);
        dropdown.RefreshShownValue();
    }

    public void OnLeftChanged(int index)
    {
        attacksManager.leftAttackParticle = attacksManager.availableAttacks[index];
        attacksManager.ApplySelectedAttacks();
    }

    public void OnRightChanged(int index)
    {
        attacksManager.rightAttackParticle = attacksManager.availableAttacks[index];
        attacksManager.ApplySelectedAttacks();
    }

    public void OnAdd1Changed(int index)
    {
        attacksManager.add1AttackParticle = attacksManager.availableAttacks[index];
        attacksManager.ApplySelectedAttacks();
    }

    public void OnAdd2Changed(int index)
    {
        attacksManager.add2AttackParticle = attacksManager.availableAttacks[index];
        attacksManager.ApplySelectedAttacks();
    }
}
