using Unity.VisualScripting;
using UnityEngine;

public class PieceKing : Piece
{
    public override void CalculateAvailableMoves(bool checkForChecks)
    {
        _availableMoves.Clear();

        if (ToggleManager.Instance.IsAnalysisModeActivated)
            CalculateAnalysisMoves(checkForChecks);

        Square possibleMoveSquare, possibleMoveSquare2, possibleMoveSquare3, rookSquare;

        foreach (Vector2Int move in _basicMoves)
        {
            possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + move.x, _square.SquareY + move.y);

            // if there is no square, move on
            if (possibleMoveSquare == null) continue;

            // if the piece is landing on a piece of the same colour, we can't go there, move on
            if (possibleMoveSquare.PieceOnSquare != null && possibleMoveSquare.PieceOnSquare.IsWhite == _isWhite)
                continue;

            _availableMoves.Add(new MoveDetails
            {
                MoveNumber = -1,
                isWhite = _isWhite,
                PieceToMove = this,
                StartSquare = _square,
                EndSquare = possibleMoveSquare,
                PGNCode = "K" + (possibleMoveSquare.PieceOnSquare == null ? "" : "x") + possibleMoveSquare.SquarePGNCode
            });
        }

        // remove invalid moves from the list before checking if we can castle
        // this will allow us to see if the moves before castling are available
        // this should stop us from trying to castle through checks
        if (checkForChecks)
            RemovePinnedPieceMovesFromAvailableMoves();

        // see if we can castle king side
        possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + 1, _square.SquareY);
        possibleMoveSquare2 = BoardManager.Instance.GetSquare(_square.SquareX + 2, _square.SquareY);
        rookSquare = BoardManager.Instance.GetSquare(7, _square.SquareY);

        // if the king is yet to move, and there is a rook that hasnt moved on its square and both squares in between don't have a piece on them
        if (_isFirstMove &&
        CheckIfValidMove(possibleMoveSquare) &&
        rookSquare.PieceOnSquare != null &&
        rookSquare.PieceOnSquare.IsWhite == _isWhite &&
        rookSquare.PieceOnSquare.PieceType == PIECE_TYPE.Rook &&
        rookSquare.PieceOnSquare.IsFirstMove &&
        possibleMoveSquare.PieceOnSquare == null &&
        possibleMoveSquare2.PieceOnSquare == null)
            _availableMoves.Add(new MoveDetails
            {
                MoveNumber = -1,
                isWhite = _isWhite,
                PieceToMove = this,
                StartSquare = _square,
                EndSquare = BoardManager.Instance.GetSquare(_square.SquareX + 2, _square.SquareY),
                SecondPieceToMove = rookSquare.PieceOnSquare,
                SecondEndSquare = possibleMoveSquare,
                PGNCode = "0-0"
            });

        // see if we can castle queen side
        possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX - 1, _square.SquareY);
        possibleMoveSquare2 = BoardManager.Instance.GetSquare(_square.SquareX - 2, _square.SquareY);
        possibleMoveSquare3 = BoardManager.Instance.GetSquare(_square.SquareX - 3, _square.SquareY);
        rookSquare = BoardManager.Instance.GetSquare(0, _square.SquareY);

        // if the king is yet to move, and there is a rook that hasnt moved on its square and both squares in between don't have a piece on them
        if (_isFirstMove &&
        CheckIfValidMove(possibleMoveSquare) &&
        rookSquare.PieceOnSquare != null &&
        rookSquare.PieceOnSquare.IsWhite == _isWhite &&
        rookSquare.PieceOnSquare.PieceType == PIECE_TYPE.Rook &&
        rookSquare.PieceOnSquare.IsFirstMove &&
        possibleMoveSquare.PieceOnSquare == null &&
        possibleMoveSquare2.PieceOnSquare == null &&
        possibleMoveSquare3.PieceOnSquare == null)
            _availableMoves.Add(new MoveDetails
            {
                MoveNumber = -1,
                isWhite = _isWhite,
                PieceToMove = this,
                StartSquare = _square,
                EndSquare = BoardManager.Instance.GetSquare(_square.SquareX - 2, _square.SquareY),
                SecondPieceToMove = rookSquare.PieceOnSquare,
                SecondEndSquare = possibleMoveSquare,
                PGNCode = "0-0-0"
            });

        // this will remove the castling move if it is not valid
        if (checkForChecks)
            RemovePinnedPieceMovesFromAvailableMoves();
    }
}
