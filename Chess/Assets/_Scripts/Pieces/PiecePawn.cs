using UnityEngine;

public class PiecePawn : Piece
{
    [SerializeField] private Vector2Int _firstMove;
    [SerializeField] private Vector2Int[] _captureMoves;

    public override void CalculateAvailableMoves(bool checkForChecks)
    {
        // don't run the base function as the basic movement for a pawn does not allow taking another piece
        //base.CalculateAvailableMoves(checkForChecks);

        _availableMoves.Clear();
        Square possibleMoveSquare;
        bool isPromotion = false;

        // check the base move
        possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + _basicMoves[0].x, _square.SquareY + _basicMoves[0].y);
        if (possibleMoveSquare != null && ((_isWhite && _square.SquareY == 6) || (!_isWhite && _square.SquareY == 1)))
            isPromotion = true;

        if (possibleMoveSquare.PieceOnSquare == null)
            _availableMoves.Add(new MoveDetails
            {
                PieceToMove = this,
                MoveToSquare = possibleMoveSquare,
                IsPromotion = isPromotion
            });

        // only check to see if we can move two squares if we can already move one
        if (_availableMoves.Count == 1 && _isFirstMove)
        {
            possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + _firstMove.x, _square.SquareY + _firstMove.y);
            if (possibleMoveSquare != null && ((_isWhite && _square.SquareY == 6) || (!_isWhite && _square.SquareY == 1)))
                isPromotion = true;

            if (possibleMoveSquare.PieceOnSquare == null)
                _availableMoves.Add(new MoveDetails
                {
                    PieceToMove = this,
                    MoveToSquare = possibleMoveSquare,
                    IsPromotion = isPromotion
                });
        }

        foreach (Vector2Int move in _captureMoves)
        {
            possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + move.x, _square.SquareY + move.y);
            if (possibleMoveSquare != null && ((_isWhite && _square.SquareY == 6) || (!_isWhite && _square.SquareY == 1)))
                isPromotion = true;

            if (possibleMoveSquare == null) continue;

            if (possibleMoveSquare.PieceOnSquare != null && possibleMoveSquare.PieceOnSquare.IsWhite != _isWhite)
                _availableMoves.Add(new MoveDetails
                {
                    PieceToMove = this,
                    MoveToSquare = possibleMoveSquare,
                    IsPromotion = isPromotion
                });
        }

        if (checkForChecks)
            RemovePinnedPieceMovesFromAvailableMoves();
    }
}
