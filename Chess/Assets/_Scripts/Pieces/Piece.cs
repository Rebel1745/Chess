using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private PieceMovesSO _possibleMoves;
    public PieceMovesSO PossibleMoves { get { return _possibleMoves; } }
}
