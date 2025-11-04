using UnityEngine;

[System.Serializable]
public struct MoveDetails
{
    public int MoveNumber;
    public bool isWhite;
    public Piece PieceToMove;
    public Square MoveToSquare;
    public Piece SecondPieceToMove;
    public Square SecondMoveToSquare;
    public bool IsPromotion;
    public bool ActivatesEnPassant;
    public Piece RemovePieceEnPassant;
    public PIECE_TYPE PromotionPieceType;
    public GameObject PromotedPiece;
}
