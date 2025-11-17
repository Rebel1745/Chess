using UnityEngine;

[System.Serializable]
public struct MoveDetails
{
    public int MoveNumber;
    public bool isWhite;
    public Piece PieceToMove;
    public Square StartSquare;
    public Square EndSquare;
    public Piece SecondPieceToMove;
    public Square SecondEndSquare;
    public bool IsPromotion;
    public bool ActivatesEnPassant;
    public Piece RemovePieceEnPassant;
    public PIECE_TYPE PromotionPieceType;
    public GameObject PromotedPiece;
    public string PGNCode;
}
