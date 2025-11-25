using UnityEngine;

public class ToggleManager : MonoBehaviour
{
    public static ToggleManager Instance { get; private set; }

    private bool _isAnalysisModeActivated = false;
    public bool IsAnalysisModeActivated { get { return _isAnalysisModeActivated; } }
    private bool _showCaptureMoves = false;
    public bool ShowCaptureMoves { get { return _showCaptureMoves; } }
    private bool _showProtectionMoves = false;
    public bool ShowProtectionMoves { get { return _showProtectionMoves; } }
    private bool _showXRayMoves = false;
    public bool ShowXRayMoves { get { return _showXRayMoves; } }
    private bool _showMoveIcons = false;
    public bool ShowMoveIcons { get { return _showMoveIcons; } }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _isAnalysisModeActivated = PlayerPrefs.GetInt("AnalysisModeActivated", 0) != 0;
        _showCaptureMoves = PlayerPrefs.GetInt("ShowCaptureMoves", 0) != 0;
        _showProtectionMoves = PlayerPrefs.GetInt("ShowProtectionMoves", 0) != 0;
        _showXRayMoves = PlayerPrefs.GetInt("ShowXRayMoves", 0) != 0;
        _showMoveIcons = PlayerPrefs.GetInt("ShowMoveIcons", 0) != 0;
    }

    public void SetAnalysisModeActivated(bool active)
    {
        _isAnalysisModeActivated = active;

        if (active)
        {
            PieceManager.Instance.UpdateAllPieceAnalysisMoves();
            PlayerPrefs.SetInt("AnalysisModeActivated", 1);
        }
        else
            PlayerPrefs.SetInt("AnalysisModeActivated", 0);
    }

    public void SetCaptureMoves(bool active)
    {
        _showCaptureMoves = active;

        if (active)
        {
            PieceManager.Instance.UpdateAllPieceAnalysisMoves();
            PlayerPrefs.SetInt("ShowCaptureMoves", 1);
        }
        else
            PlayerPrefs.SetInt("ShowCaptureMoves", 0);
    }

    public void SetProtectionMoves(bool active)
    {
        _showProtectionMoves = active;

        if (active)
        {
            PieceManager.Instance.UpdateAllPieceAnalysisMoves();
            PlayerPrefs.SetInt("ShowProtectionMoves", 1);
        }
        else
            PlayerPrefs.SetInt("ShowProtectionMoves", 0);
    }

    public void SetXRayMoves(bool active)
    {
        _showXRayMoves = active;

        if (active)
        {
            PieceManager.Instance.UpdateAllPieceAnalysisMoves();
            PlayerPrefs.SetInt("ShowXRayMoves", 1);
        }
        else
            PlayerPrefs.SetInt("ShowXRayMoves", 0);
    }

    public void SetShowMoveIcons(bool show)
    {
        _showMoveIcons = show;

        if (show) PlayerPrefs.SetInt("ShowMoveIcons", 1);
        else PlayerPrefs.SetInt("ShowMoveIcons", 0);
    }
}
