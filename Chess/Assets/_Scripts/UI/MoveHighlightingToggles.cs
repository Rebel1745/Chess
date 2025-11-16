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
        //InputManager.Instance.OnRightClickFinished += InputManager_OnRightClickFinished;

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
