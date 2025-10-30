using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _fenInput;
    [SerializeField] private Button _loadFENButton;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _loadFENButton.onClick.AddListener(OnLoadFENButtonClicked);
    }

    private void OnLoadFENButtonClicked()
    {
        PieceManager.Instance.LoadPosition(_fenInput.text);
    }

    public void UpdateFENText(string text)
    {
        _fenInput.text = text;
    }
}
