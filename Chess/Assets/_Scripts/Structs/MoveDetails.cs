[System.Serializable]
public struct MoveDetails
{
    public Piece PieceToMove;
    public Square MoveToSquare;
    public Piece SecondPieceToMove;
    public Square SecondMoveToSquare;
    public bool IsPromotion;
    public bool ActivatesEnPassant;
    public Piece RemovePieceEnPassant;
}
