using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveHighlightingToggles : MonoBehaviour
{
    [SerializeField] private GameObject _squareDetailsPanel;
    private SquareDetails _squareDetails;
    [SerializeField] private Toggle _activateAnalysisModeToggle;
    [SerializeField] private Toggle _showCaptureMovesToggle;
    [SerializeField] private Toggle _showProtectionMovesToggle;
    [SerializeField] private Toggle _showXRayMovesToggle;
    [SerializeField] private Toggle _flipBoardToggle;
    [SerializeField] private Button _checkForChecksButton;
    [SerializeField] private TMP_Text _checksCountText;
    [SerializeField] private Button _checkForCapturesButton;
    [SerializeField] private TMP_Text _capturesCountText;

    private void Awake()
    {
        _activateAnalysisModeToggle.onValueChanged.AddListener(OnAnalysisModeChanged);
        _showCaptureMovesToggle.onValueChanged.AddListener(OnShowCaptureMovesChanged);
        _showProtectionMovesToggle.onValueChanged.AddListener(OnShowProtectionMovesChanged);
        _showXRayMovesToggle.onValueChanged.AddListener(OnShowXRayMovesChanged);
        _flipBoardToggle.onValueChanged.AddListener(OnFlipBoardClicked);

        _checkForChecksButton.onClick.AddListener(OnCheckForChecksClicked);
        _checkForCapturesButton.onClick.AddListener(OnCheckForCapturesClicked);

        _activateAnalysisModeToggle.isOn = PlayerPrefs.GetInt("AnalysisModeActivated", 0) != 0;
        _showCaptureMovesToggle.isOn = PlayerPrefs.GetInt("ShowCaptureMoves", 0) != 0;
        _showProtectionMovesToggle.isOn = PlayerPrefs.GetInt("ShowProtectionMoves", 0) != 0;
        _showXRayMovesToggle.isOn = PlayerPrefs.GetInt("ShowXRayMoves", 0) != 0;
    }

    private void Start()
    {
        _squareDetails = _squareDetailsPanel.GetComponent<SquareDetails>();
    }

    private void OnAnalysisModeChanged(bool selected)
    {
        ToggleManager.Instance.SetAnalysisModeActivated(selected);
    }

    private void OnShowCaptureMovesChanged(bool selected)
    {
        ToggleManager.Instance.SetCaptureMoves(selected);
    }

    private void OnShowProtectionMovesChanged(bool selected)
    {
        ToggleManager.Instance.SetProtectionMoves(selected);
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

    private void OnCheckForChecksClicked()
    {
        ArrowManager.Instance.DestroyAllArrows();
        BoardManager.Instance.RemoveAllHighlightingFromSquares();

        _checksCountText.text = "        Calculating...";

        int checkCount = PieceManager.Instance.DrawArrowsForAllPossibleChecks(GameManager.Instance.IsCurrentPlayerWhite);
        _checksCountText.text = "        " + checkCount + " checks";
    }

    private void OnCheckForCapturesClicked()
    {
        ArrowManager.Instance.DestroyAllArrows();
        BoardManager.Instance.RemoveAllHighlightingFromSquares();

        _capturesCountText.text = "        Calculating...";

        int captureCount = PieceManager.Instance.DrawArrowsForAllPossibleCaptures(GameManager.Instance.IsCurrentPlayerWhite);
        _capturesCountText.text = "        " + captureCount + " captures";
    }
}
