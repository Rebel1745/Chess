using System;
using UnityEngine;
using UnityEngine.UI;

public class MoveHighlightingToggles : MonoBehaviour
{
    [SerializeField] private GameObject _squareDetailsPanel;
    private SquareDetails _squareDetails;
    [SerializeField] private Toggle _showSafeSquaresToggle;
    [SerializeField] private Toggle _showDangerousSquaresToggle;
    [SerializeField] private Toggle _flipBoardToggle;

    private void Awake()
    {
        _showSafeSquaresToggle.onValueChanged.AddListener(OnShowSafeSquaresClicked);
        _showDangerousSquaresToggle.onValueChanged.AddListener(OnShowDangerousSquaresClicked);
        _flipBoardToggle.onValueChanged.AddListener(OnFlipBoardClicked);
    }

    private void Start()
    {
        InputManager.Instance.OnRightClickStarted += InputManager_OnRightClickStarted;

        _squareDetails = _squareDetailsPanel.GetComponent<SquareDetails>();
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

    private void InputManager_OnRightClickStarted(object sender, InputManager.OnRightClickArgs e)
    {
        if (e.CurrentSquare.IsHighlighted)
            RemoveSquareDetails();
        else
            ShowSquareDetails(e.CurrentSquare);
    }

    private void ShowSquareDetails(Square selectedSquare)
    {
        _squareDetailsPanel.SetActive(true);
        _squareDetails.SetSquareDetails(selectedSquare);
    }

    private void RemoveSquareDetails()
    {
        _squareDetailsPanel.SetActive(false);
    }
}
