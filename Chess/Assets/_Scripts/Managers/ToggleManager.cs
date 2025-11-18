using UnityEngine;

public class ToggleManager : MonoBehaviour
{
    public static ToggleManager Instance { get; private set; }

    private bool _isAnalysisModeActivated = false;
    public bool IsAnalysisModeActivated { get { return _isAnalysisModeActivated; } }
    private bool _showXRayMoves = false;
    public bool ShowXRayMoves { get { return _showXRayMoves; } }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SetAnalysisModeActivated(bool active)
    {
        _isAnalysisModeActivated = active;

        if (active)
        {
            PieceManager.Instance.UpdateAllPieceAnalysisMoves();
        }
    }

    public void SetXRayMoves(bool active)
    {
        _showXRayMoves = active;
    }
}
