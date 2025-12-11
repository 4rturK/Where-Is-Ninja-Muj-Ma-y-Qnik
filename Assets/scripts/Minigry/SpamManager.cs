using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SpamManager : MonoBehaviour
{
    [Header("Powiązania")]
    public WalterBlackPanelUI walterPanelScript; // Powrót do Waltera
    public GameObject spamGamePanel;           // SpamCanvas
    public CanvasGroup spamCanvasGroup;        // Do ukrywania/pokazywania

    [Header("UI")]
    public Slider Meter;
    public TMP_Text HudText;
    public TMP_Text PromptText;
    public TMP_Text ResultText;
    public Button BackButton;
    public Button ResetButton; // Dodatkowy przycisk z Twojego screena

    [Header("Parametry bazowe")]
    public int StartLevel = 1;
    public float BaseGain = 0.1f;
    public float BaseDecay = 0.15f;
    public float BasePenalty = 0.10f;
    public float BaseTimeLimit = 5.0f;

    [Header("Skalowanie trudności")]
    public float GainFactorPerLevel = 0.90f;
    public float DecayFactorPerLevel = 1.1f;
    public float PenaltyFactorPerLevel = 1.05f;
    public float TimeFactorPerLevel = 0.95f;

    [Header("Efekty wizualne")]
    public Color MeterBase = new Color(0.6f, 0.6f, 0.6f);
    public Color MeterGood = new Color(0.2f, 0.85f, 0.3f);
    public Color MeterBad = new Color(1.0f, 0.3f, 0.3f);

    // ——— wewnętrzne ———
    private float TargetFill = 1;
    private bool _startedInput = false;
    private int _level;
    private float _gain, _decay, _penalty, _timeLimit;
    private float _timeLeft;
    private float _meter;
    private int _lastDir = 0;
    private bool _inRound = false;
    private Image _meterFill;

    private void Awake()
    {
        if (Meter != null)
        {
            // Szukamy obrazka wypełnienia w sliderze
            if (Meter.fillRect != null)
                _meterFill = Meter.fillRect.GetComponent<Image>();
        }
    }

    private void Start()
    {
        // Na starcie ukrywamy panel
        HideSpamUI();

        // Podpinamy przyciski
        if (BackButton)
        {
            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(ReturnToWalter);
        }

        if (ResetButton)
        {
            ResetButton.onClick.RemoveAllListeners();
            ResetButton.onClick.AddListener(ResetLevel);
        }
    }

    // --- KOMUNIKACJA Z WALTEREM ---
    public void OpenSpamGame()
    {
        ShowSpamUI();
        _level = Mathf.Max(1, StartLevel);
        StartRound();
    }

    public void ReturnToWalter()
    {
        HideSpamUI();
        _inRound = false; // Zatrzymujemy logikę gry
        if (walterPanelScript != null)
        {
            walterPanelScript.OpenMenu(false); // Wracamy do Waltera
        }
        else Debug.LogError("Brak przypisanego Walter Panel Script w SpamManager!");
    }

    private void ShowSpamUI()
    {
        if (spamGamePanel != null) spamGamePanel.SetActive(true);
        if (spamCanvasGroup != null)
        {
            spamCanvasGroup.alpha = 1f;
            spamCanvasGroup.interactable = true;
            spamCanvasGroup.blocksRaycasts = true;
        }
    }

    private void HideSpamUI()
    {
        if (spamCanvasGroup != null)
        {
            spamCanvasGroup.alpha = 0f;
            spamCanvasGroup.interactable = false;
            spamCanvasGroup.blocksRaycasts = false;
        }
        if (spamGamePanel != null) spamGamePanel.SetActive(false);
    }
    // -----------------------------

    public void ResetLevel()
    {
        _level = 1;
        StartRound();
    }

    private void StartRound()
    {
        int L = _level - 1;
        _gain = Mathf.Max(0.03f, BaseGain * Mathf.Pow(GainFactorPerLevel, L));
        _decay = BaseDecay * Mathf.Pow(DecayFactorPerLevel, L);
        _penalty = BasePenalty * Mathf.Pow(PenaltyFactorPerLevel, L);
        _timeLimit = Mathf.Max(2.5f, BaseTimeLimit * Mathf.Pow(TimeFactorPerLevel, L));

        _meter = 0f;
        _timeLeft = _timeLimit;
        _lastDir = 0;
        _startedInput = false;
        _inRound = true;

        UpdateHUD();
        UpdateMeter(0f, MeterBase);

        if (ResultText) ResultText.text = "";
        if (PromptText) PromptText.text = "Spamuj: A / D  lub  ← / →";
    }

    private void Update()
    {
        if (!_inRound) return;

        int inputDir = ReadDirection();

        if (!_startedInput && inputDir != 0)
            _startedInput = true;

        if (!_startedInput)
        {
            HandleInput(inputDir); // Pozwala nabić trochę paska przed startem czasu
            UpdateMeter(_meter, MeterBase);
            UpdateHUD();
            return;
        }

        // UŻYWAMY UNSCALED DELTA TIME BO GRA JEST NA PAUZIE
        float dt = Time.unscaledDeltaTime;

        _meter = Mathf.Max(0f, _meter - _decay * dt);
        UpdateMeter(_meter, MeterBase);

        HandleInput(inputDir);

        _timeLeft = Mathf.Max(0f, _timeLeft - dt);
        UpdateHUD();

        if (_meter >= TargetFill)
        {
            RoundWin();
        }
        else if (_timeLeft <= 0f)
        {
            RoundLose();
        }
    }

    private void RoundWin()
    {
        _inRound = false;
        if (ResultText) ResultText.text = "Sukces!";
        StartCoroutine(FlashMeter(MeterGood, 0.5f));
        _level++;
        // Zastąpiono Invoke korutyną (Invoke nie działa na pauzie)
        StartCoroutine(WaitAndRestart(0.7f));
    }

    private void RoundLose()
    {
        _inRound = false;
        if (ResultText) ResultText.text = "Porażka :(";
        StartCoroutine(FlashMeter(MeterBad, 0.5f));
        StartCoroutine(WaitAndRestart(0.8f));
    }

    private IEnumerator WaitAndRestart(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        StartRound();
    }

    private int ReadDirection()
    {
        // Obsługa klawiatury działa nawet przy TimeScale = 0
        bool left = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        bool right = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);

        if (left == right) return 0;
        return right ? +1 : -1;
    }

    private void UpdateMeter(float value, Color c)
    {
        if (Meter) Meter.value = value;
        if (_meterFill != null)
        {
            c.a = 1f;
            _meterFill.color = c;
        }
    }

    private IEnumerator FlashMeter(Color c, float t)
    {
        if (_meterFill != null)
        {
            var before = _meterFill.color;
            _meterFill.color = c;
            yield return new WaitForSecondsRealtime(t);
            _meterFill.color = before;
        }
    }

    private void UpdateHUD()
    {
        if (!HudText) return;
        HudText.text = $"Poziom: {_level}\nCzas: {_timeLeft:0.0}s";
    }

    private void HandleInput(int inputDir)
    {
        if (inputDir != 0)
        {
            if (inputDir != _lastDir)
            {
                _lastDir = inputDir;
                _meter = Mathf.Min(1f, _meter + _gain);
                UpdateMeter(_meter, MeterGood);
            }
            else
            {
                _meter = Mathf.Max(0f, _meter - _penalty);
                UpdateMeter(_meter, MeterBad);
            }
        }
    }
}