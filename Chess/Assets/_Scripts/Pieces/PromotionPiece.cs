using UnityEngine;

public class PromotionPiece : MonoBehaviour
{
    [SerializeField] private PIECE_TYPE _pieceType;
    public PIECE_TYPE PieceType { get { return _pieceType; } }
}
