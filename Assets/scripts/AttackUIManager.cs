using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class AttackUIManager : MonoBehaviour
{
    public AttacksManager attacksManager;

    public TMP_Dropdown leftDropdown;
    public TMP_Dropdown rightDropdown;
    public TMP_Dropdown add1Dropdown;
    public TMP_Dropdown add2Dropdown;
    public TMP_Dropdown micDropdown;

    public GameObject startPanel;
    public GameObject attackSelectPanel;
    public GameObject gamePanel;

    public Button startButton;

    public ManagerGame gameManager;
    //public MicrophoneInput micInput;

    public bool pauseOnStart = true;

    private List<string> _options = new List<string>();
    private const string PlayerPrefsKey = "mic_device";

    private void Awake()
    {
        if (pauseOnStart)
        {
            Time.timeScale = 0f;
        }

        if (startPanel != null) startPanel.SetActive(true);

        if (gamePanel != null) gamePanel.SetActive(false);
        if (attackSelectPanel != null) attackSelectPanel.SetActive(false);
    }

    void Start()
    {
        //Debug.Log("Dupa");
        var attackNames = attacksManager.availableAttacks
            .Select(a => a.name)
            .Concat(attacksManager.availableInstances.Select(a => a.name))
            .ToList();

        //Debug.Log(attackNames.Count);

        var devices = Microphone.devices;
        _options = new List<string>(devices);

        SetupDropdown(leftDropdown, attackNames, attacksManager.leftAttack);
        SetupDropdown(rightDropdown, attackNames, attacksManager.rightAttack);
        SetupDropdown(add1Dropdown, attackNames, attacksManager.add1Attack);
        SetupDropdown(add2Dropdown, attackNames, attacksManager.add2Attack);
        SetupDropdown(micDropdown, _options, attacksManager.add2Attack);

        leftDropdown.onValueChanged.AddListener(OnLeftChanged);
        rightDropdown.onValueChanged.AddListener(OnRightChanged);
        add1Dropdown.onValueChanged.AddListener(OnAdd1Changed);
        add2Dropdown.onValueChanged.AddListener(OnAdd2Changed);
        micDropdown.onValueChanged.AddListener(OnMicChanged);

        startButton.onClick.AddListener(OnStartClicked);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = attackSelectPanel.active ? 1f: 0.01f;
            gameManager.gameLoop = attackSelectPanel.active;
            attackSelectPanel.SetActive(!attackSelectPanel.active);
            gamePanel.SetActive(!attackSelectPanel.active);
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

    private void OnMicChanged(int index)
    {
        if (_options == null || _options.Count == 0) return;
        index = Mathf.Clamp(index, 0, _options.Count - 1);
        string dev = _options[index];

        // Zapisz wybór
        PlayerPrefs.SetString(PlayerPrefsKey, dev);
        PlayerPrefs.Save();

        // Przeka¿ do MicrophoneInput
        //if (micInput != null)
        //{
        //    micInput.SetDevice(dev);
        //}
        //else
        //{
        //    Debug.LogWarning($"StartMenuUI: Brak referencji do MicrophoneInput. Wybrane urz¹dzenie: {dev}");
        //}
    }

    private void OnStartClicked()
    {
        Debug.Log("StartMenuUI: klikniêto 'Graj'.");

        if (startPanel) startPanel.SetActive(false);
        if (gamePanel) gamePanel.SetActive(true);

        Time.timeScale = 1f;
    }
}
