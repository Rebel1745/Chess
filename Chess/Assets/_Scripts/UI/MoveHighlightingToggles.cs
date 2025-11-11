using System;
using UnityEngine;
using UnityEngine.UI;

public class MoveHighlightingToggles : MonoBehaviour
{
    [SerializeField] private Toggle _showSafeSquaresToggle;
    [SerializeField] private Toggle _showDangerousSquaresToggle;
    [SerializeField] private Toggle _flipBoardToggle;

    private void Awake()
    {
        _showSafeSquaresToggle.onValueChanged.AddListener(OnShowSafeSquaresClicked);
        _showDangerousSquaresToggle.onValueChanged.AddListener(OnShowDangerousSquaresClicked);
        _flipBoardToggle.onValueChanged.AddListener(OnFlipBoardClicked);
    }

    private void OnShowSafeSquaresClicked(bool selected)
    {
        ToggleManager.Instance.ShowHideSafeSquares(selected);
    }

    private void OnShowDangerousSquaresClicked(bool selected)
    {
        ToggleManager.Instance.ShowHideDangerousSquares(selected);
    }

    private void OnFlipBoardClicked(bool selected)
    {
        CameraManager.Instance.SetFlipBoard(selected);
    }
}
