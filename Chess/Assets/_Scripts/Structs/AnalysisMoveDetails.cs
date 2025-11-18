using UnityEngine;

[System.Serializable]
public struct AnalysisMoveDetails
{
    public Square StartSquare;
    public Square EndSquare;
    public ANALYSIS_MOVE_TYPE AnalysisMoveType;
}

public enum ANALYSIS_MOVE_TYPE
{
    Standard,
    Capture,
    XRay,
    Protection
}
