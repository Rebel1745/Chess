using System;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private Vector2Int[] _basicMoves;
    protected bool _isWhite;
    protected Square _square;
    public Square Square { get { return _square; } }
    public bool IsWhite { get { return _isWhite; } }
    protected bool _isFirstMove = false; // only used for pawns so can default to false

    protected List<Square> _availableMoves = new();

    public void SetupPiece(Square square, bool isWhite)
    {
        _isWhite = isWhite;
        _square = square;
    }

    public void SetIsFirstMove(bool isFirstMove)
    {
        _isFirstMove = isFirstMove;
    }

    public void SetPieceSquare(Square square)
    {
        _square = square;
    }

    // TODO: Ignore rest of current direction if the piece is able to capture an enemy piece
    // i.e. dont show moves past the captured piece as being legal

    public virtual void CalculateAvailableMoves()
    {
        _availableMoves.Clear();

        int startX = _square.SquareX;
        int startY = _square.SquareY;
        Square possibleMoveSquare;
        bool ignoreCurrentDirection = false;
        float lastXSign = Mathf.Sign(_basicMoves[0].x);
        float lastYSign = Mathf.Sign(_basicMoves[0].y);

        foreach (Vector2Int move in _basicMoves)
        {
            // if the signs don't match, we are checking in a different direction, reset the ignore variable
            if (Mathf.Sign(move.x) != lastXSign || Mathf.Sign(move.y) != lastYSign)
                ignoreCurrentDirection = false;

            possibleMoveSquare = BoardManager.Instance.GetSquare(startX + move.x, startY + move.y);

            // if there is no square, move on
            if (possibleMoveSquare == null) continue;

            // if the piece is landing on a piece of the same colour, we can't go there, move on
            if (possibleMoveSquare.PieceOnSquare != null && possibleMoveSquare.PieceOnSquare.IsWhite == _isWhite)
            {
                ignoreCurrentDirection = true;
                lastXSign = Mathf.Sign(move.x);
                lastYSign = Mathf.Sign(move.y);
                continue;
            }

            if (!ignoreCurrentDirection)
                _availableMoves.Add(possibleMoveSquare);

            // if we land on an oponent piece, we can't move past it
            // if (possibleMoveSquare.PieceOnSquare != null && possibleMoveSquare.PieceOnSquare.IsWhite != _isWhite)
            //     ignoreCurrentDirection = true;
        }
    }

    public void ShowHideAvailableMoves(bool show)
    {
        if (_availableMoves.Count == 0) return;

        foreach (Square square in _availableMoves)
        {
            square.ShowHidePossibleMoveIndicator(show);
        }
    }

    public bool CheckIfValidMove(Square square)
    {
        if (_availableMoves.Contains(square)) return true;
        else return false;
    }
}
