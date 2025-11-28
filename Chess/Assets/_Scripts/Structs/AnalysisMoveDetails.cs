using UnityEngine;

[System.Serializable]
public struct AnalysisMoveDetails
{
    public Square StartSquare;
    public Square EndSquare;
    public ANALYSIS_MOVE_TYPE AnalysisMoveType;
    public bool IsXRayMove;
}

public enum ANALYSIS_MOVE_TYPE
{
    Standard,
    Capture,
    Protection,
    NonCapture // non-capture (i.e. 1 or 2 pawn moves forward and castling)
}
