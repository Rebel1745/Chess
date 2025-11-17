using System;
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

        if (ToggleManager.Instance.IsAnalysisModeActivated)
            CalculateAnalysisMoves(checkForChecks);

        Square possibleMoveSquare, possibleMoveSquare2;
        bool isPromotion = false;
        bool captureAvailable;
        Piece enPassantablePiece;

        // check the base move
        possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + _basicMoves[0].x, _square.SquareY + _basicMoves[0].y);
        if (possibleMoveSquare != null && ((_isWhite && _square.SquareY == 6) || (!_isWhite && _square.SquareY == 1)))
            isPromotion = true;

        if (possibleMoveSquare.PieceOnSquare == null)
            _availableMoves.Add(new MoveDetails
            {
                MoveNumber = -1,
                isWhite = _isWhite,
                PieceToMove = this,
                StartSquare = _square,
                EndSquare = possibleMoveSquare,
                IsPromotion = isPromotion,
                PGNCode = possibleMoveSquare.SquarePGNCode
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
                    MoveNumber = -1,
                    isWhite = _isWhite,
                    PieceToMove = this,
                    StartSquare = _square,
                    EndSquare = possibleMoveSquare,
                    IsPromotion = isPromotion,
                    ActivatesEnPassant = true,
                    PGNCode = possibleMoveSquare.SquarePGNCode
                });
        }

        foreach (Vector2Int move in _captureMoves)
        {
            captureAvailable = false;
            possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + move.x, _square.SquareY + move.y);

            if (possibleMoveSquare != null && ((_isWhite && _square.SquareY == 6) || (!_isWhite && _square.SquareY == 1)))
                isPromotion = true;

            if (possibleMoveSquare == null) continue;

            if (possibleMoveSquare.PieceOnSquare != null && possibleMoveSquare.PieceOnSquare.IsWhite != _isWhite)
                captureAvailable = true;

            if (captureAvailable)
                _availableMoves.Add(new MoveDetails
                {
                    MoveNumber = -1,
                    isWhite = _isWhite,
                    PieceToMove = this,
                    StartSquare = _square,
                    EndSquare = possibleMoveSquare,
                    IsPromotion = isPromotion,
                    PGNCode = _square.SquarePGNCode[..1] + "x" + possibleMoveSquare.SquarePGNCode
                });
        }

        // en passant check
        foreach (Vector2Int move in _captureMoves)
        {
            possibleMoveSquare2 = null;
            enPassantablePiece = null;
            captureAvailable = false;

            // even it we en passant, move to the normal capture square
            possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + move.x, _square.SquareY + move.y);

            // if the pawn is on the 5th rank (or 4th for black) 
            if ((_isWhite && Square.SquareY == 4) || (!_isWhite && Square.SquareY == 3))
            {
                possibleMoveSquare2 = BoardManager.Instance.GetSquare(_square.SquareX + move.x, _square.SquareY);
                // and the other pawn has just advanced 2 squares on its last move it can be captured as if it had only moved one square
                if (possibleMoveSquare2 != null && possibleMoveSquare2.PieceOnSquare != null && possibleMoveSquare2.PieceOnSquare.IsCanBeEnPassanted)
                {
                    captureAvailable = true;
                    enPassantablePiece = possibleMoveSquare2.PieceOnSquare;
                }
            }

            if (captureAvailable)
                _availableMoves.Add(new MoveDetails
                {
                    MoveNumber = -1,
                    isWhite = _isWhite,
                    PieceToMove = this,
                    StartSquare = _square,
                    EndSquare = possibleMoveSquare,
                    IsPromotion = isPromotion,
                    RemovePieceEnPassant = enPassantablePiece,
                    PGNCode = _square.SquarePGNCode[..1] + "x" + possibleMoveSquare.SquarePGNCode
                });
        }

        if (checkForChecks)
            RemovePinnedPieceMovesFromAvailableMoves();
    }

    public override void CalculateAnalysisMoves(bool checkForChecks)
    {
        _analysisMoves.Clear();

        Square possibleMoveSquare, possibleMoveSquare2;
        bool captureAvailable;

        // check the base move
        possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + _basicMoves[0].x, _square.SquareY + _basicMoves[0].y);

        if (possibleMoveSquare.PieceOnSquare == null)
            _analysisMoves.Add(new AnalysisMoveDetails
            {
                StartSquare = _square,
                EndSquare = possibleMoveSquare
            });

        // only check to see if we can move two squares if we can already move one
        if (_analysisMoves.Count == 1 && _isFirstMove)
        {
            possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + _firstMove.x, _square.SquareY + _firstMove.y);

            if (possibleMoveSquare.PieceOnSquare == null)
                _analysisMoves.Add(new AnalysisMoveDetails
                {
                    StartSquare = _square,
                    EndSquare = possibleMoveSquare
                });
        }

        foreach (Vector2Int move in _captureMoves)
        {
            captureAvailable = false;
            possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + move.x, _square.SquareY + move.y);

            if (possibleMoveSquare == null) continue;

            if (possibleMoveSquare.PieceOnSquare != null && possibleMoveSquare.PieceOnSquare.IsWhite != _isWhite)
                captureAvailable = true;

            //if (captureAvailable) change the move type to capture
            if (possibleMoveSquare != null)
                _analysisMoves.Add(new AnalysisMoveDetails
                {
                    StartSquare = _square,
                    EndSquare = possibleMoveSquare
                });
        }

        // en passant check
        foreach (Vector2Int move in _captureMoves)
        {
            possibleMoveSquare2 = null;
            captureAvailable = false;

            // even it we en passant, move to the normal capture square
            possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + move.x, _square.SquareY + move.y);

            // if the pawn is on the 5th rank (or 4th for black) 
            if ((_isWhite && Square.SquareY == 4) || (!_isWhite && Square.SquareY == 3))
            {
                possibleMoveSquare2 = BoardManager.Instance.GetSquare(_square.SquareX + move.x, _square.SquareY);
                // and the other pawn has just advanced 2 squares on its last move it can be captured as if it had only moved one square
                if (possibleMoveSquare2 != null && possibleMoveSquare2.PieceOnSquare != null && possibleMoveSquare2.PieceOnSquare.IsCanBeEnPassanted)
                    captureAvailable = true;
            }

            //if (captureAvailable) change the move type to capture
            if (possibleMoveSquare != null && possibleMoveSquare2 != null)
                _analysisMoves.Add(new AnalysisMoveDetails
                {
                    StartSquare = _square,
                    EndSquare = possibleMoveSquare
                });
        }

        if (checkForChecks)
            RemovePinnedPieceMovesFromAnalysisMoves();
    }
}
