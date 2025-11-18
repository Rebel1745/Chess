using System;
using UnityEngine;
using UnityEngine.UI;

public class MoveHighlightingToggles : MonoBehaviour
{
    [SerializeField] private GameObject _squareDetailsPanel;
    private SquareDetails _squareDetails;
    [SerializeField] private Toggle _activateAnalysisModeToggle;
    [SerializeField] private Toggle _showXRayMovesToggle;
    [SerializeField] private Toggle _flipBoardToggle;

    private void Awake()
    {
        _activateAnalysisModeToggle.onValueChanged.AddListener(OnAnalysisModeChanged);
        _showXRayMovesToggle.onValueChanged.AddListener(OnShowXRayMovesChanged);
        _flipBoardToggle.onValueChanged.AddListener(OnFlipBoardClicked);
    }

    private void Start()
    {
        _squareDetails = _squareDetailsPanel.GetComponent<SquareDetails>();
    }

    private void OnAnalysisModeChanged(bool selected)
    {
        ToggleManager.Instance.SetAnalysisModeActivated(selected);
    }

    private void OnShowXRayMovesChanged(bool selected)
    {
        ToggleManager.Instance.SetXRayMoves(selected);
    }

    private void OnFlipBoardClicked(bool selected)
    {
        CameraManager.Instance.SetFlipBoard(selected);
    }

    private void InputManager_OnRightClickFinished(object sender, InputManager.OnRightClickArgs e)
    {
        if (e.CurrentSquare.IsHighlighted)
            RemoveSquareDetails();
        else
            ShowSquareDetails(e.CurrentSquare);
    }

    public void ShowSquareDetails(Square selectedSquare)
    {
        _squareDetailsPanel.SetActive(true);
        _squareDetails.SetSquareDetails(selectedSquare);
    }

    public void RemoveSquareDetails()
    {
        _squareDetailsPanel.SetActive(false);
    }
}
