using UnityEngine;

public class PiecePawn : Piece
{
    [SerializeField] private Vector2Int _firstMove;
    [SerializeField] private Vector2Int[] _captureMoves;

    public override void CalculateAvailableMoves()
    {
        base.CalculateAvailableMoves();

        Square possibleMoveSquare;

        if (_isFirstMove)
        {
            possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + _firstMove.x, _square.SquareY + _firstMove.y);

            if (possibleMoveSquare.PieceOnSquare == null)
                _availableMoves.Add(possibleMoveSquare);
        }

        foreach (Vector2Int move in _captureMoves)
        {
            possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + move.x, _square.SquareY + move.y);

            if (possibleMoveSquare == null) continue;

            if (possibleMoveSquare.PieceOnSquare != null && possibleMoveSquare.PieceOnSquare.IsWhite != _isWhite)
                _availableMoves.Add(possibleMoveSquare);
        }
    }
}
