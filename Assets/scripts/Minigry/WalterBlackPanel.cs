using UnityEngine;
using UnityEngine.UI;

public class WalterBlackPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject menuPanel;          // Tu przypisz: WaterBlackPanel
    public CanvasGroup menuCanvasGroup;   // Tu przypisz: WaterBlackPanel

    [Header("Minigry")]
    public MemoryManager memoryMinigame;  // <-- TO JEST TO NOWE GNIAZDKO

    private bool isOpen = false;
    private AttacksManager attacksManager;
    private bool attacksActive = true;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        attacksManager = FindObjectOfType<AttacksManager>();

        if (menuPanel != null)
        {
            menuPanel.SetActive(false);

            // 1. Przycisk Wyjœcia
            Transform backBtnTr = menuPanel.transform.Find("Back_BTN");
            if (backBtnTr != null)
            {
                Button btn = backBtnTr.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ResumeGameButton);
            }

            // 2. Przycisk Broni 1 -> URUCHAMIA MEMORY
            Transform bron1Tr = menuPanel.transform.Find("Bron1_BTN");
            if (bron1Tr != null)
            {
                Button btn = bron1Tr.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(UruchomMemory); // <-- Podpinamy Memory
            }

            SetupWeaponButton("Bron2_BTN", 2);
            SetupWeaponButton("Bron3_BTN", 3);
        }

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }
    }

    public void UruchomMemory()
    {
        // Ukrywamy Waltera
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }
        menuPanel.SetActive(false);

        // Odpalamy Memory
        if (memoryMinigame != null)
        {
            memoryMinigame.OpenMemoryGame();
        }
        else
        {
            Debug.LogError("Nie przypisa³eœ GameManager do pola Memory Minigame w modelu!");
        }
    }

    void SetupWeaponButton(string btnName, int weaponIndex)
    {
        Transform btnTr = menuPanel.transform.Find(btnName);
        if (btnTr != null)
        {
            Button btn = btnTr.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => Debug.Log("Wybrano broñ numer: " + weaponIndex));
            btn.onClick.AddListener(ResumeGameButton);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOpen) return;
        if (other.CompareTag("Player"))
            OpenMenu(true);
    }

    public void OpenMenu(bool pauseTime)
    {
        if (menuPanel == null) return;

        menuPanel.SetActive(true);
        if (pauseTime) Time.timeScale = 0f;
        isOpen = true;

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 1f;
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
        }

        if (attacksManager != null)
        {
            attacksManager.enabled = false;
            attacksActive = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGameButton()
    {
        Time.timeScale = 1f;
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }
        menuPanel.SetActive(false);
        isOpen = false;

        if (attacksManager != null && !attacksActive)
        {
            attacksManager.enabled = true;
            attacksActive = true;
        }
    }
}