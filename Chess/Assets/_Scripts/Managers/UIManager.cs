using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _fenInput;
    [SerializeField] private Button _loadFENButton;
    [SerializeField] private TMP_InputField _pgnInput;
    [SerializeField] private Button _loadPGNButton;
    [SerializeField] private Button _nextMovePGNButton;
    [SerializeField] private Button _previousMovePGNButton;
    [SerializeField] private Button _firstMovePGNButton;
    [SerializeField] private Button _lastMovePGNButton;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _loadFENButton.onClick.AddListener(OnLoadFENButtonClicked);
        _loadPGNButton.onClick.AddListener(OnLoadPGNButtonClicked);
        _nextMovePGNButton.onClick.AddListener(_nextMovePGNButtonClicked);
        _previousMovePGNButton.onClick.AddListener(_previousMovePGNButtonClicked);
        _firstMovePGNButton.onClick.AddListener(OnFirstMovePGNButtonClicked);
        _lastMovePGNButton.onClick.AddListener(OnLastMovePGNButtonClicked);

        _nextMovePGNButton.gameObject.SetActive(false);
        _previousMovePGNButton.gameObject.SetActive(false);
        _firstMovePGNButton.gameObject.SetActive(false);
        _lastMovePGNButton.gameObject.SetActive(false);
    }

    private void OnLoadFENButtonClicked()
    {
        PieceManager.Instance.LoadPosition(_fenInput.text);
    }

    private void OnLoadPGNButtonClicked()
    {
        PGNManager.Instance.ParsePGN(_pgnInput.text);

        _nextMovePGNButton.gameObject.SetActive(true);
        _previousMovePGNButton.gameObject.SetActive(true);
        _firstMovePGNButton.gameObject.SetActive(true);
        _lastMovePGNButton.gameObject.SetActive(true);
    }

    private void _nextMovePGNButtonClicked()
    {
        PGNManager.Instance.NextMove();
    }

    private void _previousMovePGNButtonClicked()
    {
        PGNManager.Instance.PreviousMove();
    }

    private void OnFirstMovePGNButtonClicked()
    {
        PGNManager.Instance.FirstMove();
    }

    private void OnLastMovePGNButtonClicked()
    {
        PGNManager.Instance.LastMove();
    }

    public void UpdateFENText(string text)
    {
        _fenInput.text = text;
    }
}
