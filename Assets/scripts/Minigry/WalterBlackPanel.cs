using UnityEngine;
using UnityEngine.UI;

public class WalterBlackPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject menuPanel;
    public CanvasGroup menuCanvasGroup;

    [Header("Minigry")]
    public MemoryManager memoryMinigame;
    public SpamManager spamMinigame;     // <--- NOWE GNIAZDKO

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

            // Back
            Transform backBtnTr = menuPanel.transform.Find("Back_BTN");
            if (backBtnTr != null) backBtnTr.GetComponent<Button>().onClick.AddListener(ResumeGameButton);

            // Bron 1 -> Memory
            Transform bron1Tr = menuPanel.transform.Find("Bron1_BTN");
            if (bron1Tr != null)
            {
                bron1Tr.GetComponent<Button>().onClick.RemoveAllListeners();
                bron1Tr.GetComponent<Button>().onClick.AddListener(UruchomMemory);
            }

            // Bron 2 -> Spam Game (NOWOŒÆ)
            Transform bron2Tr = menuPanel.transform.Find("Bron2_BTN");
            if (bron2Tr != null)
            {
                bron2Tr.GetComponent<Button>().onClick.RemoveAllListeners();
                bron2Tr.GetComponent<Button>().onClick.AddListener(UruchomSpam);
            }

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
        HideWalterUI();
        if (memoryMinigame != null) memoryMinigame.OpenMemoryGame();
        else Debug.LogError("Brak przypisanego MemoryManager!");
    }

    // --- NOWA FUNKCJA ---
    public void UruchomSpam()
    {
        HideWalterUI();
        if (spamMinigame != null) spamMinigame.OpenSpamGame();
        else Debug.LogError("Brak przypisanego SpamManager!");
    }
    // --------------------

    private void HideWalterUI()
    {
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }
        menuPanel.SetActive(false);
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
        if (other.CompareTag("Player")) OpenMenu(true);
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
        HideWalterUI();
        isOpen = false;
        if (attacksManager != null && !attacksActive)
        {
            attacksManager.enabled = true;
            attacksActive = true;
        }
    }
}