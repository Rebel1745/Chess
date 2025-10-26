using Unity.VisualScripting;
using UnityEngine;

public class PieceKing : Piece
{
    public override void CalculateAvailableMoves(bool checkForChecks)
    {
        base.CalculateAvailableMoves(checkForChecks);
        Square possibleMoveSquare1, possibleMoveSquare2, possibleMoveSquare3, rookSquare;

        // see if we can castle king side
        possibleMoveSquare1 = BoardManager.Instance.GetSquare(_square.SquareX + 1, _square.SquareY);
        possibleMoveSquare2 = BoardManager.Instance.GetSquare(_square.SquareX + 2, _square.SquareY);
        rookSquare = BoardManager.Instance.GetSquare(7, _square.SquareY);

        // if the king is yet to move, and there is a rook that hasnt moved on its square and both squares in between don't have a piece on them
        if (_isFirstMove && rookSquare.PieceOnSquare != null && rookSquare.PieceOnSquare.IsFirstMove && possibleMoveSquare1.PieceOnSquare == null && possibleMoveSquare2.PieceOnSquare == null)
            _availableMoves.Add(BoardManager.Instance.GetSquare(_square.SquareX + 2, _square.SquareY));

        // see if we can castle queen side
        possibleMoveSquare1 = BoardManager.Instance.GetSquare(_square.SquareX - 1, _square.SquareY);
        possibleMoveSquare2 = BoardManager.Instance.GetSquare(_square.SquareX - 2, _square.SquareY);
        possibleMoveSquare3 = BoardManager.Instance.GetSquare(_square.SquareX - 3, _square.SquareY);
        rookSquare = BoardManager.Instance.GetSquare(0, _square.SquareY);

        // if the king is yet to move, and there is a rook that hasnt moved on its square and both squares in between don't have a piece on them
        if (_isFirstMove && rookSquare.PieceOnSquare != null && rookSquare.PieceOnSquare.IsFirstMove && possibleMoveSquare1.PieceOnSquare == null && possibleMoveSquare2.PieceOnSquare == null && possibleMoveSquare3.PieceOnSquare == null)
            _availableMoves.Add(BoardManager.Instance.GetSquare(_square.SquareX - 2, _square.SquareY));

        if (checkForChecks)
            RemovePinnedPieceMovesFromAvailableMoves();
    }
}
