using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoryManager : MonoBehaviour
{
    [Header("Powi¹zania")]
    public WalterBlackPanelUI walterPanelScript; // Referencja do skryptu Waltera, ¿eby do niego wróciæ
    public GameObject memoryGamePanel;         // Panel UI Memory (MemoryCanvas)
    public CanvasGroup memoryCanvasGroup;      // CanvasGroup Memory (¿eby blokowaæ klikanie)

    [Header("Ustawienia Gry")]
    public List<TileButton> Tiles = new List<TileButton>();
    public TMP_Text HudText;
    public Button BackButton;

    [Header("Kolory i Czas")]
    public Color PlayColor = new Color(0.2f, 0.5f, 1f);
    public Color BaseColor = new Color(0.75f, 0.75f, 0.75f);
    public Color ShowColor = new Color(0.3f, 0.7f, 1f);
    public Color DisabledColor = new Color(0.55f, 0.55f, 0.55f);
    public Color ErrorColor = new Color(1f, 0.25f, 0.25f);
    public Color SuccessColor = new Color(0.2f, 0.9f, 0.3f);

    public float AllFlashTime = 0.6f;
    public float ErrorFlashTime = 0.5f;
    public float AfterErrorDelay = 0.25f;
    public float AfterSuccessDelay = 0.25f;
    public float ShowDelay = 0.6f;
    public float StepFlashTime = 0.35f;
    public float BetweenSteps = 0.25f;

    public int Level = 1;
    public int Lives = 3;

    private List<int> _sequence = new List<int>();
    private int _inputPos = 0;
    private bool _isPlaying = false;

    private void Start()
    {
        // Na starcie ukrywamy Memory, bo czekamy na klikniêcie u Waltera
        HideMemoryUI();

        if (BackButton != null)
        {
            BackButton.onClick.RemoveAllListeners();
            BackButton.onClick.AddListener(ReturnToWalter);
        }
    }
    public void OpenMemoryGame()
    {
        ShowMemoryUI();
        NewGame();
    }

    public void ReturnToWalter()
    {
        HideMemoryUI();
        if (walterPanelScript != null)
        {
            walterPanelScript.OpenMenu(false);
        }
        else
        {
            Debug.LogError("Nie przypisa³eœ skryptu modelu (Waltera) do pola 'Walter Panel Script' w GameManagerze!");
        }
    }

    private void ShowMemoryUI()
    {
        if (memoryGamePanel != null) memoryGamePanel.SetActive(true);
        if (memoryCanvasGroup != null)
        {
            memoryCanvasGroup.alpha = 1f;
            memoryCanvasGroup.interactable = true;
            memoryCanvasGroup.blocksRaycasts = true;
        }
    }

    private void HideMemoryUI()
    {
        if (memoryCanvasGroup != null)
        {
            memoryCanvasGroup.alpha = 0f;
            memoryCanvasGroup.interactable = false;
            memoryCanvasGroup.blocksRaycasts = false;
        }
        if (memoryGamePanel != null) memoryGamePanel.SetActive(false);
    }

    public void NewGame()
    {
        _sequence.Clear();
        Level = 1;
        Lives = 3;
        UpdateHUD();
        GenerateNextStep();
        PlayCurrentSequence();
    }

    private void GenerateNextStep()
    {
        if (Tiles.Count == 0) return;
        _sequence.Add(Random.Range(0, Tiles.Count));
    }

    public void PlayCurrentSequence()
    {
        StopAllCoroutines();
        StartCoroutine(CoPlay());
    }

    private IEnumerator CoPlay()
    {
        _isPlaying = true;
        _inputPos = 0;

        foreach (var t in Tiles) { t.SetInteractable(false); t.SetColor(DisabledColor); }

        yield return new WaitForSecondsRealtime(ShowDelay);

        for (int i = 0; i < _sequence.Count; i++)
        {
            int idx = _sequence[i];
            Tiles[idx].SetColor(ShowColor);
            yield return new WaitForSecondsRealtime(StepFlashTime);
            Tiles[idx].SetColor(DisabledColor);
            yield return new WaitForSecondsRealtime(BetweenSteps);
        }

        foreach (var t in Tiles) { t.SetColor(BaseColor); t.SetInteractable(true); }

        _isPlaying = false;
        UpdateHUD();
    }

    public void OnTileClicked(int index)
    {
        if (_isPlaying) return;

        if (index == _sequence[_inputPos])
        {
            Tiles[index].Pulse(PlayColor, StepFlashTime * 0.9f);
            _inputPos++;
            if (_inputPos >= _sequence.Count) StartCoroutine(CoAdvanceLevel());
        }
        else
        {
            StartCoroutine(CoHandleError(index));
        }
        UpdateHUD();
    }

    private IEnumerator CoHandleError(int index)
    {
        _isPlaying = true;
        foreach (var t in Tiles) t.SetInteractable(false);
        Tiles[index].Pulse(ErrorColor, ErrorFlashTime);

        if (Lives == 1)
        {
            yield return new WaitForSecondsRealtime(ErrorFlashTime * 0.6f);
            yield return StartCoroutine(FlashAll(ErrorColor, AllFlashTime, DisabledColor));
        }

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, ErrorFlashTime + AfterErrorDelay - (ErrorFlashTime * 0.6f)));

        Lives--;
        UpdateHUD();

        if (Lives <= 0)
        {
            Level = 1; Lives = 3; _sequence.Clear();
            GenerateNextStep(); UpdateHUD();
        }
        PlayCurrentSequence();
    }

    private IEnumerator CoAdvanceLevel()
    {
        _isPlaying = true;
        foreach (var t in Tiles) t.SetInteractable(false);
        Level++; UpdateHUD();
        yield return StartCoroutine(FlashAll(SuccessColor, AllFlashTime, BaseColor));
        yield return new WaitForSecondsRealtime(AfterSuccessDelay);
        GenerateNextStep();
        PlayCurrentSequence();
    }

    private void UpdateHUD()
    {
        if (HudText) HudText.text = $"Poziom: {Level}    ¯ycia: {Lives}";
    }

    private IEnumerator FlashAll(Color c, float time, Color afterColor)
    {
        foreach (var t in Tiles) t.SetColor(c);
        yield return new WaitForSecondsRealtime(time);
        foreach (var t in Tiles) t.SetColor(afterColor);
    }
}