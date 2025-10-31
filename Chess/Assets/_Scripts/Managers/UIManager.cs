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

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _loadFENButton.onClick.AddListener(OnLoadFENButtonClicked);
        _loadPGNButton.onClick.AddListener(OnLoadPGNButtonClicked);
    }

    private void OnLoadFENButtonClicked()
    {
        PieceManager.Instance.LoadPosition(_fenInput.text);
    }

    private void OnLoadPGNButtonClicked()
    {
        PGNManager.Instance.ParsePGN(_pgnInput.text);
    }

    public void UpdateFENText(string text)
    {
        _fenInput.text = text;
    }
}
