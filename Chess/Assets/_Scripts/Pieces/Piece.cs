using System;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private PIECE_TYPE _pieceType;
    public PIECE_TYPE PieceType { get { return _pieceType; } }
    [SerializeField] protected Vector2Int[] _basicMoves;
    protected bool _isWhite;
    protected Square _square;
    public Square Square { get { return _square; } }
    public bool IsWhite { get { return _isWhite; } }
    protected bool _isFirstMove = true;
    public bool IsFirstMove { get { return _isFirstMove; } }

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

    public virtual void CalculateAvailableMoves(bool checkForChecks)
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
            if (possibleMoveSquare.PieceOnSquare != null && possibleMoveSquare.PieceOnSquare.IsWhite != _isWhite)
            {
                ignoreCurrentDirection = true;
                lastXSign = Mathf.Sign(move.x);
                lastYSign = Mathf.Sign(move.y);
            }
        }
    }

    public void RemovePinnedPieceMovesFromAvailableMoves()
    {
        Square currentSquare;
        bool remove;
        Piece capturedPiece;

        // loop backwards through the list so elements can be removed as we go
        for (int i = _availableMoves.Count - 1; i >= 0; i--)
        {
            remove = false;
            currentSquare = _square;

            if (_availableMoves[i].PieceOnSquare != null)
                capturedPiece = _availableMoves[i].PieceOnSquare;
            else capturedPiece = null;

            // move to valid square
            SetPieceSquare(_availableMoves[i]);
            _availableMoves[i].SetPieceOnSquare(this);
            currentSquare.SetPieceOnSquare(null);

            // check to see if on this new square the player would be in check
            // if they are that means that the piece was pinned and shouldn't be able to move
            if (PieceManager.Instance.CheckIfAnyPieceCanTakeKing(!_isWhite, capturedPiece))
                remove = true;

            // reset pieces and squares
            SetPieceSquare(currentSquare);
            _availableMoves[i].SetPieceOnSquare(capturedPiece);
            currentSquare.SetPieceOnSquare(this);

            if (remove) _availableMoves.RemoveAt(i);
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

    // if the piece can take the king, that means that the king would be in check
    public bool CheckIfPieceCanTakeKing()
    {
        foreach (Square square in _availableMoves)
        {
            if (square.PieceOnSquare != null && square.PieceOnSquare.PieceType == PIECE_TYPE.King)
                return true;
        }

        return false;
    }
}

public enum PIECE_TYPE
{
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}