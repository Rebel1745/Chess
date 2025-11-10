using System;
using UnityEngine;
using UnityEngine.UI;

public class MoveHighlightingToggles : MonoBehaviour
{
    [SerializeField] private Toggle _showSafeSquaresToggle;
    [SerializeField] private Toggle _showDangerousSquaresToggle;

    private void Awake()
    {
        _showSafeSquaresToggle.onValueChanged.AddListener(OnShowSafeSquaresClicked);
        _showDangerousSquaresToggle.onValueChanged.AddListener(OnShowDangerousSquaresClicked);
    }

    private void OnShowSafeSquaresClicked(bool selected)
    {
        ToggleManager.Instance.ShowHideSafeSquares(selected);
    }

    private void OnShowDangerousSquaresClicked(bool selected)
    {
        ToggleManager.Instance.ShowHideDangerousSquares(selected);
    }
}
