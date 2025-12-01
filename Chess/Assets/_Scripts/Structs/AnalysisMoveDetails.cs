using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AnalysisMoveDetails
{
    public Square StartSquare;
    public Square EndSquare;
    public ANALYSIS_MOVE_TYPE AnalysisMoveType;
    public bool IsXRayMove;
    public List<Piece> PiecesToXRayThrough; // a list of the pieces in the way of the current move
}

public enum ANALYSIS_MOVE_TYPE
{
    Standard,
    Capture,
    Protection,
    NonCapture // non-capture (i.e. 1 or 2 pawn moves forward and castling)
}
