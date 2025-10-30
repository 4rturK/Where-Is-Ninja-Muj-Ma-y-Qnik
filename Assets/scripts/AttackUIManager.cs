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
        var attackNames = attacksManager.availableAttacks
            .Select(a => a.name)
            .Concat(attacksManager.availableInstances.Select(a => a.name))
            .ToList();

        SetupDropdown(leftDropdown, attackNames, attacksManager.leftAttack);
        SetupDropdown(rightDropdown, attackNames, attacksManager.rightAttack);
        SetupDropdown(add1Dropdown, attackNames, attacksManager.add1Attack);
        SetupDropdown(add2Dropdown, attackNames, attacksManager.add2Attack);

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

    void SetupDropdown(TMP_Dropdown dropdown, System.Collections.Generic.List<string> names, Object current)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(names);

        int index = current ? names.IndexOf(current.name) : 0;
        dropdown.value = Mathf.Clamp(index, 0, names.Count - 1);
        dropdown.RefreshShownValue();
    }

    public void OnLeftChanged(int index)
    {
        string selectedName = leftDropdown.options[index].text;

        var particle = attacksManager.availableAttacks.FirstOrDefault(a => a.name == selectedName);
        var prefab = attacksManager.availableInstances.FirstOrDefault(a => a.name == selectedName);

        if (particle != null)
            attacksManager.leftAttack = particle;
        else if (prefab != null)
            attacksManager.leftAttack = prefab;
        else
            attacksManager.leftAttack = null;

        attacksManager.ApplySelectedAttacks();
    }

    public void OnRightChanged(int index)
    {
        string selectedName = leftDropdown.options[index].text;

        var particle = attacksManager.availableAttacks.FirstOrDefault(a => a.name == selectedName);
        var prefab = attacksManager.availableInstances.FirstOrDefault(a => a.name == selectedName);
        if (particle != null)
            attacksManager.rightAttack = particle;
        else if (prefab != null)
            attacksManager.rightAttack = prefab;
        else
            attacksManager.rightAttack = null;
        attacksManager.ApplySelectedAttacks();
    }

    public void OnAdd1Changed(int index)
    {
        string selectedName = leftDropdown.options[index].text;

        var particle = attacksManager.availableAttacks.FirstOrDefault(a => a.name == selectedName);
        var prefab = attacksManager.availableInstances.FirstOrDefault(a => a.name == selectedName);
        if (particle != null)
            attacksManager.add1Attack = particle;
        else if (prefab != null)
            attacksManager.add1Attack = prefab;
        else
            attacksManager.add1Attack = null;
        attacksManager.ApplySelectedAttacks();
    }

    public void OnAdd2Changed(int index)
    {
        string selectedName = leftDropdown.options[index].text;

        var particle = attacksManager.availableAttacks.FirstOrDefault(a => a.name == selectedName);
        var prefab = attacksManager.availableInstances.FirstOrDefault(a => a.name == selectedName);
        if (particle != null)
            attacksManager.add2Attack = particle;
        else if (prefab != null)
            attacksManager.add2Attack = prefab;
        else
            attacksManager.add2Attack = null;
        attacksManager.ApplySelectedAttacks();
    }
}
