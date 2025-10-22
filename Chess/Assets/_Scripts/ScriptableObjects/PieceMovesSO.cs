using UnityEngine;

[CreateAssetMenu(fileName = "PieceMoves", menuName = "PieceData/Moves")]
public class PieceMovesSO : ScriptableObject
{
    public Vector2Int[] PossibleMoves;
}
